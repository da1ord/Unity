using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

	Transform player_;
	NavMeshAgent nav_;
	EnemyHealth enemyHealth_;
//	EnemyShooting enemyShooting_;
//	PlayerController playerController_;
	PlayerControllerAnimated playerController_;

	Vector3 enemyPosition_;
	Vector3 playerPosition_;
	CapsuleCollider capsule_;
    // TODO: Redo to IFace
    // Player's active gun
    SciFiRifle activeGun_;
    //M4Rifle activeGun_;
	Animator anim_;
	// Walk audio source
	AudioSource walkAudio_;
	// Speech audio source
	AudioSource speechAudio_;
	// Walk audio clip
	public AudioClip walkClip_;
	// Run audio clip
	public AudioClip runClip_;

    // Player detected audio clip
    public AudioClip playerDetectedClip_;
    // Player lost (seeking start) audio clip
    public AudioClip playerLostClip_;

    Ray shootRay_;
    RaycastHit shootHit_;
    Vector3 gunOffset_ = new Vector3( 0.0f, 0.5f, 0.0f );

    bool playerDetected_ = false;

	enum EnemyState { Patrolling, Distracted, Seeking, Following, Shooting };
	EnemyState enemyState_ = EnemyState.Patrolling;
	EnemyState lastEnemyState_ = EnemyState.Patrolling;
	LayerMask environmentMask_;

	public float sightDistance_ = 30.0f;
	public float fovYHalf_ = 30.0f;

	float walkSpeed_ = 1.5f;/*4.5f*/
	float rushSpeed_ = 6.0f;/*7.5f*/

	// TODO: Get from gun
	float minShootRange_;
	float maxShootRange_;

	float seekingTimer_ = 5.0f;
	float playerDistance_;
    bool isPlayerAlive_ = true;

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
//		enemyShooting_ = gameObject.GetComponentInChildren<EnemyShooting>();
//		playerController_ = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<PlayerController>();
		playerController_ = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<PlayerControllerAnimated>();

        shootRay_ = new Ray();

		activeGun_ = GetComponentInChildren<SciFiRifle>();
		anim_ = GetComponent<Animator>();
		walkAudio_ = GetComponents<AudioSource>()[0];
		speechAudio_ = GetComponents<AudioSource>()[1];

		environmentMask_ = LayerMask.GetMask( "Environment" );

		capsule_ = GetComponent<CapsuleCollider>();

		minShootRange_ = activeGun_.GetRange() / 4.0f - 5.0f; // 10
		maxShootRange_ = activeGun_.GetRange() / 4.0f + 5.0f; // 20
		distractionPoint_ = NO_DISTRACTION_SET;

		GoToNextPoint();
