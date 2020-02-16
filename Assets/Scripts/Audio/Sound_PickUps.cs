using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_PickUps : MonoBehaviour {

    public AudioClip PickUpSound; // drag into audiosource in inspector
    // public AudioClip PickUpLaser; *will have 4 different pick up sounds
    // public AudioClip PickUpExplosive;
    // public AudioClip PickUpSmoke;
    // public AudioClip PickUpHealth();

    public AudioSource source;

    private void Start()
    { 
        source = GetComponent<AudioSource>();
    }
    private void PlayExplosion()
    {
        source.PlayOneShot(PickUpSound, 1F); // play once at '1F' scaled volume
    }
}
