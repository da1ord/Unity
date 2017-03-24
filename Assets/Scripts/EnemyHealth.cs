using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour 
{
	public int health_ = 100;
	public GameObject clip_;

	Rigidbody rb_;
	NavMeshAgent nav_;
    Animator anim_;
    bool isDead_ = false;

    AudioSource walkAudio_;
    AudioSource speechAudio_;
    public AudioClip hurtClip_;
    public AudioClip deathClip_;

    void Awake() 
	{
		rb_ = GetComponent<Rigidbody>();
        nav_ = GetComponent<NavMeshAgent>();
        anim_ = GetComponent<Animator>();
        walkAudio_ = GetComponents<AudioSource>()[0];
        speechAudio_ = GetComponents<AudioSource>()[1];
    }
	
	void Update () 
	{
		
	}

	public void TakeDamage( int damage )
	{
        if( isDead_ )
        {
            return;
        }

		health_ -= damage;

        speechAudio_.clip = hurtClip_;
        speechAudio_.Play();

		if( health_ <= 0 )
		{
			StartCoroutine( Death() );
		}

	}

	IEnumerator Death()
	{
        // Set dead state
        isDead_ = true;

        // TODO: death animation
        walkAudio_.Stop();
        speechAudio_.clip = deathClip_;
        speechAudio_.Play();

        // Stop navMeshAgent and current animation 
        anim_.Stop();
        if( nav_.enabled )
        {
            nav_.Stop();
        }
        // Unfreeze rotation to be able to fall
        rb_.freezeRotation = false;
		// Apply force to fall
		rb_.AddForceAtPosition( new Vector3( 2.0f, -0.5f, 0.0f ), transform.position + new Vector3( 0.0f, 1.0f, 0.0f ), ForceMode.Impulse );
		// Wait until fall is finished
		yield return new WaitForSeconds( 1 );

        // Spawn ammo clip
		// Make sure the ammo clip spawns in the air and is oriented properly
		Vector3 clipSpawnPosition = transform.position;
		clipSpawnPosition.y = 1.0f;
        Quaternion rotation = new Quaternion( 1, 0, 0, 0 );

        Instantiate( clip_, clipSpawnPosition, rotation );

		Destroy( gameObject );
	}
}
