using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MonoBehaviour
{
    private GameBoard board;
    private SpriteRenderer sr;
    private BoxCollider2D col;

    private void Start()
    {
        board = GetComponentInParent<GameBoard>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent < BoxCollider2D>();
    }

    private void Update()
    {
        sr.enabled = !board.isSimulating;
        col.enabled = !board.isSimulating;
    }

    private void OnMouseDown()
    {
        board.StartSimulation();
    }
}
