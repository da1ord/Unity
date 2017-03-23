using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SciFiRifle : MonoBehaviour 
{
	int clipBullets_ = 30;
	int maxClipBullets_ = 30;
	int totalBullets_ = 120;
	int maxBullets_ = 120;

	int damagePerShot_ = 10;
	float timeBetweenShots_ = 0.15f;
	float range_ = 60.0f;

	float noiseLevel_;

	float timer_ = 0.0f;
	float reloadTime_ = 1.7f;
	float effectsDisplayTime_ = 0.01f;

	bool canShoot_ = true;

	public AudioSource shootAudio_;
	public Light gunLight_;
	// Gun Animator instance
	Animator anim_;

	void Start()
	{
		noiseLevel_ = shootAudio_.minDistance;
		anim_ = GetComponentInParent<Animator>();
	}

	void Update()
	{
		timer_ -= Time.deltaTime;

		if( timer_ < timeBetweenShots_ - effectsDisplayTime_ )
		{
			DisableEffects();
		}

		// Enable shooting if timer elapsed and clip i snot empty
		if( timer_ <= 0.0f && clipBullets_ > 0 )
		{
			canShoot_ = true;
		}
	}

	public void Shoot()
	{
		timer_ = timeBetweenShots_;
		clipBullets_--;

		shootAudio_.Play();

		gunLight_.enabled = true;
		canShoot_ = false;
	}

	public bool Reload()
	{
        // Either shoot or reload timer is active -> don't allow reloading
        if( timer_ > 0 && canShoot_ == false )
        {
            return false;
        }
		anim_.SetTrigger( "Reload" );
		clipBullets_ = ( totalBullets_ < maxClipBullets_ ) ? totalBullets_ : maxClipBullets_;
		totalBullets_ -= clipBullets_;
		timer_ = reloadTime_;
		canShoot_ = false;

        return true;
	}

	public void DisableEffects()
	{
		gunLight_.enabled = false;
	}

	// Getters/setters
	public int GetClipBullets() 
	{
		return clipBullets_;
	}
	public void SetClipBullets( int clip_bullets ) 
	{
		clipBullets_ = clip_bullets;
	}
	public int GetTotalBullets() 
	{
		return totalBullets_;
	}
	public void SetTotalBullets( int total_bullets ) 
	{
		totalBullets_ = total_bullets;
	}
	public int GetMaxBullets() 
	{
		return maxBullets_;
	}
	public bool CanShoot() 
	{
		return canShoot_;
	}
	public float GetNoiseLevel() 
	{
		return noiseLevel_;
	}
	public int GetDamagePerShot() 
	{
		return damagePerShot_;
	}
	public float GetRange() 
	{
		return range_;
	}
	public void AddClip() 
	{
		totalBullets_ += maxClipBullets_;
		if( totalBullets_ > maxBullets_ )
		{
			totalBullets_ = maxBullets_;
		}
	}
}
