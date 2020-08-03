using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//Door mainly for transitions between rooms; must have second instance to joint with.
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class DoorBase : MonoBehaviour
{
    //Integer for enum corresponds to index for sprite in array of door sprites to use!
    public enum DoorDirection
    {
        Horizontal = 0,
        Vertical = 1
    }

    protected readonly Vector2 BaseHorizSize = new Vector2(2, 2);
    protected readonly Vector2 BaseVertSize = new Vector2(2, 3);

    protected Vector3Int _position;
    protected Tilemap _wallTilemap;
    [SerializeField] protected Vector3Int _TRneighbor;
    [SerializeField] protected Vector3Int _BLneighbor;
    protected SpriteRenderer _renderer;
    protected BoxCollider2D _collider;
    protected bool isOpen = false;
    [SerializeField] private DoorDirection _faceDirection;

    public Vector3Int Position { get => _position; }
    public Vector3Int TRNeighbor { get => _TRneighbor; }   //Neighbor above/to the right of door, depending on orientation.
    public Vector3Int BLNeighbor { get => _BLneighbor; }   //Neighbor below/to the left of door, depending on orientation.
    public DoorDirection FaceDirection { get => _faceDirection; }

    //Anyways, this event is called when entering a new room; passes Vector3Int
    //of the sector for the new room, and the player that entered it.
    //This allows Multiplayer to move the camera if the player that entered a room
    //is the player it is following(?)
    public event Action<Vector3Int, PlayerControl> OnNewRoomEntered;

    private void Awake()
    {
        //Init data
        _wallTilemap = LevelGenerator.Instance.WallsTilemap;

        _renderer = gameObject.GetComponent<SpriteRenderer>();
        _collider = gameObject.GetComponent<BoxCollider2D>();
        _renderer.sortingOrder = 5;

        gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
    }

    public virtual Vector2Int[] GetLayoutDoorPos(RoomBase currRoom)
    {
        //Position of door relative to origin of room (which is 0,0 on RoomData texture)
        Vector2Int relativePos = new Vector2Int(Position.x - currRoom.RoomOrigin.x, Position.y - currRoom.RoomOrigin.y);

        //Move relative position to check in bounds of texture.
        //If less/greater than a boundary value (ex. layout.height), set to boundary value
        Texture2D layout = currRoom.Data.Layout;
        if (relativePos.x < 0)
            relativePos.x = 0;

        if (relativePos.y < 0)
            relativePos.y = 0;

        if (relativePos.x >= layout.width)
            relativePos.x = layout.width - 1;

        if (relativePos.y >= layout.height)
            relativePos.y = layout.height - 1;

        Vector2Int[] retArr = new Vector2Int[2];
        retArr[0] = relativePos;
        retArr[1] = (_faceDirection == DoorDirection.Vertical)   ?
                    new Vector2Int(relativePos.x - 1, relativePos.y) :
                    new Vector2Int(relativePos.x, relativePos.y - 1);
        return retArr;
    }

    // Pass in map locations of rooms so that doors can place properly relative
    // to the center of the sectors being checked by level generator. This allows
    // rooms that are larger than normal to have multiple doors in the same direction.
    // as long as those rooms have width and height as multiples of the base values.
    public void SpawnDoor(Vector3Int currRoom, Vector3Int neighbor)
    {
        //Vector pointing from current room to its neighbor. Used to determine door direction.
        Vector3 doorDir = (currRoom - neighbor);

        //Set correct door orientation based on relative location of connecting sector.
        //If room isn't neighbor, return since door is actually invalid (reflected in
        //difference in position of the rooms).
        if (doorDir.x != 0 && doorDir.y == 0)
        {
            this._faceDirection = DoorDirection.Horizontal;
        }
        else if (doorDir.y != 0 && doorDir.x == 0)
        {
            this._faceDirection = DoorDirection.Vertical;
        }
        else
        {
            Debug.Log("Error. Rooms aren't neighbors. Cannot place door.");
            Debug.Log(doorDir);
            return;
        }

        //Place door at midpoint between rooms
        Vector3Int currSectorCen = RoomBase.GetSectorCenter(currRoom);
        Vector3Int neighborSectorCen = RoomBase.GetSectorCenter(neighbor);
        int midX = (currSectorCen.x + neighborSectorCen.x) / 2;
        int midY = (currSectorCen.y + neighborSectorCen.y) / 2;
        //Offset according to orientation; accounts for rounding error of midpoint for vertical doors
        Vector3 dirOffset;
        if (_faceDirection == DoorDirection.Horizontal)
            dirOffset = Vector3.zero;
        else
            dirOffset = new Vector3(0,-0.5f, 0); //Arbitrary offset that sets vertical doors correctly.
        _position = new Vector3Int(midX, midY, currSectorCen.z);
        gameObject.transform.position = _position + dirOffset;

        //Too lazy to create architecture for something so simple, just switch the fucker
        // with correct properties needed. 
        switch (_faceDirection)
        {
            case DoorDirection.Horizontal:
                _collider.size = BaseHorizSize;
                if (currRoom.x < neighbor.x)
                {
                    _BLneighbor = currRoom;
                    _TRneighbor = neighbor;
                }
                else
                {
                    _BLneighbor = neighbor;
                    _TRneighbor = currRoom;
                }
                break;
            case DoorDirection.Vertical:
                _collider.size = BaseVertSize;
                if (currRoom.y < neighbor.y)
                {
                    _BLneighbor = currRoom;
                    _TRneighbor = neighbor;
                }
                else
                {
                    _BLneighbor = neighbor;
                    _TRneighbor = currRoom;
                }
                break;
            default:
                Debug.Log("Error, invalid door direction");
                break;
        }

        //TODO: Get index of wall/bound sprites from function in Floor script (not built yet)
        // use that same index and multiply by face direction.

        //Also there will definetely always be 2 sprite types, so fuck chaching them or getting enum length.
        _renderer.sprite = RoomUtils.GetLevelDoorSprite(_faceDirection);

        this.OpenDoor();
    }

    public virtual List<Vector3Int> GetOccupiedSpaces()
    {
        List<Vector3Int> spaces = new List<Vector3Int>();
        Vector2 colliderBaseSize;
        if (_faceDirection == DoorDirection.Horizontal)
            colliderBaseSize = BaseHorizSize;
        else
            colliderBaseSize = BaseVertSize;

        for (int x = 0; x < colliderBaseSize.x; x++)
        {
            for (int y = 0; y < colliderBaseSize.y + (int)_faceDirection; y++)
            {
                spaces.Add(new Vector3Int(_position.x - x, _position.y - y, _position.z));
            }
        }
        return spaces;
    }

    public virtual void OpenDoor()
    {
        if (isOpen)
            return;
        isOpen = true;

        _collider.isTrigger = true;

        //TODO: Create animation instead of disable sprite renderer!
        _renderer.enabled = false;
    }

    public virtual void ShutDoor()
    {
        if (!isOpen || _collider == null)
            return;
        isOpen = false;

        //Reset collider to original state.
        _collider.isTrigger = false;

        //TODO: Create animation instead of disable sprite renderer!
        _renderer.enabled = true;
    }

    //Only called when door is open, since it only becomes a trigger then.
    public virtual void OnTriggerExit2D(Collider2D col)
    {
        //If colliding with player, go to another room.
        if(col.gameObject.tag == "Player")
        {
            //We know the object has PlayerControl because it is a player.
            Vector3Int newRoomSector = Vector3Int.zero;
            float checkVelocityDir;

            //use rigidbody velocity to determine room to get. Direction to check
            //is based on door's face direction.
            if (_faceDirection == DoorDirection.Horizontal)
                checkVelocityDir = col.transform.position.x - Position.x;
            else
                checkVelocityDir = col.transform.position.y - Position.y;

            //Get correct sector relative to direction entered.
            if (checkVelocityDir < 0)
                newRoomSector = BLNeighbor;
            else if (checkVelocityDir > 0)
                newRoomSector = TRNeighbor;

            //Fire event; pass in new sector the player is in and a reference to the player
            OnNewRoomEntered(newRoomSector, col.gameObject.GetComponent<PlayerControl>());
        }
    }

    public virtual bool IsDoorAvailable(Vector3Int neighbor1, Vector3Int neighbor2)
    {
        //Ensure sectors aren't a part of the same RoomBase instance!
        if (LevelGenerator.Instance.LevelMap[neighbor1] == LevelGenerator.Instance.LevelMap[neighbor2])
            return false;

        //Check if door already exists by seeing if one already references
        //the same two rooms being checked now.
        if (this.TRNeighbor == neighbor1 && this.BLNeighbor == neighbor2)
            return false;
        if (this.BLNeighbor == neighbor1 && this.TRNeighbor == neighbor2)
            return false;

        return true;
    }

    protected void OnDestroy()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
