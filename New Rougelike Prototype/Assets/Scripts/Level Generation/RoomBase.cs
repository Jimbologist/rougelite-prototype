using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Collections;
using System;

//State of room in terms of player knowledge (visitation, items, etc.)
public enum RoomState
{
    Invisible = 0,  //Distant from player; not neighboring found rooms
    Nearby = 1,     //Not visited, but is neighbor of found room
    Visited = 2,    //Player has been inside room, but not currently in it
    Current = 3     //Player is currently in this room
}

//Main class for all standard rooms in the game. Important data
// includes neighbors, doors, location, occupied sectors, and 
// Grid2D data that shows all walkable spaces that AI can use.
public class RoomBase : MonoBehaviour
{
    public List<Vector3Int> neighbors = new List<Vector3Int>();
    public List<DoorBase> doors = new List<DoorBase>();
    public Vector3Int initialLocation;
    public bool isPosFinal;
    public bool isDeadEnd;
    public bool isLoaded;

    private RoomData roomData;
    private Vector3Int roomOrigin;    //Bottom-leftmost corner of room relative to world space.
    private Vector3Int[,] roomSectors;
    private PathGrid<GameObject> roomGrid;
    private RoomState roomState;
    private int roomWidth;
    private int roomHeight;

    //Kinda dumb, but these two tilemaps should be all that's needed.
    //Other objects or types of obstacles will just be GameObjects or be
    //on the floorTilemap with added collision.
    protected List<Vector3Int> floorPosList = new List<Vector3Int>();
    protected List<TileBase> floorTileList = new List<TileBase>();
    protected List<Vector3Int> wallPosList = new List<Vector3Int>();
    protected List<TileBase> wallTileList = new List<TileBase>();

    public RoomState RoomState { get { return roomState; } }
    public Vector3Int RoomCenter { get => new Vector3Int(roomHeight / 2, roomWidth / 2, 0) + roomOrigin; }
    public Vector3Int RoomOrigin { get => roomOrigin; }
    public RoomData Data { get => roomData; }
    public Vector3Int[,] RoomSectors { get => roomSectors; }    

    //If room contains more than one sector, it is a large room and will
    //re-place pre-placed normal sized rooms.
    public bool IsLargeRoom { get => RoomSectors.Length > 1; }

    protected void Awake()
    {
        isPosFinal = false;
        isDeadEnd = false;
        isLoaded = false;
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
        if (Data != null)
            return;
        this.initialLocation = mapPosition;

        //Set room origin based on map location.
        //ONLY ACCOUNTS FOR ROOMS OF THE BASE SIZE!!!
        int offsetX = Mathf.Abs(RoomUtils.DOOR_OFFSET * initialLocation.x);
        int offsetY = Mathf.Abs(RoomUtils.DOOR_OFFSET * initialLocation.y);

        int originX = (initialLocation.x * RoomUtils.BASE_ROOM_X);
        if (originX != 0)
            originX += (originX > 0) ? offsetX : -offsetX;

        int originY = initialLocation.y * RoomUtils.BASE_ROOM_Y;
        if (originY != 0)
            originY += (originY > 0) ? offsetY : -offsetY;

        this.roomOrigin = new Vector3Int(originX, originY, initialLocation.z);
        this.gameObject.transform.position = this.roomOrigin;
        isPosFinal = true;
    }

    public void SetRoomData(RoomData roomData)
    {
        this.roomData = roomData;
        roomWidth = roomData.Layout.width;
        roomHeight = roomData.Layout.height;

        roomSectors = new Vector3Int[roomData.NumSectorsX, roomData.NumSectorsY];
        for(int x = 0; x < roomData.NumSectorsX; x++)
        {
            for(int y = 0; y < roomData.NumSectorsY; y++)
            {
                roomSectors[x, y] = initialLocation + new Vector3Int(x, y, initialLocation.z);
            }
        }
    }

    public virtual void UpdateNeighbors()
    {
        //Add new neighbors to rooms' lists
        // This should work for any size/type of room using direction, as long as
        // different sized rooms are stored multiple times for each tile the occupy!!!
        // ^^^^^ this is like, super important to keep in mind!!!
        RoomBase value = null;
        Dictionary<Vector3Int, RoomBase> map = LevelGenerator.Instance.LevelMap;
        if (map.TryGetValue(initialLocation + Vector3Int.up, out value))
        {
            if (value != this)
            {
                this.neighbors.Add(initialLocation + Vector3Int.up);
                value.neighbors.Add(initialLocation);
            }
        }
        if (map.TryGetValue(initialLocation + Vector3Int.down, out value))
        {
            if (value != this)
            {
                this.neighbors.Add(initialLocation + Vector3Int.down);
                value.neighbors.Add(initialLocation);
            }
        }
        if (map.TryGetValue(initialLocation + Vector3Int.left, out value))
        {
            if (value != this)
            {
                this.neighbors.Add(initialLocation + Vector3Int.left);
                value.neighbors.Add(initialLocation);
            }
        }
        if (map.TryGetValue(initialLocation + Vector3Int.right, out value))
        {
            if (value != this)
            {
                this.neighbors.Add(initialLocation + Vector3Int.right);
                value.neighbors.Add(initialLocation);
            }
        }
    }

