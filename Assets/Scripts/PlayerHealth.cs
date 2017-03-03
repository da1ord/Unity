using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour 
{
	public int health_ = 100;
	public Slider healthSlider_;
	public Text healthText_;
	public Image damageImage_;
	public float flashSpeed_ = 5.0f;
	Color flashColor_ = new Color( 1.0f, 0.0f, 0.0f, 0.1f );

	bool damaged_ = false;
//	CapsuleCollider capsuleCollider_;

	void Awake() 
	{
//		capsuleCollider_ = GetComponent<CapsuleCollider>();
	}

	void Update () 
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

	public void TakeDamage( int damage )
	{
		damaged_ = true;

		// Alive
		if( health_ > 0 )
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
			Death();
		}

	}

	void Death()
	{
		// is dead - true
//		capsuleCollider_.isTrigger = true;
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
