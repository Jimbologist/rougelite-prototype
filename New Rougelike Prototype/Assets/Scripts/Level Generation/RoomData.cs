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
    [SerializeField] private uint roomID = 0;

    public Texture2D Layout { get { return roomLayout; } }
    public uint RoomID { get { return roomID; } }
    public int NumSectorsX 
    {
        get 
        {
            if (roomLayout.width % RoomUtils.BASE_ROOM_X != 0)
                return -1;
            return roomLayout.width / RoomUtils.BASE_ROOM_X; 
        } 
    }
    public int NumSectorsY
    {
        get
        {
            if (roomLayout.height % RoomUtils.BASE_ROOM_Y != 0)
                return -1;
            return roomLayout.height / RoomUtils.BASE_ROOM_Y;
        }
    }
}
