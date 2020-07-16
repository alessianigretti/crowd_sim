using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject highlightPrefab;
    public int amount = 30;
    public float spawnRange = 30f;
    public int nNearest = 5;

    private NavMeshAgent mainAgent;
    private List<NavMeshAgent> agents = new List<NavMeshAgent>();
    private RandomMovement randomMovement;
    private KDTreeBuilder kdTree;
    
    void OnEnable()
    {
        kdTree = new KDTreeBuilder();
        randomMovement = new RandomMovement();
        mainAgent = GameObject.Find("Agent").GetComponent<NavMeshAgent>();
        
        for (int i = 0; i < amount; i++)
        {
            var agent = Instantiate(agentPrefab, randomMovement.PickRandomDestination(Vector3.zero, spawnRange), Quaternion.identity);
            agents.Add(agent.GetComponent<NavMeshAgent>());
        }
    }

    void Update()
    {
        var tree = kdTree.BuildKDTree(agents);
        //var nearestNeighbours = kdTree.NearestNeighbours(nNearest, tree, mainAgent);
        List<KDTreeBuilder.Neighbour> nearestNeighbours = new List<KDTreeBuilder.Neighbour>();
        foreach (var agent in agents)
        {
            nearestNeighbours.Add(new KDTreeBuilder.Neighbour
            {
                Agent = agent
            });
        }
        //var constraints = steeringBehaviour.ComputeFVOConstraints(nearestNeighbours);
    }
}