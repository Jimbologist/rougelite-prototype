using System.Collections.Generic;
using UnityEngine;

//Provide information for a new floor as well as default information
//for rooms in case it isn't given.
[CreateAssetMenu(menuName = "Floor")]
public class Floor : ScriptableObject
{
    public const uint BASE_MIN_ROOMS = 8;

    //All rooms for floor. Only add room datas through editor.
    [SerializeField] private List<RoomData> startRooms = new List<RoomData>();
    [SerializeField] private List<RoomData> easyRooms = new List<RoomData>();
    [SerializeField] private List<RoomData> mediumRooms = new List<RoomData>();
    [SerializeField] private List<RoomData> hardRooms = new List<RoomData>();
    [SerializeField] private List<RoomData> extremeRooms = new List<RoomData>();

    [SerializeField] private RuleTile[] wallTiles;
    [SerializeField] private RuleTile[] floorTiles;
    [SerializeField] private RuleTile[] boundTiles;
    //Indeces should correspond to DoorBase.DoorDirection enum!
    [SerializeField] private Sprite[] doorSprites;       //If multiple types of doors, should match with wallTiles!
    [SerializeField] private byte floorID = 0;
    [SerializeField] private byte extraDeadEnds = 0;    //Any floor-specific rooms that require dead ends.

    public List<RoomData> StartRooms { get => startRooms; }
    public List<RoomData> EasyRooms { get => easyRooms; }
    public List<RoomData> MediumRooms { get => mediumRooms; }
    public List<RoomData> HardRooms { get => hardRooms; }
    public List<RoomData> ExtremeRooms { get => extremeRooms; }

    public RuleTile[] WallTiles { get => wallTiles; }
    public RuleTile[] FloorTiles { get => floorTiles; }
    public RuleTile[] BoundTiles { get => boundTiles; }
    public Sprite[] DoorSprites { get => doorSprites; }
    public uint FloorID { get { return floorID; } }
}
