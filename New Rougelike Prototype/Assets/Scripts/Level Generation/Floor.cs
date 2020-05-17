using System.Collections.Generic;
using UnityEngine;

//Provide information for a new floor as well as defualt information
//for rooms in case it isn't given.
[CreateAssetMenu(menuName = "Floor")]
public class Floor : ScriptableObject
{
    private const uint baseRoomPoolSize = 10;

    //All rooms for floor. Only add room datas through editor.
    [SerializeField] private List<RoomData> startRooms = new List<RoomData>();
    [SerializeField] private List<RoomData> easyRooms = new List<RoomData>();
    [SerializeField] private List<RoomData> mediumRooms = new List<RoomData>();
    [SerializeField] private List<RoomData> hardRooms = new List<RoomData>();
    [SerializeField] private List<RoomData> extremeRooms = new List<RoomData>();

    [SerializeField] private RuleTile defaultWallTiles;
    [SerializeField] private RuleTile defaultFloorTiles;
    [SerializeField] private RuleTile defaultBoundTiles;
    [SerializeField] private uint floorID = 0;

    public List<RoomData> StartRooms { get => startRooms; }
    public List<RoomData> EasyRooms { get => easyRooms; }
    public List<RoomData> MediumRooms { get => mediumRooms; }
    public List<RoomData> HardRooms { get => hardRooms; }
    public List<RoomData> ExtremeRooms { get => extremeRooms; }

    public RuleTile DefaultWallTiles { get => defaultWallTiles; }
    public RuleTile DefaultFloorTiles { get => defaultFloorTiles; }
    public RuleTile DefaultBoundTiles { get => defaultBoundTiles; }
    public uint FloorID { get { return floorID; } }
    public int RoomPoolSize { get { return Mathf.CeilToInt(baseRoomPoolSize * Mathf.Sqrt(floorID)); } }
}
