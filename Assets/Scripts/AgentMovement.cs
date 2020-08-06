using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sets random destinations for agents so they keep moving
/// </summary>
public class AgentMovement : MonoBehaviour
{
    public bool GoingLeft;
    
    private NavMeshAgent agent;
    private float additionalRemainingDistance = 0.001f;    // avoid characters getting stuck
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    
    void Update()
    {
        if (!agent.enabled)
        {
            return;
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance + additionalRemainingDistance)
        {
            //Vector3 destination = Utils.PickRandomPointInCircle(Vector3.zero, movementRange);

            Vector3 destination = GoingLeft ? new Vector3(-40, 0, 0) : new Vector3(40, 0, 0); 
            
            agent.SetDestination(destination);
        }
    }
}