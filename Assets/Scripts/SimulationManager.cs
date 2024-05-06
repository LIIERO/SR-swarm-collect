using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationManager : MonoBehaviour
{
    public List<Agent> agents; // Lista agent�w w symulacji
    private float updateInterval = 0.05f; // Czas w sekundach mi�dzy aktualizacjami
    private bool acquisitionActive;
    private bool acquisitionFinished;

    private void Awake()
    {
        acquisitionActive = false;
        acquisitionFinished = false;
    }

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

        ToggleAcquisition();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) // Reload current scene
        {
            Agent.nextId = 0; // reset id
            Ball.Count = 0; // reset ball count
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Wstrzymaj lub wznow akwizycj�
        {
            ToggleAcquisition();
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

    private void ToggleAcquisition()
    {
        if (acquisitionFinished) return;

        if (acquisitionActive)
        {
            EventManager.InvokeAcquisitionPauseEvent();
        }
        else
        {
            EventManager.InvokeAcquisitionStartEvent();
        }
    }


    private void PauseAcquisition() { acquisitionActive = false; }
    private void StartAcquisition() { acquisitionActive = true; }
    private void EndAcquisition() { acquisitionFinished = true; }

    private void OnEnable()
    {
        EventManager.AcquisitionPauseEvent += PauseAcquisition;
        EventManager.AcquisitionStartEvent += StartAcquisition;
        EventManager.AcquisitionEndEvent += EndAcquisition;
    }

    private void OnDisable()
    {
        EventManager.AcquisitionPauseEvent -= PauseAcquisition;
        EventManager.AcquisitionStartEvent -= StartAcquisition;
        EventManager.AcquisitionEndEvent -= EndAcquisition;
    }
}
