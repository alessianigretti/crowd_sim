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
    
    private float rayDistance = 1f;
    private int obstacleWidthMultiplier = 5;

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
        var directionRight = (agentVelocityObstacle.forward + agentVelocityObstacle.right * navMeshAgent.radius * obstacleWidthMultiplier * 1/distance).normalized;
        var directionLeft = (agentVelocityObstacle.forward - agentVelocityObstacle.right * navMeshAgent.radius * obstacleWidthMultiplier * 1/distance).normalized;

        var rayObstacleRight = new Ray(origin, directionRight);
        var rayObstacleLeft = new Ray(origin, directionLeft);

        // Draw ray visualisation
        Debug.DrawLine(rayObstacleRight.origin, rayDistance * (rayObstacleRight.origin + rayObstacleRight.direction), Color.red);
        Debug.DrawLine(rayObstacleLeft.origin, rayDistance * (rayObstacleLeft.origin + rayObstacleLeft.direction), Color.blue);

        reusedVelocityVectors.Add(new VelocityObstacleData {agent = navMeshAgent, ray = rayObstacleLeft});
        reusedVelocityVectors.Add(new VelocityObstacleData {agent = navMeshAgent, ray = rayObstacleRight});
    }

    public void DoSteering(string agentName, List<VelocityObstacleData> velocityVectors)
    {
        var (intersectionFound, intersectionPoints, agent1, agent2) = ComputeIntersectionPoint(agentName, velocityVectors);
        if (intersectionFound)
        {
            agent1.velocity = CalculateClosestIntersectionPoint(intersectionPoints, agent1);
            agent2.velocity = CalculateClosestIntersectionPoint(intersectionPoints, agent2);
            agent1.acceleration = Mathf.Min(agent1.acceleration, MAX_ACCELERATION);
            agent2.acceleration = Mathf.Min(agent2.acceleration, MAX_ACCELERATION);
            Debug.Log("updating velocity to " + agent1.velocity);
        }
    }

    private Vector3 CalculateClosestIntersectionPoint(List<Vector3> intersectionPoints, NavMeshAgent agent)
    {
        SortedDictionary<float, Vector3> pointToDistance = new SortedDictionary<float, Vector3>();
        foreach (var point in intersectionPoints)
        {
            pointToDistance[Vector3.Distance(point, agent.desiredVelocity)] = point;
        }

        return pointToDistance.Values.First();
    }

    private (bool, List<Vector3>, NavMeshAgent, NavMeshAgent) ComputeIntersectionPoint(string agentName, List<VelocityObstacleData> velocityVectors)
    {
        var intersectionPoints = new List<Vector3>();
        NavMeshAgent agent1 = null;
        NavMeshAgent agent2 = null;
        foreach (var uData in velocityVectors)
        {
            var u = uData.ray;
            agent1 = uData.agent;
            
            foreach (var vData in velocityVectors)
            {
                var v = vData.ray;
                agent2 = vData.agent;
                
                // Ensure we don't find intersection with own rays
                if (uData.agent.name == vData.agent.name)
                {
                    continue;
                }

                if (v.origin == u.origin)
                {
                    continue;
                }

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
                var intersectionPoint = vOrigin + vDirection * t;

                // 5. Validate intersection point
                // Check that it is inside the line range and it satisfies the plane equation
                double truncatedCalc = Math.Truncate(planeNormal.x * intersectionPoint.x + planeNormal.y * intersectionPoint.y +
                                                     planeNormal.z * intersectionPoint.z * 100) / 100;
                double truncatedD = Math.Truncate(d * 100) / 100;
                var isValid = Mathf.Approximately((float) (truncatedCalc + truncatedD), 0f);
                if (isValid)
                {
                    intersectionPoints.Add(intersectionPoint);
                }
            }
        }

        return (intersectionPoints.Count > 0, intersectionPoints, agent1, agent2);
    }
    
    // private void OnCollisionEnter(Collision other)
    // {
    //     switch (other.gameObject.tag)
    //     {
    //         case "Outside":
    //             contactPoints = new ContactPoint[other.contactCount];
    //             other.GetContacts(contactPoints);
    //             Array.Sort(contactPoints,
    //                 (point1, point2) =>
    //                     Vector3.Distance(point1.point, navMeshAgent.transform.position).CompareTo(Vector3.Distance(point2.point, navMeshAgent.transform.position)));
    //             var closestOutsidePoint = contactPoints[0].point;
    //             
    //             navMeshAgent.velocity = closestOutsidePoint.normalized;
    //             
    //             break;
    //         case "Inside":
    //             break;
    //     }
    // }
    
    //private float rayDistance = 10f;
    //private List<FVOConstraint> constraints = new List<FVOConstraint>();

    //private GameObject colliderRightIn, colliderRightOut, colliderLeftIn, colliderLeftOut;

    // public SteeringBehaviour()
    // {
    //     colliderRightIn = new GameObject("RightIn");
    //     colliderRightIn.AddComponent<EdgeCollider2D>();
    //     colliderRightOut = new GameObject("RightOut");
    //     colliderRightOut.AddComponent<EdgeCollider2D>();
    //     colliderLeftIn = new GameObject("LeftIn");
    //     colliderLeftIn.AddComponent<EdgeCollider2D>();
    //     colliderLeftOut = new GameObject("LeftOut");
    //     colliderLeftOut.AddComponent<EdgeCollider2D>();
    // }
    //
    // public List<FVOConstraint> ComputeFVOConstraints(List<KDTreeBuilder.Neighbour> obstaclesPos)
    // {
    //     constraints.Clear();
    //     
    //     foreach (var obstacle in obstaclesPos)
    //     {
    //         var agentTransform = obstacle.Agent.transform;
    //
    //         colliderRightIn.transform.position = agentTransform.position + agentTransform.forward * 0.25f;
    //         RaycastHit2D[] resultsRightIn = new RaycastHit2D[2];
    //         colliderRightIn.GetComponent<EdgeCollider2D>().Raycast((agentTransform.forward + agentTransform.right * obstacle.Agent.radius).normalized, resultsRightIn, rayDistance - 0.25f);
    //         foreach (var result in resultsRightIn)
    //         {
    //             if (result.transform != null && result.transform.gameObject.name != "RightIn")
    //             {
    //                 Debug.Log(result.transform.gameObject.name);
    //             }
    //         }
    //         
    //         colliderRightOut.transform.position = agentTransform.position;
    //         RaycastHit2D[] resultsRightOut = new RaycastHit2D[2];
    //         colliderRightOut.GetComponent<EdgeCollider2D>().Raycast((agentTransform.forward + agentTransform.right * obstacle.Agent.radius).normalized, resultsRightOut, rayDistance);
    //         foreach (var result in resultsRightOut)
    //         {
    //             if (result.transform != null && result.transform.gameObject.name != "RightOut")
    //             {
    //                 Debug.Log(result.transform.gameObject.name);
    //             }
    //         }
    //         
    //         colliderLeftIn.transform.position = agentTransform.position + agentTransform.forward * 0.25f;
    //         RaycastHit2D[] resultsLeftIn = new RaycastHit2D[2];
    //         colliderLeftIn.GetComponent<EdgeCollider2D>().Raycast((agentTransform.forward - agentTransform.right * obstacle.Agent.radius).normalized, resultsLeftIn, rayDistance - 0.25f);
    //         foreach (var result in resultsLeftIn)
    //         {
    //             if (result.transform != null && result.transform.gameObject.name != "LeftIn")
    //             {
    //                 Debug.Log(result.transform.gameObject.name);
    //             }
    //         }
    //         
    //         colliderLeftOut.transform.position = agentTransform.position;
    //         RaycastHit2D[] resultsLeftOut = new RaycastHit2D[2];
    //         colliderLeftOut.GetComponent<EdgeCollider2D>().Raycast((agentTransform.forward - agentTransform.right * obstacle.Agent.radius).normalized, resultsLeftOut, rayDistance);
    //         foreach (var result in resultsLeftOut)
    //         {
    //             if (result.transform != null && result.transform.gameObject.name != "LeftOut")
    //             {
    //                 Debug.Log(result.transform.gameObject.name);
    //             }
    //         }
    //         
    //         var rayObstacleRightInside = new Ray(agentTransform.position + agentTransform.forward * 0.25f, (agentTransform.forward + agentTransform.right * obstacle.Agent.radius).normalized);
    //         var rayObstacleRightOutside = new Ray(agentTransform.position, (agentTransform.forward + agentTransform.right * obstacle.Agent.radius).normalized);
    //         var rayObstacleLeftInside = new Ray(agentTransform.position + agentTransform.forward * 0.25f, (agentTransform.forward - agentTransform.right * obstacle.Agent.radius).normalized);
    //         var rayObstacleLeftOutside = new Ray(agentTransform.position, (agentTransform.forward - agentTransform.right * obstacle.Agent.radius).normalized);
    //         
    //         Debug.DrawRay(rayObstacleRightInside.origin, rayObstacleRightInside.direction * (rayDistance - 0.25f), Color.red);
    //         Debug.DrawRay(rayObstacleLeftInside.origin, rayObstacleLeftInside.direction * (rayDistance - 0.25f), Color.red);
    //         Debug.DrawRay(rayObstacleRightOutside.origin, rayObstacleRightOutside.direction * rayDistance, Color.blue);
    //         Debug.DrawRay(rayObstacleLeftOutside.origin, rayObstacleLeftOutside.direction * rayDistance, Color.blue);
    //         
    //         constraints.Add(new FVOConstraint
    //         {
    //             RightRayInside = rayObstacleRightInside,
    //             RightRayOutside = rayObstacleRightOutside,
    //             LeftRayInside = rayObstacleLeftInside,
    //             LeftRayOutside = rayObstacleLeftOutside
    //         });
    //     }
    //
    //     return constraints;
    // }
}