    public virtual void LoadRoom()
    {
        if (isLoaded)
            return;
        Texture2D layout = roomData.Layout;
        if (roomData == null || roomData.Layout == null)
        {
            Debug.Log("Error. No room texture found");
            return;
        }

        //TODO: use flood fill method for floors before filling in holes or
        //anything else that would replace a floor tile on its same tilemap.

        ReadRoom(layout);
        LevelGenerator.Instance.WallsTilemap.SetTiles(wallPosList.ToArray(), wallTileList.ToArray());
        LevelGenerator.Instance.FloorTilemap.SetTiles(floorPosList.ToArray(), floorTileList.ToArray());

        //Remove unneeded tiles from where doors are placed.
        foreach (var door in doors)
        {
            List<Vector3Int> tempDoorPos = door.GetOccupiedSpaces();
            TileBase[] emptyTiles = new TileBase[tempDoorPos.Count];
            LevelGenerator.Instance.WallsTilemap.SetTiles(tempDoorPos.ToArray(), emptyTiles);
        }

        isLoaded = true;
    }

    private void ReadRoom(Texture2D layout)
    {
        /* Check each tile in Texture2D layout from top to bottom for easier
           replacement of perspective tiles (ex. side view of wall) when scanning room 
        */
        for (int y = layout.height; y >= -1; y--)
        {
            for (int x = layout.width; x >= -1; x--)
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
                //NOTE: If needed for performance, make each "spawn" method return a tilebase, then store 
                // all tiles and positions in arrays, then finally use Tilemap.SetTiles with those arrays.
                // also, for perspective walls, add perspecive walls into correct index of TileBase array. Because
                // of this, check if index is null EACH time you add a tile to the array.
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
        RuleTile wallTile = null;

        //Base wall value has full opacity (FF)
        if (colorHex.EndsWith("FF") || colorHex == RoomUtils.BASE_WALL)
        {
            wallTileList.Add(LevelGenerator.Instance.Floor.BoundTiles[0]);
            wallPosList.Add((new Vector3Int(x, y, 0)) + roomOrigin);
        }


        /*This is where other wall types MIGHT be checked later somehow.*/

        //Add floors under wall in case it is removed!
        floorTileList.Add(LevelGenerator.Instance.Floor.FloorTiles[0]);
        floorPosList.Add(new Vector3Int(x, y, 0) + roomOrigin);

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
        if (y > 1)
        {
            Color color1Down = roomData.Layout.GetPixel(x, y - 1);
            Color color2Down = roomData.Layout.GetPixel(x, y - 2);

            //If not bottom of room and the two spaces below are free, place perspective wall tiles.
            if (RoomUtils.IsSpaceFree(color1Down) && RoomUtils.IsSpaceFree(color2Down))
            {
                //If wall tile not already determined, make default wall.
                //(may already be a boundary instead due to invalid placement).
                if (wallTile == null)
                    wallTile = LevelGenerator.Instance.Floor.WallTiles[0];

                Vector3Int tile1Down = new Vector3Int(x, y - 1, initialLocation.z);
                Vector3Int tile2Down = new Vector3Int(x, y - 2, initialLocation.z);

                //Place two wall tiles below lowest wall bound!
                wallTileList.Add(wallTile);
                wallPosList.Add(roomOrigin + tile1Down);
                wallTileList.Add(wallTile);
                wallPosList.Add(roomOrigin + tile2Down);

                //Place floor tiles under walls; hidden but there if wall is removed!
                floorTileList.Add(LevelGenerator.Instance.Floor.FloorTiles[0]);
                floorPosList.Add(roomOrigin + tile1Down);
                floorTileList.Add(LevelGenerator.Instance.Floor.FloorTiles[0]);
                floorPosList.Add(roomOrigin + tile2Down);
            }
        }
        //Place determined rule tile at position (roomX, roomY, roomZ) relative to room origin,
        //Vector3Int tilePos = new Vector3Int(x, y, location.z);
        //LevelGenerator.Instance.WallsTilemap.SetTile((roomOrigin + tilePos), placeTile);
    }

    //Right now, only spawns base floor tiles.
    public virtual void SpawnFloor(string colorHex, int x, int y)
    {
        //Base floor value has full opacity (FF)
        if (colorHex.EndsWith("FF"))
        {
            floorTileList.Add(LevelGenerator.Instance.Floor.FloorTiles[0]);
            floorPosList.Add(new Vector3Int(x, y, 0) + roomOrigin);
        }

        //This is where other floor types will be checked later somehow.
    }

    //Get center of 1x1 room area. Big rooms will have multiple sectors
    // that make up its area, so pass in the map position of a sector,
    // and the function will return the center Vector3Int position of this sector
    // of a room in world space. (ex. pass in (1,1,1), returns -> (44, 26, 1))
    public static Vector3Int GetSectorCenter (Vector3Int mapPosition)
    {
        mapPosition.x = (mapPosition.x * (RoomUtils.BASE_ROOM_X + RoomUtils.DOOR_OFFSET));
        mapPosition.x += (RoomUtils.BASE_ROOM_X / 2);

        mapPosition.y = (mapPosition.y * (RoomUtils.BASE_ROOM_Y + RoomUtils.DOOR_OFFSET));
        mapPosition.y += (RoomUtils.BASE_ROOM_Y / 2);
        return mapPosition;
    }

    //Check if path can be created to the two doors provided in the room.
    public virtual bool CanPathFindToDoors()
    {
        Tilemap test = GameObject.Find("Test PathFinding").GetComponent<Tilemap>();
        //Check for a path from an aribitrary first door to each other door.
        DoorBase firstDoor = null;
        Vector2Int[] firstDoorPos = null;
        Vector2Int[] checkDoorPos = null;
        foreach (var door in doors)
        {
            //Set arbitrary first door.
            if(firstDoor == null)
            {
                firstDoor = door;
                firstDoorPos = firstDoor.GetLayoutDoorPos(this);
                continue;
            }
            checkDoorPos = door.GetLayoutDoorPos(this);
            /* For normal, 1x1 rooms, pathfind to each door needed from current room.
            Do something else for large rooms/other types.
            Large rooms will have array of locations, so use that
            and read Texture2D at relative location of sector, using
            bottom-leftmost sector as (0,0)!!!! */

            //Make a new grid of same size as current room layout.
            PathGrid<Color> layoutGrid = new PathGrid<Color>(new Vector2(roomData.Layout.width, roomData.Layout.height));
            layoutGrid.MakeNewGridData(Vector2.zero); //use 0,0 so layout positions are accurate.

            //Set walkable states for nodes
            for(int x = 0; x < roomData.Layout.width; x++)
            {
                for(int y = 0; y < roomData.Layout.height; y++)
                {
                    layoutGrid.GetNodeDirect(x, y).walkable = RoomUtils.IsSpaceFree(roomData.Layout.GetPixel(x, y));
                    /*if(layoutGrid.GetNodeDirect(x, y).walkable)
                        test.SetTile(new Vector3Int(x, y, 0), LevelGenerator.Instance.Floor.FloorTiles[0]);
                    else
                        test.SetTile(new Vector3Int(x, y, 0), LevelGenerator.Instance.Floor.BoundTiles[0]);
                        */
                    //test
                    if (layoutGrid.GetNodeDirect(x, y).walkable != RoomUtils.IsSpaceFree(roomData.Layout.GetPixel(x, y)))
                        Debug.Log("Graph nodes do not align with image nodes");
                }
            }


            //Now do the actual pathfinding on both checkDoorPos.
            foreach(var startPos in firstDoorPos)
            {
                foreach(var endPos in checkDoorPos)
                {
                    //test.SetTile(new Vector3Int(startPos.x, startPos.y, 0), null);
                    //test.SetTile(new Vector3Int(endPos.x, endPos.y, 0), null);
                    if (layoutGrid.FindPathAStar(startPos, endPos) == null)
                        return false;
                    /*else
                        foreach(var node in layoutGrid.grid)
                        {
                            if(list.Contains(node))
                            {
                                test.SetTile(new Vector3Int(node.gridX, node.gridY, 0), null);
                            }
                        }
                        */
                }
            }
        }
        return true;
    }

    public virtual bool SpaceNearDoorWalkable(int index)
    {
        Vector2Int[] doorPos = doors[index].GetLayoutDoorPos(this);

        foreach (var pos in doorPos)
        {
            if (!RoomUtils.IsSpaceFree(roomData.Layout.GetPixel(pos.x, pos.y)))
                return false;
        }

        Debug.Log("Space is walkable!");
        return true;
    }
}
