using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour 
{
	public int health_ = 100;
	public Slider healthSlider_;
	public Text healthText_;
	public Image damageImage_;
	public float flashSpeed_ = 5.0f;
	Color flashColor_ = new Color( 1.0f, 0.0f, 0.0f, 0.1f );

	Color deathTextColor_ = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
	Color restartTextColor_ = new Color( 0.8f, 0.8f, 0.8f, 1.0f );
	Color deathImageColor_ = new Color( 0.0f, 0.0f, 0.0f, 1.0f );
	float deathElapsedTime = 0.0f;

	bool damaged_ = false;
	bool isDead_ = false;

	Rigidbody rb_;
	Text deathText_;
	Text restartText_;
	Image deathImage_;

	void Awake() 
	{
		rb_ = GetComponent<Rigidbody>();
		deathText_ =  GameObject.Find( "DeathText" ).GetComponent<Text>();
		restartText_ =  GameObject.Find( "RestartText" ).GetComponent<Text>();
		deathImage_ = GameObject.Find( "DeathImage" ).GetComponent<Image>();
	}

	void Update () 
	{
		if( isDead_ )
		{
			deathElapsedTime += Time.deltaTime / 2.0f;

			deathImage_.color = Color.Lerp( Color.clear, deathImageColor_, deathElapsedTime );
			deathText_.color = Color.Lerp( Color.clear, deathTextColor_, deathElapsedTime / 2.0f );

			if( deathElapsedTime > 2.0f )
			{
				Time.timeScale = 0;
				AudioListener.pause = true;

				// Show "Press SPACE to restart" text
				restartText_.color = restartTextColor_;

				// Check for Space to restart the game
				if( Input.GetKeyUp( KeyCode.Space ) )
				{
					Time.timeScale = 1;
					AudioListener.pause = false;
					SceneManager.LoadScene( "Shooter" );
				}
			}
		}
		else
		{
			if( damaged_ )
			{
				damageImage_.color = flashColor_;
			}
			else
			{
				damageImage_.color = Color.Lerp( damageImage_.color, Color.clear, flashSpeed_ * Time.deltaTime );
			}
			damaged_ = false;
		}
	}

	public void TakeDamage( int damage )
	{
		damaged_ = true;

		// Alive
		if( health_ > 80 )
		{
			health_ -= damage;
			healthSlider_.value = health_;
			healthText_.text = health_.ToString();
			// play sound
			// flash screen

		}
		// Dead
		else 
		{
			StartCoroutine( Death() );
//			Death();
		}

	}

	IEnumerator Death()
	{
		// Set dead state
		isDead_ = true;

		// Unfreeze rotation to be able to fall
		rb_.freezeRotation = false;
		// Apply force to fall
		rb_.AddForceAtPosition( new Vector3( 10.0f, 0.0f, 0.0f ), transform.position +  new Vector3( 0.0f, 0.5f, 0.0f ) );
		// Wait to fall
		yield return new WaitForSeconds( 2 );
		// Set kinematic to avoid collisions
		rb_.isKinematic = true;

		// animation of death
		// play sound
		// disable movement



		// Disable navigation mesh agent
//		GetComponent<NavMeshAgent>().enabled = false;
		// Stop recalculating the static geometry
//		GetComponent<Rigidbody>().isKinematic = true;

		// TODO: Respawn?
		//Destroy( gameObject );
	}
}
