using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public struct FVOConstraint
{
    public Ray LeftRayInside;
    public Ray LeftRayOutside;
    public Ray RightRayInside;
    public Ray RightRayOutside;
}

public class SteeringBehaviour
{
    private float rayDistance = 10f;
    private List<FVOConstraint> constraints = new List<FVOConstraint>();
    
    public List<FVOConstraint> ComputeFVOConstraints(NavMeshAgent agent, List<KDTreeBuilder.Neighbour> obstaclesPos)
    {
        constraints.Clear();
        
        var rayAgentRight = new Ray(agent.transform.position, (agent.transform.forward + agent.transform.right * agent.radius).normalized);
        var rayAgentLeft = new Ray(agent.transform.position, (agent.transform.forward - agent.transform.right * agent.radius).normalized);
        Debug.DrawRay(rayAgentRight.origin, rayAgentRight.direction * rayDistance, Color.red);
        Debug.DrawRay(rayAgentLeft.origin, rayAgentLeft.direction * rayDistance, Color.blue);
        
        foreach (var obstacle in obstaclesPos)
        {
            var agentTransform = obstacle.Agent.transform;
            var rayObstacleRightInside = new Ray(agentTransform.position + agentTransform.forward * 0.25f, (agentTransform.forward + agentTransform.right * agent.radius).normalized);
            var rayObstacleRightOutside = new Ray(agentTransform.position, (agentTransform.forward + agentTransform.right * agent.radius).normalized);
            var rayObstacleLeftInside = new Ray(agentTransform.position + agentTransform.forward * 0.25f, (agentTransform.forward - agentTransform.right * agent.radius).normalized);
            var rayObstacleLeftOutside = new Ray(agentTransform.position, (agentTransform.forward - agentTransform.right * agent.radius).normalized);
            Debug.DrawRay(rayObstacleRightInside.origin, rayObstacleRightInside.direction * rayDistance, Color.red);
            Debug.DrawRay(rayObstacleLeftInside.origin, rayObstacleLeftInside.direction * rayDistance, Color.red);
            Debug.DrawRay(rayObstacleRightOutside.origin, rayObstacleRightOutside.direction * rayDistance, Color.blue);
            Debug.DrawRay(rayObstacleLeftOutside.origin, rayObstacleLeftOutside.direction * rayDistance, Color.blue);

            constraints.Add(new FVOConstraint
            {
                RightRayInside = rayObstacleRightInside,
                RightRayOutside = rayObstacleRightOutside,
                LeftRayInside = rayObstacleLeftInside,
                LeftRayOutside = rayObstacleLeftOutside
            });
        }

        return constraints;
    }
}
