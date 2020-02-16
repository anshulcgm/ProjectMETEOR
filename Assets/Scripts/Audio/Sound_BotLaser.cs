using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_BotLaser : MonoBehaviour
{

    public AudioClip BotLaser; // drag into audiosource in inspector

    public AudioSource source;
    public float laserHighPitch = 0.8f;
    public float laserLowPitch = 1.2f;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }
    private void PlayExplosion()
    {
        source.pitch = Random.Range(laserHighPitch, laserLowPitch); // randomize pitch within set range
        source.PlayOneShot(BotLaser, 1F); // play once at '1F' scaled volume
    }
}
