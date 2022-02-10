using System;
using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;


    [SerializeField] private GameObject Player;
    [SerializeField] private List<GameObject> EnemiesLeft;
    [SerializeField] private List<GameObject> SpawnPoints;

    [SerializeField] private KeyCode restartKey;


    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameManager();
            }

            return _instance;
        }
    }


    // Start is called before the first frame update
    private void Start()
    {
        SpawnPlayer();
        SpawnEnemies();
    }

    //Manage all the spawnpoints in the level here for the enemies and player.
    //A good way to do it is by just making set spawn points, getting all the points and spawninig the player or enemies on the points randomly, with the same rotation as the spawnpoints z axis.


    // Update is called once per frame
    private void Update()
    {
        RestartLevel();
        EndGame();
    }

    //Implement a simple pause when they press escape on the keyboard.


    private void RestartLevel()
    {
        if (Input.GetKey(restartKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().ToString());
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
    }


    //spawn the enemies at the spawn points and then add them to the enemies list in order to keep track of the amount of enemies on the scene.
    private void SpawnEnemies()
    {
    }
}