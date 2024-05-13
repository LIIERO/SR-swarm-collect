using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGeneratorScript : MonoBehaviour
{
    public GameObject Ball;
    public int numberOfBalls = 10;
    public float spawnRadius = 10f;
    public float planeHeight = 0f;
    // Start is called before the first frame update
    void Start()
    {
        SpawnBalls();
    }

    void SpawnBalls()
    {
        for (int i = 0; i < numberOfBalls; i++)
        {
            Vector3 randomPos = Random.insideUnitSphere * spawnRadius;
            randomPos.y = planeHeight;

            Instantiate(Ball, randomPos, Quaternion.identity);
        }
    }
}

