using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

public class RoomBase : MonoBehaviour
{
    public RoomData roomData;
    public Vector3Int mapLocation;

    private Vector3Int roomOrigin;    //Bottom-leftmost corner of room relative to world space.
    private List<RoomBase> neighbors;
    private int layoutWidth;
    private int layoutHeight;

    protected void Awake()
    {
        
    }

    // Start is called before the first frame update
    protected void Start()
    {
        
    }

    // Update is called once per frame
    protected void Update()
    {
        
    }

    //Set roomdata for this Room object; ideally random data from LevelGenerator.
    public void InitializeRoom(RoomData randData, Vector3Int originPosition, Vector3Int mapPosition)
    {
        this.roomData = randData;
        this.roomOrigin = originPosition;
        this.mapLocation = mapPosition;
    }

    public virtual void LoadRoom()
    {
        Texture2D layout = roomData.RoomLayout;
        if (roomData == null || roomData.RoomLayout == null)
        {
            Debug.Log("Error. No room texture found");
            return;
        }

        //TODO: use flood fill method for floors before filling in holes or
        //anything else that would replace a floor tile on its same tilemap.

        /* Check each tile in Texture2D layout from top to bottom for easier
           replacement of perspective tiles (ex. side view of wall) when scanning room 
        */
        for(int y = layout.height; y >= -1; y--)
        {
            for(int x = layout.width; x >= -1; x--)
            {
                //Check pre-conditions that are true for every room and are out of texture bounds.
                //Top two rows of map can only have a wall:
                if (y == layout.height || y < 0)
                {
                    SpawnWallBound(RoomUtils.BASE_WALL, x, y);
                    continue;
                }

                //Very left and right columns should always be the room's base wall.
                else if (x == layout.width || x < 0)
                {
                    SpawnWallBound(RoomUtils.BASE_WALL, x, y);
                    continue;
                }

                string colorHex = ColorUtility.ToHtmlStringRGBA(layout.GetPixel(x, y));
                string colorHexRGB = colorHex.Remove(colorHex.Length - 2);

                //Check pre-conditions that require reading in bounds of texture.
                //Ensure only walls are placed in tiles 1 and 2 below top room boundary!
                if (y >= layout.height - RoomUtils.BASE_WALL_HEIGHT)
                {
                    if (colorHexRGB == RoomUtils.BASE_WALL)
                        SpawnWallBound(colorHex, x, y);
                    else
                        Debug.Log("Attempted to spawn invalid object in top of room!");
                    continue;
                }

                //Placement is valid and in bounds; check base color and place tile accordingly:
                switch (colorHexRGB)
                {
                    case RoomUtils.BASE_WALL:
                        SpawnWallBound(colorHex, x, y);
                        Debug.Log(colorHex);
                        break;
                    case RoomUtils.BASE_FLOOR:
                        SpawnFloor(colorHex, x, y);
                        Debug.Log(colorHex);
                        break;
                    default:
                        Debug.Log("Unrecognized base tile color (RGB)!");
                        break;
                }
            }
        }
    }

    //Right now, only spawns base wall tiles. 
    public virtual void SpawnWallBound(string colorHex, int x, int y)
    {
        TileBase placeTile = null;
        TileBase wallTile = null; //may not spawn based on conditions.

        //Base wall value has full opacity (FF)
        if (colorHex.EndsWith("FF") || colorHex == RoomUtils.BASE_WALL)
        {
            if (roomData.DefaultBoundTiles != null)
                placeTile = roomData.DefaultBoundTiles;
            else
                placeTile = LevelGenerator.Instance.Floor.DefaultBoundTiles;
        }

        /*This is where other wall types MIGHT be checked later somehow.*/

        //Validate placement of wall relative to other walls by checking if above tile(s)
        //are perspective walls ( guarantees invalid placement and overrides any other conditions)
        Vector3Int up1TilePos = new Vector3Int(x, y + 1, mapLocation.z);
        Vector3Int up2TilePos = new Vector3Int(x, y + 2, mapLocation.z);
        TileBase up1Tile = LevelGenerator.Instance.WallsTilemap.GetTile(up1TilePos);
        TileBase up2Tile = LevelGenerator.Instance.WallsTilemap.GetTile(up2TilePos);

        if(up1Tile != null && up2Tile != null)
        {
            if (up1Tile == roomData.DefaultWallTiles || up1Tile == LevelGenerator.Instance.Floor.DefaultWallTiles ||
                up2Tile == roomData.DefaultWallTiles || up2Tile == LevelGenerator.Instance.Floor.DefaultWallTiles)
            {
                Debug.Log("Error. Invalid boundary placement (1-2 spaces between separate boundaries)");
                //Replace perspective walls with boundaries for valid room.
                wallTile = placeTile;
                LevelGenerator.Instance.WallsTilemap.SetTile(up1TilePos, wallTile);
                LevelGenerator.Instance.WallsTilemap.SetTile(up2TilePos, wallTile);
            }
        }

        if(y >= 1)
        {
            Color color1Down = roomData.RoomLayout.GetPixel(x, y - 1);
            Color color2Down = roomData.RoomLayout.GetPixel(x, y - 2);

            //If not bottom of room and the two spaces below are free, place perspective wall tiles.
            if (RoomUtils.IsSpaceEmpty(color1Down) && RoomUtils.IsSpaceEmpty(color2Down))
            {
                Debug.Log("Wall tiles should be placed now");

                //If wall tile not already determined, make default wall.
                //(may already be a boundary instead due to invalid placement).
                if(wallTile == null)
                    wallTile = roomData.DefaultWallTiles;
                if (wallTile == null)
                    wallTile = LevelGenerator.Instance.Floor.DefaultWallTiles;

                Vector3Int tile1Down = new Vector3Int(x, y - 1, mapLocation.z);
                Vector3Int tile2Down = new Vector3Int(x, y - 2, mapLocation.z);

                //Place two wall tiles below lowest wall bound!
                LevelGenerator.Instance.WallsTilemap.SetTile((roomOrigin + tile1Down), wallTile);
                LevelGenerator.Instance.WallsTilemap.SetTile((roomOrigin + tile2Down), wallTile);
            }
        }

        //Place determined rule tile at position (roomX, roomY, roomZ) relative to room origin,
        Vector3Int tilePos = new Vector3Int(x, y, mapLocation.z);
        LevelGenerator.Instance.WallsTilemap.SetTile((roomOrigin + tilePos), placeTile);
    }

    //Right now, only spawns base floor tiles.
    public virtual void SpawnFloor(string colorHex, int x, int y)
    {
        RuleTile placeTile = null;

        //Base floor value has full opacity (FF)
        if (colorHex.EndsWith("FF"))
        {
            if (roomData.DefaultFloorTiles != null)
                placeTile = roomData.DefaultFloorTiles;
            else
                placeTile = LevelGenerator.Instance.Floor.DefaultFloorTiles;
        }

        //This is where other floor types will be checked later somehow.

        //Place determined rule tile at position (roomX, roomY, roomZ) relative to room origin.
        Vector3Int tilePos = new Vector3Int(x, y, mapLocation.z);
        LevelGenerator.Instance.FloorTilemap.SetTile((roomOrigin + tilePos), placeTile);
    }
}
