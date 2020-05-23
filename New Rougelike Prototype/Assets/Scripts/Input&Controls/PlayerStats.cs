using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    //ADD OTHER IMPORTANT STATS WHEN NECESSARY FOR CODING OTHER MECHANICS!!!
    [SerializeField] private float moveSpeed;
    [SerializeField] private float shotSpeedMultiplier;
    [SerializeField] private float accuracyMultiplier;

    /*
     * The below functions change a player's stat based on given parameters
     * and returns the new stat if needed. Can be used to get current stat by passing zero.
     */

    public float ChangeMoveSpeed(float changeAmt)
    {
        moveSpeed += changeAmt;
        return moveSpeed;
    }

    public float ChangeShotSpeed(float changeAmt)
    {
        shotSpeedMultiplier += changeAmt;
        return shotSpeedMultiplier;
    }

    public float ChangeAccuracy(float changeAmt)
    {
        accuracyMultiplier += changeAmt;
        return accuracyMultiplier;
    }

    void Awake()
    {
        moveSpeed = 5f;
        shotSpeedMultiplier = 1.0f;
        accuracyMultiplier = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
