using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour 
{
    // Player's initial health
	public int health_ = 100;
    // Health slider bar
	public Slider healthSlider_;
    // Health text
	public Text healthText_;
    // Damage image that will flash if hurt
	public Image damageImage_;
    // Damage image flashing speed
	public float flashSpeed_ = 5.0f;
    // Flag indicating if the player is dead
    public bool isDead_ = false;
    // Death sound
    public AudioClip deathClip_;

    // Damage flashing color
    Color damageFlashColor_ = new Color( 1.0f, 0.0f, 0.0f, 0.1f );
    // Restart text color
    Color restartTextColor_ = new Color( 0.8f, 0.8f, 0.8f, 1.0f );
    // Death text color
    Color deathTextColor_ = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
    // Death image color
	Color deathImageColor_ = new Color( 0.0f, 0.0f, 0.0f, 1.0f );
    // Elapsed time since the death
	float deathElapsedTime = 0.0f;

    // Flag indicating if the player just took damage
	bool damaged_ = false;

    // Player's rigidbody component
	Rigidbody rb_;
    // Death text
	Text deathText_;
    // Restart text
	Text restartText_;
    // Death image
	Image deathImage_;
    // Hurt audio source
    AudioSource hurtAudio_;

    // Init function
    void Awake()
    {
        // Get rigidbody component
        rb_ = GetComponent<Rigidbody>();
        // Get death text component
        deathText_ =  GameObject.Find( "DeathText" ).GetComponent<Text>();
        // Get restart text component
        restartText_ =  GameObject.Find( "RestartText" ).GetComponent<Text>();
        // Get death image component
        deathImage_ = GameObject.Find( "DeathImage" ).GetComponent<Image>();
        // Get hurt sudio source component
        hurtAudio_ = GetComponents<AudioSource>()[2];
    }

    // Update function
    void Update () 
	{
        // Check if player is dead
		if( isDead_ )
		{
            // Increase death time
			deathElapsedTime += Time.deltaTime / 2.0f;

            // Change death image color
			deathImage_.color = Color.Lerp( Color.clear, deathImageColor_, deathElapsedTime );
            // Change death text color
            deathText_.color = Color.Lerp( Color.clear, deathTextColor_, deathElapsedTime / 2.0f );
            
            // Check if dead for a long time
            if( deathElapsedTime > 2.0f )
			{
                // Pause the game
				Time.timeScale = 0;
                // Pause the audio listener
				AudioListener.pause = true;

				// Show "Press SPACE to restart" text
				restartText_.color = restartTextColor_;

				// Check for Space press to restart the game
				if( Input.GetKeyUp( KeyCode.Space ) )
				{
                    // Set the default timescale (unpause the game)
					Time.timeScale = 1;
                    // Unpause the audio listener
					AudioListener.pause = false;
                    // Reset the game
					SceneManager.LoadScene( "Shooter" );
				}
			}
		}
        // Player is alive
		else
		{
            // Check if the player just took damage
            if( damaged_ )
			{
                // Set damage image color
				damageImage_.color = damageFlashColor_;
			}
			else
            {
                // Change death image color
                damageImage_.color = Color.Lerp( damageImage_.color, Color.clear, flashSpeed_ * Time.deltaTime );
			}
            // Clear damaged flag
			damaged_ = false;
		}
	}

    // Player hurt function
	public void TakeDamage( int damage )
	{
        // Set damaged flag
		damaged_ = true;

		// Player is alive
		if( health_ - damage > 0 )
		{
            // Decrease the health, update healt slider and the text
			health_ -= damage;
			healthSlider_.value = health_;
			healthText_.text = health_.ToString();
            // Play hurt audio
            hurtAudio_.Play();

		}
        // Player is dead
        else
        {
            // Set health to 0, update healt slider and the text
            health_ = 0;
            healthSlider_.value = 0;
            healthText_.text = health_.ToString();

            // Set audio sound to death sound and play it
            hurtAudio_.clip = deathClip_;
            hurtAudio_.Play();

            // Start death routine
			StartCoroutine( Death() );
		}

	}

    // Death routine
    IEnumerator Death()
    {
		// Set dead state
		isDead_ = true;

        // Unfreeze rotation to allow the player to fall
        rb_.freezeRotation = false;
        // Apply force to make player fall
        rb_.AddForceAtPosition( new Vector3( 2.0f, -0.5f, 0.0f ), transform.position + new Vector3( 0.0f, 1.0f, 0.0f ), ForceMode.Impulse );
        // Wait until player has fallen
        yield return new WaitForSeconds( 2 );
	}
}
