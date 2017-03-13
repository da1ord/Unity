using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour 
{

	//Player movement speed
	public float movementSpeed_ = 5.0f;
	// Mouse sensitivity value
	public float mouseSensitivity_ = 3.0f;
	// Player camera instance
	public Camera cam_;
	// Player gun instance
	public GameObject gun_;
	// Walk audio clip
	public AudioClip walkClip_;
	// Run audio clip
	public AudioClip runClip_;

	public Image crosshair_;

	public Vector3 startPosition_ = new Vector3( 0.1f, 0.9f, 0.1f );

	// Pivot point along which the guns rotate (for looking around X-axis)
	GameObject gunsPivot_;
	// Player RigidBody instance
	Rigidbody rb_;
	// Player Animator instance
//	Animator anim_;
	// Walk audio source
	AudioSource walkAudio_;

	// Player shooting script
	PlayerShooting shooting_;

	CapsuleCollider capsule_;

	// Information if player is on the ground
	bool isGrounded_ = true;
	// Information if player is crouching
	bool isCrouching_ = false;
	// Mouse rotation angle along X-axis
	float xRot_ = 0.0f;

	bool isMoving_ = false;
	Vector3 position_;
	Vector3 gunPosition_;
	float noiseLevel_ = 0.0f;

	// Init function
	void Start()
	{
		rb_ = GetComponent<Rigidbody>();
//		anim_ = GetComponentInChildren<Animator>();
		walkAudio_ = GetComponent<AudioSource>();
		gunsPivot_ = GameObject.Find( "Guns" );
		shooting_ = GetComponentInChildren<PlayerShooting>();

		capsule_ = GetComponentInChildren<CapsuleCollider>();

		// Avoid playing of walk sound
		rb_.MovePosition( startPosition_ );
		position_.x = rb_.position.x;
		position_.z = rb_.position.z;

		// Hide mouse cursor
		Cursor.visible = false;
		// Lock mouse cursor to screen center
		Cursor.lockState = CursorLockMode.Locked;

		gunPosition_ = gun_.transform.position;
	}

	// Update function that runs before every frame rendering?
	void Update ()
	{
		ProcessKeyboardInput();
	}

	// Update function that runs when physics is calculated?
	void FixedUpdate () 
	{
		PlayerMovement();
	}

	// Processes player movement
	void PlayerMovement()
	{
		// Strafing - A, D
		float axisH = Input.GetAxisRaw( "Horizontal" );
		// Forward and backward - W, S
		float axisV = Input.GetAxisRaw( "Vertical" );

		// Calculate movement vector
		Vector3 moveH = transform.right * axisH;
		Vector3 moveV = transform.forward * axisV;
		Vector3 movement = ( moveH + moveV ).normalized * movementSpeed_;

		// Move RigidBody in movement direction per frametime
		rb_.MovePosition( rb_.position + movement * Time.fixedDeltaTime );

		// Get mouse left-right rotation angle
		float yRot = Input.GetAxisRaw( "Mouse X" ) * mouseSensitivity_;
		Vector3 rotation = new Vector3( 0.0f, yRot, 0.0f );

		// Rotate RigidBody by acquired angle along Y-axis
		rb_.MoveRotation( rb_.rotation * Quaternion.Euler( rotation ) ); 

		// Accumulate mouse rotation angle along X-axis
		xRot_ += Input.GetAxisRaw( "Mouse Y" ) * mouseSensitivity_;
		// Limit maximum angle from -90(looking down) to 90(looking up) degrees
		xRot_ = Mathf.Clamp( xRot_, -90.0f, 90.0f );

		// Rotate just the camera along X-axis
		cam_.transform.localEulerAngles = new Vector3( -xRot_, 0.0f, 0.0f );

		// Rotate gun around pivot point
		gunsPivot_.transform.localEulerAngles = new Vector3( -xRot_, 0.0f, 0.0f );

		// Check if the player is moving - play sound
		if(( position_.x != rb_.position.x || position_.z != rb_.position.z ) && isGrounded_ )
		{
			isMoving_ = true;
//			Debug.Log( position_.z + " " + rb_.position.z );
//			Debug.DrawLine( transform.position, transform.position + transform.up * 10 );
			if( !walkAudio_.isPlaying )
			{
				walkAudio_.Play();
			}
			// Set distraction point of each enemy
			// TODO: use observer pattern
//			EnemyController[] allEnemies = GameObject.FindObjectsOfType<EnemyController>();
//			foreach( EnemyController enemy in allEnemies )
//			{
//				if( ( enemy.transform.position - transform.position ).magnitude < walkAudio_.minDistance )
//				{
//					enemy.SetDistractionPoint( transform.position );
//				}
//			}
			noiseLevel_ = walkAudio_.minDistance;
		}
		// Player is not moving
		else
		{
			isMoving_ = false;
//			Debug.DrawLine( transform.position, transform.position + transform.up * 5 );
			walkAudio_.Stop();
		}
		// Save player position
		position_.x = rb_.position.x;
		position_.z = rb_.position.z;
	}

	// Processes keyboard input except from player movement
	void ProcessKeyboardInput()
	{
		// Lock/unlock mouse cursor
		if( Input.GetKeyUp( KeyCode.F8 ) ) 
		{
			if( Cursor.lockState == CursorLockMode.Locked )
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			} 
			else 
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		}

		// Shoot - LMB
		if( Input.GetMouseButton( 0 ) )
		{
			shooting_.Shoot();
		}

		// Enter scope mode - RMB
		if( Input.GetMouseButtonDown( 1 ) )
		{
			gunPosition_ = gun_.transform.localPosition;
			gun_.transform.position = cam_.transform.position;
			gun_.transform.localPosition += new Vector3( -0.006f, -0.21f, 0.0f );
			cam_.fieldOfView /= 2.0f;
			crosshair_.enabled = false;
		}
		// Exit scope mode
		else if( Input.GetMouseButtonUp( 1 ) )
		{
			gun_.transform.localPosition = gunPosition_;
			cam_.fieldOfView *= 2.0f;
			crosshair_.enabled = true;
		}

		// Reload a weapon
		if( Input.GetKeyDown( KeyCode.R ) /*&& shooting_.totalBullets_ > 0*/ )
		{
//			anim_.SetTrigger( "Reload" );
//			shooting_.clipBullets_ = ( shooting_.totalBullets_ < 30 ) ? shooting_.totalBullets_ : 30;
//			shooting_.totalBullets_ -= shooting_.clipBullets_;
//			shooting_.SetReloadTimer();
			shooting_.Reload();
		}

		// Run
		if( Input.GetKeyDown( KeyCode.LeftShift ) )
		{
			movementSpeed_ = 10.0f;
			walkAudio_.clip = runClip_;
			walkAudio_.minDistance = 15;
			walkAudio_.maxDistance = 30;
		}
		// Walk
		if( Input.GetKeyUp( KeyCode.LeftShift ) )
		{
			movementSpeed_ = 5.0f;
			walkAudio_.clip = walkClip_;
			walkAudio_.minDistance = 5;
			walkAudio_.maxDistance = 10;
		}

		// Crouch
		if( Input.GetKeyDown( KeyCode.LeftControl ) )
		{
			if( !isCrouching_ )
			{
				isCrouching_ = true;
				capsule_.height /= 2.0f;
				capsule_.center /= 2.0f;
			}
//			transform.localScale = new Vector3( transform.localScale.x, 0.5f, transform.localScale.z );
//			transform.localPosition -= new Vector3( 0.0f, 0.4f, 0.0f );
		}
		// Stand up
		if( Input.GetKeyUp( KeyCode.LeftControl ) )
		{
			if( isCrouching_ )
			{
				isCrouching_ = false;
				capsule_.height *= 2.0f;
				capsule_.center *= 2.0f;
			}
//			transform.localScale = new Vector3( transform.localScale.x, 1.0f, transform.localScale.z );
//			transform.localPosition += new Vector3( 0.0f, 0.4f, 0.0f );
		}

		if( Physics.Raycast( transform.position, -Vector3.up, 1.0f ) )
		{
			isGrounded_ = true;
		}
		else
		{
			isGrounded_ = false;
		}

		// Jump if grounded
		if( Input.GetKeyDown( KeyCode.Space ) && isGrounded_ )
		{
			rb_.AddForce( new Vector3 ( 0.0f, 5.0f, 0.0f ), ForceMode.Impulse );
		}
	}

	public void SetNoiseLevel( float level )
	{
		noiseLevel_ = level;
	}

	public float GetNoiseLevel()
	{
		return noiseLevel_;
	}

	void OnTriggerEnter( Collider other )
	{
		//		if (other.gameObject.CompareTag ("Pick Up")) 
		//		{
		//			other.gameObject.SetActive (false);
		//			count++;
		//			SetCountText ();
		//		}
	}
}
