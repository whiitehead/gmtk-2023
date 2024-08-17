using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public bool isMuted;
    public List<AudioClip> clips;

    private AudioSource src;

    private void Start()
    {
        src = GetComponent<AudioSource>();
    }

    public void PlaySound(string soundName)
    {
        if (isMuted)
        {
            return;
        }
        
        foreach (var clip in clips)
        {
            if (clip.name == soundName)
            {
                src.clip = clip;
                    
                src.Play();
                return;
            }
        }

        Debug.LogError("NO CLIP NAMED: " + soundName);
    }
}
