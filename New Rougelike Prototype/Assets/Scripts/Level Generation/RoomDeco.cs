using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Room Visuals")]
public class RoomDeco : ScriptableObject
{
    [SerializeField] private RuleTile wallTile;
    [SerializeField] private RuleTile floorTile;
    [SerializeField] private RuleTile boundTile;
    [SerializeField] private Sprite[] specialDoorSprites;

    public RuleTile WallTile { get => wallTile; }
    public RuleTile FloorTile { get => floorTile; }
    public RuleTile BoundTile { get => boundTile; }
    public Sprite[] SpecialDoorSprites { get => specialDoorSprites; }
}
