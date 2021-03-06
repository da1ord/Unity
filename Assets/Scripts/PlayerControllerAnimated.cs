﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerAnimated : MonoBehaviour 
{
    // Player's movement speed
    public float movementSpeed_ = 5.0f;
	// Mouse sensitivity value
	public static float mouseSensitivity_ = 3.0f;
    // Player's initial position
    public Vector3 playerPosition_;
    // Mouse rotation angle along X-axis
    public float xRot_ = 0.0f;

    // Player's camera instance
    public Camera playerCamera_;
	// Player gun instance
	public GameObject gun_;
	// Walk audio clip
	public AudioClip walkClip_;
	// Run audio clip
	public AudioClip runClip_;
	// Jump audio clip
	public AudioClip jumpClip_;
	// Fall audio clip
	public AudioClip fallClip_;

	// Pivot point along which the guns rotate (for looking around X-axis)
	GameObject gunsPivot_;
    // Player's rigidbody component
    Rigidbody rb_;
    // Player's animator component
    Animator anim_;
    // Walk audio source
    AudioSource walkAudio_;
	// Walk audio source
	AudioSource jumpAudio_;

    // Player's shooting script
    PlayerShooting shooting_;
    // Player's health script
    PlayerHealth health_;
    // Player's collider component
    CapsuleCollider capsule_;

    // Menu controller script
    MenuController menuController_;
    // Map canvas
    //Canvas mapCanvas_;
    // Crosshair canvas
    Canvas crosshairCanvas_;

    // For storing of gun position in local space (for scope mode)
    Vector3 gunPosition_;

    // Flag indicating if player is on the ground
    bool isGrounded_ = true;
    // Flag indicating if player is crouching
    bool isCrouching_ = false;
    // Flag indicating  is player is moving
    bool isMoving_ = false;
    // Player's actual noise level
	float noiseLevel_ = 0.0f;
    // Flag indicating if the menu is open
    bool isMenuOpen_ = false;
    // Flag indicating if the game has started
    bool hasGameStarted_ = false;

    // Init function
    void Awake()
    {
        // Get rigidbody component
        rb_ = GetComponent<Rigidbody>();
        // Get walk audio source component
        walkAudio_ = GetComponents<AudioSource>()[0];
        // Get jump audio source component
        jumpAudio_ = GetComponents<AudioSource>()[1];
        // Get guns pivot object
        gunsPivot_ = GameObject.Find( "Guns" );
        // Get player shooting script
        shooting_ = GetComponentInChildren<PlayerShooting>();
        // Get player health script
        health_ = GetComponentInChildren<PlayerHealth>();
        // Get collider component
        capsule_ = GetComponent<CapsuleCollider>();

        menuController_ = GameObject.Find( "Menu" ).GetComponent<MenuController>();
        // Get map canvas component
        //mapCanvas_ = GameObject.Find( "Map" ).GetComponent<Canvas>();
        // Get crosshair canvas component
        crosshairCanvas_ = GameObject.Find( "DynamicCrosshair" ).GetComponent<Canvas>();

        // Get aimator component
        anim_ = GetComponent<Animator>();
        // Set animator speed variable to 0 (not moving)
		anim_.SetFloat( "Forward", 0.0f );

		// Hide mouse cursor
		//Cursor.visible = false;
        //Cursor.visible = true;
        // Lock mouse cursor to screen center
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.lockState = CursorLockMode.None;

        // Store gun position in local space
        gunPosition_ = gun_.transform.position;
    }

	// Update function
	void Update()
    {
        // Process keyboard input only when the player is alive
        if( !health_.isDead_ && hasGameStarted_ )
        {
            ProcessKeyboardInput();
        }
        hasGameStarted_ = menuController_.HasGameStarted();
	}

    // Update function that runs when physics is calculated?
    void FixedUpdate()
    {
        // Process player's movement only when the player is alive
        if( !health_.isDead_ && hasGameStarted_ )
        {
            playerPosition_ = capsule_.bounds.center;
            PlayerMovement();
        }
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

        // If not jumping, set jumping noise level to 0
        if( !jumpAudio_.isPlaying )
		{
			jumpAudio_.minDistance = 0;
			jumpAudio_.maxDistance = 0;
		}

        // If the movement vector is > 0, set the moving flag as true
        if( movement.magnitude > 0 )
		{
			isMoving_ = true;
		}
		else
		{
			isMoving_ = false;
		}

		// Move rigidbody in movement direction
		rb_.MovePosition( rb_.position + movement * Time.fixedDeltaTime );

		// Get mouse left-right rotation angle
		float yRot = Input.GetAxisRaw( "Mouse X" ) * mouseSensitivity_;

		// Rotate RigidBody by acquired angle along Y-axis
		rb_.MoveRotation( rb_.rotation * Quaternion.Euler( new Vector3( 0.0f, yRot, 0.0f ) ) ); 

		// Update mouse rotation angle along X-axis
		xRot_ += Input.GetAxisRaw( "Mouse Y" ) * mouseSensitivity_;
		// Limit maximum angle from -90 (looking down) to 90 (looking up) degrees
		xRot_ = Mathf.Clamp( xRot_, -90.0f, 90.0f );

        // Rotate gun around pivot point and add recoil
        float recoil = PlayerShooting.aimSpread_ * 10;
        gunsPivot_.transform.localEulerAngles = new Vector3( -xRot_ - recoil, 0.0f, 0.0f );

		// Check if the player is moving and on ground
		if( isMoving_ && isGrounded_ )
		{
            // If walk audio is not playing, play it
			if( !walkAudio_.isPlaying )
			{
				walkAudio_.Play();
			}
		}
		// Player is not moving
		else
		{
            // Set walk noise level to 0
			walkAudio_.minDistance = 0;
			walkAudio_.maxDistance = 0;
            // Set animator forward speed variable to 0 (not moving)
            anim_.SetFloat( "Forward", 0.0f );
            // Stop walk audio
			walkAudio_.Stop();
		}

		// Set player's noise level. Either walk or jump sound noise
		noiseLevel_ = ( walkAudio_.minDistance > jumpAudio_.minDistance ) ? walkAudio_.minDistance : jumpAudio_.minDistance;
	}

	// Processes keyboard input except from player movement
	void ProcessKeyboardInput()
	{
        // Show map
        if( Input.GetKeyDown( KeyCode.Tab ) )
        {
            if( !isMenuOpen_ )
            {
                menuController_.ShowMap();
            }
        }
        // Hide map
        else if( Input.GetKeyUp( KeyCode.Tab ) )
        {
            menuController_.HideMap();
        }

        // Show/hide menu and unlock/lock the mouse cursor
        if( Input.GetKeyUp( KeyCode.F8 ) )
        {
            // Invert menu open flag
            isMenuOpen_ = !isMenuOpen_;
            if( isMenuOpen_ )
            {
                menuController_.ShowMainMenu();
            }
            else
            {
                menuController_.ExitMenu();
            }
        }
        
        // Don't process the controlls if the menu is open
        if( isMenuOpen_ )
        {
            return;
        }

        // Shoot - LMB
        if( Input.GetButton( "Fire1" ) )
		{
			shooting_.Shoot();
		}

		// Enter scope mode - RMB
		if( Input.GetButtonDown( "Fire2" ) )
		{
            crosshairCanvas_.enabled = false;

			gunPosition_ = gun_.transform.localPosition;
			gun_.transform.position = playerCamera_.transform.position;
			gun_.transform.localPosition += new Vector3( -0.003f, -0.108f, 0.2f );
			playerCamera_.fieldOfView /= 2.0f;
		}
		// Exit scope mode
		else if( Input.GetButtonUp( "Fire2" ) )
        {
            crosshairCanvas_.enabled = true;

            gun_.transform.localPosition = gunPosition_;
			playerCamera_.fieldOfView *= 2.0f;
		}

		// Reload a weapon
		if( Input.GetKeyDown( KeyCode.R ) )
		{
			shooting_.Reload();
		}

		// Run
		if( Input.GetKeyDown( KeyCode.LeftShift ) && !isCrouching_ )
		{
            // Set run movement speed, change audio sound to run sound, 
            //  and set the noise level
			movementSpeed_ = 7.5f;
			walkAudio_.clip = runClip_;
			walkAudio_.minDistance = 15;
			walkAudio_.maxDistance = 30;
		}
		// Walk
		else if( Input.GetKeyUp( KeyCode.LeftShift ) )
        {
            // Set walk movement speed, change audio sound to walk sound, 
            //  and set the noise level
            movementSpeed_ = 5.0f;
			walkAudio_.clip = walkClip_;
			walkAudio_.minDistance = 5;
			walkAudio_.maxDistance = 10;
		}

		// Crouch
		if( Input.GetKeyDown( KeyCode.LeftControl ) )
		{
            // Check if not crouching
			if( !isCrouching_ )
			{
                // Set crouching flag, change capsule center and height,
                //  move guns pivot
				isCrouching_ = true;
				capsule_.height /= 2.0f;
				capsule_.center /= 2.0f;
				gunsPivot_.transform.localPosition -= new Vector3( 0.0f, 0.65f, -0.2f );
			}
		}
		// Stand up
		else if( Input.GetKeyUp( KeyCode.LeftControl ) )
        {
            // Check if crouching
            if( isCrouching_ )
            {
                // Clear crouching flag, change capsule center and height,
                //  move guns pivot
                isCrouching_ = false;
				capsule_.height *= 2.0f;
				capsule_.center *= 2.0f;
				gunsPivot_.transform.localPosition += new Vector3( 0.0f, 0.65f, -0.2f );
			}
		}

        // Raycast against floor to check if player is gorunded
		if( Physics.Raycast( playerPosition_, -Vector3.up, 1.0f ) )
		{
			// Check if the player just reached the ground (falling)
			if( isGrounded_ == false )
            {
                // Set jump audio to fall sound, set the noise level and play it
                jumpAudio_.clip = fallClip_;
				jumpAudio_.minDistance = 15;
				jumpAudio_.maxDistance = 30;
				jumpAudio_.Play();
			}
            // Set grounded flag
			isGrounded_ = true;
		}
		else
        {
            // Clear grounded flag
            isGrounded_ = false;
		}

        if( Input.GetKeyDown( KeyCode.Space ) && menuController_.HasPlayerWon() )
        {
            menuController_.ResetScene();
        }

		// Jump (if on ground)
		if( Input.GetKeyDown( KeyCode.Space ) && isGrounded_ )
		{
            // Apply jump force to rigidbody
			rb_.AddForce( new Vector3 ( 0.0f, 4.0f, 0.0f ), ForceMode.Impulse );
            // Set jump audio to jump sound, set the noise level and play it
            jumpAudio_.clip = jumpClip_;
			jumpAudio_.minDistance = 10;
			jumpAudio_.maxDistance = 20;
			jumpAudio_.Play();
		}

        // Check if player is moving
		if( isMoving_ )
		{
            // Set animator forward speed
            anim_.SetFloat( "Forward", movementSpeed_ );
		}
        // Player is not moving
		else
        {
            // Set animator forward speed variable to 0 (not moving)
            anim_.SetFloat( "Forward", 0.0f );
        }
        // Set animator crouch variable
        anim_.SetBool( "Crouch", isCrouching_ );
	}

    // Set palyer's noise level
	public void SetNoiseLevel( float level )
	{
		noiseLevel_ = level;
	}

    // Get player's noise level
	public float GetNoiseLevel()
	{
		return noiseLevel_;
	}
}
