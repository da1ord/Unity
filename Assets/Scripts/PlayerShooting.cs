using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour 
{
//	public float timeBetweenShots_ = 0.15f;
//	public float range_ = 100.0f;
	public Text ammoText_;

	public Camera playerCamera_;

//	float timer_;
//	float reloadTime_ = 1.7f;
//	float effectsDisplayTime_ = 0.01f;
	Ray shootRay_;
	RaycastHit shootHit_;

	LayerMask shootableMask_;
	PlayerController playerController_;

	// TODO: Redo to IFace
	// Player's active gun
	SciFiRifle activeGun_;

	// Init function
	void Awake() 
	{
		shootableMask_ = LayerMask.GetMask( "Shootable" );
		playerController_ = transform.root.GetComponent<PlayerController>();

		activeGun_ = GetComponentInChildren<SciFiRifle>();
	}

	void Update() 
	{
//		timer_ -= Time.deltaTime;

		// Update ammo text
//		ammoText_.text = clipBullets_.ToString() + "/" + totalBullets_.ToString();
		ammoText_.text = activeGun_.GetClipBullets().ToString() + "/" + activeGun_.GetTotalBullets().ToString();
	}

	public void Shoot()
	{
		// Cannot shoot at this moment (so fast) or clip is empty
		if( !activeGun_.CanShoot() )
		{
			return;
		}

		activeGun_.Shoot();

		// Setup shoot ray
		shootRay_.origin = playerCamera_.transform.position;
		shootRay_.direction = playerCamera_.transform.forward;

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
					Debug.Log( activeGun_.GetDamagePerShot() );
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
		activeGun_.Reload();
	}

	void OnTriggerEnter( Collider other )
	{
		if( other.gameObject.CompareTag( "Pick Up" ) )
		{
			other.gameObject.SetActive( false );
			activeGun_.AddClip();
		}
	}
}
