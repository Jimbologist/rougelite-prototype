using UnityEngine;

[CreateAssetMenu(menuName = "RoomData")]
public class RoomData : ScriptableObject
{
    [SerializeField] private Texture2D roomLayout = null;
    [SerializeField] private RuleTile wallTiles = null;
    [SerializeField] private RuleTile boundTiles = null;
    [SerializeField] private RuleTile floorTiles = null;
    [SerializeField] private uint roomID = 0;

    public Texture2D RoomLayout { get { return roomLayout; } }
    public RuleTile DefaultWallTiles { get { return wallTiles; } }
    public RuleTile DefaultFloorTiles { get { return floorTiles; } }
    public RuleTile DefaultBoundTiles { get { return boundTiles; } }
    public uint RoomID { get { return roomID; } }
    
    public virtual bool CheckTopDoor()
    {
        Color[] doorCheck = new Color[4];
        doorCheck[0] = GetTileColor((roomLayout.width / 2), roomLayout.height);
        doorCheck[1] = GetTileColor((roomLayout.width / 2) + 1, roomLayout.height);
        doorCheck[2] = GetTileColor((roomLayout.width / 2), roomLayout.height - 1);
        doorCheck[3] = GetTileColor((roomLayout.width / 2) + 1, roomLayout.height - 1);

        foreach(Color tile in doorCheck)
        {
            if (!RoomUtils.IsSpaceEmpty(tile))
                return false;
        }

        return true;
    }
    public virtual bool CheckBottomDoor()
    {
        Color[] doorCheck = new Color[4];
        doorCheck[0] = GetTileColor((roomLayout.width / 2), 0);
        doorCheck[1] = GetTileColor((roomLayout.width / 2) + 1, 0);
        doorCheck[2] = GetTileColor((roomLayout.width / 2), 1);
        doorCheck[3] = GetTileColor((roomLayout.width / 2) + 1, 1);

        foreach (Color tile in doorCheck)
        {
            if (!RoomUtils.IsSpaceEmpty(tile))
                return false;
        }

        return true;
    }
    public virtual bool CheckRightDoor()
    {
        Color[] doorCheck = new Color[4];
        doorCheck[0] = GetTileColor(roomLayout.width, (roomLayout.height / 2) + 1);
        doorCheck[1] = GetTileColor(roomLayout.width, (roomLayout.height / 2));
        doorCheck[2] = GetTileColor(roomLayout.width - 1, (roomLayout.height / 2) + 1);
        doorCheck[3] = GetTileColor(roomLayout.width - 1, (roomLayout.height / 2));

        foreach (Color tile in doorCheck)
        {
            if (!RoomUtils.IsSpaceEmpty(tile))
                return false;
        }

        return true;
    }
    public virtual bool CheckLeftDoor()
    {
        Color[] doorCheck = new Color[4];
        doorCheck[0] = GetTileColor(roomLayout.width, (roomLayout.height / 2) + 1);
        doorCheck[1] = GetTileColor(roomLayout.width, (roomLayout.height / 2));
        doorCheck[2] = GetTileColor(roomLayout.width - 1, (roomLayout.height / 2) + 1);
        doorCheck[3] = GetTileColor(roomLayout.width - 1, (roomLayout.height / 2));

        foreach (Color tile in doorCheck)
        {
            if (!RoomUtils.IsSpaceEmpty(tile))
                return false;
        }

        return true;
    }

    public virtual bool IsDeadEnd()
    {
        int count = 0;
        count += CheckTopDoor() ? 1 : 0;
        count += CheckBottomDoor() ? 1 : 0;
        count += CheckLeftDoor() ? 1 : 0;
        count += CheckRightDoor() ? 1 : 0;
        return count > 1;
    }

    public virtual Color GetTileColor(int x, int y)
    {
        return roomLayout.GetPixel(x, y);
    }
}
