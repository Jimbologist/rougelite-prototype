using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public SeedRandom LevelRNG { get; private set; }
    public Vector2Int RoomSpawnPos { get; private set; }
    //Map of rooms in given floor. Z-axis dictates subfloor. Large rooms occupy multiple positions
    public Dictionary<Vector3Int, RoomBase> LevelMap { get; private set; }
    public List<RoomBase> DeadEnds { get; private set; }

    //MinRooms = Ceil(7 * sqrt(floorNum))
    public uint MinRooms { get { return (uint)Mathf.CeilToInt(Floor.baseMinRooms * Mathf.Sqrt(floorNum)); } }
    //MaxRooms = Ceil(MinRooms + sqrt(MinRooms))
    public uint MaxRooms { get { return (uint)Mathf.CeilToInt(MinRooms + Mathf.Sqrt(MinRooms)); } }

    //Private data for specifics of level generation
    private uint floorNum = 1;  // <- Right now starts at 1 and increases each scene transition. Later will involve temp save functionality

    protected override void Awake()
    {
        base.Awake();

        //TODO: Use new abc values and seed from GameManager later!
        LevelRNG = new SeedRandom();
        LevelMap = new Dictionary<Vector3Int, RoomBase>();
        DeadEnds = new List<RoomBase>();
        RoomSpawnPos = Vector2Int.zero;

        //Right now starts at 1 and increases each scene transition.
        // later will get num from GameManager, which contains temp save information of current run!
        floorNum = 1;


        StartCoroutine(GenerateMap());
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
    public IEnumerator GenerateMap()
    {
        yield return new WaitForSeconds(1.5f);
        //Start will always be at 0,0,0 for now on map (subject to change)
        RoomBase currRoom;
        Vector3Int currPos = new Vector3Int(0, 0, 0);

        //Get and spawn a start room before random generation.
        RoomData startRoomData = Floor.StartRooms[Mathf.Abs(LevelRNG.Rand32S()) % Floor.StartRooms.Count];
        currRoom = InitializeRoom(currPos, "Start Room");
        currRoom.SetRoomData(startRoomData);
        LevelMap.Add(currPos, currRoom);

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
                yield return null;
            }

            currRoom = InitializeRoom(currPos, string.Format("Room_{0}", LevelMap.Count));
            LevelMap.Add(currPos, currRoom);
            currRoom.UpdateNeighbors();
            yield return null;
        }

        //TODO: Find, then create more dead ends if there are currently not enough by branching
        // from a random room w/ 2 or 3 neighbors and creating one intentionally until a minimum is created.
        foreach (var room in LevelMap.Values)
        {
            yield return null;
            if(room.neighbors.Count < 2)
            {
                room.isDeadEnd = true;
                DeadEnds.Add(room);
            }
        }
        StartCoroutine(LoadRooms());
    }

    //Loads all rooms on current floor given certain conditions; you should disable all
    // non-neighbors after calling this method!
    private IEnumerator LoadRooms()
    {
        //For testing, only grabs data from easy rooms!!
        //TODO: Randomly grab rooms from correct difficulty lists for normal rooms!
        foreach(var room in LevelMap.Values)
        {
            if (room.roomData == null)
            {
                //if(DeadEnds.Contains(room))
                //{
                //    continue;
                //}
                RoomData normRoomData = Floor.EasyRooms[Mathf.Abs(LevelRNG.Rand32S()) % Floor.EasyRooms.Count];
                room.SetRoomData(normRoomData);
                //TODO: Check if room data is valid with neighbors; i.e. if doors can be placed to
                // transition into neighboring rooms. Should probably be its own long method that
                // checks direction and type of room, then returns a bool if layout is valid.
            }
            room.LoadRoom();
            yield return null;
        }

        //TODO: For each dead end, get special rooms in a pre-determined order
        // for necessary ones and then random order for other kinds!!
    }
}
