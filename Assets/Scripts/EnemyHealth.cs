using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    // Enemy's initial health
    public int health_ = 100;
    // Ammo clip gameobject
	public GameObject clip_;
    // Hurt sound
    public AudioClip hurtClip_;
    // Death sound
    public AudioClip deathClip_;

    // Flag indicating if the enemy is dead
    bool isDead_ = false;

    // Enemy's rigidbody component
    Rigidbody rb_;
    // Enemy's navigation mesh agent component
    NavMeshAgent nav_;
    // Enemy's animator component
    Animator anim_;
    // Enemy's blood splash particle system component
    ParticleSystem bloodSplash_;

    // Enemy's walk audio source
    AudioSource walkAudio_;
    // Enemy's speech audio source
    AudioSource speechAudio_;

    MenuController menuController_;
    EnemyManager enemyManager_;
    Text enemyCountText_;
    string enemyCountString_;

    // Init function
    void Awake()
    {
        // Get rigidbody component
        rb_ = GetComponent<Rigidbody>();
        // Get navigation mesh agent component
        nav_ = GetComponent<NavMeshAgent>();
        // Get animator component
        anim_ = GetComponent<Animator>();
        // Get blood splash particle system component
        bloodSplash_ = GameObject.Find( "BloodSplash" ).GetComponent<ParticleSystem>();

        // Get walk audio source component
        walkAudio_ = GetComponents<AudioSource>()[0];
        // Get speech audio source component
        speechAudio_ = GetComponents<AudioSource>()[1];

        // Get menu controller script
        menuController_ = GameObject.Find( "Menu" ).GetComponent<MenuController>();
        // Get enemy manager script
        enemyManager_ = GameObject.Find( "EnemyManager" ).GetComponent<EnemyManager>();
        // Get enemy count text component
        enemyCountText_ = GameObject.Find( "RemainingEnemiesText" ).GetComponent<Text>();
    }

    // Enemy hurt function
    public void TakeDamage( int damage, Vector3 hitPoint )
    {
        // Check if enemy is dead
        if( isDead_ )
        {
            return;
        }

        // Decrease the health
        health_ -= damage;

        // Happens that the particle system throws a NullReference exception
        if( bloodSplash_ == null )
        {
            bloodSplash_ = GameObject.Find( "BloodSplash" ).GetComponent<ParticleSystem>();
        }

        // Move blood splash particle system to the hit point and animate it
        bloodSplash_.transform.position = hitPoint;
        bloodSplash_.Play();

        // Set speech audio sound to hurt sound and play it
        speechAudio_.clip = hurtClip_;
        speechAudio_.Play();

        // If health is 0 or less, enemy is dead
		if( health_ <= 0 )
        {
            Death();
        }
	}
    
    // Enemy death function
	void Death()
	{
        // Set dead state
        isDead_ = true;
        
        // Stop walk audio
        walkAudio_.Stop();
        // Set speech audio sound to death sound and play it
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

        // Spawning of ammo clip
		// Make sure the ammo clip spawns in the air and is oriented properly
		Vector3 clipSpawnPosition = transform.position;
		clipSpawnPosition.y += 1.0f;
        Quaternion rotation = new Quaternion( 1, 0, 0, 0 );
        // Spawn ammo clip
        Instantiate( clip_, clipSpawnPosition, rotation );

        // Tell enemy manager that enemy was killed
        enemyManager_.EnemyKilled();
        // Update enemy count text
        enemyCountText_.text = "Enemies remaining: " + enemyManager_.GetEnemyCount().ToString();

        // Destroy the enemy after 1s
        Destroy( gameObject, 1.0f );
        
        // Show win screen if enemy count is 0
        if( enemyManager_.GetEnemyCount() == 0 )
        {
            menuController_.ShowWinScreen();
        }
	}
}
