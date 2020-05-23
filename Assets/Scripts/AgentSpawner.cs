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
        var tree = kdTree.BuildKDTree(GetPoints());
        var pointPosition = mainAgent.transform.position;
        var nearestNeighbours = kdTree.NearestNeighbours(nNearest, tree, new Point(pointPosition.x, pointPosition.z));
        List<Vector3> nearestPositions = new List<Vector3>();
        foreach (var point in nearestNeighbours)
        {
            var position = new Vector3(point.Point.x, 0, point.Point.y);
            nearestPositions.Add(position);
            if (Input.GetKeyDown(KeyCode.N))
            {
                Instantiate(highlightPrefab, position, highlightPrefab.transform.rotation);
            }
        }
        steeringBehaviour.ComputeFVOConstraints(pointPosition, nearestPositions, mainAgent.radius);
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