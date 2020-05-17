using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine;

/**
 * This is a base class for any weapons that can be used by the player to attack.
 * Any weapon prefab MUST have a left hand pivot OR no pivots at all for player hands to function.
 * (Basically cannot have only a right hand pivot!)
 * 
 * Also holds all weapon stats, attributes, and components.
 */
public class Weapon : Lootable
{
    //Base stats for all weapons; all need getters so separate script can 
    //randomize and decide a name based on these.
    [Header("Weapon Stats")]
    [SerializeField] protected string fullName;
    [SerializeField] protected string flavorText;
    [SerializeField] protected float baseDamage;
    [SerializeField] protected float baseAccuracy;
    [SerializeField] protected float baseFireDelay;
    [SerializeField] protected float baseReloadSpeed;
    [SerializeField] protected float baseShotSpeed;
    [SerializeField] protected int baseMagSize;
    [SerializeField] protected int baseAmmoExpended;
    [SerializeField] protected Bullet mainBullet;

    [Header("True Stats")]
    [SerializeField] protected float trueDamage;
    [SerializeField] protected float trueAccuracy;
    [SerializeField] protected float trueFireDelay;
    [SerializeField] protected float trueReloadSpeed;
    [SerializeField] protected float trueShotSpeed;
    [SerializeField] protected int trueMagSize;
    [SerializeField] protected int trueAmmoExpended;

    [Header("Debug")]
    [SerializeField] protected bool isEquipped;
    [SerializeField] protected BoxCollider2D pickupBox;
    [SerializeField] protected Transform leftHandPivot;
    [SerializeField] protected Transform rightHandPivot;
    [SerializeField] protected Transform muzzleTransform;
    [SerializeField] protected PlayerControl currentPlayer;


    //Placeholder weapon types. Subject to change.
    //Other type has own exclusive ammo type; typically very rare and unique.
    //Melee weapons supress default Bullet object, unless otherwise specified.
    [SerializeField] protected enum weaponType
    {
        Pistol,
        SMG,
        AR,
        LMG,
        Shotgun,
        Launcher,
        Melee,
        Other
    }

    public Transform LeftHandPivot { get => leftHandPivot; }
    public Transform RightHandPivot { get => rightHandPivot; }

    public float TrueDamage { get => trueDamage; }
    public float TrueAccuracy { get => trueAccuracy; }
    public float TrueFireDelay { get => trueFireDelay; }
    public float TrueReloadSpeed { get => trueReloadSpeed; }
    public float TrueShotSpeed { get => trueShotSpeed; }
    public int TrueMagSize { get => trueMagSize; }
    public int TrueAmmoExpended { get => trueAmmoExpended; }

    protected virtual void Awake()
    {
        leftHandPivot = gameObject.transform.Find("leftHandPivot");
        rightHandPivot = gameObject.transform.Find("rightHandPivot");
        muzzleTransform = gameObject.transform.Find("shotPosition");
        pickupBox = gameObject.GetComponent<BoxCollider2D>();
    }

    //virtual firing method that shoots gun based on fire rate,
    //assuming its value = shots per second. Possible override uses base.fire(), for example,
    //and adds other attributes of shooting the gun based on uniqueness or lack thereof.
    public virtual void FireWeapon(float shotSpeedMultiplier, Vector3 playerAimPosition, Vector3 playerPosition)
    {
        Bullet newBullet = Instantiate(mainBullet, muzzleTransform.position, Quaternion.identity);
        Vector2 shootDir;

        shootDir = (playerAimPosition - playerPosition);
        shootDir = shootDir.normalized;

        //If crosshair is between player and muzzle, reverse direction vector to avoid shooting backwards.
        //Also if crosshair is in short distance of muzzle, don't reverse shootDir.
        if (Vector3.Distance(playerPosition, muzzleTransform.position) > Vector3.Distance(playerPosition, playerAimPosition))
        {
            shootDir = -(shootDir);
        }

        float finalShotSpeed = shotSpeedMultiplier * baseShotSpeed;
        float finalAccuracy = baseAccuracy * currentPlayer.GetPlayerStats().ChangeAccuracy(0f);
        newBullet.OnShoot(finalShotSpeed, shootDir, finalAccuracy);
    }

    //Drops instance of weapon on ground that can be viewed.
    //Ideally, can work regardless of source of drop after stats have been determined.
    public virtual void DropWeapon()
    {

    }

    public virtual void Equip(PlayerControl pickupPlayer)
    {
        this.currentPlayer = pickupPlayer;

        if(leftHandPivot != null)
            leftHandPivot.gameObject.SetActive(true);
        if(rightHandPivot != null)
            rightHandPivot.gameObject.SetActive(true);

        //Set collision box to zero to avoid player collsion while equipped.
        gameObject.SetActive(true);
        pickupBox.enabled = false;
        isEquipped = true;
    }

    public virtual void Unequip()
    {
        if (leftHandPivot != null)
            leftHandPivot.gameObject.SetActive(true);
        if (rightHandPivot != null)
            rightHandPivot.gameObject.SetActive(true);

        gameObject.SetActive(false);
    }

    /**
     * UI Method implementations
     */
    
    //Returns colored full name of weapon instance based on rarity on prefab.
    public override string GetColoredName()
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(Rarity.RarityColor);
        return $"<color=#{hexColor}>{fullName}</color>\n";
    }

    //Returns string of all necessary info for stat section of tooltip UI.
    public override string GetTooltipStatsText()
    {
        StringBuilder text = new StringBuilder();

        text.Append("<align=left>\u2022  Damage: <line-height=0.001%>\n").Append("<align=right>").Append(TrueDamage.ToString("#.##")).Append("</line-height>\n");
        text.Append("<align=left>\u2022  Accuracy: <line-height=0.001%>\n").Append("<align=right>").Append(TrueAccuracy).Append("</line-height>\n");
        text.Append("<align=left>\u2022  Reload Speed: <line-height=0.001%>\n").Append("<align=right>").Append(TrueReloadSpeed).Append("</line-height>\n");
        text.Append("<align=left>\u2022  Fire Rate: <line-height=0.001%>\n").Append("<align=right>").Append((1/TrueFireDelay).ToString("#.##")).Append("</line-height>\n");
        text.Append("<align=left>\u2022  Magazine: <line-height=0.001%>\n").Append("<align=right>").Append(TrueMagSize).Append("</line-height>\n");

        return text.ToString();
    }

    //Returns string containing all notes for tooltip UI (buffs/nerfs/flavored text/abilities)
    public override string GetTooltipNotesText()
    {
        //TODO: Add Functionality to get "notes" that come with buffs/nerfs for weapon (ex. +13% damage)
        //Right now, only gets flavor text.
        StringBuilder text = new StringBuilder();

        if (flavorText != null)
            text.Append("<align=left>\u2022  <color=#FF008F>").Append(flavorText).AppendLine();
        text.Append("</color><i>").Append(Rarity.Name).Append("</i>");
        return text.ToString();
    }
}