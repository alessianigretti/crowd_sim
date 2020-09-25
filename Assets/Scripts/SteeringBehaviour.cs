using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

public class SteeringBehaviour
{
    private float rayDistance = 2f;
    private int obstacleWidthMultiplier = 5;

    public List<Ray> DrawVelocityObstacles(NavMeshAgent navMeshAgent, NavMeshAgent nearestNeighbour)
    {
        var agentTransform = navMeshAgent.transform;
        var agentVelocityObstacle = navMeshAgent.transform.GetChild(0);

        // Compute VO position depending on average of velocities of colliding agents
        var avgVelocity = (navMeshAgent.velocity + nearestNeighbour.velocity) * 0.5f;
        var origin = agentVelocityObstacle.position + avgVelocity;
        
        var distance = Vector3.Distance(agentTransform.position, nearestNeighbour.transform.position);
        var directionRight = (agentVelocityObstacle.forward + agentVelocityObstacle.right * navMeshAgent.radius * obstacleWidthMultiplier * 1/distance).normalized;
        var directionLeft = (agentVelocityObstacle.forward - agentVelocityObstacle.right * navMeshAgent.radius * obstacleWidthMultiplier * 1/distance).normalized;

        var rayObstacleRight = new Ray(origin, directionRight);
        var rayObstacleLeft = new Ray(origin, directionLeft);

        var endRight = rayObstacleRight.GetPoint(rayDistance);
        var endLeft = rayObstacleLeft.GetPoint(rayDistance);

        agentVelocityObstacle.LookAt(nearestNeighbour.transform);

        Debug.DrawRay(rayObstacleRight.origin, rayObstacleRight.direction * rayDistance, Color.red);
        Debug.DrawRay(rayObstacleLeft.origin, rayObstacleLeft.direction * rayDistance, Color.blue);
        
        return new List<Ray> {rayObstacleLeft, rayObstacleRight};
    }

    public void DoSteering(List<Ray> velocityVectors)
    {
        // todo: calculate collision points between two lines
        // todo: choose closest to the desired velocity vector
        // todo: that point becomes the new velocity

        var intersectionPoint = ComputeIntersectionPoint(velocityVectors);
    }

    private Vector3 ComputeIntersectionPoint(List<Ray> velocityVectors)
    {
        // foreach (var u in velocityVectors)
        // {
        //     foreach (var v in velocityVectors)
        //     {
                var u = new Ray(new Vector3(-0.5f, -0.5f, 0), Vector3.up);
                var v = new Ray(new Vector3(0, 0, 0), Vector3.left);
        
                // 1. Find perpendicular vector to U by doing cross product between its direction and the direction of the other vector V
                // This describes a shared plane between the two vectors
                var sharedPlane = Vector3.Cross(u.direction, v.direction);
                
                // 2. Find D from the equation of the plane Ax + By + Cz + D = 0
                // (A,B,C) are replaced by the normal of the plane, and (x,y,z) with the origin of U
                var planeNormal = sharedPlane.normalized;
                var uOrigin = u.origin;
                var d = -(planeNormal.x * uOrigin.x + planeNormal.y * uOrigin.y + planeNormal.z * uOrigin.z);

                // 3. Replace the ray parametric values for V in the plane equation to find t
                // This means calculating A*(ox + dx*t) + B*(oy + dy*t) + C*(oz + dz*t) + D = 0
                // Replacing A, B, C, D with normal and the calculated D, and replacing OX, OY, OZ with the origin vector of V, and DX, DY, DZ with the direction vector of V.
                var vOrigin = v.origin;
                var vDirection = v.direction;
                var t = -(planeNormal.x * vOrigin.x + planeNormal.y * vOrigin.y + planeNormal.z * vOrigin.z + d) / 
                        (planeNormal.x * vDirection.x + planeNormal.y * vDirection.y + planeNormal.z * vDirection.z);

                // 4. Use the parametric ray equation with t to get the intersection point
                // This means solving A*(ox + dx*t) + B*(oy + dy*t) + C*(oz + dz*t) + D = 0 for vector V replacing t with the newly obtained parameter t.
                var intersectionPoint = new Vector3(vOrigin.x + vDirection.x * t, vOrigin.y + vDirection.y * t, vOrigin.z + vDirection.z * t);
                Debug.Log(intersectionPoint);

                // 5. Validate intersection point
                // Check that it is inside the line range and it satisfies the plane equation
                var isValid = Mathf.Approximately(planeNormal.x * intersectionPoint.x + planeNormal.y * intersectionPoint.y + planeNormal.z * intersectionPoint.z + d, 0f);

                return isValid ? intersectionPoint : new Vector3(999, 999, 999);
        //     }
        // }
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
