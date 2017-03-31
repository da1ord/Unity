using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour 
{
    // Player's health script
	public PlayerHealth playerHealth_;
    // Enemy gameobject
	public GameObject enemy;
    // Enemy spawn time
	public float spawnTime_ = 10.0f;
    // Flag indicating if the enemies will be spawning
	public bool enableSpawning_ = true;
    // Spawn point locations
	public Transform[] spawnPoints_;

    // Init function
	void Start() 
	{
        // Repeat invoking of spawning function
        InvokeRepeating( "Spawn", 1.0f, spawnTime_ );
	}

    // Enemy spawn function
    void Spawn() 
	{
        // Check if spawning is enabled and player is alive
		if( enableSpawning_ && playerHealth_.health_ > 0 )
		{
            // Get random spawn point
			int spawnPointId = Random.Range( 0, spawnPoints_.Length );
            // Instantiate enamy at the spawn point
			Instantiate( enemy, spawnPoints_[spawnPointId].position, spawnPoints_[spawnPointId].rotation );
		}
	}
}
