using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour 
{

	public int clipBullets_ = 30;
	public int totalBullets_ = 120;

	public int damagePerShot_ = 10;
	public float timeBetweenShots_ = 0.15f;
	public float range_ = 100.0f;
	public Text ammoText_;

	public Camera playerCamera_;

	float timer_;
	float reloadTime_ = 1.7f;
	float effectsDisplayTime_ = 0.01f;
	Ray shootRay_;
	RaycastHit shootHit_;
	LineRenderer gunLine_;
	AudioSource gunAudio_;
	Light gunLight_;

	LayerMask shootableMask_;
	PlayerController playerController_;

	// Init function
	void Awake() 
	{
		gunLine_ = GetComponent<LineRenderer>();
		gunLight_ = GetComponent<Light>();
		gunAudio_ = GetComponent<AudioSource>();

		shootableMask_ = LayerMask.GetMask( "Shootable" );
		playerController_ = transform.root.GetComponent<PlayerController>();
	}

	void Update() 
	{
		timer_ -= Time.deltaTime;

		if( Input.GetMouseButton( 0 ) && timer_ <= 0.0f && clipBullets_ > 0 )
		{
			Shoot();
		}

		if( timer_ < timeBetweenShots_ - effectsDisplayTime_ )
		{
			DisableEffects();
		}

		// Update ammo text
		ammoText_.text = clipBullets_.ToString() + "/" + totalBullets_.ToString();
	}

	// Set reload timer. Time of the reload animation to wait
	public void SetReloadTimer()
	{
		timer_ = reloadTime_;
	}

	void Shoot()
	{
		timer_ = timeBetweenShots_;
		clipBullets_--;

		gunAudio_.Play();

		gunLight_.enabled = true;

		// Setup shoot ray
		shootRay_.origin = playerCamera_.transform.position;
		shootRay_.direction = playerCamera_.transform.forward;

		// Enable gunline rendering
		gunLine_.enabled = true;
		gunLine_.SetPosition( 0, transform.position );

		// Hit enemy
		if( Physics.Raycast( shootRay_, out shootHit_, range_, shootableMask_ ) )
		{
			EnemyHealth enemyHealth = shootHit_.collider.GetComponent<EnemyHealth>();
			if( enemyHealth != null )
			{
				enemyHealth.TakeDamage( damagePerShot_ );
			}

			gunLine_.startColor = Color.green;
			gunLine_.SetPosition( 1, shootHit_.point );
		}
		// Missed enemy
		else
		{
			gunLine_.startColor = Color.red;
			gunLine_.SetPosition( 1, shootRay_.origin + shootRay_.direction * range_ );
		}

		// Set distraction point of each enemy
		// TODO: use observer pattern
//		EnemyController[] allEnemies = GameObject.FindObjectsOfType<EnemyController>();
//		foreach( EnemyController enemy in allEnemies )
//		{
//			if( ( enemy.transform.position - transform.position ).magnitude < gunAudio_.minDistance )
//			{
//				enemy.SetDistractionPoint( transform.position );
//			}
//		}
		playerController_.SetNoiseLevel( gunAudio_.minDistance );
	}

	void DisableEffects()
	{
		gunLine_.enabled = false;
		gunLight_.enabled = false;
	}
}
