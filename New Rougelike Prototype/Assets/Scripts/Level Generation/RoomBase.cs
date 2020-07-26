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
}

//Main class for all standard rooms in the game. Important data
// includes neighbors, doors, location, occupied sectors, and 
// Grid2D data that shows all walkable spaces that AI can use.
public class RoomBase : MonoBehaviour
{
    public Vector3Int initialLocation;
    public bool isPosFinal;
    public bool isLoaded;
    public bool isCleared;
    public bool isLarge = false; //TODO: REMOVE THIS AFTER USING IT TO DEBUG!!
    public bool flaggedToDestroy = false;

    [SerializeField] private List<Vector3Int> neighbors = new List<Vector3Int>();
    [SerializeField] private List<DoorBase> doors = new List<DoorBase>();
    [SerializeField] private Vector3Int roomOrigin;    //Bottom-leftmost corner of room relative to world space.
    [SerializeField] private RoomState roomState;
    [SerializeField] private List<Vector3Int> serRoomSectors = new List<Vector3Int>();

    private PathGrid<GameObject> roomGrid;
    private RoomData roomData;
    private int roomWidth;
    private int roomHeight;

    protected Vector3Int[,] roomSectors;
    protected List<Vector3Int> sectorsAdded = new List<Vector3Int>();
    protected List<RoomBase> roomsRemoved = new List<RoomBase>();
    protected List<DoorBase> possibleDoors = new List<DoorBase>();
    protected List<DoorBase> removedDoors = new List<DoorBase>();

    //Kinda dumb, but these two tilemaps should be all that's needed.
    //Other objects or types of obstacles will just be GameObjects or be
    //on the floorTilemap with added collision.
    protected List<Vector3Int> floorPosList = new List<Vector3Int>();
    protected List<TileBase> floorTileList = new List<TileBase>();
    protected List<Vector3Int> wallPosList = new List<Vector3Int>();
    protected List<TileBase> wallTileList = new List<TileBase>();

    #region "Initializers and Getters"

    public Vector3Int RoomCenter { get => new Vector3Int(roomHeight / 2, roomWidth / 2, 0) + roomOrigin; }
    public Vector3Int RoomOrigin
    {
        get => roomOrigin;
        set
        {
            roomOrigin = value;
            gameObject.transform.position = roomOrigin;
        }
    }
    public RoomData Data { get => roomData; }
    public Vector3Int[,] RoomSectors { get => roomSectors; }    
    public List<Vector3Int> RoomSectorsToList
    {
        get
        {
            List<Vector3Int> sectorList = new List<Vector3Int>();
            foreach(var sector in roomSectors)
            {
                sectorList.Add(sector);
            }
            return sectorList;
        }
    }
    public List<DoorBase> Doors { get => doors; }
    public List<Vector3Int> Neighbors { get => neighbors; }
    public bool IsDeadEnd { get => (Doors.Count < 2 || Neighbors.Count < 2); }
    public bool IsLargeRoom { get => RoomSectors.Length > 1; }
    public RoomState RoomState
    {
        get { return roomState; }
        set { roomState = value; }
              //Call map ui update event?
    }

    //used just to set list ver. of roomSectors for debugging.
    private void SetSectorsList()
    {
        serRoomSectors.Clear();
        foreach(var sector in roomSectors)
        {
            serRoomSectors.Add(sector);
        }
    }

    #endregion

    #region "Events and Basic Unity Methods"

    public delegate void RoomCleared();
    public event RoomCleared OnRoomCleared;

    public GameObject clearEventObj;

