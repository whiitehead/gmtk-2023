using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public List<AudioClip> clips;

    private AudioSource src;

    private void Start()
    {
        src = GetComponent<AudioSource>();
    }

    public void PlaySound(string name)
    {

        foreach (var clip in clips)
        {
            if (clip.name == name)
            {
                src.clip = clip;
                
                src.Play();
                return;
            }
        }

        Debug.LogError("NO CLIP NAMED: " + name);
    }
}
