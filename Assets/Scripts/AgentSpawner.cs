using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public int amount = 30;
    public float spawnRange = 30f;

    private RandomMovement randomMovement;
    
    void Start()
    {
        randomMovement = new RandomMovement();
        
        for (int i = 0; i < amount; i++)
        {
            Instantiate(agentPrefab, randomMovement.PickRandomDestination(Vector3.zero, spawnRange), Quaternion.identity);
        }
    }
}