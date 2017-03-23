using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour 
{
	public Text ammoText_;

	public Camera playerCamera_;

	Ray shootRay_;
	RaycastHit shootHit_;

	LayerMask shootableMask_;
	PlayerControllerAnimated playerController_;

	// TODO: Redo to IFace
	// Player's active gun
	SciFiRifle activeGun_;

    public static float aimSpread_;

    // Init function
    void Awake() 
	{
		shootableMask_ = LayerMask.GetMask( "Shootable" );
		playerController_ = transform.root.GetComponent<PlayerControllerAnimated>();

		activeGun_ = GetComponentInChildren<SciFiRifle>();

        aimSpread_ = 1.0f;
    }

	void Update() 
	{
		// Update ammo text
		ammoText_.text = activeGun_.GetClipBullets().ToString() + "/" + activeGun_.GetTotalBullets().ToString();

        aimSpread_ -= Time.deltaTime * 1.2f;
        aimSpread_ = Mathf.Clamp( aimSpread_, 1.0f, 2.0f );

        /* Debug - render jittered shootRay */
        //Vector3 jitter = Mathf.Pow( aimSpread_, 3 ) * Random.insideUnitSphere / 200.0f;
        //shootRay_.origin = playerCamera_.transform.position;
        //shootRay_.direction = playerCamera_.transform.forward + jitter;
        //Debug.DrawLine( shootRay_.origin, shootRay_.origin + shootRay_.direction * 100.0f );
    }

	public void Shoot()
	{
		// Cannot shoot at this moment (so fast) or clip is empty
		if( !activeGun_.CanShoot() )
		{
			return;
		}

        aimSpread_ += Time.deltaTime * 15;

        activeGun_.Shoot();

        // Setup shoot ray
        Vector3 jitter = Mathf.Pow( aimSpread_, 3 ) * Random.insideUnitSphere / 200.0f;
        shootRay_.origin = playerCamera_.transform.position;
        shootRay_.direction = playerCamera_.transform.forward + jitter;

        // Enable gunline rendering
        //		gunLine_.enabled = true;
        //		gunLine_.SetPosition( 0, transform.position );

        // Hit enemy
        // TODO: Remove mask to test raycast against environment?
        //		if( Physics.Raycast( shootRay_, out shootHit_, range_, shootableMask_ ) )
        if( Physics.Raycast( shootRay_, out shootHit_, activeGun_.GetRange() ) )
		{
			if( shootHit_.collider.tag == "Enemy" )
			{
				EnemyHealth enemyHealth = shootHit_.collider.GetComponent<EnemyHealth>();
				if( enemyHealth != null )
				{
					enemyHealth.TakeDamage( activeGun_.GetDamagePerShot() );
//					Debug.Log( activeGun_.GetDamagePerShot() );
//					Debug.Log( "Enemy" );
				}
			}
			else if( shootHit_.collider.tag == "EnemyHead" )
			{
				EnemyHealth enemyHealth = shootHit_.collider.GetComponentInParent<EnemyHealth>();
				if( enemyHealth != null )
				{
					enemyHealth.TakeDamage( activeGun_.GetDamagePerShot() * 5 );
//					Debug.Log( activeGun_.GetDamagePerShot() );
//					Debug.Log( "Head" );
				}
			}

//			gunLine_.startColor = Color.green;
//			gunLine_.SetPosition( 1, shootHit_.point );
		}
		// Missed enemy
		else
		{
//			gunLine_.startColor = Color.red;
//			gunLine_.SetPosition( 1, shootRay_.origin + shootRay_.direction * range_ );
		}

		playerController_.SetNoiseLevel( activeGun_.GetNoiseLevel() );
	}

	public void Reload()
	{
        // Test if reload was successful
        if( activeGun_.Reload() )
        {
            aimSpread_ = 0;
        }
	}

	void OnTriggerEnter( Collider other )
	{
        // Picking up ammo clip
		if( other.gameObject.CompareTag( "Pick Up" ) )
		{
			other.gameObject.SetActive( false );
			activeGun_.AddClip();
		}
	}
}
