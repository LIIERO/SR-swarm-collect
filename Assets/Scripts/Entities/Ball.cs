using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour, ICollectable
{
    [SerializeField] private float carryHeight = 0.25f;
    private bool reserved;
    private bool collected;
    private IDetectable carryingAgent;

    public Vector3 GetPosition() { return transform.position; }
    public string GetTag() { return tag; }
    public void Reserve() { reserved = true; }
    public bool IsReserved() { return reserved; }
    public void Collect(IDetectable collectedBy)
    { 
        collected = true;
        carryingAgent = collectedBy;
    }
    public bool IsCollected() { return collected; }
    public void Dispose() { Destroy(gameObject); }

    private void Start()
    {
        reserved = false;
        collected = false;
    }

    private void Update()
    {
        if (collected)
        {
            transform.position = carryingAgent.GetPosition() + (Vector3.up * carryHeight);
        }
    }
}
