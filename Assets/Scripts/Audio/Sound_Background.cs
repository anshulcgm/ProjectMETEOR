using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_Background : MonoBehaviour {

    // Set Audio source to a transform ~1m behind camera

    // Soundtrack
    public AudioClip intro; // beginning of soundtrack, set in inspector
    public AudioClip looped; // looped section of soundtrack, set in inspector

    // Launch sound

    public AudioClip Launch;
    // PickUp
    public AudioClip PickUpSound;


    // Weapons
    public AudioClip PlayerLaserSound; // drag into audiosource in inspector
    public AudioClip PlayerExplosiveSound;
    public float laserHighPitch = 1.2f;
    public float laserLowPitch = 0.8f;

    // Bots
    // Gunner
    public AudioClip GunnerLaser; // drag in inspector

    public float GunnerLaserHighPitch = 1.2f;
    public float GunnerLaserLowPitch = 0.8f;
    // Sniper
    public AudioClip SniperLaser;

    // Bot Collide
    public AudioClip BotCollide;

    // SpinnerWhirr
    public AudioClip SpinnerWhirr;

    // Seeker
    public AudioClip SeekerFound;

    // Booster
    public AudioClip Booster;

    // Idle
    public AudioClip Idle1;
    public AudioClip Idle2;

    public float IdleHighPitch = 0.5f;
    public float IdleLowPitch = 1.5f;
    
    // Explosion
    public AudioClip explosionSound; // drag in inspector

    private AudioSource source;

    public void Start()
    {
        source = GetComponent<AudioSource>(); // <-- can alternatively drag and drop audiosource
    }

    // Soundtrack
    public void playSoundtrack()
    {
         
        source.loop = true;
        source.clip = looped;
        source.Play();
    }
    public void playIntro()
    {
        source.clip = intro;
        source.Play();
    }
    public void stopSoundtrack()
    {
        source.Stop();
    }

    // Launch
    public void playLaunch()
    {
        source.PlayOneShot(Launch, 1f);
    }
    
    // Pick Ups
    public void PlayPickUp()
    {
        source.PlayOneShot(PickUpSound, 1F); // play once at '1F' scaled volume
    }

    // Weapons
    public void PlayLaser()
    {
        source.pitch = Random.Range(laserHighPitch, laserLowPitch); // randomize pitch within set range
        source.PlayOneShot(PlayerLaserSound, 1F); // play once at '1F' scaled volume
        source.pitch = 1F;
    }
    public void PlayExplosive()
    {
        source.PlayOneShot(PlayerExplosiveSound, 1F);
    }

    // GunnerLaser
    public void PlayGunnerLaser()
    {
        source.pitch = Random.Range(GunnerLaserHighPitch, GunnerLaserLowPitch); // randomize pitch within set range
        source.PlayOneShot(GunnerLaser, 1F); // play once at '1F' scaled volume
        source.pitch = 1F;
    }
    // SniperLaser
    public void PlaySniperLaser()
    {
        source.PlayOneShot(SniperLaser, 1F);
    }

    // BotCollide
    public void PlayBotCollide()
    {
        source.PlayOneShot(BotCollide, 1F);
    }

    // SpinnerWhirr
    public void PlaySpinnerWhirr()
    {
        source.PlayOneShot(SpinnerWhirr, 1F);
    }

    // SeekerFound
    public void PlaySeekerFound()
    {
        source.PlayOneShot(SeekerFound, 1F);
    }

    // Booster
    public void PlayBooster()
    {
        source.PlayOneShot(Booster, 1F);
    }

    // Idle
    public void PlayIdle()
    {
        source.pitch = Random.Range(IdleHighPitch, IdleLowPitch); // randomize pitch within set range
        int oneOrTwo = 1;
        // 1 or 2
        if(oneOrTwo == 1)
        {
            source.PlayOneShot(Idle1);
        } else
        {
            source.PlayOneShot(Idle2);
        }
        source.pitch = 1F;
    }

    // Explosion
    public void PlayExplosion()
    {
        source.PlayOneShot(explosionSound, 1F); // play once at '1F' scaled volume
    }
}
