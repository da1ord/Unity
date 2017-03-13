using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour 
{
	public PlayerHealth playerHealth_;
	public GameObject enemy;
	public float spawnTime_ = 10.0f;
	public bool enableSpawning_ = true;
	public Transform[] spawnPoints_;

	void Start() 
	{
		InvokeRepeating( "Spawn", 1.0f, spawnTime_ );
	}
	
	void Spawn() 
	{
		if( enableSpawning_ && playerHealth_.health_ > 0 )
		{
			int spawnPointId = Random.Range( 0, spawnPoints_.Length );

			Instantiate( enemy, spawnPoints_[spawnPointId].position, spawnPoints_[spawnPointId].rotation );
		}
	}
}
