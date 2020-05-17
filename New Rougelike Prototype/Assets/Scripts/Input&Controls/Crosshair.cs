using System;
using System.Collections;
using UnityEngine;

/**
 * This class handles the sprites and effects of the crosshair of the player,
 * which is only active in normal top-down play. Position and movement are
 * handled in PlayerControl.
 */
public class Crosshair : MonoBehaviour
{
    
    [SerializeField] private SpriteRenderer crosshairSprite;
    [SerializeField] private Vector2 mousePosition;

    private void Awake()
    {
        
    }

    private void Update()
    {
        
    }
}