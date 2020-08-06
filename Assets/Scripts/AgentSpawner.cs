﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Initialises scene and begins navigation
/// </summary>
public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject agentGoingLeftPrefab;
    public GameObject agentGoingRightPrefab;
    public int amount = 30;
    public float spawnRange = 30f;
    public int nNearest = 5;

    private List<NavMeshAgent> agents = new List<NavMeshAgent>();
    
    void OnEnable()
    {
        for (int i = 0; i < amount / 2; i++)
        {
            // var agent = Instantiate(agentPrefab, Utils.PickRandomPointInCircle(Vector3.zero, spawnRange), Quaternion.identity);
            // agents.Add(agent.GetComponent<NavMeshAgent>());

            var agent = Instantiate(agentGoingLeftPrefab, Utils.PickRandomPointInCircle(new Vector3(40, 0, 0), spawnRange), Quaternion.identity);
            agents.Add(agentGoingLeftPrefab.GetComponent<NavMeshAgent>());
        }
        
        for (int i = 0; i < amount / 2; i++)
        {
            var agent = Instantiate(agentGoingRightPrefab, Utils.PickRandomPointInCircle(new Vector3(-40, 0, 0), spawnRange), Quaternion.identity);
            agents.Add(agentGoingRightPrefab.GetComponent<NavMeshAgent>());
        }
    }

    void Update()
    {
        // Build KD-Tree and use it to identify N nearest neighbours
        var tree = KDTreeBuilder.BuildKDTree(agents);

        foreach (var agent in agents)
        {
            var nearestNeighbours = NearestNeighbour.Compute(nNearest, tree, agent);
            
            // Compute VO position depending on average of velocities of colliding agents
            Vector3 resultingVelocityObstaclePosition = agent.velocity;
            for (int i = 0; i < nearestNeighbours.Count; i++)
            {
                resultingVelocityObstaclePosition += nearestNeighbours[i].Agent.velocity;
            }
            resultingVelocityObstaclePosition /= nearestNeighbours.Count + 1;
            
            agent.transform.GetChild(0).transform.localPosition = resultingVelocityObstaclePosition;
            
            for (int i = 0; i < nearestNeighbours.Count; i++)
            {
                nearestNeighbours[i].Agent.gameObject.transform.GetChild(0).transform.localPosition = resultingVelocityObstaclePosition;
            }
        }
    }
}