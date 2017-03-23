using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour 
{
	public int health_ = 100;
	public GameObject clip_;
	CapsuleCollider capsuleCollider_;

	Rigidbody rb_;
	NavMeshAgent nav_;

	void Awake() 
	{
		capsuleCollider_ = GetComponent<CapsuleCollider>();

		rb_ = GetComponent<Rigidbody>();
		nav_ = GetComponent<NavMeshAgent>();
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
			StartCoroutine( Death() );
//			Death();
		}

	}

	IEnumerator Death()
	{
		// is dead - true
		capsuleCollider_.isTrigger = true;
		// animation of death
		// TODO: play sound

		/*Test*/
		nav_.enabled = false;
		nav_.updatePosition = false;
		// Unfreeze rotation to be able to fall
		rb_.freezeRotation = false;
		// Apply force to fall
		rb_.AddForceAtPosition( new Vector3( 10.0f, 0.0f, 0.0f ), transform.position + new Vector3( 0.0f, 1.0f, 0.0f ) );
		// Wait to fall
		yield return new WaitForSeconds( 0 );
		// Set kinematic to avoid collisions
		rb_.isKinematic = true;
		/*Test*/

		// Make sure the clip spawns in the air
		Vector3 clipSpawnPosition = transform.position;
		clipSpawnPosition.y = 1.0f;

		Instantiate( clip_, clipSpawnPosition, transform.rotation );

		// Disable navigation mesh agent
//		GetComponent<NavMeshAgent>().enabled = false;
		// Stop recalculating the static geometry
//		GetComponent<Rigidbody>().isKinematic = true;

		Destroy( gameObject );
	}
}
