using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGeneratorScript : MonoBehaviour
{
    public GameObject Ball;
    public int numberOfBalls = 10;
    public float planeHeight = 0.125f;
    public float ballRadius = 0.5f;
    public Vector2 outerSquareMin = new Vector2(-4.9f, -4.9f);
    public Vector2 outerSquareMax = new Vector2(4.9f, 4.9f);
    public Vector2 innerSquareMin = new Vector2(-1, -1);
    public Vector2 innerSquareMax = new Vector2(1, 1);

    private List<Vector3> spawnedPositions = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        SpawnBalls();
    }

    void SpawnBalls()
    {
        for (int i = 0; i < numberOfBalls; i++)
        {
            Vector3 randomPos = GetRandomPosition();
            Instantiate(Ball, randomPos, Quaternion.identity);
            spawnedPositions.Add(randomPos);
        }
    }

    Vector3 GetRandomPosition()
    {
        Vector3 randomPos = Vector3.zero;
        bool insideOuterSquare = false;
        bool insideInnerSquare = false;
        bool validPosition = false;

        while (!insideOuterSquare || insideInnerSquare || !validPosition)
        {
            randomPos = new Vector3(Random.Range(outerSquareMin.x, outerSquareMax.x), planeHeight, Random.Range(outerSquareMin.y, outerSquareMax.y));
            insideOuterSquare = randomPos.x >= outerSquareMin.x && randomPos.x <= outerSquareMax.x && randomPos.z >= outerSquareMin.y && randomPos.z <= outerSquareMax.y;
            insideInnerSquare = randomPos.x >= innerSquareMin.x && randomPos.x <= innerSquareMax.x && randomPos.z >= innerSquareMin.y && randomPos.z <= innerSquareMax.y;
            
            if (!IsCollidingWithExistingBalls(randomPos) && !IsInsideWall(randomPos) && !IsInsideAgent(randomPos))
            {
                validPosition = true;
            }
        }

        return randomPos;
    }
    bool IsCollidingWithExistingBalls(Vector3 position)
    {
        foreach (Vector3 existingPos in spawnedPositions)
        {
            if (Vector3.Distance(position, existingPos) < ballRadius * 2)
            {
                return true;
            }
        }
        return false;
    }
    bool IsInsideWall(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, ballRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Wall"))
            {
                return true;
            }
        }
        return false;
    }
    bool IsInsideAgent(Vector3 position)
    {
        Collider[] hitColliders = Physics.OverlapSphere(position, ballRadius);
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Agent"))
            {
                return true;
            }
        }
        return false;
    }
}


