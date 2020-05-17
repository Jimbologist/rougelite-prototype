using System.Collections.Generic;
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
    [SerializeField] private Floor currentFloor;

    //All tilemaps in GameObject/children GameObjects.
    //ALL ARE REFERENCES SET IN INSPECTOR!!!
    [SerializeField] private Tilemap objectTilemap;
    [SerializeField] private Tilemap wallsTilemap;
    [SerializeField] private Tilemap floorTilemap;

    public Floor Floor { get { return currentFloor; } }
    public Tilemap WallsTilemap { get => wallsTilemap; }
    public Tilemap ObjectTilemap { get => objectTilemap; }
    public Tilemap FloorTilemap { get => floorTilemap; }
    public SeedRandom LevelRNG { get; private set; }
    public Vector2Int RoomSpawnPos { get; private set; }

    //Map of rooms in given floor. Z-axis dictates subfloor.
    private Dictionary<Vector3Int, RoomBase> levelMap;

    protected override void Awake()
    {
        base.Awake();
         
        //TODO: Use new abc values and seed from GameManager later!
        LevelRNG = new SeedRandom();
        RoomSpawnPos = new Vector2Int(0, 0);

        //Get random start room; equally weighted based on size of start room list.
        uint roomIndex = LevelRNG.Random32U();
        //RoomData startRoomData = ScriptableObject.CreateInstance<RoomData>();
        RoomData startRoomData = null;
        if (Floor != null)
        {
            startRoomData = Floor.StartRooms[(int)roomIndex % Floor.StartRooms.Count];
            Debug.Log((int)roomIndex % Floor.StartRooms.Count);
        }

        SpawnNewRoom(startRoomData, new Vector3Int(0,0,0),  "Start Room");
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
        this.currentFloor = newFloor;
    }

    public void SetNewRoomsFromFloor()
    {
        
    }

    //As of now, only grabs regular room. No special rooms implemented.
    public void GetRandStandardRoom()
    {
        //TODO: Make list of all special rooms on level init, as in
        //grab one random room layout for all rooms guaranteed to appear on that floor.
        LevelRNG.RandomFloat();
    }

    public void SpawnNewRoom(RoomData roomData, Vector3Int mapLocation, string roomName)
    {
        GameObject newRoomObj = new GameObject(roomName);
        RoomBase newRoom = newRoomObj.AddComponent<RoomBase>();
        newRoom.InitializeRoom(roomData, new Vector3Int(0, 0, 0), new Vector3Int(0, 0, 0));
        newRoom.LoadRoom();
    }

    public void DungeonCrawl()
    {
        Vector3Int crawlerLocation = new Vector3Int(0, 0, 0);

    }
}
