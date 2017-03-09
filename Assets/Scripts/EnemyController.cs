using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

	Transform player_;
	NavMeshAgent nav_;
	EnemyHealth enemyHealth_;
	EnemyShooting enemyShooting_;
	PlayerController playerController_;
	bool playerDetected_ = false;

	enum EnemyState { Patrolling, Distracted, Seeking, Following, Shooting };
	EnemyState enemyState_ = EnemyState.Patrolling;
	EnemyState lastEnemyState_ = EnemyState.Patrolling;

	public float sightDistance_ = 30.0f;
	public float fovYHalf_ = 30.0f;

	float walkSpeed_ = 4.5f;
	float fastWalkSpeed_ = 6.0f;
	float rushSpeed_ = 7.5f;

	// TODO: Get from gun
	float gunRange_ = 10.0f;
	float minShootRange_;
	float maxShootRange_;

	float seekingTimer_ = 5.0f;

	Vector3[] path_ = new []{ new Vector3( -20.0f, 1.0f, 20.0f ), 
							  new Vector3( -20.0f, 1.0f, -20.0f ), 
							  new Vector3( 29.0f, 1.0f, -29.0f ),
							  new Vector3( 20.0f, 1.0f, 20.0f )};

	int destPointId_ = 3;
	int nextDestPointId_ = 0;

	static Vector3 NO_DISTRACTION_SET = new Vector3( 1000.0f, 1000.0f, 1000.0f );
	Vector3 distractionPoint_;

	void Awake()  // Start
	{
		player_ = GameObject.FindGameObjectWithTag( "Player" ).transform;
		nav_ = GetComponent<NavMeshAgent>();
		enemyHealth_ = GetComponent<EnemyHealth>();
		enemyShooting_ = gameObject.GetComponentInChildren<EnemyShooting>();
		playerController_ = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<PlayerController>();

		minShootRange_ = gunRange_ - 2;
		maxShootRange_ = gunRange_ + 2;
		distractionPoint_ = NO_DISTRACTION_SET;

		GoToNextPoint();
//		Debug.Log( "Start" );
	}

	void Update()
	{
//		Debug.Log( enemyState_ );
//		Debug.Log( distractionPoint_ );

		// Enemy is alive
		if( enemyHealth_.health_ > 0 )
		{
			// Calculate and draw the sight boundaries
			Vector3 left = Quaternion.Euler( 0, -fovYHalf_, 0 ) * transform.forward;
			Vector3 right = Quaternion.Euler( 0, fovYHalf_, 0 ) * transform.forward;
			Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.red );
			Debug.DrawRay( transform.position, left * sightDistance_, Color.blue );
			Debug.DrawRay( transform.position, right * sightDistance_, Color.blue );

			/*
			 * check distance to player
			 * 	- check angle to player
			 * 		- follow
			 * 		- check fire distance
			 * 			- check angle
			 * 				- shoot
			 * distracted - follow point;
			 * patrolling - follow point;
			 * 	- if last state was distracted, look around for a while
			*/

			/**/
			// Get direction to player
			Vector3 playerDirection = player_.position - transform.position;
			float playerDistance = playerDirection.magnitude;

			nav_.enabled = true;
			playerDetected_ = false;

			// Player is in sight of enemy
			if( playerDistance < sightDistance_ )
			{
				float angleToPlayer = Vector3.Angle( playerDirection, transform.forward );
				if( angleToPlayer < fovYHalf_ )
				{
					// Player is seen by enemy (no environment in a way)
//					if( Physics.Raycast( transform.position, playerDirection, sightDistance_, LayerMask.GetMask( "Player" ) ) )
					if( !Physics.Raycast( transform.position, playerDirection, playerDistance, LayerMask.GetMask( "Environment" ) ) )
					{
						Debug.Log( distractionPoint_ );
						playerDetected_ = true;
						enemyState_ = EnemyState.Following;

						distractionPoint_ = player_.transform.position;

						// TODO: LookAt player? no need to check for the next angle

						// Check if Player is in shooting range of enemy
						if( playerDistance < maxShootRange_ )
						{
							enemyShooting_.Shoot();
							Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.green );

							// Check if Player is too close to enemy
							if( playerDistance < minShootRange_ )
							{
								nav_.enabled = false;
							}
						}
					}
					// Enemy just lost the Player
					else if( lastEnemyState_ > EnemyState.Seeking ) // Following, Shooting
					{
						Debug.Log( "Player lost" );
						//enemyState_ = EnemyState.Seeking;
						/* Routine */
						if( distractionPoint_ == NO_DISTRACTION_SET )
						{
							enemyState_ = EnemyState.Seeking;
						}
						else
						{
							enemyState_ = EnemyState.Distracted;
						}
					}
					// Player is not seen by enemy
					else
					{
						if( enemyState_ == EnemyState.Seeking )
						{
						}
						else
						{
							/* Routine */
							if( distractionPoint_ == NO_DISTRACTION_SET )
							{
								enemyState_ = EnemyState.Patrolling;
							}
							else
							{
								enemyState_ = EnemyState.Distracted;
							}
						}
					}
				}
				else
				{
					if( enemyState_ == EnemyState.Seeking )
					{
					}
					else
					{/* Routine */
						if( distractionPoint_ == NO_DISTRACTION_SET )
						{
							enemyState_ = EnemyState.Patrolling;
						}
						else
						{
							enemyState_ = EnemyState.Distracted;
						}
					}
				}
			}
			else
			{
				if( enemyState_ == EnemyState.Seeking )
				{
				}
				else
				{
					/* Routine */
					if( distractionPoint_ == NO_DISTRACTION_SET )
					{
						enemyState_ = EnemyState.Patrolling;
					}
					else
					{
						enemyState_ = EnemyState.Distracted;
					}
				}
			}

			if( enemyState_ < EnemyState.Following ) // Patrolling, Distracted, Seeking
			{
				if( playerController_.GetNoiseLevel() > playerDistance )
				{
					enemyState_ = EnemyState.Distracted;
					if( distractionPoint_ == NO_DISTRACTION_SET )
					{
						distractionPoint_ = player_.transform.position;
					}
				}
			}

