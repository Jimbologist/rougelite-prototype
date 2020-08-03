using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Threading;

//This is a test comment

/**
 * Generates a level using data from a Floor ScriptableObject
 * and random RoomData objects. Must be attached to a GameObject with
 * a grid and tilemap, with each new tilemap used in children GameObjects
 * with their own tilemaps. 
 * ALL TILEMAP REFERENCES ARE SET IN INSPECTOR TO REMOVE NullReferenceExceptions.
 */
[RequireComponent(typeof(Tilemap))]
public class LevelGenerator : Singleton<LevelGenerator>
{
    [SerializeField] private Floor _currentFloor;

    //All tilemaps in GameObject/children GameObjects.
    //ALL ARE REFERENCES SET IN INSPECTOR!!!
    [SerializeField] private Tilemap _objectTilemap;
    [SerializeField] private Tilemap _wallsTilemap;
    [SerializeField] private Tilemap _floorTilemap;

    public Floor Floor { get { return _currentFloor; } }
    public Tilemap WallsTilemap { get => _wallsTilemap; }
    public Tilemap ObjectTilemap { get => _objectTilemap; }
    public Tilemap FloorTilemap { get => _floorTilemap; }
    public Collider2D WallsTilemapCollider { get; private set; }
    public SeedRandom LevelRNG { get; private set; }
    public long RNGInitialValue { get; private set; }
    public Vector2Int RoomSpawnPos { get; private set; }
    //Map of rooms in given floor. Z-axis dictates subfloor. Large rooms occupy multiple positions
    public Dictionary<Vector3Int, RoomBase> LevelMap { get; private set; }
    public List<RoomBase> DeadEnds
    {
        get
        {
            List<RoomBase> deadEnds = new List<RoomBase>();
            foreach (var room in LevelMap.Values)
                if (room.IsDeadEnd) deadEnds.Add(room);

            return deadEnds;
        }
    }

    //MinRooms = Ceil(7 * sqrt(floorNum))
    public uint MinRooms { get { return (uint)Mathf.CeilToInt(Floor.BASE_MIN_ROOMS * Mathf.Sqrt(floorNum)); } }
    //MaxRooms = Ceil(MinRooms + sqrt(MinRooms))
    public uint MaxRooms { get { return (uint)Mathf.CeilToInt(MinRooms + Mathf.Sqrt(MinRooms)); } }

    //Private data for specifics of level generation
    private uint floorNum = 1;  // <- Right now starts at 1 and increases each scene transition. Later will involve temp save functionality

    private RoomBase start;

    protected override void Awake()
    {
        WallsTilemapCollider = WallsTilemap.GetComponent<TilemapCollider2D>();

        //TODO: Use new abc values and seed from GameManager later!
        LevelRNG = new SeedRandom();
        //LevelRNG.SetSeed(1592834287);
        //LevelRNG.SetSeed(1592838565);
        //LevelRNG.SetSeed(1592852066);
        //LevelRNG.SetSeed(1592922363);
        //LevelRNG.SetSeed(1592936793);
        Debug.Log(LevelRNG.State);
        RNGInitialValue = LevelRNG.State;

        LevelMap = new Dictionary<Vector3Int, RoomBase>();
        RoomSpawnPos = Vector2Int.zero;

        //Right now starts at 1 and increases each scene transition.
        // later will get num from GameManager, which contains temp save information of current run!
        floorNum = 1;

        //StartCoroutine(GenerateMap());
        GenerateMap();
    }

