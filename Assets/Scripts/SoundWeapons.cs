using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundWeapons : MonoBehaviour {


    // Input the following code into scripts which detect when the activity is triggered

    public AudioClip PlayerLaserSound; // drag into audiosource in inspector
    public AudioClip PlayerExplosiveSound;

    public AudioSource source;
    public float laserHighPitch = 0.8f;
    public float laserLowPitch = 1.2f;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }
    private void PlayLaser()
    {
        source.pitch = Random.Range(laserHighPitch, laserLowPitch); // randomize pitch within set range
        source.PlayOneShot(PlayerLaserSound, 1F); // play once at '1F' scaled volume
    }
    private void PlayExplosive()
    {
        source.PlayOneShot(PlayerExplosiveSound, 1F);
    }
}
