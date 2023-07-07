using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject ThePlayer;
    void Start()
    {
        if (ThePlayer == null)
        {
            throw new UnityException("Player not set in camera");
        }
    }

    void Update()
    {
        transform.position = new Vector3(ThePlayer.transform.position.x,  ThePlayer.transform.position.y, transform.position.z);
    }
}