    // Start is called before the first frame update
    private void Start()
    {

    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    public void SetNewFloor(Floor newFloor)
    {
        this._currentFloor = newFloor;
    }

    public void SetNewRoomsFromFloor()
    {
        
    }

    //As of now, only grabs regular room. No special rooms implemented.
    public void GetRandStandardRoom()
    {
        //TODO: Make list of all special rooms on level init, as in
        //grab one random room layout for all rooms guaranteed to appear on that floor.
        LevelRNG.RandFloat();
    }

    //Initializes a room at a position on the current floor's map. Does NOT load the room.
    public RoomBase InitializeRoom(Vector3Int mapLocation, string roomName)
    {
        GameObject newRoomObj = new GameObject(roomName);
        RoomBase newRoom = newRoomObj.AddComponent<RoomBase>();
        newRoom.InitLocation(mapLocation);
        return newRoom;
    }

    //Spawn rooms first (don't load). Then, after GenerateMapLayout finishes, load all rooms.
    //Finally, deactivate every room excepts all of current room's neighbors (maybe in GameManager?)

    //Generates locations that rooms will have. No RoomData is collected yet.
    public void GenerateMap()
    {
        //Start will always be at 0,0,0 for now on map (subject to change)
        RoomBase currRoom = null;
        Vector3Int currPos = new Vector3Int(0, 0, 0);

        //Get and spawn a start room before random generation.
        RoomData startRoomData = Floor.StartRooms[Mathf.Abs(LevelRNG.Rand32S()) % Floor.StartRooms.Count];
        currRoom = InitializeRoom(currPos, "Start Room");
        currRoom.SetRoomData(startRoomData);
        LevelMap.Add(currPos, currRoom);
        start = currRoom;

        /*Randomly generate level. No rooms will be loaded, just initialized at random positions. */
        uint baseNumRooms = LevelRNG.RandRangeUInt(MinRooms, MaxRooms);

        //Generate specified number of rooms; will make more dead ends later if not enough.
        // Also only generates base number of rooms for "skeleton" of layout. Many more
        // can potentially be added by branching from random locations.
        while(LevelMap.Count < baseNumRooms)
        {
            //Choose random direction for next room until unallocated space is found.
            Vector3Int nextRoomDir;
            while(LevelMap.ContainsKey(currPos))
            {
                //50/50 chance to move in x/y, and 50/50 for +1/-1.
                nextRoomDir = Vector3Int.zero;
                int dirVal = LevelRNG.Rand32S();
                dirVal = (dirVal < 0) ? -1 : 1;
                if ((int)LevelRNG.Rand32U() % 2 == 0)
                    nextRoomDir.x = dirVal;
                else
                    nextRoomDir.y = dirVal;
                currPos += nextRoomDir;
            }

            currRoom = InitializeRoom(currPos, string.Format("Room_{0}", LevelMap.Count));
            LevelMap.Add(currPos, currRoom);
            currRoom.UpdateNeighbors();
        }

        //TODO: Find, then create more dead ends if there are currently not enough by branching
        // from a random room w/ 2 or 3 neighbors and creating one intentionally until a minimum is created.
        foreach (var room in LevelMap.Values)
        {
            if(room.Neighbors.Count < 2)
            {
                DeadEnds.Add(room);
            }
        }
        //StartCoroutine(LoadRooms());
        LoadRooms();
    }

    //Loads all rooms on current floor given certain conditions; you should disable all
    // non-neighbors after calling this method!
    private void LoadRooms()
    {
        List<RoomData> roomDiff;
        //Temp list of doors made to prevent duplicating valid doors!

        //Initialized positions of all rooms on the map. No rooms have
        // a layout, but will have doors and neighbors initialized.
        foreach(var roomPos in LevelMap)
        {
            //if (roomPos.Value.isDeadEnd)
            //  continue;
            //Spawn doors at each unloaded neighbor. Layout not accounting for this yet.
            foreach (var neighbor in roomPos.Value.Neighbors)
            {
                if (!LevelMap[neighbor].isPosFinal)
                    continue;

                CreateDoorObj(roomPos.Key, neighbor);
            }
        }

        //All doors are spawned, now can verify layouts. Loops through
        // all doors and verify that the layout allows access to all neighbors;
        // if layout is invalid, re-roll the room until valid one is found.

        var mapCopy = new Dictionary<Vector3Int, RoomBase>(LevelMap);
        foreach (var roomPos in mapCopy)
        {
            if (roomPos.Value.isLoaded || roomPos.Value.flaggedToDestroy)
                continue;

            //For testing, only grabs data from easy rooms!!
            //TODO: Randomly grab rooms from correct difficulty lists for normal rooms!
            roomDiff = Floor.EasyRooms;
            if (roomPos.Value == start) roomDiff = Floor.StartRooms;
            RoomData normRoomData = null;

            //Get a new layout for a room that allows all neighbors and doors created
            //to be accessible. A path should be available from each door to each other door!
            while (true)
            {
                normRoomData = roomDiff[Mathf.Abs(LevelRNG.Rand32S()) % roomDiff.Count];
                roomPos.Value.SetRoomData(normRoomData);

                if (VerifyLayout(roomPos.Value))
                    break;
            }
            roomPos.Value.LoadRoom();
        }

        //TODO: For each dead end, get special rooms in a pre-determined order
        // for necessary ones and then random order for other kinds!!
    }

    private bool VerifyLayout(RoomBase room)
    {
        //Check if size is valid. If it is, neighbors automatically updated.
        if (!room.VerifyLayoutSize())
        {
            //Debug.Log(room.gameObject.name + " could not fit!");
            return false;
        }

        //Check if 2x2 area in front of door can be walked on.
        //Also only 1 door since dead end, so check index 0.
        if (room.IsDeadEnd)
        {
            //Debug.Log("Dead end found " + room.gameObject.name);
            if (room.SpaceNearDoorWalkable(0))
                return true;
        }
        else if (room.CanPathFindToDoors())
        {
            //Debug.Log("Pathfinding successful in " + room.gameObject.name);
            return true;
        }

        room.RefreshRoom();
        return false;
    }

    //Tries to placed a door object between two rooms. If one already exists, returns the existing door.
    //If not, place the new door and return the new instance.
    public DoorBase CreateDoorObj(Vector3Int currRoom, Vector3Int neighbor)
    {
        if (!LevelMap.ContainsKey(currRoom) || !LevelMap.ContainsKey(neighbor))
            return null;

        //Check if door already exists by seeing if one already references
        //the same two rooms being check now. Bool is to continue to next neighbor
        //only if conditions inside nested loop are met!
        foreach (var door in LevelMap[neighbor].Doors)
        {
            if (!door.IsDoorAvailable(neighbor, currRoom))
            {
                return door;
            }
        }

        GameObject doorObj = new GameObject(string.Format("Room_{0}_Door_{1}", currRoom.ToString(), LevelMap[currRoom].Doors.Count));
        DoorBase newDoor = doorObj.AddComponent<DoorBase>();
        newDoor.SpawnDoor(currRoom, neighbor);

        //Add door to rooms' lists of doors; set references to neighbors
        //and subscribe to RoomClear event so door will open on clear.
        LevelMap[currRoom].TryAddDoor(newDoor);
        LevelMap[neighbor].TryAddDoor(newDoor);
        return newDoor;
    }

    private List<RoomData> GetStandardRoomList()
    {
        float difficultyChance = LevelRNG.RandFloat();
        if (difficultyChance < 0.05f)
            return Floor.ExtremeRooms;
        else if (difficultyChance < 0.20f)
            return Floor.HardRooms;
        else if (difficultyChance < 0.55f)
            return Floor.MediumRooms;
        else
            return Floor.EasyRooms;
    }

    //Check if there are any dead ends near this potential sector to
    //add as an extenstion for a larger room. extension = larger room extending from.
    //Certain special rooms at dead ends can only have 1 neighbor, so this ensures
    //large rooms won't break that.
    public bool CanExtendSector(Vector3Int sector, RoomBase extension)
    {
        RoomBase neighbor = null;

        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if (x != 0 && y != 0) continue;
                Vector3Int neighborDir = new Vector3Int(x, y, 0);
                if (LevelMap.TryGetValue(sector + neighborDir, out neighbor))
                {
                    if (extension == neighbor)
                        continue;
                    if (neighbor.IsDeadEnd || neighbor.isLoaded)
                        return false;
                }
            }
        }
        return true;
    }
}
