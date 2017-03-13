using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour 
{
	public int health_ = 100;
	public GameObject clip_;
	CapsuleCollider capsuleCollider_;

	void Awake() 
	{
		capsuleCollider_ = GetComponent<CapsuleCollider>();
	}
	
	void Update () 
	{
		
	}

	public void TakeDamage( int damage )
	{
		health_ -= damage;

		// TODO: play sound

		if( health_ <= 0 )
		{
			Death();
		}

	}

	void Death()
	{
		// is dead - true
		capsuleCollider_.isTrigger = true;
		// animation of death
		// TODO: play sound

		Instantiate( clip_, transform.position, transform.rotation );

		// Disable navigation mesh agent
		GetComponent<NavMeshAgent>().enabled = false;
		// Stop recalculating the static geometry
		GetComponent<Rigidbody>().isKinematic = true;

		Destroy( gameObject );
	}
}
