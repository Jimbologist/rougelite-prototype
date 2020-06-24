using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : Singleton<CameraBehaviour>
{
    public Camera mainCamera;
    public PlayerControl trackedPlayer;
    private Vector3 velocity;

    private Coroutine currMovementCoroutine;

    protected override void Awake()
    {
        mainCamera = Camera.main;
        velocity = Vector3.zero;
    }

    public void MoveToNewRoom(Vector3Int newSector, PlayerControl player)
    {
        Debug.Log("Moving to new room");
        if (player != trackedPlayer)
            return;

        Vector3Int newPos = RoomBase.GetSectorCenter(newSector);
        if(currMovementCoroutine != null)
            StopCoroutine(currMovementCoroutine);
        currMovementCoroutine = StartCoroutine(EaseToNewPos(newPos));
    }

    public IEnumerator EaseToNewPos(Vector3 newPos)
    {
        Debug.Log("Camera moved");
        newPos.z = transform.position.z;
        while (Vector2.Distance(transform.position, newPos) > 0.01f)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, newPos, 0.2f);
            yield return null;
        }
        yield return null;
    }
}
