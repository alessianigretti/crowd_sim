using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public int amount = 30;
    public float spawnRange = 30f;
    
    void Start()
    {
        for (int i = 0; i < amount; i++)
        {
            Instantiate(agentPrefab, Helpers.PickRandomDestination(Vector3.zero, spawnRange), Quaternion.identity);
        }
    }
}