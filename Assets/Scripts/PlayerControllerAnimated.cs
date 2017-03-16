using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerAnimated : MonoBehaviour 
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

	Animator anim_;

	// Player shooting script
	PlayerShooting shooting_;

	CapsuleCollider capsule_;

	public Vector3 playerPosition_;

	// Information if player is on the ground
	bool isGrounded_ = true;
	// Information if player is crouching
	bool isCrouching_ = false;
	// Mouse rotation angle along X-axis
	public float xRot_ = 0.0f;

	bool isMoving_ = false;
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

		capsule_ = GetComponent<CapsuleCollider>();

		anim_ = GetComponent<Animator>();
		anim_.SetFloat( "Forward", 0.0f );

		// Avoid playing of walk sound
		rb_.MovePosition( startPosition_ );

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
		playerPosition_ = capsule_.bounds.center;
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

		if( movement.magnitude > 0 )
		{
			isMoving_ = true;
		}
		else
		{
			isMoving_ = false;
		}
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
		//cam_.transform.localEulerAngles = new Vector3( -xRot_, 0.0f, 0.0f );

		// Rotate gun around pivot point
		gunsPivot_.transform.localEulerAngles = new Vector3( -xRot_, 0.0f, 0.0f );

		// Check if the player is moving - play sound
		if( isMoving_ && isGrounded_ )
		{
			if( !walkAudio_.isPlaying )
			{
				walkAudio_.Play();
			}
			noiseLevel_ = walkAudio_.minDistance;
		}
		// Player is not moving
		else
		{
			anim_.SetFloat( "Forward", 0.0f );
			walkAudio_.Stop();
		}
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
			gun_.transform.localPosition += new Vector3( -0.003f, -0.108f, 0.2f );/*new Vector3( -0.006f, -0.11f, 0.2f );*/
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
			shooting_.Reload();
		}

		// Run
		if( Input.GetKeyDown( KeyCode.LeftShift ) && !isCrouching_ )
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
				//cam_.transform.localPosition -= new Vector3( 0.0f, 0.6f, -0.2f );
				gunsPivot_.transform.localPosition -= new Vector3( 0.0f, 0.65f, -0.2f );
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
				//cam_.transform.localPosition += new Vector3( 0.0f, 0.6f, -0.2f );
				gunsPivot_.transform.localPosition += new Vector3( 0.0f, 0.65f, -0.2f );
			}
//			transform.localScale = new Vector3( transform.localScale.x, 1.0f, transform.localScale.z );
//			transform.localPosition += new Vector3( 0.0f, 0.4f, 0.0f );
		}

		if( Physics.Raycast( playerPosition_, -Vector3.up, 1.0f ) )/*transform.position*/
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


		if( isMoving_ )
		{
			anim_.SetFloat( "Forward", movementSpeed_ );
		}
		else
		{
			anim_.SetFloat( "Forward", 0.0f );
		}
		anim_.SetBool( "Crouch", isCrouching_ );
		//anim_.SetBool( "OnGround", isGrounded_ );
	}

	public void SetNoiseLevel( float level )
	{
		noiseLevel_ = level;
	}

	public float GetNoiseLevel()
	{
		return noiseLevel_;
	}
}
