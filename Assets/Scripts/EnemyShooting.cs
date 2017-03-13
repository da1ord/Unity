using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour 
{

//	public int clipBullets_ = 5;
//	public int damagePerShot_ = 5;
//	public float timeBetweenShots_ = 0.15f;
//	public float range_ = 100.0f;

//	float timer_;
//	float reloadTime_ = 1.7f;
//	float effectsDisplayTime_ = 0.02f;
	Ray shootRay_;
	RaycastHit shootHit_;
	LineRenderer gunLine_;
	AudioSource gunAudio_;
	Light gunLight_;
	Animator anim_;

	GameObject player_;
	PlayerHealth playerHealth_;

	// TODO: Redo to IFace
	// Player's active gun
	SciFiRifle activeGun_;

//	LayerMask shootableMask_;

	// Init function
	void Awake() 
	{
		gunLine_ = GetComponent<LineRenderer>();
		gunLight_ = GetComponent<Light>();
		gunAudio_ = GetComponent<AudioSource>();
		anim_ = GetComponentInParent<Animator>();

		player_ = GameObject.FindGameObjectWithTag( "Player" );
		playerHealth_ = player_.GetComponent<PlayerHealth>();

		activeGun_ = GetComponentInChildren<SciFiRifle>();

//		timer_ = 0.0f;
//
//		damagePerShot_ = 0;

//		shootableMask_ = LayerMask.GetMask( "Shootable" );
	}

	void Update() 
	{
//		timer_ -= Time.deltaTime;
//		if( timer_ < timeBetweenShots_ - effectsDisplayTime_ )
//		{
//			DisableEffects();
//		}
	}

	public void Shoot()
	{
		// Cannot shoot at this moment (so fast) or clip is empty
		if( !activeGun_.CanShoot() )
		{
			return;
		}

		activeGun_.Shoot();

//		if( timer_ > 0.0f )
//		{
//			return;
//		}
//		timer_ = timeBetweenShots_;
//
//		if( clipBullets_ == 0 )
//		{
//			anim_.SetTrigger( "Reload" );
//			// Set reload timer
//			timer_ = reloadTime_;
//			clipBullets_ = 5;
//			return;
//		}

//		if( timer_ > timeBetweenShots_ * effectsDisplayTime_ )
//		{
//			return;
//		}

//		clipBullets_--;
//
//		gunAudio_.Play();
//
//		gunLight_.enabled = true;

//		gunLine_.enabled = true;
//		gunLine_.SetPosition( 0, transform.position );

//		shootRay_.origin = transform.position;
//		shootRay_.direction = transform.forward;

		// Hurt player
		playerHealth_.TakeDamage( 1 ); // activeGun_.GetDamagePerShot()
//		gunLine_.SetPosition( 1, shootRay_.origin + shootRay_.direction * activeGun_.GetRange() );

//		if( Physics.Raycast( shootRay_, out shootHit_, range_, shootableMask_ ) )
//		{
//			EnemyHealth enemyHealth = shootHit_.collider.GetComponent<EnemyHealth>();
//			if( enemyHealth != null )
//			{
//				enemyHealth.TakeDamage( damagePerShot_ );
//			}
//
//			gunLine_.SetPosition( 1, shootHit_.point );
//		}
//		else
//		{
//			gunLine_.SetPosition( 1, shootRay_.origin + shootRay_.direction * range_ );
//		}
	}

//	void DisableEffects()
//	{
//		gunLine_.enabled = false;
//		gunLight_.enabled = false;
//	}
}
