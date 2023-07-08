using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ClickableTilemap : MonoBehaviour
{
    public GameBoard.TileType tileType;
    private GameBoard gameBoard;
    private Tilemap tilemap;
    private Camera cam;
    void Start()
    {
        gameBoard = GetComponentInParent<GameBoard>();
        tilemap = GetComponent<Tilemap>();
        cam = Camera.main;
        
        if (gameBoard == null)
        {
            Debug.LogError("Must have GameBoard as parent.");
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            var cellPos = tilemap.WorldToCell(mousePos);
            if (tilemap.HasTile(cellPos))
            {
                gameBoard.OnClick(tileType, tilemap, cellPos);
            }
        }
    }
}
