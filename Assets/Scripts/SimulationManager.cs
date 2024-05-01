using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationManager : MonoBehaviour
{
    public List<Agent> agents; // Lista agent�w w symulacji
    private float updateInterval = 0.05f; // Czas w sekundach mi�dzy aktualizacjami

    private void Start()
    {
        agents = new List<Agent>(); // Inicjalizacja listy agent�w

        // Znajd� wszystkie obiekty z tagiem "Agent" i dodaj je do listy agents
        GameObject[] agentObjects = GameObject.FindGameObjectsWithTag("Agent");
        foreach (GameObject agentObject in agentObjects)
        {
            Agent agentComponent = agentObject.GetComponent<Agent>();
            if (agentComponent != null)
            {
                agents.Add(agentComponent);
            }
        }
        StartCoroutine(UpdateGraphs());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // Reload current scene
        {
            Agent.nextId = 0; // reset id, temporary
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Wstrzymaj lub wznow akwizycj�
        {
            Debug.Log(agents);
            ToggleAgentsAcquisition();
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

    private void ToggleAgentsAcquisition()
    {
        foreach (Agent agent in agents)
        {
            agent.ToggleAcquisition(); // Wstrzymaj lub wznow akwizycj� dla ka�dego agenta
        }
    }
}
