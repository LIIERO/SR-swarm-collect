using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationManager : MonoBehaviour
{
    private float updateInterval = 0.05f; // Czas w sekundach miêdzy aktualizacjami

    private void Start()
    {
        StartCoroutine(UpdateGraphs());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // Reload current scene
        {
            Agent.nextId = 0; // reset id, temporary
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private IEnumerator UpdateGraphs()
    {
        while (true)
        {
            GameObject[] Obstacle = GameObject.FindGameObjectsWithTag("Agent");

            foreach (GameObject obstacle in Obstacle)
            {
                AstarPath.active.UpdateGraphs(obstacle.GetComponent<Collider>().bounds);
            }

            GameObject[] ObstacleBall = GameObject.FindGameObjectsWithTag("Ball");

            foreach (GameObject obstacle in ObstacleBall)
            {
                AstarPath.active.UpdateGraphs(obstacle.GetComponent<Collider>().bounds);
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }
}
