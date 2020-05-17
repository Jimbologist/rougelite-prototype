using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Test script for specific implemenation of a gun. ALL guns will
 * work similarly to this one, in that stats, quirks, rarity, etc.
 * will ALL be defined here so all can be tailored specifically to each gun.
 * Inner workings of picking up, holding, etc. that all guns will have (unless
 * some specific quirk overrides those for whatever reason) are handled in base class "Weapon"
 * 
 */
public class OtherTestGun : Weapon
{

    protected override void Awake()
    {
        //Modifiers, pivot, dropping if not equipped, etc. applied, then specific actions necessary based on weapon.
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        if (isEquipped)
        {

        }


    }

    //TO ADD: Method/functionality when on equipped, player hands move to left/rightHandPivot points
    //that are specifically on weapon prefab moved in the weapon prefab to a certain area on it.
}
