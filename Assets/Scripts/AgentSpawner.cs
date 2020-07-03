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
    private SteeringBehaviour steeringBehaviour;
    private KDTreeBuilder kdTree;
    
    void Start()
    {
        kdTree = new KDTreeBuilder();
        randomMovement = new RandomMovement();
        steeringBehaviour = new SteeringBehaviour();
        mainAgent = GameObject.Find("Agent").GetComponent<NavMeshAgent>();
        
        for (int i = 0; i < amount; i++)
        {
            var agent = Instantiate(agentPrefab, randomMovement.PickRandomDestination(Vector3.zero, spawnRange), Quaternion.identity);
            agents.Add(agent.GetComponent<NavMeshAgent>());
        }
    }

    void Update()
    {
        var tree = kdTree.BuildKDTree(agents);//GetPoints());
        var nearestNeighbours = kdTree.NearestNeighbours(nNearest, tree, mainAgent);//new Point(mainAgent.transform.position.x, mainAgent.transform.position.z));
        foreach (var point in nearestNeighbours)
        {
            var position = new Vector3(point.Agent.transform.position.x, 0, point.Agent.transform.position.y);
            if (Input.GetKeyDown(KeyCode.N))
            {
                Instantiate(highlightPrefab, position, highlightPrefab.transform.rotation);
            }
        }
        var constraints = steeringBehaviour.ComputeFVOConstraints(mainAgent, nearestNeighbours);
    }

    // private List<Point> GetPoints()
    // {
    //     List<Point> points = new List<Point>();
    //     
    //     foreach (var agent in agents)
    //     {
    //         var position = agent.transform.position;
    //         points.Add(new Point(position.x, position.z));
    //     }
    //
    //     return points;
    // }
}