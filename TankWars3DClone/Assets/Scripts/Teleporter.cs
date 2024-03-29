using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private GameManager gm;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 7)
        {
            other.gameObject.transform.position = gm.GenerateNewPosition();
        }
        else if (other.gameObject.layer == 8 || other.gameObject.layer == 10)
        {
            other.gameObject.transform.root.position = gm.GenerateNewPosition();

            print("Teleporter Activiated.");
        }
    }
}