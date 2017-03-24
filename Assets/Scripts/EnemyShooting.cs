using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour 
{
	Ray shootRay_;
	RaycastHit shootHit_;

	GameObject player_;
	PlayerHealth playerHealth_;

	// TODO: Redo to IFace
	// Enemy's active gun
	SciFiRifle activeGun_;

//	LayerMask shootableMask_;

	// Init function
	void Awake() 
	{
		player_ = GameObject.FindGameObjectWithTag( "Player" );
		playerHealth_ = player_.GetComponent<PlayerHealth>();

		activeGun_ = GetComponentInChildren<SciFiRifle>();
	}

	void Update() 
	{
	}

	public void Shoot()
	{
		// Cannot shoot at this moment (so fast) or clip is empty
		if( !activeGun_.CanShoot() )
		{
			return;
		}

		activeGun_.Shoot();

		// Hurt player
		playerHealth_.TakeDamage( 1 ); // activeGun_.GetDamagePerShot()
	}
}
