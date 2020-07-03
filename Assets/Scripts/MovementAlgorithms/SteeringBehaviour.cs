using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SteeringBehaviour
{
    private float rayDistance = 10f;
    public void ComputeFVOConstraints(NavMeshAgent agent, List<KDTreeBuilder.Neighbour> obstaclesPos, float agentsRadius)
    {
        Debug.DrawRay(agent.transform.position, (agent.transform.forward + agent.transform.right * agentsRadius).normalized * rayDistance, Color.red);
        Debug.DrawRay(agent.transform.position, (agent.transform.forward - agent.transform.right * agentsRadius).normalized * rayDistance, Color.blue);
        
        foreach (var obstacle in obstaclesPos)
        {
            var agentTransform = obstacle.Agent.transform;
            Debug.DrawRay(agentTransform.position, (agentTransform.forward + agentTransform.right * agentsRadius).normalized * rayDistance, Color.red);
            Debug.DrawRay(agentTransform.position, (agentTransform.forward - agentTransform.right * agentsRadius).normalized * rayDistance, Color.blue);
        }
    }
}
