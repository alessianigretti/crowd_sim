using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sets random destinations for agents so they keep moving
/// </summary>
public class AgentMovement : MonoBehaviour
{
    public bool GoingLeft;
    public float movementRange = 10f;
    
    private NavMeshAgent agent;
    private AgentSpawner spawner;
    private float additionalRemainingDistance = 0.001f;    // avoid characters getting stuck
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        spawner = GameObject.Find("Manager").GetComponent<AgentSpawner>();
    }
    
    void Update()
    {
        if (!agent.enabled)
        {
            return;
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance + additionalRemainingDistance)
        {
            Vector3 destination = agent.destination;
            
            if (spawner.crowdSimType == AgentSpawner.CrowdSimType.RandomCrowd)
            {
                destination = Utils.PickRandomPointInCircle(Vector3.zero, movementRange);
            }
            else if (spawner.crowdSimType == AgentSpawner.CrowdSimType.EmergentLanes)
            {
                destination = GoingLeft ? new Vector3(-40, 0, 0) : new Vector3(40, 0, 0); 
            }

            agent.SetDestination(destination);
        }
    }
}