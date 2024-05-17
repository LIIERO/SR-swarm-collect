using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentGeneratorScript : MonoBehaviour
{
    public GameObject Agent;
    public int numberOfAgents = 10;
    public float startX = 4.5f;
    public float startY = 0.125f;
    public float startZ = 3.5f;
    public float stepSize = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        SpawnAgents();
    }

    void SpawnAgents()
    {
        float currentZ = startZ;

        for (int i = 0; i < numberOfAgents; i++)
        {
            Vector3 spawnPos = new Vector3(startX, startY, currentZ);

            Instantiate(Agent, spawnPos, Quaternion.identity);

            currentZ -= stepSize;
        }
    }
}
