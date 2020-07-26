using UnityEngine;

//Add more room types here as they are made. Each type will also
// have inherited types from RoomBase. This enum will allow the
// RoomData object to associate with the correct type of room
public enum RoomType
{
    Base = 0
}

[CreateAssetMenu(menuName = "RoomData")]
public class RoomData : ScriptableObject
{
    [SerializeField] private Texture2D roomLayout = null;
    [SerializeField] private string roomName;
    [SerializeField] private RoomDeco decoration;

    public Texture2D Layout { get { return roomLayout; } }
    public RoomDeco Decoration { get { return decoration; } }
    public string RoomName { get { return roomName; } }
    public int NumSectorsX 
    {
        get 
        {
            //If 1 sector, will be base room width, otherwise must account for
            //door offset, so check if it is a multiple of base width + offset.
            if (roomLayout.width == RoomUtils.BASE_ROOM_X)
                return 1;
            else if ((roomLayout.width + RoomUtils.DOOR_OFFSET) % (RoomUtils.BASE_ROOM_X + RoomUtils.DOOR_OFFSET) == 0)
                return (roomLayout.width + RoomUtils.DOOR_OFFSET) / (RoomUtils.BASE_ROOM_X + RoomUtils.DOOR_OFFSET);
            else
                return -1;
        } 
    }
    public int NumSectorsY
    {
        get
        {
            //If 1 sector, will be base room height, otherwise must account for
            //door offset, so check if it is a multiple of base height + offset.
            if (roomLayout.height == RoomUtils.BASE_ROOM_Y)
                return 1;
            else if ((roomLayout.height + RoomUtils.DOOR_OFFSET) % (RoomUtils.BASE_ROOM_Y + RoomUtils.DOOR_OFFSET) == 0)
                return (roomLayout.height + RoomUtils.DOOR_OFFSET) / (RoomUtils.BASE_ROOM_Y + RoomUtils.DOOR_OFFSET);
            else
                return -1;
        }
    }
}
