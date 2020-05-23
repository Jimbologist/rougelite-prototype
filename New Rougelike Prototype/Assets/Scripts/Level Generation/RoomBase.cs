using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System;

//State of room in terms of player knowledge (visitation, items, etc.)
public enum RoomState
{
    Invisible = 0,  //Distant from player; not neighboring found rooms
    Nearby = 1,     //Not visited, but is neighbor of found room
    Visited = 2,    //Player has been inside room, but not currently in it
    Current = 3     //Player is currently in this room
}

public class RoomBase : MonoBehaviour
{
    public RoomData roomData;
    public List<RoomBase> neighbors;
    public Vector3Int location;
    public bool isDeadEnd;

    private Vector3Int roomOrigin;    //Bottom-leftmost corner of room relative to world space.
    private RoomState roomState;
    private int roomWidth;
    private int roomHeight;

    public RoomState RoomState { get { return roomState; } }

    protected void Awake()
    {
        neighbors = new List<RoomBase>();
        isDeadEnd = false;
        roomState = RoomState.Invisible;
    }

    // Start is called before the first frame update
    protected void Start()
    {
        
    }

    // Update is called once per frame
    protected void Update()
    {
        
    }

    //Initialize location of room on map at start of level; should only
    // be done before roomData is determined already.
    // Note: make a ChangeLocation() method if location needs to change for whatever reason
    // that accounts for the current roomData.
    public virtual void InitLocation(Vector3Int mapPosition)
    {
        if (roomData != null)
            return;
        this.location = mapPosition;

        //Set room origin based on map location.
        //ONLY ACCOUNTS FOR ROOMS OF THE BASE SIZE!!!
        int offsetX = Mathf.Abs(RoomUtils.DOOR_OFFSET * location.x);
        int offsetY = Mathf.Abs(RoomUtils.DOOR_OFFSET * location.y);

        int originX = (location.x * RoomUtils.BASE_ROOM_X);
        if(originX != 0)
            originX += (originX > 0) ? offsetX : -offsetX;

        int originY = location.y * RoomUtils.BASE_ROOM_Y;
        if(originY != 0)
            originY += (originY > 0) ? offsetY : -offsetY;

        this.roomOrigin = new Vector3Int(originX, originY, location.z);
        this.gameObject.transform.position = this.roomOrigin;
    }

    public void SetRoomData(RoomData roomData)
    {
        this.roomData = roomData;
        roomWidth = roomData.RoomLayout.width;
        roomHeight = roomData.RoomLayout.height;
    }

    public virtual void UpdateNeighbors()
    {
        //Add new neighbors to rooms' lists
        // This should work for any size/type of room using direction, as long as
        // different sized rooms are stored multiple times for each tile the occupy!!!
        // ^^^^^ this is like, super important to keep in mind!!!
        RoomBase value = null;
        Dictionary<Vector3Int, RoomBase> map = LevelGenerator.Instance.LevelMap;
        if (map.TryGetValue(location + Vector3Int.up, out value))
        {
            if (value != this)
            {
                this.neighbors.Add(value);
                value.neighbors.Add(this);
            }
        }
        if (map.TryGetValue(location + Vector3Int.down, out value))
        {
            if (value != this)
            {
                this.neighbors.Add(value);
                value.neighbors.Add(this);
            }
        }
        if (map.TryGetValue(location + Vector3Int.left, out value))
        {
            if (value != this)
            {
                this.neighbors.Add(value);
                value.neighbors.Add(this);
            }
        }
        if (map.TryGetValue(location + Vector3Int.right, out value))
        {
            if (value != this)
            {
                this.neighbors.Add(value);
                value.neighbors.Add(this);
            }
        }
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

                //Now get color string; we know we need it now that we know we're in texture bounds
                string colorHex = ColorUtility.ToHtmlStringRGBA(layout.GetPixel(x, y));
                string colorHexRGB = colorHex.Remove(colorHex.Length - 2);

                //Check pre-conditions that require reading in bounds of texture.
                //Ensure only walls are placed in tiles 1 and 2 below top room boundary!
                if (y >= layout.height - RoomUtils.WALL_HEIGHT)
                {
                    if (colorHexRGB == RoomUtils.BASE_WALL)
                    {
                        SpawnWallBound(colorHex, x, y);
                    }
                    else
                    {
                        if (colorHexRGB != RoomUtils.BASE_FLOOR)
                            Debug.Log("Attempted to spawn invalid object in top of room!");
                    }
                    continue;
                }

                //Placement is valid and in bounds; check base color and place tile accordingly:
                //TODO: Make this switch statement into its own function! It will eventually become very complex.
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

        /*Validate placement of wall relative to other walls by checking if above tile(s)
        //are perspective walls ( guarantees invalid placement and overrides any other conditions)
        Vector3Int up1TilePos = new Vector3Int(x, y + 1, location.z);
        Vector3Int up2TilePos = new Vector3Int(x, y + 2, laocation.z);
        TileBase up1Tile = LevelGenerator.Instance.WallsTilemap.GetTile(up1TilePos);
        TileBase up2Tile = LevelGenerator.Instance.WallsTilemap.GetTile(up2TilePos);

        if(up1Tile != null && up2Tile != null)
        {
            if (up1Tile == roomData.DefaultWallTiles || up1Tile == LevelGenerator.Instance.Floor.DefaultWallTiles ||
                up2Tile == roomData.DefaultWallTiles || up2Tile == LevelGenerator.Instance.Floor.DefaultWallTiles)
            {
                Debug.Log("Error. Invalid boundary placement (1-2 spaces between separate boundaries)");
                //Replace perspective walls with boundaries for valid room.
                LevelGenerator.Instance.WallsTilemap.SetTile(up1TilePos, placeTile);
                LevelGenerator.Instance.WallsTilemap.SetTile(up2TilePos, placeTile);
            }
        }
        */
        if(y > 1)
        {
            Color color1Down = roomData.RoomLayout.GetPixel(x, y - 1);
            Color color2Down = roomData.RoomLayout.GetPixel(x, y - 2);

            //If not bottom of room and the two spaces below are free, place perspective wall tiles.
            if (RoomUtils.IsSpaceEmpty(color1Down) && RoomUtils.IsSpaceEmpty(color2Down))
            {
                //If wall tile not already determined, make default wall.
                //(may already be a boundary instead due to invalid placement).
                if(wallTile == null)
                    wallTile = roomData.DefaultWallTiles;
                if (wallTile == null)
                    wallTile = LevelGenerator.Instance.Floor.DefaultWallTiles;

                Vector3Int tile1Down = new Vector3Int(x, y - 1, location.z);
                Vector3Int tile2Down = new Vector3Int(x, y - 2, location.z);

                //Place two wall tiles below lowest wall bound!
                LevelGenerator.Instance.WallsTilemap.SetTile((roomOrigin + tile1Down), wallTile);
                LevelGenerator.Instance.WallsTilemap.SetTile((roomOrigin + tile2Down), wallTile);
            }
        }

        //Place determined rule tile at position (roomX, roomY, roomZ) relative to room origin,
        Vector3Int tilePos = new Vector3Int(x, y, location.z);
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
        Vector3Int tilePos = new Vector3Int(x, y, location.z);
        LevelGenerator.Instance.FloorTilemap.SetTile((roomOrigin + tilePos), placeTile);
    }
}
