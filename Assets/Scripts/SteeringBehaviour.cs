using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
using Vector3 = UnityEngine.Vector3;

public class SteeringBehaviour
{
    public class VelocityObstacleData
    {
        // hacky, used to associate a ray with an agent and know what agent's velocity to adjust upon collision
        public NavMeshAgent agent;

        public Ray ray;
    }

    private const float MAX_ACCELERATION = 0.5f;

    private float rayDistance = 5f;
    public int obstacleWidthMultiplier = 5;
    public Vector3 intersectionPoint;

    private Vector3 Rotate2D(Vector3 source, float angle)
    {
        var result = source;
        result.x = Mathf.Cos(angle) * source.x - Mathf.Sin(angle) * source.y;
        result.y = Mathf.Sin(angle) * source.x + Mathf.Cos(angle) * source.y;
        return result;
    }

    public void DrawVelocityObstacles(List<VelocityObstacleData> reusedVelocityVectors, NavMeshAgent navMeshAgent, NavMeshAgent nearestNeighbour)
    {
        var agentVelocityObstacle = navMeshAgent.transform.GetChild(0);
        // Orientate VO towards neighbour
        agentVelocityObstacle.LookAt(nearestNeighbour.transform);

        // Compute RVO position depending on average of velocities of colliding agents
        var avgVelocity = (navMeshAgent.velocity + nearestNeighbour.velocity) * 0.5f;
        var origin = agentVelocityObstacle.position + avgVelocity;

        // Compute direction of each velocity obstacle depending on distance from nearest neighbour
        var distance = Vector3.Distance(navMeshAgent.transform.position, nearestNeighbour.transform.position);
        // var directionRight = (agentVelocityObstacle.forward + agentVelocityObstacle.right * navMeshAgent.radius * obstacleWidthMultiplier * 1 / distance).normalized;
        // var directionLeft = (agentVelocityObstacle.forward - agentVelocityObstacle.right * navMeshAgent.radius * obstacleWidthMultiplier * 1 / distance).normalized;
        var alpha = Mathf.Asin(navMeshAgent.radius * obstacleWidthMultiplier / distance);
        var directionRight = Rotate2D(agentVelocityObstacle.forward, alpha);
        var directionLeft = Rotate2D(agentVelocityObstacle.forward, -alpha);

        var rayObstacleRight = new Ray(origin, directionRight);
        var rayObstacleLeft = new Ray(origin, directionLeft);

        // Draw ray visualisation
        Debug.DrawLine(rayObstacleRight.origin, rayObstacleRight.origin + rayObstacleRight.direction * rayDistance, Color.red);
        Debug.DrawLine(rayObstacleLeft.origin,  rayObstacleLeft.origin + rayObstacleLeft.direction * rayDistance, Color.blue);
        // Debug.DrawLine(navMeshAgent.transform.position, nearestNeighbour.transform.position, Color.green);
        Debug.DrawLine(navMeshAgent.transform.position, navMeshAgent.transform.position + rayObstacleRight.direction * distance, Color.green);
        Debug.DrawLine(navMeshAgent.transform.position, navMeshAgent.transform.position + rayObstacleLeft.direction * distance, Color.green);

        reusedVelocityVectors.Add(new VelocityObstacleData {agent = navMeshAgent, ray = rayObstacleLeft});
        reusedVelocityVectors.Add(new VelocityObstacleData {agent = navMeshAgent, ray = rayObstacleRight});
    }

    public void DoSteering(Dictionary<string, List<VelocityObstacleData>> agentToVelocityVectors)
    {
        // Group all VOs together for each agent to check for intersections against
        var othersVelocityObstacles = new List<VelocityObstacleData>();
        foreach (var v in agentToVelocityVectors)
        {
            othersVelocityObstacles.AddRange(v.Value);
        }

        // For each agent to velocity vector
        foreach (var agentToVelocityVector in agentToVelocityVectors)
        {
            // Compute intersection point between agent's velocity vectors and the rest
            var (intersectionFound, intersectionPoints) = ComputeIntersectionPoint(agentToVelocityVector.Value, othersVelocityObstacles);
            if (intersectionFound)
            {
                // If intersection is found, adjust velocity (within certain acceleration)
                var agent = agentToVelocityVector.Value[0].agent;
                List<Vector3> remappedIntersectionPoints = new List<Vector3>();
                foreach (var intersectionPoint in intersectionPoints)
                {
                    var remappedPoint = agent.transform.TransformVector(intersectionPoint);
                    remappedIntersectionPoints.Add(remappedPoint);
                }
                //agent.velocity = CalculateClosestIntersectionPoint(remappedIntersectionPoints, agent);
                var intersectionPointLocal = CalculateClosestIntersectionPoint(intersectionPoints, agent);
                var intersectionPointWorldSpace = CalculateClosestIntersectionPoint(remappedIntersectionPoints, agent);
                agent.velocity = intersectionPointWorldSpace;
                Debug.Log($"point {intersectionPointLocal}, remapped point {intersectionPointWorldSpace}");
                GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = intersectionPointWorldSpace;
                agent.acceleration = Mathf.Min(agent.acceleration, MAX_ACCELERATION);
                // Debug.Log("updating velocity to " + agent.velocity);
            }
        }
    }

    private Vector3 CalculateClosestIntersectionPoint(List<Vector3> intersectionPoints, NavMeshAgent agent)
    {
        SortedDictionary<float, Vector3> pointToDistance = new SortedDictionary<float, Vector3>();
        foreach (var point in intersectionPoints)
        {
            // Sort by closest velocity to desired velocity
            pointToDistance[Vector3.Distance(point, agent.desiredVelocity)] = point;
        }

        return pointToDistance.Values.First();
    }

    private (bool, List<Vector3>) ComputeIntersectionPoint(List<VelocityObstacleData> agentsVelocityObstacles, List<VelocityObstacleData> othersVelocityObstacles)
    {
        var intersectionPoints = new List<Vector3>();
        foreach (var uData in agentsVelocityObstacles)
        {
            var u = uData.ray;

            foreach (var vData in othersVelocityObstacles)
            {
                var v = vData.ray;

                // Ensure we don't find intersection with own rays
                if (uData.agent.name == vData.agent.name)
                {
                    continue;
                }

                // if (v.origin == u.origin)
                // {
                //     continue;
                // }

                var up1 = u.origin;
                var up2 = rayDistance * (u.origin + u.direction);

                var vp1 = v.origin;
                var vp2 = rayDistance * (v.origin + v.direction);

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
                double truncatedCalc = Math.Truncate(planeNormal.x * intersectionPointLocal.x +
                                                     planeNormal.y * intersectionPointLocal.y +
                                                     planeNormal.z * intersectionPointLocal.z * 100) / 100;
                double truncatedD = Math.Truncate(d * 100) / 100;
                var isValid = Mathf.Approximately((float) (truncatedCalc + truncatedD), 0f);
                if (isValid)
                {
                    intersectionPoint = intersectionPointLocal;
                    intersectionPoints.Add(intersectionPointLocal);
                }
            }
        }

        return (intersectionPoints.Count > 0, intersectionPoints);
    }
}