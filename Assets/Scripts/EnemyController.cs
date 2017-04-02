using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    // Walk audio clip
    public AudioClip walkClip_;
    // Run audio clip
    public AudioClip runClip_;
    // Player detected audio clip
    public AudioClip playerDetectedClip_;
    // Player lost (seeking start) audio clip
    public AudioClip playerLostClip_;
    // Enemy sight distance
    public float sightDistance_ = 30.0f;
    // Enemy field of view angle (half)
    public float fovYHalf_ = 30.0f;

    // Player transform matrix
    Transform player_;
    // Navigation mesh agent component
	NavMeshAgent nav_;
    // Enemy health script
	EnemyHealth enemyHealth_;
    // Player controller script
	PlayerControllerAnimated playerController_;
    // Capsule component
    CapsuleCollider capsule_;
    // Enemy§s animator component
    Animator anim_;

    // Enemy position vector
    Vector3 enemyPosition_;
    // Player position vector
	Vector3 playerPosition_;
    // Player direction vector
    Vector3 playerDirection_;
    // Distance from the player
    float playerDistance_;
    // Flag indicating if the player is alive
    bool isPlayerAlive_ = true;
    // Enemy's active gun
    SciFiRifle activeGun_;
	// Walk audio source
	AudioSource walkAudio_;
	// Speech audio source
	AudioSource speechAudio_;

    // Enemy rotation speed
    float rotationSpeed_ = 1.5f;

    // Shooting ray for raycasting
    Ray shootRay_;
    // Shooting hit point
    RaycastHit shootHit_;
    // Environment mask for shooting test
    LayerMask environmentMask_;
    // Gun offset from the enemy position (center)
    Vector3 gunOffset_ = new Vector3( 0.0f, 0.5f, 0.0f );

    // Flag indicating if the player was detected
    bool playerDetected_ = false;

    // States in which the enemy can be
	enum EnemyState { Patrolling, Distracted, Seeking, Following };
    // Initial state
	EnemyState enemyState_ = EnemyState.Patrolling;
    // Last known state
	EnemyState lastEnemyState_ = EnemyState.Patrolling;

    // Enemy walking speed
    const float walkSpeed_ = 1.5f;
    // Enemy distrected speed
    const float distractedSpeed_ = 2.5f;
    // Enemy running speed
    const float rushSpeed_ = 4.0f;

    // Enemy minimum distance to be able to move and shoot
    float minShootRange_;
    // Enemy maximum distance to be able to shoot
    float maxShootRange_;
    // Timer for enemy seeking state
    float seekingTimer_ = 5.0f;
    // Timer for enemy seeking state
    const float initSeekingTimerValue_ = 5.0f;

    // Enemy patrolling path
    Vector3[] path_ = new []{ new Vector3( -20.0f, 1.0f, 20.0f ), 
							  new Vector3( -20.0f, 1.0f, -20.0f ), 
							  new Vector3( 29.0f, 1.0f, -29.0f ),
							  new Vector3( 20.0f, 1.0f, 20.0f )};
    // Path destination point ID
	int destPointId_ = 3;
    // Path next destination point ID
    int nextDestPointId_ = 0;
    // Default distraction point position when it is not set
    static Vector3 NO_DISTRACTION_SET = new Vector3( 1000.0f, 1000.0f, 1000.0f );
    // Distraction point position
	Vector3 distractionPoint_;

    const bool DEBUG = false;

    // Init function
    void Start()
	{
        // Get player transform matrix
		player_ = GameObject.FindGameObjectWithTag( "Player" ).transform;
        // Get navigation mesh agent component
		nav_ = GetComponent<NavMeshAgent>();
        // Get capsule component
        capsule_ = GetComponent<CapsuleCollider>();
        // Get enemy health script
        enemyHealth_ = GetComponent<EnemyHealth>();
        // Get player controller script
        playerController_ = GameObject.FindGameObjectWithTag( "Player" ).GetComponent<PlayerControllerAnimated>();

        // Get active gun component
        activeGun_ = GetComponentInChildren<SciFiRifle>();
        // Get animator component
        anim_ = GetComponent<Animator>();
        // Get walk audio component
        walkAudio_ = GetComponents<AudioSource>()[0];
        // Get speech audio component
        speechAudio_ = GetComponents<AudioSource>()[1];

        // Get environment layer mask for shooting
        environmentMask_ = LayerMask.GetMask( "Environment" );

        // TEST
        sightDistance_ = 10.0f;
        // TEST
        
        // Create shoot ray
        shootRay_ = new Ray();
        // Get minimum distance to be able to move and shoot
        minShootRange_ = activeGun_.GetRange() / 6.0f - 8.0f; // 2
        //nShootRange_ = activeGun_.GetRange() / 4.0f - 5.0f; // 10
        // Get maximum distance to be able to shoot
        maxShootRange_ = activeGun_.GetRange() / 6.0f - 4.0f; // 6
        //maxShootRange_ = activeGun_.GetRange() / 4.0f + 5.0f; // 20
        // Clear distraction point
        distractionPoint_ = NO_DISTRACTION_SET;

        // Go to the next point in defined path
		GoToNextPoint();
	}

    // Update function
    void Update()
    {
        // Update enemy and player positions
        enemyPosition_ = capsule_.bounds.center + gunOffset_;
        playerPosition_ = playerController_.playerPosition_;

        // Enemy is alive
        if( enemyHealth_.health_ > 0 )
        {
            #region DEBUG
            if( true/*Debug.isDebugBuild*/ )
            {
                // Calculate and draw the sight boundaries
                Vector3 left = Quaternion.Euler( 0, -fovYHalf_, 0 ) * transform.forward;
                Vector3 right = Quaternion.Euler( 0, fovYHalf_, 0 ) * transform.forward;
                Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.red );
                Debug.DrawRay( transform.position, left * sightDistance_, Color.blue );
                Debug.DrawRay( transform.position, right * sightDistance_, Color.blue );
            }
            #endregion //DEBUG

            // Get direction and distance from player
            playerDirection_ = playerPosition_ - enemyPosition_;
            playerDistance_ = playerDirection_.magnitude;

            // Update audio volume according to the distance from player
            walkAudio_.volume = Mathf.Min( 1.0f, 1.0f / playerDistance_ );
            speechAudio_.volume = Mathf.Min( 1.0f, 20.0f / playerDistance_ );

            // Enable nav mesh agent
            nav_.enabled = true;
            // Disable updating of rotation (enable only in patrolling state)
            nav_.updateRotation = false;
            // Set player detected flag to false
            playerDetected_ = false;

            //// Vision detection
            // Player is in enemy sight distance
            if( playerDistance_ < sightDistance_ && isPlayerAlive_ )
            {
                // Get angle to th player
                float angleToPlayer = Vector3.Angle( playerDirection_, transform.forward );
                // Check if player is in enemy sight cone
                if( angleToPlayer < fovYHalf_ )
                {
                    // Check if player could be seen by enemy (no environment in way)
                    //if( !Physics.Raycast( enemyPosition_, playerDirection_, playerDistance_, environmentMask_ ) )
                    if( !Physics.Raycast( enemyPosition_, playerDirection_, sightDistance_, environmentMask_ ) )
                    {
                        #region DEBUG
                        if( Debug.isDebugBuild )
                        {
                            Debug.DrawLine( enemyPosition_, playerPosition_ );
                        }
                        #endregion

                        // Set player detected flag
                        playerDetected_ = true;
                        // Set following state
                        enemyState_ = EnemyState.Following;

                        distractionPoint_ = playerPosition_;

                        // Check if Player is in shooting range of enemy
                        if( playerDistance_ < maxShootRange_ )
                        {
                            Shoot();

                            // Check if Player is too close to enemy
                            if( playerDistance_ < minShootRange_ )
                            {
                                // Disable enemy movement
                                nav_.enabled = false;
                            }
                        }
                    }
                }
            }

            // Player is outside of enemy vision
            //  Player is behind environment structure or
            //  Player out of enemy sight cone or
            //  Player out of enemy sight distance
            if( !playerDetected_ )
            {
                PlayerNotInSight();
            }

            //Debug.Log( enemyState_ );
            switch( enemyState_ )
            {
                case EnemyState.Patrolling:
                {
                    // Set walk speed
                    nav_.speed = walkSpeed_;
                    nav_.acceleration = walkSpeed_;
                    // Set animator walk speed
                    anim_.SetFloat( "Speed", walkSpeed_ );

                    // Check if the destination point has been almost reached
                    if( nav_.remainingDistance < 1.5f )
                    {
                        // Go to the next point
                        GoToNextPoint();
                    }
                    else
                    {
                        // Go to actual point
                        GoToActualPoint();
                    }
                    nav_.updateRotation = true;
                    break;
                }
                case EnemyState.Distracted:
                {
                    // If enemy state changed since last frame
                    if( lastEnemyState_ != enemyState_ )
                    {
                        // Walk faster towards distraction point
                        nav_.speed = distractedSpeed_;
                        nav_.acceleration = distractedSpeed_;
                        // Set animator distracted speed
                        anim_.SetFloat( "Speed", distractedSpeed_ );

                        // Set enemy rotation speed to walk speed value
                        rotationSpeed_ = walkSpeed_;

                        // Set distraction point and break out of this case block
                        nav_.SetDestination( distractionPoint_ );
                        break;
                    }

                    // Check if the destination point has been almost reached
                    if( nav_.remainingDistance < 1.5f )
                    {
                        // Clear distraction point
                        distractionPoint_ = NO_DISTRACTION_SET;

                        // Set seeking state
                        enemyState_ = EnemyState.Seeking;
                    }
                    else
                    {
                        // Go to the distraction point
                        nav_.SetDestination( distractionPoint_ );
                    }

                    break;
                }
                case EnemyState.Following:
                {
                    // Following just started
                    if( lastEnemyState_ != enemyState_ )
                    {
                        // Play player detected audio
                        speechAudio_.clip = playerDetectedClip_;
                        speechAudio_.Play();

                        // Set enemy rotation speed to rush speed value
                        rotationSpeed_ = rushSpeed_;
                    }

                    // Check if nav mesh agent is disabled == enemy is just shooting
                    if( nav_.enabled == false )
                    {
                        // Stop walking animation
                        anim_.SetFloat( "Speed", 0.0f );

                        // Stop walk audio
                        if( walkAudio_.isPlaying )
                        {
                            walkAudio_.Stop();
                        }
                    }
                    // Nav mesh agent is enabled - shooting and running
                    else
                    {
                        // Run towards player
                        nav_.speed = rushSpeed_;
                        nav_.acceleration = rushSpeed_;
                        // Set animator rush speed
                        anim_.SetFloat( "Speed", rushSpeed_ );

                        // Follow the player
                        nav_.SetDestination( player_.position );

                        #region DEBUG
                        if( Debug.isDebugBuild )
                        {
                            Debug.DrawRay( transform.position, transform.up * 5.0f, Color.yellow );
                        }
                        #endregion

                    }
                    break;
                }
                case EnemyState.Seeking:
                {
                    //Debug.Log( seekingTimer_ );
                    //Debug.Log( enemyPosition_ );
                    // Seeking just started
                    if( seekingTimer_ == initSeekingTimerValue_ )
                    {
                        // Play player lost audio
                        speechAudio_.clip = playerLostClip_;
                        speechAudio_.Play();
                    }
                    // Seeking in progress
                    if( seekingTimer_ > 0.0f )
                    {
                        // Decrease seeking timer
                        seekingTimer_ -= Time.deltaTime;

                        // Seek for Player - rotate around
                        transform.Rotate( transform.up, 2.0f );
                    }
                    // Seeking done
                    else
                    {
                        // Reset seeking timer
                        seekingTimer_ = initSeekingTimerValue_;

                        // Set patrolling state
                        enemyState_ = EnemyState.Patrolling;
                        // Go to actual point
                        GoToActualPoint();
                    }
                    break;
                }
            }

            // Enemy is moving
            if( enemyState_ != EnemyState.Seeking )
            {
                // Check if enemy state has changed
                if( lastEnemyState_ != enemyState_ )
                {
                    // Set audio clips according to the movement speed
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
                }
                // Play walk audio if it is not playing
                if( !walkAudio_.isPlaying )
                {
                    walkAudio_.Play();
                }
            }
            // Enemy is not moving
            else
            {
                // Stop walk audio
                walkAudio_.Stop();
            }

            // If the enemy is distracted or following, update his rotation using this code
            if( enemyState_ == EnemyState.Distracted || enemyState_ == EnemyState.Following )
            //if( nav_.updateRotation == false && enemyState_ != EnemyState.Seeking )
            {
                Vector3 rotation = ( distractionPoint_ - enemyPosition_ ).normalized;//playerDirection_.normalized;
                Quaternion lookRotation = Quaternion.LookRotation( new Vector3( rotation.x, 0, rotation.z ) );
                transform.rotation = Quaternion.Slerp( transform.rotation, lookRotation, Time.deltaTime * rotationSpeed_ );
            }
            
            // Reset seeking timer when leaving seeking state
            if( lastEnemyState_ == EnemyState.Seeking && enemyState_ != EnemyState.Seeking )
            {
                // Reset seek timer
                seekingTimer_ = initSeekingTimerValue_;
            }
            // Update last enemy state
            lastEnemyState_ = enemyState_;
		}
        // Enemy is dead
		else
		{
            // Disable enemy's nav mesh agent
			nav_.enabled = false;
		}
	}

    // Shoot function
    void Shoot()
    {
        // Can shoot at this moment (shoot-enable timer elapsed) and clip is not empty
        if( activeGun_.CanShoot() )
        {
            // Call active gun shoot function
            activeGun_.Shoot();

            // Add jitter to the shootRay
            Vector3 jitter = Random.insideUnitSphere / 25.0f; // TODO: Change difficulty by altering jitter size
            shootRay_.origin = enemyPosition_;
            shootRay_.direction = playerDirection_.normalized + jitter;

            // Test shoot ray against player
            if( Physics.Raycast( shootRay_, out shootHit_, activeGun_.GetRange() ) )
            {
                // Player was hit
                if( shootHit_.collider.tag == "Player" )
                {
                    // Get player health component
                    PlayerHealth playerHealth = shootHit_.collider.GetComponent<PlayerHealth>();
                    if( playerHealth != null )
                    {
                        // Hurt player
                        playerHealth.TakeDamage( activeGun_.GetDamagePerShot() / 2 );

                        // Set player alive flag (get value from player health script)
                        isPlayerAlive_ = !playerHealth.isDead_;

                        #region DEBUG
                        if( Debug.isDebugBuild )
                        {
                            Debug.DrawLine( shootRay_.origin, shootRay_.origin + shootRay_.direction * 100.0f );
                        }
                        #endregion
                    }
                }
            }
            #region DEBUG
            if( Debug.isDebugBuild )
            {
                Debug.DrawRay( transform.position, transform.forward * sightDistance_, Color.green );
            }
            #endregion
        }

        // Reload if needed
        if( activeGun_.GetClipBullets() == 0 )
        {
            // Disable effects after last bullet
            activeGun_.DisableEffects();
            // Reload
            activeGun_.Reload();
        }
    }

    // Routine called when Player is not detected by enemy
    void PlayerNotInSight()
    {
        //// Sound detection
        // Enemy can hear a noise
        if( playerController_.GetNoiseLevel() > playerDistance_ )
        {
            // Set distracted state
            enemyState_ = EnemyState.Distracted;
            // Set distraction point to player position
            distractionPoint_ = playerPosition_;
            nav_.SetDestination( distractionPoint_ );
            return;
        }

        // Set patrolling or distracted state according to set/cleared 
        //  distraction point
        // Check if enemy is not in seeking state
        if( enemyState_ != EnemyState.Seeking )
		{
            // No distraction point set -> set patrolling state
			if( distractionPoint_ == NO_DISTRACTION_SET )
			{
				enemyState_ = EnemyState.Patrolling;
            }
            // Distraction point exists -> set distracted state
            else
            {
				enemyState_ = EnemyState.Distracted;
                nav_.SetDestination( distractionPoint_ );
            }
		}
	}

    // Set enemy distraction point
	public void SetDistractionPoint( Vector3 distraction_point )
	{
        // Check if not already following the player
		if( enemyState_ != EnemyState.Following )
		{
            // Set distracted state
			enemyState_ = EnemyState.Distracted;
            // Set distraction point
			distractionPoint_ = distraction_point;
		}
	}

    // Go to the next point when patrolling
	void GoToNextPoint()
	{
        // Return if the path is not defined
		if( path_.Length == 0 )
		{
			return;
		}

        // Set destination point to the next one in path
		nav_.SetDestination( path_[nextDestPointId_] );
        // Update next point ID
		nextDestPointId_ = ( nextDestPointId_ + 1 ) % path_.Length;
        // Update actual point ID
		destPointId_ = ( destPointId_ + 1 ) % path_.Length;
	}

    // Go to actual point
	void GoToActualPoint()
    {
        // Return if the path is not defined
        if( path_.Length == 0 )
		{
			return;
		}

        // If the distance to the actual destination point is further than 
        //  2x the next destination point distance, go to the next one
        if( ( path_[destPointId_] - enemyPosition_ ).magnitude >
            2.0f * ( path_[nextDestPointId_] - enemyPosition_ ).magnitude )
        {
            GoToNextPoint();
        }
        else
        {
            // Set destination point to the actual one
            nav_.SetDestination( path_[destPointId_] );
        }
	}
}
