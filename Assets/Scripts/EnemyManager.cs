using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour 
{
    // Player's health script
	public PlayerHealth playerHealth_;
    // Enemy gameobject
	public GameObject enemy;
    // Flag indicating if the enemies will be spawning
	public bool enableSpawning_ = true;
    // Spawn point locations
    //public Transform[] spawnPoints_;
    // Spawn point locations
    public GameObject[] spawnPoints2_;

    // Enemy spawn time
    float spawnTime_ = 20.0f;
    // Enemy count variable
    int enemyCount_ = 0;

    // Spawn timer value
    float spawnTimeValue_;
    // Spawn time text componnet
    Text enemySpawnTimeText_;
    // Enemy count text componnet
    Text enemyCountText_;

    // Init function
    void Start()
    {
        // Reset the spawn timer
        spawnTimeValue_ = spawnTime_;

        // Get spawn time text component
        enemySpawnTimeText_ = GameObject.Find( "SpawnTimeText" ).GetComponent<Text>();
        // Get enemy count text component
        enemyCountText_ = GameObject.Find( "RemainingEnemiesText" ).GetComponent<Text>();

        // Set enemy count text
        enemyCountText_.text = "Enemies remaining: " + enemyCount_.ToString();

        Spawn( 0 );
        Spawn( 1 );
        Spawn( 2 );
    }
    
    // Update function
    void Update()
    {
        // Decrease the spawn timer if it is greater than 0
        if( spawnTimeValue_ > 0.0f )
        {
            spawnTimeValue_ -= Time.deltaTime;
        }

        // Spawn time has elapsed - spawn the enemy
        if( spawnTimeValue_ <= 0.0f && enemyCount_ < 20 )
        {
            // Spawn the enemy at random spawn point
            Spawn( -1 );

            // Decrease spawn time if it is greater than 10
            if( spawnTime_ > 10.0f )
            {
                spawnTime_ -= 1.0f;
            }
            // Reset the spawn timer value
            spawnTimeValue_ = spawnTime_;
        }

        // Update the spawn time text
        enemySpawnTimeText_.text = "Next enemy spawns in " + spawnTimeValue_.ToString("F0");
    }

    // Spawn the enemy
    void Spawn( int id )
    {
        // Increase enemy count
        enemyCount_ += 1;
        // Update enemy count text
        enemyCountText_.text = "Enemies remaining: " + enemyCount_.ToString();

        // Get random spawn point
        int spawnPointId = Random.Range( 0, spawnPoints2_.Length );

        if( id >= 0 )
        {
            spawnPointId = id;
        }
        // Get waypoints array
        Transform[] waypoints = spawnPoints2_[spawnPointId].GetComponentsInChildren<Transform>();

        // Instantiate enemy at the spawn point
        GameObject instance = (GameObject)Instantiate( enemy, waypoints[0].position, waypoints[0].rotation );
        // Get enemy controller script
        EnemyController ec = instance.GetComponent<EnemyController>();
        Debug.Log( waypoints.Length );
        // Set enemy's path
        ec.SetPath( waypoints );
    }

    // Get enemy count
    public int GetEnemyCount()
    {
        return enemyCount_;
    }

    // Enemy was killed
    public void EnemyKilled()
    {
        // Decrease the enemy count
        enemyCount_ -= 1;
    }
}
