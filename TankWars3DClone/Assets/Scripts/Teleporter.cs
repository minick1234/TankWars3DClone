using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private GameManager gm;

    private void OnTriggerEnter(Collider other)
    {
        print(other);
        if (other.gameObject.layer == 7)
        {
            other.gameObject.transform.position = gm.GenerateNewPosition();

        }
        else
        {
            other.gameObject.transform.parent.position = gm.GenerateNewPosition();
        }
        
    }
}