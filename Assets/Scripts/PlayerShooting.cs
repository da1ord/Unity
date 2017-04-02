using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour 
{
    // Ammo text to draw on HUD
	public Text ammoText_;
    // Player's main camera
	public Camera playerCamera_;
    // Crosshair spread amount
    public static float aimSpread_;
    // Sound to play on ammo pickup
    public AudioClip ammoPickUp_;
    // Shooting mask (layers to test against)
    public LayerMask shootMask_;


    // Shooting ray for raycasting
    Ray shootRay_;
    // Shooting hit point
	RaycastHit shootHit_;
    // Player's controller script
    PlayerControllerAnimated playerController_;
	// Player's active gun
	SciFiRifle activeGun_;

    // Init function
    void Awake() 
	{
        // Initialize shoot ray
        shootRay_ = new Ray();
        // Get player controller component
		playerController_ = transform.root.GetComponent<PlayerControllerAnimated>();
        // Get active gun component
		activeGun_ = GetComponentInChildren<SciFiRifle>();
        // Set aim spread to default value
        aimSpread_ = 1.0f;
    }

    // Update function
    void Update() 
	{
		// Update ammo text
		ammoText_.text = activeGun_.GetClipBullets().ToString() + "/" + activeGun_.GetTotalBullets().ToString();

        // Decrease crosshair spread and clamp it
        aimSpread_ -= Time.deltaTime * 1.2f;
        aimSpread_ = Mathf.Clamp( aimSpread_, 1.0f, 2.0f );
    }

    // Shoot function
    public void Shoot()
	{
		// Cannot shoot at this moment (so fast) or clip is empty
		if( !activeGun_.CanShoot() )
		{
			return;
		}

        // Increase crosshair spread while shooting
        aimSpread_ += Time.deltaTime * 13;

        // Shoot with active gun
        activeGun_.Shoot();

        // Setup shoot ray
        // Define jitter
        Vector3 jitter = Mathf.Pow( aimSpread_, 3 ) * Random.insideUnitSphere / 200.0f;
        // Set ray origin to camera position
        shootRay_.origin = playerCamera_.transform.position;
        // Set ray direction to camera forward vector and add the jitter
        shootRay_.direction = playerCamera_.transform.forward + jitter;

        // Test shoot ray against environment and enemy layers
        if( Physics.Raycast( shootRay_, out shootHit_, activeGun_.GetRange(), shootMask_ ) )
		{
            // Shoot ray hit the enemy
			if( shootHit_.collider.tag == "Enemy" )
            {
                // Get enemy health component
                EnemyHealth enemyHealth = shootHit_.collider.GetComponent<EnemyHealth>();
                if( enemyHealth != null )
                {
                    // Harm the enemy with the gun damage
                    enemyHealth.TakeDamage( activeGun_.GetDamagePerShot(), shootHit_.point );
				}
            }
            // Shoot ray hit the enemy's head
            else if( shootHit_.collider.tag == "EnemyHead" )
			{
                // Get enemy health component
                EnemyHealth enemyHealth = shootHit_.collider.GetComponentInParent<EnemyHealth>();
                if( enemyHealth != null )
                {
                    // Harm the enemy with 5 times the gun damage
					enemyHealth.TakeDamage( activeGun_.GetDamagePerShot() * 5, shootHit_.point );
				}
			}
		}

        // Set player's noise level to gun noise
		playerController_.SetNoiseLevel( activeGun_.GetNoiseLevel() );
	}

    // Reload function
	public void Reload()
	{
        // Test if reload was successful
        if( activeGun_.Reload() )
        {
            // Reset crosshair spread on reload
            aimSpread_ = 0;
        }
	}

    // Test for colliders hit (ammo pickup)
	void OnTriggerEnter( Collider other )
	{
        // Check if the object is pickable
		if( other.gameObject.CompareTag( "Pick Up" ) )
        {
            // Play ammo pickup sound
            AudioSource.PlayClipAtPoint( ammoPickUp_, other.gameObject.transform.position );
            // Add ammo clip (increase total bullet ammount)
            activeGun_.AddClip();
            // Destroy the ammo clip
            Destroy( other.gameObject );
        }
    }
}
