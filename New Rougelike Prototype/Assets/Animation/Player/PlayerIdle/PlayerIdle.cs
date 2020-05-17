using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdle : PlayerStateBase
{
    private Vector2 currMovement;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Subscribe all actions possible in idle state (or any state) to PlayerActions.
        GetPlayerControl(animator).pActions.MainActions.Move.performed += ctx => currMovement = ctx.ReadValue<Vector2>();
        
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Check player inputs for movement; does not move player!
        if (GetPlayerControl(animator).Movement != Vector2.zero)
        {
            //player_idle -> player_walk if zero movement.
            animator.SetBool("isMoving", true);
            return;
        }

    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GetPlayerControl(animator).pActions.MainActions.Move.performed -= ctx => currMovement = ctx.ReadValue<Vector2>();
    }
}
