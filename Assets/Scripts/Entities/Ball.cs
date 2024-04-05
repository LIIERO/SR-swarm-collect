using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField] private float carryHeight = 0.25f;
    public bool Reserved { get; set; }
    public bool Carried { get; private set; }
    private Transform carryingAgent;

    private void Start()
    {
        Reserved = false;
        Carried = false;
    }

    public void AttachToAgent(Transform agent)
    {
        Carried = true;
        carryingAgent = agent;
    }

    private void Update()
    {
        if (Carried)
        {
            transform.position = carryingAgent.position + (Vector3.up * carryHeight);
        }
    }
}
