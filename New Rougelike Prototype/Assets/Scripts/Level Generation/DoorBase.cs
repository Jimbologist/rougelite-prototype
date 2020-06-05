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
    public enum DoorOrientation
    {
        Vertical = 1,
        Horizontal = 0
    }

    protected Vector3Int _position;
    protected Tilemap _wallTilemap;
    protected Vector3Int _neighbor1;
    protected Vector3Int _neighbor2;
    protected SpriteRenderer _renderer;
    protected BoxCollider2D _collider;
    [SerializeField] private DoorOrientation _faceDirection;

    public Vector3Int Position { get => _position; }
    public Vector3Int Neighbor1 { get => _neighbor1; }
    public Vector3Int Neighbor2 { get => _neighbor2; }
    public DoorOrientation FaceDirection { get => _faceDirection; }

    private void Awake()
    {
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
        retArr[1] = (_faceDirection == DoorOrientation.Vertical)   ?
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
            this._faceDirection = DoorOrientation.Horizontal;
        }
        else if (doorDir.y != 0 && doorDir.x == 0)
        {
            this._faceDirection = DoorOrientation.Vertical;
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
        if (_faceDirection == DoorOrientation.Horizontal)
            dirOffset = Vector3.zero;
        else
            dirOffset = new Vector3(0,-0.5f, 0);
        _position = new Vector3Int(midX, midY, currSectorCen.z);
        gameObject.transform.position = _position + dirOffset;

        //Add door to rooms' lists of doors
        LevelGenerator.Instance.LevelMap[currRoom].doors.Add(this);
        LevelGenerator.Instance.LevelMap[neighbor].doors.Add(this);

        //Too lazy to create architecture for something so simple, just switch the fucker
        // with correct properties needed. 
        switch (_faceDirection)
        {
            case DoorOrientation.Horizontal:
                _collider.size = new Vector2(2, 2);
                break;
            case DoorOrientation.Vertical:
                _collider.size = new Vector2(2, 3);
                break;
            default:
                Debug.Log("Error, invalid door direction");
                break;
        }
        //TODO: Get index of wall/bound sprites from function in Floor script (not built yet)
        // use that same index and multiply by face direction.
        int roomDesignIndex = 0;
        //Also there will definetely always be 2 sprite types, so fuck chaching them or getting enum length.
        _renderer.sprite = LevelGenerator.Instance.Floor.DoorSprites[(int)_faceDirection + (2 * roomDesignIndex)];
    }

    public virtual List<Vector3Int> GetOccupiedSpaces()
    {
        List<Vector3Int> spaces = new List<Vector3Int>();
        for (int x = 0; x < _collider.size.x; x++)
        {
            for (int y = 0; y < _collider.size.y + (int)_faceDirection; y++)
            {
                spaces.Add(new Vector3Int(_position.x - x, _position.y - y, _position.z));
            }
        }
        return spaces;
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
