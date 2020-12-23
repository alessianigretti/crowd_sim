using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class SteeringBehaviour
{
    public class VelocityObstacleData
    {
        // Hacky, used to associate a ray with an agent and know what agent's velocity to adjust upon collision
        // TODO: find a better way to uniquely identify agents
        public NavMeshAgent agent;

        public Ray ray;
    }

    private float rayDistance = 5f;
    private List<Vector3> remappedIntersectionPoints = new List<Vector3>();
    private SortedDictionary<float, Vector3> pointToDistance = new SortedDictionary<float, Vector3>();
    private List<Vector3> intersectionPoints = new List<Vector3>();
    private NavMeshAgent intersectingAgent;

    public void ComputeVelocityObstacles(List<VelocityObstacleData> reusedVelocityObstacles, NavMeshAgent navMeshAgent, NavMeshAgent nearestNeighbour)
    {
        var agentVelocityObstacle = navMeshAgent.transform.GetChild(0);
        // Orientate VO towards destination
        agentVelocityObstacle.LookAt(navMeshAgent.destination);

        // Compute RVO position depending on average of velocities of colliding agents
        var avgVelocity = (navMeshAgent.velocity + nearestNeighbour.velocity) * 0.5f;
        var origin = navMeshAgent.transform.position + avgVelocity;

        // Compute direction of each velocity obstacle depending on distance from nearest neighbour
        // Note: this is a variation to ClearPath that also uses a ray for straight direction instead of a solid obstacle cone
        var distance = Vector3.Distance(navMeshAgent.transform.position, nearestNeighbour.transform.position);
        var directionStraight = (agentVelocityObstacle.forward / distance).normalized;
        var directionRight = (agentVelocityObstacle.forward + agentVelocityObstacle.right * navMeshAgent.radius / distance).normalized;
        var directionLeft = (agentVelocityObstacle.forward - agentVelocityObstacle.right * navMeshAgent.radius / distance).normalized;

        var rayStraight = new Ray(origin, directionStraight);
        var rayObstacleRight = new Ray(origin, directionRight);
        var rayObstacleLeft = new Ray(origin, directionLeft);

        reusedVelocityObstacles.Add(new VelocityObstacleData {agent = navMeshAgent, ray = rayStraight});
        reusedVelocityObstacles.Add(new VelocityObstacleData {agent = navMeshAgent, ray = rayObstacleLeft});
        reusedVelocityObstacles.Add(new VelocityObstacleData {agent = navMeshAgent, ray = rayObstacleRight});
    }

    public void DoSteering(List<VelocityObstacleData> agentToVelocityObstacles)
    {
        // Note: for performance reasons, currently only adjusting one agent's velocity per tick
        
        // Compute intersection point between agent's velocity obstacles and the rest
        var (intersectionFound, intersectionPoints, agent) = ComputeIntersectionPoint(agentToVelocityObstacles);
    
        if (intersectionFound)
        {
            // If intersection is found, remap to world space
            remappedIntersectionPoints.Clear();
            foreach (var intersectionPoint in intersectionPoints)
            {
                var remappedPoint = agent.transform.TransformVector(intersectionPoint);
                remappedIntersectionPoints.Add(remappedPoint);
            }
        
            // Adjust agent velocity
            agent.velocity = CalculateClosestIntersectionPoint(remappedIntersectionPoints, agent);
            Debug.Log($"Agent {agent.name} velocity adjusted to {agent.velocity}");
        }
    }

    private Vector3 CalculateClosestIntersectionPoint(List<Vector3> intersectionPoints, NavMeshAgent agent)
    {
        pointToDistance.Clear();
        foreach (var point in intersectionPoints)
        {
            // Sort by closest velocity to current destination (TODO: why is division necessary?)
            pointToDistance[Vector3.Distance(point, agent.destination)] = point / 10;
        }

        return pointToDistance.Values.First();
    }

    private (bool, List<Vector3>, NavMeshAgent) ComputeIntersectionPoint(List<VelocityObstacleData> agentsVelocityObstacles)
    {
        intersectionPoints.Clear();
        intersectingAgent = null;
        
        for (int i = 0; i < agentsVelocityObstacles.Count; i++)
        {
            for (int j = 0; j < agentsVelocityObstacles.Count; j++)
            {
                // Ensure we don't find intersection with ray itself
                if (agentsVelocityObstacles[i].agent.name == agentsVelocityObstacles[j].agent.name)
                {
                    continue;
                }
    
                var u = agentsVelocityObstacles[i].ray;
                var v = agentsVelocityObstacles[j].ray;
                
                var up1 = u.origin;
                var up2 = u.origin + u.direction * rayDistance;
                
                var vp1 = v.origin;
                var vp2 = v.origin + v.direction * rayDistance;
                
                Debug.DrawLine(up1, up2, Color.red);
                Debug.DrawLine(vp1,  vp2, Color.blue);
                
                // 1. Find a perpendicular vector to U's direction (normal of the plane containing U).
                // This can be computed using the cross product or simply by swapping two components (X and Y) and negating one of them.
                var tan = Vector3.Cross(up2, vp2);
                var planeNormal = Vector3.Cross(up2, tan);
                
                // 2. Find D from the equation of the plane Ax + By + Cz + D = 0
                // (A,B,C) are replaced by the normal of the plane, and (x,y,z) with the origin of U
                var uOrigin = up1;
                var d = -1 * (planeNormal.x * uOrigin.x + planeNormal.y * uOrigin.y + planeNormal.z * uOrigin.z);
                
                // 3. Replace the ray parametric values for V in the plane equation to find t
                // This means calculating A*(ox + dx*t) + B*(oy + dy*t) + C*(oz + dz*t) + D = 0
                // Replacing A, B, C, D with normal and the calculated D, and replacing OX, OY, OZ with the origin vector of V, and DX, DY, DZ with the direction vector of V.
                var vOrigin = vp1;
                var vDirection = vp2;
                var t = -(planeNormal.x * vOrigin.x + planeNormal.y * vOrigin.y + planeNormal.z * vOrigin.z + d) /
                        (planeNormal.x * vDirection.x + planeNormal.y * vDirection.y +
                         planeNormal.z * vDirection.z);
                
                // 4. Use the parametric ray equation with t to get the intersection point
                // This means solving A*(ox + dx*t) + B*(oy + dy*t) + C*(oz + dz*t) + D = 0 for vector V replacing t with the newly obtained parameter t.
                var intersectionPointLocal = vOrigin + vDirection * t;
                
                // 5. Validate intersection point
                // Check that it is inside the line range and it satisfies the plane equation
                // Note: hacky calculations to check for approx the same values, probably source of some bugs where collision is not detected 
                double truncatedCalc = Math.Truncate(planeNormal.x * intersectionPointLocal.x +
                                                     planeNormal.y * intersectionPointLocal.y +
                                                     planeNormal.z * intersectionPointLocal.z * 100) / 100;
                double truncatedD = Math.Truncate(d * 100) / 100;
                var isValid = Mathf.Approximately((float) (truncatedCalc + truncatedD), 0f);
                if (isValid)
                {
                    intersectionPoints.Add(intersectionPointLocal);
                    intersectingAgent = agentsVelocityObstacles[i].agent;
                }
            }
        }
        
        return (intersectionPoints.Count > 0, intersectionPoints, intersectingAgent);
    }
}