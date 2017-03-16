using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

	Transform player_;
	NavMeshAgent nav_;
	EnemyHealth enemyHealth_;
	EnemyShooting enemyShooting_;
//	PlayerController playerController_;
	PlayerControllerAnimated playerController_;
	PlayerHealth playerHealth_;

	// TODO: Redo to IFace
	// Player's active gun
	//	SciFiRifle activeGun_;
	M4Rifle activeGun_;
	Animator anim_;

	bool playerDetected_ = false;

	enum EnemyState { Patrolling, Distracted, Seeking, Following, Shooting };
	EnemyState enemyState_ = EnemyState.Patrolling;
	EnemyState lastEnemyState_ = EnemyState.Patrolling;
	LayerMask environmentMask_;

	public float sightDistance_ = 30.0f;
	public float fovYHalf_ = 30.0f;

	float walkSpeed_ = 1.5f;/*4.5f*/
	float fastWalkSpeed_ = 6.0f;/*6.0f*/
	float rushSpeed_ = 6.0f;/*7.5f*/

	// TODO: Get from gun
	float minShootRange_;
	float maxShootRange_;

	float seekingTimer_ = 5.0f;
	float playerDistance_;

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
//		playerController_ = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<PlayerController>();
		playerController_ = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<PlayerControllerAnimated>();
		playerHealth_ = player_.GetComponent<PlayerHealth>();

		activeGun_ = GetComponentInChildren<M4Rifle>();
		anim_ = GetComponent<Animator>();

		environmentMask_ = LayerMask.GetMask( "Environment" );

		minShootRange_ = activeGun_.GetRange() / 4.0f - 5.0f; // 10
		maxShootRange_ = activeGun_.GetRange() / 4.0f + 5.0f; // 20
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
		
			// Get direction to player
			Vector3 playerDirection = player_.position - transform.position;
			playerDistance_ = playerDirection.magnitude;

			nav_.enabled = true;
			playerDetected_ = false;

			// Player is in enemy sight
			if( playerDistance_ < sightDistance_ )
			{
				float angleToPlayer = Vector3.Angle( playerDirection, transform.forward );
				// Player in enemy sight cone
				if( angleToPlayer < fovYHalf_ )
				{
					// Player is seen by enemy (no environment in a way)
//					if( Physics.Raycast( transform.position, playerDirection, sightDistance_, LayerMask.GetMask( "Player" ) ) )
					if( !Physics.Raycast( transform.position, playerDirection, playerDistance_, environmentMask_ ) )
					{
						//Debug.Log( distractionPoint_ );
						playerDetected_ = true;
						enemyState_ = EnemyState.Following;

						distractionPoint_ = player_.transform.position;

						// TODO: LookAt player? no need to check for the next angle

						// Check if Player is in shooting range of enemy
						if( playerDistance_ < maxShootRange_ )
						{
//							enemyShooting_.Shoot();
							// Can shoot at this moment (shoot-enable timer elapsed) and clip is not empty
							if( activeGun_.CanShoot() )
							{
								activeGun_.Shoot();
								Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.green );
								// Hurt player
								playerHealth_.TakeDamage( 1 ); // activeGun_.GetDamagePerShot()
							}

							// Check if Player is too close to enemy
							if( playerDistance_ < minShootRange_ )
							{
//								nav_.enabled = false;
							}
						}
					}
				}
			}

			// Player is behind environment structure or
			// Player out of enemy sight cone or
			// Player out of enemy sight distance
			if( !playerDetected_ )
			{
				PlayerNotInSight();
			}

			if( enemyState_ < EnemyState.Following ) // Patrolling, Distracted, Seeking
			{
				// Enemy can hear a noise
				if( playerController_.GetNoiseLevel() > playerDistance_ )
				{
					// Set distracted state
					enemyState_ = EnemyState.Distracted;
					distractionPoint_ = player_.transform.position;
				}
			}

			// TODO: Redo somehow :)
			// Check if NavMeshAgent is enabled to avoid errors (calling of SetDestination and GetRemainingDistance 
			// on disabled NavMeshAgent)
			if( nav_.enabled == false )
			{
				lastEnemyState_ = enemyState_;
				return;
			}

//			Debug.Log( enemyState_ );
			switch( enemyState_ )
			{
				case EnemyState.Patrolling:
				{
					// Set walk speed
					nav_.speed = walkSpeed_;
					
					anim_.SetFloat( "Speed", walkSpeed_ );
					nav_.acceleration = walkSpeed_;
					
					// Go to actual point
					nav_.SetDestination( path_[destPointId_] );
					break;
				}
				case EnemyState.Distracted:
				{
					// Walk faster towards distraction point
					nav_.speed = walkSpeed_;
					
					anim_.SetFloat( "Speed", walkSpeed_ );
					nav_.acceleration = walkSpeed_;
					
					// Go to the distraction point
					nav_.SetDestination( distractionPoint_ );
					break;
				}
				case EnemyState.Following:
				{
					// Run towards player
					nav_.speed = rushSpeed_;
					
					anim_.SetFloat( "Speed", rushSpeed_ );
					nav_.acceleration = rushSpeed_;
					
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
						// Reset seek timer
						seekingTimer_ = 5.0f;
					}
					break;
				}
			}

			// TODO: Refactor!
			// Check destination remaining distance
			if( nav_.remainingDistance < 1.5f )
			{
				if( enemyState_ == EnemyState.Patrolling )
				{
					GoToNextPoint();
				}
				else if( enemyState_ == EnemyState.Distracted )
				{
					// Clear distraction point
					distractionPoint_ = NO_DISTRACTION_SET;

					// Set seeking state
					enemyState_ = EnemyState.Seeking;
				}
				// TODO: && Seeking?
				else if( seekingTimer_ == 5.0f )
				{
					// Set patrolling state
					enemyState_ = EnemyState.Patrolling;

					GoToActualPoint();
				}
//				if( ( enemyState_ == EnemyState.Patrolling || enemyState_ == EnemyState.Distracted ) )
//				{
//					// Distraction point has been set
//					if( distractionPoint_ != NO_DISTRACTION_SET )
//					{
//						// Clear distraction point
//						distractionPoint_ = NO_DISTRACTION_SET;
//					
//						// Set seeking state
//						enemyState_ = EnemyState.Seeking;
//
//						Debug.Log( enemyState_ );
//
//						// Go to actual point
//						//nav_.SetDestination( path_[destPointId_] );
//					}
//					// No distraction point set, go to next destination
//					else
//					{
//						GoToNextPoint();
//					}
//				}
//				else if( enemyState_ == EnemyState.Seeking && seekingTimer_ == 5.0f )
//				{
//					// Set patrolling state
//					enemyState_ = EnemyState.Patrolling;
//
//					GoToActualPoint();
//				}
			}

			lastEnemyState_ = enemyState_;
		}
		else
		{
			nav_.enabled = false;
		}
	}
		
	// Routine used when Player is not detected by enemy
	void PlayerNotInSight()
	{
		if( enemyState_ != EnemyState.Seeking )
		{
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
