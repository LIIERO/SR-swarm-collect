using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public bool Reserved { get; set; }

    private void Start()
    {
        Reserved = false;
    }
}
