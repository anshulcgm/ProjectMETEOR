using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySounds : MonoBehaviour {

    public AudioClip explosionSound; // drag into audiosource in inspector

    public AudioSource source;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }
    private void PlayExplosion()
    {
        source.PlayOneShot(explosionSound, 1F); // play once at '1F' scaled volume
    }
}
