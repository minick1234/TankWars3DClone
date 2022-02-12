using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

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