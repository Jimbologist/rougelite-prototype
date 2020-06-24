using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClearEvent : MonoBehaviour
{
    [SerializeField] private RoomBase currRoom;
    [SerializeField] private BoxCollider2D colliderBox;
    // Start is called before the first frame update
    void Start()
    {
        colliderBox = gameObject.GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetRoom(RoomBase room)
    {
        currRoom = room;
        gameObject.transform.position = currRoom.gameObject.transform.position + new Vector3Int(4,7,0);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.gameObject.GetComponent<Bullet>() != null || col.gameObject.GetComponent<PlayerControl>() != null)
        {
            currRoom.TestRoomClear();
        }
    }
}