//			Debug.Log( enemyState_ );
			switch( enemyState_ )
			{
				case EnemyState.Patrolling:
				{
//					Debug.Log( "Patrolling" );
					// Set walk speed
					nav_.speed = walkSpeed_;

					// Go to actual point
					nav_.SetDestination( path_[destPointId_] );
					break;
				}
				case EnemyState.Distracted:
				{
//					Debug.Log( "Distracted" );
					// Walk faster towards distraction point
					nav_.speed = fastWalkSpeed_;

					// Go to the distraction point
					nav_.SetDestination( distractionPoint_ );
					break;
				}
				case EnemyState.Following:
				{
					// Run towards player
					nav_.speed = rushSpeed_;

					// Follow the player
					nav_.SetDestination( player_.position );
					Debug.DrawRay( transform.position, transform.up * 5.0f, Color.yellow );
					break;
				}
				case EnemyState.Seeking:
				{
					// Seeking in progress
					if( seekingTimer_ > 0.0f )
					{
						seekingTimer_ -= Time.deltaTime;
						// Seek for Player - rotate around
						//float randomDir = Random.Range( -30.0f, 30.0f );
						transform.Rotate( transform.up, 2.0f );
					}
					// Seeking done
					else
					{
						seekingTimer_ = 5.0f;
						enemyState_ = EnemyState.Patrolling;

						GoToActualPoint();
						lastEnemyState_ = enemyState_;
						return;
					}

					// Set walk speed
//					nav_.speed = 20.5f;//4.5f
//					nav_.SetDestination( new Vector3( 25.0f, 1.0f, 25.0f ) );
					// Rotate for a while then move to patrolling state
					break;
				}
			}
			//Debug.Log( "Dest: " + destPointId_ );
			//Debug.Log( "Next dest: " + nextDestPointId_ );

			// TODO: Only patrolling state?
			// Check destination remaining distance
			if( nav_.remainingDistance < 1.5f && ( enemyState_ == EnemyState.Patrolling || enemyState_ == EnemyState.Distracted ) )
			{
				// Distraction point has been set
				if( distractionPoint_ != NO_DISTRACTION_SET )
				{
					// Clear distraction point
					distractionPoint_ = NO_DISTRACTION_SET;
					
					// Set seeking state
					enemyState_ = EnemyState.Seeking;

					Debug.Log( enemyState_ );

					// Go to actual point
					//nav_.SetDestination( path_[destPointId_] );
				}
				// No distraction point set, go to next destination
				else
				{
					GoToNextPoint();
				}
			}

			lastEnemyState_ = enemyState_;

