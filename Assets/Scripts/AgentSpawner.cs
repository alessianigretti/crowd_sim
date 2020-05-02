using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public int amount = 30;
    public float spawnRange = 30f;

    private NavMeshAgent mainAgent;
    private List<NavMeshAgent> agents = new List<NavMeshAgent>();
    private RandomMovement randomMovement;
    private KDTreeBuilder kdTree;
    
    void Start()
    {
        kdTree = new KDTreeBuilder();
        randomMovement = new RandomMovement();

        mainAgent = Instantiate(agentPrefab, randomMovement.PickRandomDestination(Vector3.zero, spawnRange),
            Quaternion.identity).GetComponent<NavMeshAgent>();
        
        for (int i = 0; i < amount - 1; i++)
        {
            var agent = Instantiate(agentPrefab, randomMovement.PickRandomDestination(Vector3.zero, spawnRange), Quaternion.identity);
            agents.Add(agent.GetComponent<NavMeshAgent>());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("Nearest Neighbour");
            var tree = kdTree.BuildKDTree(GetPoints());
            var pointPosition = mainAgent.transform.position;
            var nearestPoint = kdTree.NearestNeighbour(tree, new Point(pointPosition.x, pointPosition.z));
            Debug.Log("Agent position: x " + pointPosition.x + " y " + pointPosition.z);
            Debug.Log("Nearest neighbour: x " + nearestPoint.x + " y " + nearestPoint.y);
        }
    }

    private List<Point> GetPoints()
    {
        List<Point> points = new List<Point>();
        
        foreach (var agent in agents)
        {
            var position = agent.transform.position;
            points.Add(new Point(position.x, position.z));
        }

        return points;
    }
}