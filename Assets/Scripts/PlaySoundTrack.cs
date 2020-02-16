using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundTrack : MonoBehaviour {
    private Sound_Background sound;
    private void Start()
    {
        sound = Camera.main.GetComponent<Sound_Background>();
        sound.playSoundtrack();
    }
}
