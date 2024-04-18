using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Basket : MonoBehaviour, IVessel
{
    private int noFilled = 0;

    public Vector3 GetPosition() { return transform.position; }
    public string GetTag() { return tag; }

    public void Fill(ICollectable collectableToFill)
    {
        noFilled++;
        collectableToFill.Dispose(); // For now filling the basket just destroys the ball
        Debug.Log("Liczba zebranych: " + noFilled.ToString());
    }

    public int GetFilledCollectableCount()
    {
        return noFilled;
    }
}
