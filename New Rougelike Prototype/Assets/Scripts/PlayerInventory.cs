using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * PLEASE NOTE: This and PlayerControl were one of the first scripts made in this
 *  project and are not designed well in many ways. If you are reading this, it still
 *  hasn't been refactored and may have aspects that seem inefficient/redundant!!
 */

public class PlayerInventory : MonoBehaviour
{
    //NOTE: THIS IS A CONSTANT HERE FOR NOW, MAY CHANGE TO BE BASED ON SAVE FILE!!!
    public static readonly int MAX_WEAPONS = 4;

    //Index to weapon to display as equipped and allow it to fire.
    [SerializeField] private int currWeapon;
    [SerializeField] private int currNearInteraction;

    //PlayerControl reference to mainly receive event calls.
    [SerializeField] private PlayerControl pControl;

    //Lets player know if a new weapon has been equipped (either picked up
    //or swapped to). Allows new one to be instantiated in player's hands.
    [SerializeField] private bool newWeaponEquipped;

    //Only allow public setting to false for this particular bool.
    public bool NewWeaponEquipped { get => newWeaponEquipped; set => newWeaponEquipped = false; }

    /**
     * LISTS/COUNTS OF AMMO AND LOOTABLES IN INVENTORY
     */

    //List of weapons currently in inventory
    //NOTE: initialized with size of 5, but may later be based on player's
    //save file if upgrade is implemented that can increase inventory size!!
    [SerializeField] private List<Weapon> weaponInventory;
    [SerializeField] private List<IInteractable> nearbyInteractables;
    //[SerializeField] private int 

    public List<Weapon> GetWeaponInventory { get => weaponInventory; }
    public List<IInteractable> GetNearbyInteractions { get => nearbyInteractables; }

    public event Action<Lootable> lootEquipped;

    /**
    * UNITY ENGINE METHODS
    */

    private void Awake()
    {
        weaponInventory = new List<Weapon>(5);
        nearbyInteractables = new List<IInteractable>();
        pControl = GetComponentInChildren<PlayerControl>();

        currWeapon = 0;
        currNearInteraction = 0;
        newWeaponEquipped = false;

        pControl.playerInteraction += UseInteract;
    }

    private void OnDestroy()
    {
        pControl.playerInteraction -= UseInteract;
    }

    /**
     * GENERAL LOOTABLE INVENTORY METHODS,
     * APPLIES TO ITEMS, WEAPONS, ETC. (ALL LOOTABLES)
     */

    //Add weapon/item/pickup to inventory. If already full, replace current weapon with new one.
    public void AddLootable(Lootable pickupLootable)
    {
        //Add lootable to weapon inventory if it's a weapon.
        if (pickupLootable is Weapon)
        {
            //If weapon added and inventory full, drop current weapon.
            if (GetWeaponCount() == MAX_WEAPONS)
            {
                //TODO: Add functionality to drop current weapon AND replace its slot with new one.
                weaponInventory[currWeapon] = (Weapon)pickupLootable;
                newWeaponEquipped = true;
                return;
            }

            //Add weapon to inventory, and set currWeapon index to end element.
            weaponInventory.Add((Weapon)pickupLootable);
            //Hide tooltip; if another interractable is in place,
            if(TooltipPopup.GetTooltipObject().activeSelf)
            {
                TooltipPopup.HideInfo();
            }
            currWeapon = GetWeaponCount() - 1;
            Debug.Log("lootable added!");
            newWeaponEquipped = true;
        }
    }

    /**
     * WEAPON INVENTORY METHODS
     */

    //Return weapon supposed to be currently equipped by player.
    public Weapon GetCurrWeapon()
    {
        if (weaponInventory.Count > 0)
        {
            return weaponInventory[currWeapon];
        }
        return null;
    }

    //Return number of weapons in inventory
    public int GetWeaponCount()
    {
        return weaponInventory.Count;
    }

    //Currently swaps in order of elements in inventory list,
    //may have functionality later for 1-4 keys equipping corresponding weapon.
    public void SwapWeapon()
    {
        currWeapon++;
        if (currWeapon > GetWeaponCount() - 1)
        {
            currWeapon = 0;
        }
        newWeaponEquipped = true;
    }

    /**
     * NEARBY INTERACTABLE METHODS
     */
    
    //If interractable is lootable, add it to inventory instead of firing an interraction event.
    public void UseInteract(IInteractable recentInteract)
    {
        if (recentInteract is Lootable)
        {
            RemoveInteractable(recentInteract);
            if(GetWeaponCount() != 0) GetCurrWeapon().Unequip();
            AddLootable((Lootable)recentInteract);
            CheckNextInteractable();
        }
    }

    //Add nearby lootable to interact with, ideally in shop or dropped on ground
    public void AddInteractable(IInteractable collidedInteraction)
    {
        if((Lootable)collidedInteraction != GetCurrWeapon())
            nearbyInteractables.Add(collidedInteraction);
    }

    //Remove nearby lootable to interact with, ideally when no longer in pickup trigger.
    public void RemoveInteractable(IInteractable collidedInteraction)
    {
        nearbyInteractables.Remove(collidedInteraction);
    }

    //Get interactable at index selected by player currently
    public IInteractable GetCurrInteractable()
    {
        if(nearbyInteractables.Count > 0)
            return nearbyInteractables[currNearInteraction];
        return null;
    }

    //If multiple interactions possible, go to next index in list.
    public void CheckNextInteractable()
    {
        //Increment nearby lootable to view. If next to check out
        //of list bounds, reset back to lootable zero.
        currNearInteraction++;
        

        if (currNearInteraction >= nearbyInteractables.Count)
            currNearInteraction = 0;
    }
}