    protected void Awake()
    {
        //Subscribe events.
        this.OnRoomCleared += ClearedRoom;

        //Initialize state.
        isPosFinal = false;
        isLoaded = false;
        isCleared = true;
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

    //Just allows invoking this event outside this script,
    //TODO: Get rid of this shit and add functionality to call this
    //when all enemies/objectives are dead/complete!!!
    public void TestRoomClear()
    {
        OnRoomCleared();
    }


    //TODO: Probably have separate methods for different actions that could happen
    // based on activity of room, including: disabling room for performance, closing or opening doors,
    // disabling the pathfinding grid once room is cleared, overall methods that do all disabling/enabling, etc.

    //Either Enables/Disables the room if the sector entered is/isn't this room.
    //If new room is this one, enable the room. If it isn't and there are no other players
    //in this room, disable the room.
    public virtual void NewRoomEntered(Vector3Int newSector, PlayerControl player)
    {
        Debug.Log("new room entered");
        if (RoomSectorsToList.Contains(newSector))
        {
            CameraBehaviour.Instance.MoveToNewRoom(newSector, player);
            if (!isCleared)
            {
                ActivateRoom();
            }
        }
        else
        {
            foreach (var neighbor in neighbors)
            {
                if (LevelGenerator.Instance.LevelMap[neighbor].RoomState == RoomState.Invisible)
                    LevelGenerator.Instance.LevelMap[neighbor].RoomState = RoomState.Nearby;
            }

            //Also disable room if no players remain in there and current room is not
            //neighboring the new current room!!
        }
    }

    protected virtual void ClearedRoom()
    {
        RoomState = RoomState.Visited;
        for (int i = 0; i < doors.Count; i++)
        {
            //In multiplayer, enable 1-way barrier instead of shutting door
            //If players remain in a neighboring room.
            doors[i].OpenDoor();
        }
    }

    //Enable enemy-AI/pathfinding grid, shut doors, etc.
    protected virtual void ActivateRoom()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            //In multiplayer, enable 1-way barrier instead of shutting door
            //If players remain in a neighboring room.
            doors[i].ShutDoor();
        }
    }

    #endregion

    #region "Room Information"

    //Adds door to list of doors for current room. If door's neighbors BOTH
    //point to this room, the door is not added and return false!
    public bool TryAddDoor(DoorBase newDoor)
    {
        if ((LevelGenerator.Instance.LevelMap[newDoor.BLNeighbor] == this) &&
            (LevelGenerator.Instance.LevelMap[newDoor.TRNeighbor] == this))
        {
            return false;
        }

        //Should be relatively fast operation; not typically 4 or less doors unless large room.
        if (doors.Contains(newDoor))
            return false;

        doors.Add(newDoor);
        newDoor.OnNewRoomEntered += NewRoomEntered;
        return true;
    }

    public bool TryAddNeighbor(Vector3Int newNeighbor)
    {
        if(LevelGenerator.Instance.LevelMap.ContainsKey(newNeighbor))
        {
            if(LevelGenerator.Instance.LevelMap[newNeighbor] == this)
                return false;
        }

        //Should be relatively fast operation; # of neighbors should be MAXIMUM (2 * width) + (2 * height)
        if (neighbors.Contains(newNeighbor))
            return false;
        neighbors.Add(newNeighbor);
        return true;
    }

    public virtual void UpdateNeighbors()
    {
        //Add new neighbors to rooms' lists
        // This should work for any size/type of room using direction, as long as
        // different sized rooms are stored multiple times for each tile the occupy!!!
        // ^^^^^ this is like, super important to keep in mind!!!
        RoomBase value = null;
        Dictionary<Vector3Int, RoomBase> map = LevelGenerator.Instance.LevelMap;
        List<Vector3Int> sectorsList = RoomSectorsToList;

        neighbors.Clear();
        foreach (var sector in sectorsList)
        {
            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    if (x != 0 && y != 0) continue;

                    Vector3Int neighborDir = new Vector3Int(x, y, 0);

                    if (map.TryGetValue(sector + neighborDir, out value))
                    {
                        if (value != this && value != null)
                        {
                            this.TryAddNeighbor(sector + neighborDir);
                            value.TryAddNeighbor(sector);
                        }
                        else
                            neighbors.Remove(sector + neighborDir);
                    }
                }
            }
        }
    }

    protected virtual void UpdateDoors()
    {
        List<Vector3Int> sectorsList = RoomSectorsToList;

        foreach (var sector in sectorsList)
        {
            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    if (x != 0 && y != 0) continue;

                    Vector3Int neighborDir = new Vector3Int(x, y, 0);
                    if (sectorsList.Contains(sector + neighborDir)) continue;

                    else if (neighbors.Contains(sector + neighborDir))
                        LevelGenerator.Instance.CreateDoorObj(sector, sector + neighborDir);
                }
            }
        }
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
        this.roomSectors = new Vector3Int[1, 1];
        this.roomSectors[0, 0] = initialLocation;

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

        //TODO: Remove this dead code when implementing enemies/objectives to clear rooms.
        int test = Mathf.Abs(LevelGenerator.Instance.LevelRNG.Rand32S());

        /*if (test % 2 == 0 && gameObject.name != "Start Room")
        {
            clearEventObj = Instantiate(Resources.Load<GameObject>("Test/RoomClearer"));
            clearEventObj.GetComponent<TestClearEvent>().SetRoom(this);
            isCleared = false;
        }*/

        clearEventObj = Instantiate(Resources.Load<GameObject>("Test/RoomClearer"));
        clearEventObj.GetComponent<TestClearEvent>().SetRoom(this);
        isCleared = false;

        isPosFinal = true;
    }

    public void SetRoomData(RoomData roomData)
    {
        this.roomData = roomData;
        roomWidth = roomData.Layout.width;
        roomHeight = roomData.Layout.height;

        if (roomData.NumSectorsX < 1 || roomData.NumSectorsY < 1)
            return;

        roomSectors = new Vector3Int[roomData.NumSectorsX, roomData.NumSectorsY];
        for(int x = 0; x < roomData.NumSectorsX; x++)
        {
            for(int y = 0; y < roomData.NumSectorsY; y++)
            {
                roomSectors[x, y] = initialLocation + new Vector3Int(x, y, initialLocation.z);
            }
        }
        SetSectorsList();
    }

    #endregion

    #region "Layout Verfication"

    //Verfies of the size of the Layout in current instance is valid for 
    //its placement relative to other sectors.
    //Note: this function is fucking stupid and confusing, but it works
    public virtual bool VerifyLayoutSize()
    {
        int sectsX = Data.NumSectorsX;
        int sectsY = Data.NumSectorsY;

        //Ensure dimensions are valid.
        if (sectsX == -1 || sectsY == -1)
        {
            Debug.Log("Invalid size of room texture!" + Data.RoomName);
            return false;
        }

        //If room is 1x1, size is fine since only occupies one space
        if (sectsX == 1 && sectsY == 1)
            return true;

        //Definitely large room now; check if neighboring sectors can be added/replaced!
        else
        {
            //Debug.Log("Large room found");

            byte placeAttempts = 0;
            while (placeAttempts < 6)
            {
                if (!AllNewSectorsFree(ref roomSectors))
                {
                    placeAttempts++;
                    RefreshRoom();
                    continue;
                }
                
                //RoomSectors already updated if true here.
                else
                {
                    //Debug.Log("Large room being placed");

                    //If successful (is if outside of prev. loop), update neighbors, remove doors connecting replaced rooms.
                    //Remove GameObjects for old rooms, and set all their neighbors to the large room's neighbors
                    var sectorList = RoomSectorsToList;
                    foreach (var sector in sectorList)
                    {
                        if (sector == initialLocation)
                            continue;
                        if (LevelGenerator.Instance.LevelMap.ContainsKey(sector))
                        {
                            foreach (var door in LevelGenerator.Instance.LevelMap[sector].Doors.ToArray())
                            {
                                if (sectorList.Contains(door.TRNeighbor) && sectorList.Contains(door.BLNeighbor))
                                {
                                    removedDoors.Add(door);
                                    doors.Remove(door);
                                }
                                else if (!sectorList.Contains(door.TRNeighbor) && sectorList.Contains(door.BLNeighbor))
                                    possibleDoors.Add(door);
                                else if (sectorList.Contains(door.TRNeighbor) && !sectorList.Contains(door.BLNeighbor))
                                    possibleDoors.Add(door);
                            }
                            roomsRemoved.Add(LevelGenerator.Instance.LevelMap[sector]);
                        }
                        else
                            sectorsAdded.Add(sector);
                    }

                    if (SectorsInterfereWithNeighbors())
                    {
                        placeAttempts++;
                        RefreshRoom();
                        continue;
                    }

                    foreach(var room in roomsRemoved)
                    {
                        room.flaggedToDestroy = true;
                        LevelGenerator.Instance.LevelMap[room.initialLocation] = this;  //room should still be unloaded, so this is ok.
                    }

                    foreach(var sector in sectorsAdded)
                        LevelGenerator.Instance.LevelMap.Add(sector, this);

                    SetSectorsList();
                    UpdateNeighbors();
                    isLarge = true;
                    return true;
                }
            }
            return false;
        }
    }

    private bool SectorsInterfereWithNeighbors()
    {
        var sectorList = RoomSectorsToList;
        foreach (var sector in sectorList)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x != 0 && y != 0) continue;
                    Vector3Int neighborDir = new Vector3Int(x, y, 0);

                    if (sectorList.Contains(sector + neighborDir)) continue;
                    if (!LevelGenerator.Instance.LevelMap.ContainsKey(sector + neighborDir)) continue;

                    RoomBase neighbor = LevelGenerator.Instance.LevelMap[sector + neighborDir];
                    DoorBase newDoor = LevelGenerator.Instance.CreateDoorObj(sector + neighborDir, sector);
                    if (newDoor != null)
                    {
                        if (!neighbor.isLoaded)
                        {
                            if (!CanPathFindToDoors())
                                return true;

                        }
                        else if (!CanPathFindToDoors() && !neighbor.CanPathFindToDoors())
                            return true;
                    }
                }
            }
        }
        return false;
    }

    //Attempts to place new sectors at arbitrary positions based on the size of 
    //the room layout. If all random new sectors can be placed to fit the layout,
    //returns true and updates ref roomSectors to the new, valid ones. If returns false,
    //roomSectors is unchanged!!
    protected bool AllNewSectorsFree(ref Vector3Int[,] roomSectors)
    {
        Vector3Int[,] newSectors = new Vector3Int[Data.NumSectorsX, Data.NumSectorsY];
        int randSectX = Mathf.Abs(LevelGenerator.Instance.LevelRNG.Rand32S() % Data.NumSectorsX);
        int randSectY = Mathf.Abs(LevelGenerator.Instance.LevelRNG.Rand32S() % Data.NumSectorsY);

        //Calculate which sectors have content. This tells us what sectors this large room will occupy
        //relative to the bottom left sector 0,0. Return false if any conditions are broken
        for (int x = 0; x < Data.NumSectorsX; x++)
        {
            for (int y = 0; y < Data.NumSectorsY; y++)
            {
                newSectors[x, y] = initialLocation - (new Vector3Int(randSectX - x, randSectY - y, 0));

                //If sector exists, can be overwritten if it is NOT LOADED YET
                if (LevelGenerator.Instance.LevelMap.ContainsKey(newSectors[x, y]))
                {
                    if (LevelGenerator.Instance.LevelMap[newSectors[x, y]].isLoaded)
                        return false;
                }
                //Try get value around 4 cardinal directions of new possible sector.
                //if it doesn't touch ANY other sectors in the map, it can be used.
                else if (!LevelGenerator.Instance.CanExtendSector(newSectors[x, y], this))
                {
                    //Debug.Log("Cannot extend sector");
                    return false;
                }
            }
        }

        roomSectors = newSectors;
        //Probably a better way to calculate this, but it should work.
        RoomOrigin = GetSectorCenter(roomSectors[0, 0]) - (new Vector3Int(RoomUtils.BASE_ROOM_X / 2, RoomUtils.BASE_ROOM_Y / 2, 0));
        return true;
    }

    //Check if path can be created to the two doors provided in the room.
    public virtual bool CanPathFindToDoors()
    {
        Tilemap test = GameObject.Find("Test PathFinding").GetComponent<Tilemap>();
        //Check for a path from an aribitrary first door to each other door.
        DoorBase firstDoor = null;
        Vector2Int[] firstDoorPos = null;
        Vector2Int[] checkDoorPos = null;

        //If room is large, there will be contenders for possible doors when it replaces what
        //would have been a room. These need to be accounted for in pathfinding!
        List<DoorBase> allDoors = new List<DoorBase>();
        foreach (var door in doors)
            allDoors.Add(door);

        if (possibleDoors.Count > 0)
        {
            foreach (var door in possibleDoors)
                allDoors.Add(door);
        }

        bool showTiles = false;
        if (IsLargeRoom)
        {
            showTiles = true;
        }
        foreach (var door in allDoors)
        {
            //Set arbitrary first door.
            if (firstDoor == null)
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
            for (int x = 0; x < roomData.Layout.width; x++)
            {
                for (int y = 0; y < roomData.Layout.height; y++)
                {
                    Color currPixel = roomData.Layout.GetPixel(x, y);
                    layoutGrid.GetNodeDirect(x, y).walkable = RoomUtils.IsSpaceFree(currPixel);

                    if (showTiles)
                    {
                        if (layoutGrid.GetNodeDirect(x, y).walkable)
                            test.SetTile(new Vector3Int(x, y, 0) + RoomOrigin, RoomUtils.GetLevelFloorTile(currPixel));
                        else
                            test.SetTile(new Vector3Int(x, y, 0) + RoomOrigin, RoomUtils.GetLevelBoundTile(currPixel));
                    }

                    //test
                    if (layoutGrid.GetNodeDirect(x, y).walkable != RoomUtils.IsSpaceFree(roomData.Layout.GetPixel(x, y)))
                        Debug.Log("Graph nodes do not align with image nodes");
                }
            }


            //Now do the actual pathfinding on both checkDoorPos.
            foreach (var startPos in firstDoorPos)
            {
                foreach (var endPos in checkDoorPos)
                {
                    if (showTiles)
                    {
                        test.SetTile(new Vector3Int(startPos.x, startPos.y, 0) + RoomOrigin, null);
                        test.SetTile(new Vector3Int(endPos.x, endPos.y, 0) + RoomOrigin, null);
                    }
                    var list = layoutGrid.FindPathAStar(startPos, endPos, false);
                    if (list == null)
                    {
                        if (showTiles) Debug.Log("Large room pathfind has failed!");
                        return false;
                    }
                    else if (showTiles)
                        foreach (var node in layoutGrid.grid)
                        {
                            if (list.Contains(node))
                            {
                                test.SetTile((new Vector3Int(node.gridX, node.gridY, 0) + RoomOrigin), null);
                            }
                        }

                }
            }
        }
        return true;
    }

    public virtual bool SpaceNearDoorWalkable(int index)
    {
        Vector2Int[] doorPos = doors[index].GetLayoutDoorPos(this);
        //Debug.Log("Checked spaced near door in " + gameObject.name);
        foreach (var pos in doorPos)
        {
            if (!RoomUtils.IsSpaceFree(roomData.Layout.GetPixel(pos.x, pos.y)))
                return false;
        }
        return true;
    }

    public void RefreshRoom()
    {
        roomSectors = new Vector3Int[1, 1];
        roomSectors[0, 0] = initialLocation;
        RoomOrigin = GetSectorCenter(initialLocation) - (new Vector3Int(RoomUtils.BASE_ROOM_X / 2, RoomUtils.BASE_ROOM_Y / 2, 0));

        foreach(var room in roomsRemoved)
        {
            LevelGenerator.Instance.LevelMap[room.initialLocation] = room;
            room.flaggedToDestroy = false;
        }
        foreach (var sector in sectorsAdded)
            LevelGenerator.Instance.LevelMap.Remove(sector);
        foreach(var door in removedDoors)
        {
            LevelGenerator.Instance.LevelMap[door.BLNeighbor].TryAddDoor(door);
            LevelGenerator.Instance.LevelMap[door.TRNeighbor].TryAddDoor(door);
        }

        sectorsAdded.Clear();
        possibleDoors.Clear();
        removedDoors.Clear();
        roomsRemoved.Clear();
        UpdateNeighbors();
        //Debug.Log("Refreshed " + gameObject.name);
    }

    #endregion

    #region "Loading and Reading Rooms"

    public virtual void LoadRoom()
    {
        if (isLoaded)
            return;

        if (IsLargeRoom)
        {
            foreach (var room in roomsRemoved)
            {
                if (!RoomSectorsToList.Contains(room.initialLocation))
                    continue;
                neighbors.Remove(room.initialLocation);
                Destroy(room.gameObject);
            }

            if (possibleDoors.Count > 0)
            {
                foreach (var door in possibleDoors)
                    TryAddDoor(door);
            }

            foreach(var door in removedDoors)
            {
                LevelGenerator.Instance.LevelMap[door.BLNeighbor].doors.Remove(door);
                LevelGenerator.Instance.LevelMap[door.TRNeighbor].doors.Remove(door);
                Destroy(door.gameObject);
            }
        }

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

        //Remove unneeded tiles from where doors are placed, and replace wall bounds directly above
        //horizontal doors with perspective walls of the respective room.
        foreach (var door in doors)
        {
            List<Vector3Int> tempDoorPos = door.GetOccupiedSpaces();
            TileBase[] emptyTiles = new TileBase[tempDoorPos.Count];
            LevelGenerator.Instance.WallsTilemap.SetTiles(tempDoorPos.ToArray(), emptyTiles);
            if(door.FaceDirection == DoorBase.DoorDirection.Horizontal)
            {
                foreach(var pos in tempDoorPos)
                {
                    Vector3Int newPos = new Vector3Int(pos.x, pos.y + 1, pos.z);
                    
                    int index = RoomUtils.GetIndexFromTile(LevelGenerator.Instance.FloorTilemap.GetTile(newPos));
                    if (index == -1) continue;
                    LevelGenerator.Instance.WallsTilemap.SetTile(newPos, RoomUtils.GetLevelWallTile(index));
                }
            }
        }

        isLoaded = true;
    }

    private void ReadRoom(Texture2D layout)
    {
        /* Check each tile in Texture2D layout from top to bottom for easier
           replacement of perspective tiles (ex. side view of wall) when scanning room 
        */

        string baseWallWithOpacity = RoomUtils.BASE_WALL + "FF";
        for (int y = layout.height; y >= -1; y--)
        {
            for (int x = layout.width; x >= -1; x--)
            {
                //Check pre-conditions that are true for every room and are out of texture bounds.
                //Top two rows of map can only have a wall:
                if (y == layout.height || y < 0)
                {
                    SpawnWallBound(baseWallWithOpacity, x, y);
                    continue;
                }

                //Very left and right columns should always be the room's base wall.
                else if (x == layout.width || x < 0)
                {
                    SpawnWallBound(baseWallWithOpacity, x, y);
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
        TileBase boundTile = RoomUtils.GetLevelBoundTile(colorHex);
        int visualsIndex = RoomUtils.GetIndexFromColorOpacity(colorHex);

        wallTileList.Add(boundTile);
        wallPosList.Add((new Vector3Int(x, y, 0)) + roomOrigin);


        /*This is where other wall types MIGHT be checked later somehow.*/

        //Add floors under wall in case it is removed!
        floorTileList.Add(RoomUtils.GetLevelFloorTile(visualsIndex));
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
                    wallTile = RoomUtils.GetLevelWallTile(visualsIndex);

                Vector3Int tile1Down = new Vector3Int(x, y - 1, initialLocation.z);
                Vector3Int tile2Down = new Vector3Int(x, y - 2, initialLocation.z);

                //Place two wall tiles below lowest wall bound!
                wallTileList.Add(wallTile);
                wallPosList.Add(roomOrigin + tile1Down);
                wallTileList.Add(wallTile);
                wallPosList.Add(roomOrigin + tile2Down);

                //Place floor tiles under walls; hidden but there if wall is removed!
                floorTileList.Add(RoomUtils.GetLevelFloorTile(visualsIndex));
                floorPosList.Add(roomOrigin + tile1Down);
                floorTileList.Add(RoomUtils.GetLevelFloorTile(visualsIndex));
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
        floorTileList.Add(RoomUtils.GetLevelFloorTile(colorHex));
        floorPosList.Add(new Vector3Int(x, y, 0) + roomOrigin);

        //This is where other floor types will be checked later somehow.
    }

    #endregion

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



    protected void OnDestroy()
    {
        Destroy(clearEventObj);
    }
}