//		Debug.Log( "Start" );
	}

    void Update()
    {
        enemyPosition_ = capsule_.bounds.center + gunOffset_;
        playerPosition_ = playerController_.playerPosition_;
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
            //			Vector3 playerDirection = player_.position - enemyPosition_;
            Vector3 playerDirection = playerPosition_ - enemyPosition_;
            playerDistance_ = playerDirection.magnitude;

            walkAudio_.volume = Mathf.Min( 1.0f, 1.0f / playerDistance_ );
            speechAudio_.volume = Mathf.Min( 1.0f, 20.0f / playerDistance_ );

            nav_.enabled = true;
            playerDetected_ = false;

            // TODO: Refactor
            // Player is in enemy sight
            if( playerDistance_ < sightDistance_ && isPlayerAlive_ )
            {
                float angleToPlayer = Vector3.Angle( playerDirection, transform.forward );
                // Player in enemy sight cone
                if( angleToPlayer < fovYHalf_ )
                {
                    // Player is seen by enemy (no environment in a way)
                    //if( Physics.Raycast( transform.position, playerDirection, sightDistance_, LayerMask.GetMask( "Player" ) ) )
                    if( !Physics.Raycast( enemyPosition_, playerDirection, playerDistance_, environmentMask_ ) )
                    {
                        Debug.DrawLine( enemyPosition_, playerPosition_ );
                        //Debug.Log( distractionPoint_ );
                        playerDetected_ = true;
                        if( !speechAudio_.isPlaying )
                        {
                            //speechAudio_.clip = playerDetectedClip_;
                            //speechAudio_.Play();
                        }
                        enemyState_ = EnemyState.Following;

                        distractionPoint_ = playerPosition_;//player_.transform.position;

                        // TODO: LookAt player? no need to check for the next angle

                        // Check if Player is in shooting range of enemy
                        if( playerDistance_ < maxShootRange_ )
                        {
                            // Rotate to the player
                            Vector3 rotation = playerDirection.normalized;
                            Quaternion lookRotation = Quaternion.LookRotation( new Vector3( rotation.x, 0, rotation.z ) );
                            transform.rotation = Quaternion.Slerp( transform.rotation, lookRotation, Time.deltaTime * 5.0f );

                            //enemyShooting_.Shoot();
                            // Can shoot at this moment (shoot-enable timer elapsed) and clip is not empty
                            if( activeGun_.CanShoot() )
                            {
                                activeGun_.Shoot();
                                /* Test */
                                Vector3 jitter = Random.insideUnitSphere / 25.0f; // TODO: Change difficulty by altering jitter size
                                shootRay_.origin = enemyPosition_;//playerCamera_.transform.position;
                                shootRay_.direction = transform.forward + jitter;

                                // Hit player
                                if( Physics.Raycast( shootRay_, out shootHit_, activeGun_.GetRange() ) )
                                {
                                    if( shootHit_.collider.tag == "Player" )
                                    {
                                        PlayerHealth playerHealth = shootHit_.collider.GetComponent<PlayerHealth>();
                                        if( playerHealth != null )
                                        {
                                            // Set player alive flag
                                            isPlayerAlive_ = !playerHealth.isDead_;

                                            // Hurt player
                                            playerHealth.TakeDamage( 1 ); // activeGun_.GetDamagePerShot()
                                            Debug.DrawLine( shootRay_.origin, shootRay_.origin + shootRay_.direction * 100.0f );
                                        }
                                    }
                                }
                                /* Test */
                                Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.green );
                            }

                            // Reload if needed
                            if( activeGun_.GetClipBullets() == 0 )
                            {
                                // Disable effects after last bullet
                                activeGun_.DisableEffects();
                                // Reload
                                activeGun_.Reload();
                            }

                            // Check if Player is too close to enemy
                            if( playerDistance_ < minShootRange_ )
                            {
                                nav_.enabled = false;

                                //Vector3 rotation = playerDirection.normalized;
                                //Quaternion lookRotation = Quaternion.LookRotation( new Vector3( rotation.x, 0, rotation.z ) );
                                //transform.rotation = Quaternion.Slerp( transform.rotation, lookRotation, Time.deltaTime * 5.0f );
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
                    distractionPoint_ = playerPosition_;
                }
            }

            // TODO: Redo somehow :)
            // Check if NavMeshAgent is enabled to avoid errors (calling of SetDestination and GetRemainingDistance 
            // on disabled NavMeshAgent)
            if( nav_.enabled == false )
            {
                anim_.SetFloat( "Speed", 0.0f );
                lastEnemyState_ = enemyState_;

                if( walkAudio_.isPlaying )
                {
                    walkAudio_.Stop();
                }
                return;
            }

            /*TEST*/
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
            }
            /*TEST*/

            //Debug.Log( enemyState_ );
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
                    // Following just started
                    if( lastEnemyState_ != enemyState_ )
                    {
                        speechAudio_.clip = playerDetectedClip_;
                        speechAudio_.Play();
                    }

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
                    // Seeking just started
                    //if( seekingTimer_ == 5.0f )
                    if( lastEnemyState_ != enemyState_ )
                    {
                        speechAudio_.clip = playerLostClip_;
                        speechAudio_.Play();
                    }
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

            // TODO: Redo to not set it every frame
            // Enemy is moving
            if( enemyState_ != EnemyState.Seeking )
			{
				if( nav_.speed < 5.0f )
				{
					walkAudio_.clip = walkClip_;
					walkAudio_.minDistance = 5;
					walkAudio_.maxDistance = 10;
				}
				else
				{
					walkAudio_.clip = runClip_;
					walkAudio_.minDistance = 15;
					walkAudio_.maxDistance = 30;
				}
				if( !walkAudio_.isPlaying )
				{
					walkAudio_.Play();
				}
			}
			// Enemy is not moving
			else
			{
				walkAudio_.Stop();
			}

			// TODO: Refactor!
			//// Check destination remaining distance
			//if( nav_.remainingDistance < 1.5f )
			//{
			//	if( enemyState_ == EnemyState.Patrolling )
			//	{
			//		GoToNextPoint();
			//	}
			//	else if( enemyState_ == EnemyState.Distracted )
			//	{
			//		// Clear distraction point
			//		distractionPoint_ = NO_DISTRACTION_SET;

			//		// Set seeking state
			//		enemyState_ = EnemyState.Seeking;
			//	}
			//	// TODO: && Seeking?
			//	else if( seekingTimer_ == 5.0f )
			//	{
			//		// Set patrolling state
			//		enemyState_ = EnemyState.Patrolling;

			//		GoToActualPoint();
			//	}
			//}

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
