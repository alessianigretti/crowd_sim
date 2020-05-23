using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviour
{
    public void ComputeFVOConstraints(Vector3 agentPos, List<Vector3> obstaclesPos, float agentsRadius)
    {
        foreach (var obstacle in obstaclesPos)
        {
            var targetDirRight = obstacle - agentPos + new Vector3(agentsRadius, 0, agentsRadius);
            Physics.Raycast(agentPos, targetDirRight, out var hit1);
            Debug.DrawRay(agentPos, targetDirRight * hit1.distance, Color.red);
            
            var targetDirLeft = obstacle - agentPos - new Vector3(agentsRadius, 0, agentsRadius);
            Physics.Raycast(agentPos, targetDirLeft, out var hit2);
            Debug.DrawRay(agentPos, targetDirLeft * hit1.distance, Color.red);
        }
    }
}