//			// Get direction to player
//			Vector3 playerDirection = player_.position - transform.position;
//			// If player is in the cone around the calculated direction, player is in sight
//			if( Vector3.Angle( playerDirection, transform.forward ) < 30.0f )
//			{
//				// If player is visible, he becomes detected
//				if( Physics.Raycast( transform.position, playerDirection, sightDistance_, LayerMask.GetMask( "Player" ) ) )
//				{
//					//Debug.Log( "Hit" );
//					playerDetected_ = true;
//					nav_.SetDestination( player_.position );
//
//					Debug.DrawRay( transform.position, transform.up * 5.0f, Color.yellow );
//
//					// Shooting
//					if( Vector3.Angle( playerDirection, transform.forward ) < 10.0f )
//					{
//						Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.green );
//						enemyShooting_.Shoot();
//					}
//
//					// Run towards player
//					nav_.speed = 9.0f;
//				}
//				else
//				{
//					//Debug.Log( "Miss" );
//					playerDetected_ = false;
//				}
//			} 
//			// Player is not in sight
//			else
//			{
//				// Just lost player from sight
//				if( playerDetected_ == true )
//				{
//					playerDetected_ = false;
//
//					// Go to actual point
//					nav_.SetDestination( path_[destPointId_] );
//				}
//				// Player has not been detected
//				else
//				{
//					// Check if a distraction point was set
//					if( distractionPoint_ != new Vector3( 1000.0f, 1000.0f, 1000.0f ) )
//					{
//						// Walk faster towards distraction point
//						nav_.speed = 7.0f;
//
//						// Go to the distraction point
//						nav_.SetDestination( distractionPoint_ );
//					}
//					// No distraction point is set
//					else
//					{
//						// Set walk speed
//						nav_.speed = 4.5f;
//					}
//				}
//			}
//
//			//Debug.Log( "Dest: " + destPointId_ );
//			//Debug.Log( "Next dest: " + nextDestPointId_ );
//
//			// Check destination remaining distance 
//			if( nav_.remainingDistance < 1.5f )
//			{
//				// If close to destination and not following player, set destination point
//				if( !playerDetected_ )
//				{
//					// Distraction point has been set
//					if( distractionPoint_ != new Vector3( 1000.0f, 1000.0f, 1000.0f ) )
//					{
//						// Clear distraction point
//						distractionPoint_ = new Vector3( 1000.0f, 1000.0f, 1000.0f );
//
//						// Go to actual point
//						nav_.SetDestination( path_[destPointId_] );
//					}
//					// No distraction point set, go to next destination
//					else
//					{
//						GoToNextPoint();
//					}
//				}
//			}
		}
		else
		{
			nav_.enabled = false;
		}
	}

	public void SetDistractionPoint( Vector3 distraction_point )
	{
		if( enemyState_ != EnemyState.Following )
		{
			enemyState_ = EnemyState.Distracted;
			distractionPoint_ = distraction_point;
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

	void GoToActualPoint()
	{
		if( path_.Length == 0 )
		{
			return;
		}

		nav_.SetDestination( path_[destPointId_] );
	}
}
