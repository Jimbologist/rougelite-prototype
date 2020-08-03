using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: This base class will implement attributes that all items will have.
//  This includes: Ability to draw UI to screen for any type of LootableGear,
//  Ability to be placed in inventory/backpack and equipped,
//  Values that all gear will have, such as sellPrice and premSellPrice,
//  strings for full name and base name, etc.
public abstract class Lootable : MonoBehaviour, IInteractable, IFakeHeight
{
    [Header("Lootable Info")]
    [SerializeField] protected Rarity rarity;
    [SerializeField] protected string baseName;
    [SerializeField] protected string baseDescription;
    [SerializeField] protected string flavorText;
    [SerializeField] protected int sellPrice;
    [SerializeField] protected int premSellPrice;

    //Getters for private fields declared above
    public string BaseName { get => baseName; }
    public int SellPrice { get => sellPrice; }
    public int PremSellPrice { get => premSellPrice; }
    public Rarity Rarity {  get { return rarity;  } }
    public FakeHeightObject FakeHeight { get; set; }

    /**
     * Monobehaviour methods
     */
    protected virtual void Awake()
    {
    }

    /**
     * UI abstract methods; Derived classes must implement these to function w/ UI.
     */
    public abstract string GetColoredName();

    public abstract string GetTooltipStatsText();

    public abstract string GetTooltipNotesText();

    public abstract void OnFakeHeightTriggerEnter(Collider2D collider);

    public abstract void OnFakeHeightTriggerExit(Collider2D collider);

    public abstract void OnFakeHeightTriggerStay(Collider2D collider);
}
