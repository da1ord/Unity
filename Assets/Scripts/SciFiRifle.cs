using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SciFiRifle : MonoBehaviour
{
    // Gun shoot sound
    public AudioClip shootClip_;
    // Gun reload sound
    public AudioClip reloadClip_;
    // Gun light effect
    public Light gunLight_;


    // Current clip bullets ammount
    int clipBullets_ = 30;
    // Initial bullets to reload amount
    int totalBullets_ = 120;
    // Maximum clip bullets amount
    const int maxClipBullets_ = 30;
    // Maximum bullets amount you can carry
    const int maxBullets_ = 120;

    // Gun damage per shot
	const int damagePerShot_ = 10;
    // Gun shooting speed - minimal time between shots
	const float timeBetweenShots_ = 0.15f;
    // Gun shoot range
	const float range_ = 60.0f;

    // Gun noise level
	float noiseLevel_;

    // Timer for handling reload and shooting speed
	float timer_ = 0.0f;
    // Gun time to reload
	const float reloadTime_ = 1.7f;
    // Gun effects display time
	const float effectsDisplayTime_ = 0.01f;

    // Flag telling if it is possible to shoot now
	bool canShoot_ = true;

    // Gun audio source
    AudioSource gunAudio_;
	// Gun animator instance (for reloading)
	Animator anim_;
    // Muzzle flash particle system
    ParticleSystem muzzleFlash_;

    // Init function
    void Awake()
	{
        // Get animator component
		anim_ = GetComponentInParent<Animator>();
        // Get audio source component
        gunAudio_ = GetComponent<AudioSource>();
        // Set audio sound to shoot sound
        gunAudio_.clip = shootClip_;
        // Set noise level according to audio source min distance
        noiseLevel_ = gunAudio_.minDistance;
        // Get muzzle flash particle system component
        muzzleFlash_ = GetComponentInChildren<ParticleSystem>();
    }

    // Update function
    void Update()
	{
        // Decrease timer
		timer_ -= Time.deltaTime;

        // Disable effects if effect display time has elapsed
		if( timer_ < timeBetweenShots_ - effectsDisplayTime_ )
		{
			DisableEffects();
		}

		// Enable shooting if timer has elapsed and clip is not empty
		if( timer_ <= 0.0f && clipBullets_ > 0 )
		{
			canShoot_ = true;
		}
	}

    // Shoot function
	public void Shoot()
	{
        // Set timer to time between shots
		timer_ = timeBetweenShots_;
        // Decrease current clip bullets amount
		clipBullets_--;

        // Set audio source sound to shoot sound and play it
        gunAudio_.clip = shootClip_;
		gunAudio_.Play();

        // Animate muzzle flash particle system
        muzzleFlash_.Play();

        // Enable gun light
		gunLight_.enabled = true;
        // Disable possibility to shoot
		canShoot_ = false;
	}

    // Reloads the gun. 
    // @return true if reload was successful (for aim spread reset)
    public bool Reload()
	{
        // No bullets for reloading
        if( totalBullets_ == 0 )
        {
            return false;
        }
        // Either shoot or reload timer is active -> don't allow reloading
        if( timer_ > 0 /*&& canShoot_ == false*/ )
        {
            return false;
        }

        // Start reload animation
		anim_.SetTrigger( "Reload" );

        // Set audio source sound to reload sound and play it
        gunAudio_.clip = reloadClip_;
        gunAudio_.Play();

        // Set current clip bullets amount
        clipBullets_ = ( totalBullets_ < maxClipBullets_ ) ? totalBullets_ : maxClipBullets_;
        // Decrease total bullets by clip bullets amount
		totalBullets_ -= clipBullets_;
        // Set reload timer
		timer_ = reloadTime_;
        // Disable shooting
		canShoot_ = false;

        return true;
	}

    // Disable gun effects (gun light)
	public void DisableEffects()
	{
		gunLight_.enabled = false;
	}

    //// Getters/setters
    // Get clip bullets ammount
    public int GetClipBullets() 
	{
		return clipBullets_;
    }
    // Set clip bullets ammount
    public void SetClipBullets( int clip_bullets ) 
	{
		clipBullets_ = clip_bullets;
    }
    // Get initial bullets to reload amount
    public int GetTotalBullets() 
	{
		return totalBullets_;
    }
    // Set initial bullets to reload amount
    public void SetTotalBullets( int total_bullets ) 
	{
		totalBullets_ = total_bullets;
    }
    // Get maximum clip bullets amount
    public int GetMaxBullets() 
	{
		return maxBullets_;
    }
    // Get flag if it is possible to shoot now
    public bool CanShoot() 
	{
		return canShoot_;
    }
    // Get gun noise level
    public float GetNoiseLevel() 
	{
		return noiseLevel_;
    }
    // Get gun damage per shot
    public int GetDamagePerShot() 
	{
		return damagePerShot_;
    }
    // Get gun range
    public float GetRange() 
	{
		return range_;
	}
    // Add a clip after picking it up
	public void AddClip() 
	{
        // Add clip bullets amount to total bullets and cap it in case it is 
        //  more than maximum bullets amount
		totalBullets_ += maxClipBullets_;
		if( totalBullets_ > maxBullets_ )
		{
			totalBullets_ = maxBullets_;
		}
	}
}
