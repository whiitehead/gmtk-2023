using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Scaffoldinger : MonoBehaviour
{
    private AudioSource src;
    
    void Start()
    {
        src = GetComponent<AudioSource>();
        DontDestroyOnLoad(gameObject);
    }
    
    private void Awake()
    {
        src.Play();
    }
    
}
