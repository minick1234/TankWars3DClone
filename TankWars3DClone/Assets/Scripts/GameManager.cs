using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private int randomSpawnLaneIndex;

    [SerializeField] private float points = 0.00f;


    [SerializeField] private GameObject spawnZoneParent;
    [SerializeField] private GameObject Player;
    [SerializeField] private int enemyAmount;
    [SerializeField] private GameObject enemyTank;
    [SerializeField] private List<GameObject> EnemiesLeft;
    [SerializeField] private List<GameObject> SpawnLanes;

    [SerializeField] private KeyCode restartKey;

    // Start is called before the first frame update
    private void Start()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        if (SpawnLanes.Count <= 0)
        {
            PopulateSpawnZones();
        }

        if (SpawnLanes.Count >= 0)
        {
            SpawnPlayer();
            SpawnEnemies();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        RestartLevel();
        EndGame();
    }

    private void RestartLevel()
    {
        if (Input.GetKey(restartKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Debug.Log("The scene has been reloaded.");
        }
    }


    private void EndGame()
    {
        //This is where i would add some type of ui and make it show the game over and allow for the user to press the next level button or end game. Havent decided if i want to make more then one level.
        if (EnemiesLeft.Count <= 0)
        {
            Debug.Log("There is no more enemies left! Game Over!");
        }

        if (Player.gameObject != null)
        {
            Debug.Log("The player still exists.");
            if (Player.GetComponent<PlayerController>().getPlayerTankHealth() <= 0)
            {
                //this is where i would display the ui for the player tank being dead and to restart, probably the same ui for both but change text depending on the scenario.
                Debug.Log("Sorry but the player has died!");
            }
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawn = GenerateNewPosition();
        spawnObject(Player, spawn);
    }


    //spawn the enemies at the spawn points and then add them to the enemies list in order to keep track of the amount of enemies on the scene.
    private void SpawnEnemies()
    {
    }

    private bool CheckValidLocation(Vector3 positionOfObject)
    {
        return true;
    }

    public void KillEnemy(GameObject enemyToRemove)
    {
        if (EnemiesLeft.Count > 0 && enemyToRemove != null)
        {
            EnemiesLeft.Remove(enemyToRemove);
            points += 50f;
        }
    }

    public Vector3 GenerateNewPosition()
    {
        Vector3 Spawn;
        do
        {
            randomSpawnLaneIndex = Random.Range(0, SpawnLanes.Count - 1);

            if (SpawnLanes[randomSpawnLaneIndex].GetComponent<SpawnLane>().IsXLane)
            {
                float boundingBoxRange = Random.Range(
                    SpawnLanes[randomSpawnLaneIndex].GetComponent<BoxCollider>().bounds.min.x + 1f,
                    SpawnLanes[randomSpawnLaneIndex].GetComponent<BoxCollider>().bounds.max.x - 1f);

                Spawn = new Vector3(boundingBoxRange,
                    0.1f, SpawnLanes[randomSpawnLaneIndex].transform.position.z);
            }
            else
            {
                float boundingBoxRange = Random.Range(
                    SpawnLanes[randomSpawnLaneIndex].GetComponent<BoxCollider>().bounds.min.z + 1f,
                    SpawnLanes[randomSpawnLaneIndex].GetComponent<BoxCollider>().bounds.max.z - 1f);

                Spawn = new Vector3(SpawnLanes[randomSpawnLaneIndex].transform.position.x,
                    0.1f, boundingBoxRange);
            }
        } while (!CheckValidLocation(Spawn));

        return Spawn;
    }

    private void spawnObject(GameObject gameObjectToSpawn, Vector3 spawnPosition)
    {
        GameObject spawnedObject;
        int randomDirection = Random.Range(0, 2);
        print(randomDirection);
        spawnedObject = Instantiate(gameObjectToSpawn, spawnPosition, Quaternion.identity);

        if (!SpawnLanes[randomSpawnLaneIndex].GetComponent<SpawnLane>().IsXLane)
        {
            if (randomDirection == 0)
            {
                spawnedObject.GetComponent<PlayerController>().tankRenders.transform
                    .Rotate(0,
                        0,
                        0);
                spawnedObject.GetComponent<PlayerController>().turretObject.transform
                    .Rotate(0,
                        0,
                        0);
            }
            else if (randomDirection == 1)
            {
                spawnedObject.GetComponent<PlayerController>().tankRenders.transform
                    .Rotate(0,
                        180,
                        0);
                spawnedObject.GetComponent<PlayerController>().turretObject.transform
                    .Rotate(0,
                        180,
                        0);
            }
        }
        else
        {
            if (randomDirection == 0)
            {
                spawnedObject.GetComponent<PlayerController>().tankRenders.transform
                    .Rotate(0,
                        SpawnLanes[randomSpawnLaneIndex].transform.eulerAngles.y,
                        0);
                spawnedObject.GetComponent<PlayerController>().turretObject.transform
                    .Rotate(0,
                        SpawnLanes[randomSpawnLaneIndex].transform.eulerAngles.y,
                        0);
            }
            else if (randomDirection == 1)
            {
                spawnedObject.GetComponent<PlayerController>().tankRenders.transform
                    .Rotate(0,
                        -SpawnLanes[randomSpawnLaneIndex].transform.eulerAngles.y,
                        0);
                spawnedObject.GetComponent<PlayerController>().turretObject.transform
                    .Rotate(0,
                        -SpawnLanes[randomSpawnLaneIndex].transform.eulerAngles.y,
                        0);
            }
        }
    }

    private void PopulateSpawnZones()
    {
        foreach (var childSpawnZone in spawnZoneParent.GetComponentsInChildren<Transform>())
        {
            if (childSpawnZone == spawnZoneParent.transform)
            {
                continue;
            }

            SpawnLanes.Add(childSpawnZone.gameObject);
        }
    }
}