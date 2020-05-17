using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateBase : StateMachineBehaviour
{
    protected PlayerControl pControl;

    //Gets player stats/info script on player using animator,
    //as our PlayerStateBase script handles the player animator.
    public PlayerControl GetPlayerControl(Animator animator)
    {
        if(pControl == null)
            pControl = animator.GetComponentInParent<PlayerControl>();

        return pControl;
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }
}
