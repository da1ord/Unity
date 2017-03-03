using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

	Transform player_;
	NavMeshAgent nav_;
	EnemyHealth enemyHealth_;
	EnemyShooting enemyShooting_;
	bool playerDetected_ = false;

	public float sightDistance_ = 20.0f;

	Vector3[] path_ = new []{ new Vector3( -20.0f, 1.0f, 20.0f ), 
							  new Vector3( -20.0f, 1.0f, -20.0f ), 
							  new Vector3( 29.0f, 1.0f, -29.0f ),
							  new Vector3( 20.0f, 1.0f, 20.0f )};

	int destPointId_ = 3;
	int nextDestPointId_ = 0;

	public Vector3 distractionPoint_ = new Vector3( 1000.0f, 1000.0f, 1000.0f );

	void Awake()  // Start
	{
		player_ = GameObject.FindGameObjectWithTag( "Player" ).transform;
		nav_ = GetComponent<NavMeshAgent>();
		enemyHealth_ = GetComponent<EnemyHealth>();
		enemyShooting_ = gameObject.GetComponentInChildren<EnemyShooting>();

		GoToNextPoint();
	}

	void Update()
	{
		// Enemy is alive
		if( enemyHealth_.health_ > 0 )
		{
			// Calculate and draw the sight boundaries
			Vector3 left = Quaternion.Euler( 0, -30, 0 ) * transform.forward;
			Vector3 right = Quaternion.Euler( 0, 30, 0 ) * transform.forward;
			Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.red );
			Debug.DrawRay( transform.position, left * sightDistance_, Color.blue );
			Debug.DrawRay( transform.position, right * sightDistance_, Color.blue );


			// Get direction to player
			Vector3 playerDirection = player_.position - transform.position;
			// If player is in the cone around the calculated direction, player is in sight
			if( Vector3.Angle( playerDirection, transform.forward ) < 30.0f )
			{
				// If player is visible, he becomes detected
				if( Physics.Raycast( transform.position, playerDirection, sightDistance_, LayerMask.GetMask( "Player" ) ) )
				{
					//Debug.Log( "Hit" );
					playerDetected_ = true;
					nav_.SetDestination( player_.position );

					Debug.DrawRay( transform.position, transform.up * 5.0f, Color.yellow );

					// Shooting
					if( Vector3.Angle( playerDirection, transform.forward ) < 10.0f )
					{
						Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.green );
						enemyShooting_.Shoot();
					}

					// Run towards player
					nav_.speed = 9.0f;
				}
				else
				{
					//Debug.Log( "Miss" );
					playerDetected_ = false;
				}
			} 
			// Player is not in sight
			else
			{
				// Just lost player from sight
				if( playerDetected_ == true )
				{
					playerDetected_ = false;

					// Go to actual point
					nav_.SetDestination( path_[destPointId_] );
				}
				// Player has not been detected
				else
				{
					// Check if a distraction point was set
					if( distractionPoint_ != new Vector3( 1000.0f, 1000.0f, 1000.0f ) )
					{
						// Walk faster towards distraction point
						nav_.speed = 7.0f;

						// Go to the distraction point
						nav_.SetDestination( distractionPoint_ );
					}
					// No distraction point is set
					else
					{
						// Set walk speed
						nav_.speed = 4.5f;
					}
				}
			}

			//Debug.Log( "Dest: " + destPointId_ );
			//Debug.Log( "Next dest: " + nextDestPointId_ );

			// Check destination remaining distance 
			if( nav_.remainingDistance < 1.5f )
			{
				// If close to destination and not following player, set destination point
				if( !playerDetected_ )
				{
					// Distraction point has been set
					if( distractionPoint_ != new Vector3( 1000.0f, 1000.0f, 1000.0f ) )
					{
						// Clear distraction point
						distractionPoint_ = new Vector3( 1000.0f, 1000.0f, 1000.0f );

						// Go to actual point
						nav_.SetDestination( path_[destPointId_] );
					}
					// No distraction point set, go to next destination
					else
					{
						GoToNextPoint();
					}
				}
			}
		} 
		else
		{
			nav_.enabled = false;
		}
	}

	void GoToNextPoint()
	{
		if( path_.Length == 0 )
		{
			return;
		}

		nav_.SetDestination( path_[nextDestPointId_] );
		nextDestPointId_ = ( nextDestPointId_ + 1 ) % path_.Length;
		destPointId_ = ( destPointId_ + 1 ) % path_.Length;
	}
}
