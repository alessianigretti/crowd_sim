using UnityEngine;
using UnityEngine.AI;

public class SteeringBehaviour : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
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

    private void Start()
    {
        navMeshAgent = GetComponentInParent<NavMeshAgent>();
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Untagged"))
        {
            return;
        }
        
        switch (other.gameObject.tag)
        {
            case "Outside":
                // TODO: sort by contact point distance from x0
                var closestOutsidePoint = other.contacts[0].point;

                Debug.Log("Assigning new velocity");
                navMeshAgent.velocity = closestOutsidePoint;
                
                break;
            case "Inside":
                break;
        }
    }
}
