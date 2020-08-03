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
 [RequireComponent(typeof(FakeHeightObject))]
public class Weapon : Lootable
{
    //Base stats for the weapon; only apply modifiers inherent
    //to the weapon instance itself to these (buffs, debuffs, rarity, etc.)
    [Header("Base Weapon Stats")]
    [SerializeField] protected Stat damage;
    [SerializeField] protected Stat accuracy;
    [SerializeField] protected Stat fireDelay;
    [SerializeField] protected Stat reloadSpeed;
    [SerializeField] protected Stat shotSpeed;
    [SerializeField] protected Stat magSize;
    [SerializeField] protected Stat ammoExpended;
    [SerializeField] protected Bullet mainBullet;

    //True stats for the weapon; base value of these stats should ALWAYS
    //be equal to the final value of the base stats on the weapon. Use given
    //setter methods to do so easily. These stats are only for the purpose of
    //easily using the correct stats for a specific player after taking their
    //items, skills, etc. into account.
    [Header("True Stats")]
    [SerializeField] protected List<WeaponModifier> weaponModifiers;
    [SerializeField] protected WeaponType weaponType;
    [SerializeField] protected Stat trueDamage;
    [SerializeField] protected Stat trueAccuracy;
    [SerializeField] protected Stat trueFireDelay;
    [SerializeField] protected Stat trueReloadSpeed;
    [SerializeField] protected Stat trueShotSpeed;
    [SerializeField] protected Stat trueMagSize;
    [SerializeField] protected Stat trueAmmoExpended;
    
    [Header("Debug")]
    [SerializeField] protected bool isEquipped;
    [SerializeField] protected BoxCollider2D pickupBox;
    [SerializeField] protected Transform leftHandPivot;
    [SerializeField] protected Transform rightHandPivot;
    [SerializeField] protected Transform muzzleTransform;
    [SerializeField] protected PlayerControl currentPlayer;

    public PlayerControl CurrentPlayer { get => currentPlayer; }
    public Transform LeftHandPivot { get => leftHandPivot; }
    public Transform RightHandPivot { get => rightHandPivot; }
    public WeaponType WeaponType { get => weaponType; }

    //Note: Automatically added to and sorted when using Add/Remove Modifier
    //methods on a WeaponModifier object.
    public List<WeaponModifier> WeaponModifiers { get => weaponModifiers; }
      
    public SeedRandom WeaponRNG { get; private set; }
    public float BaseDamage { get => damage.FinalValue; }
    public float BaseAccuracy { get => accuracy.FinalValue; }
    public float BaseFireDelay { get => fireDelay.FinalValue; }
    public float BaseReloadSpeed { get => reloadSpeed.FinalValue; }
    public float BaseShotSpeed { get => shotSpeed.FinalValue; }
    public float BaseMagSize { get => magSize.FinalValue; }
    public float BaseAmmoExpended { get => ammoExpended.FinalValue; }

    public Stat TrueDamage { get => trueDamage; }
    public Stat TrueAccuracy { get => trueAccuracy; }
    public Stat TrueFireDelay { get => trueFireDelay; }
    public Stat TrueReloadSpeed { get => trueReloadSpeed; }
    public Stat TrueShotSpeed { get => trueShotSpeed; }
    public Stat TrueMagSize { get => trueMagSize; }
    public Stat TrueAmmoExpended { get => trueAmmoExpended; }

    public delegate void WeaponEquipped();
    public event WeaponEquipped OnWeaponEquipped;

    public delegate void WeaponUnequipped();
    public event WeaponUnequipped OnWeaponUnequipped;

    #region ModifierMethods

    /**
     * All modifier methods for base and true weapon stats.
     * When Modifying/Setting base stat, the true stat's base
     * value should always get updated in the method itself.
     * 
     * When Modifying/Setting the true stat, the base stat is
     * unaffected unless specified in a boolean method parameter.
     * If using the non-boolean overload, it will default to 
     * updating the true stat according the the base stat.
     *
     * Also fuck you if you think this is too many methods. I'm
     * not about to create a whole-ass structure just because I want
     * these stats to behave differently. Almost nothing else in the
     * game will behave this way.
     */

    //Damage methods
    public void ModifyBaseDamage(StatModifier mod)
    {
        damage.AddModifier(mod);
        trueDamage.baseValue = damage.FinalValue;
    }

    public void SetBaseDamage(float newValue, bool setTrueValue)
    {
        damage.baseValue = newValue;
        if (setTrueValue)
            trueDamage.baseValue = damage.FinalValue;
    }

    public void SetBaseDamage(float newValue)
    {
        damage.baseValue = newValue;
        trueDamage.baseValue = damage.FinalValue;
    }

    //Accuracy methods
    public void ModifyBaseAccuracy(StatModifier mod)
    {
        accuracy.AddModifier(mod);
        trueAccuracy.baseValue = accuracy.FinalValue;
    }

    public void SetBaseAccuracy(float newValue, bool setTrueValue)
    {
        accuracy.baseValue = newValue;
        if (setTrueValue)
            trueAccuracy.baseValue = accuracy.FinalValue;
    }

    public void SetBaseAccuracy(float newValue)
    {
        accuracy.baseValue = newValue;
        trueAccuracy.baseValue = accuracy.FinalValue;
    }

    //Fire Delay methods
    public void ModifyBaseFireDelay(StatModifier mod)
    {
        fireDelay.AddModifier(mod);
        trueFireDelay.baseValue = fireDelay.FinalValue;
    }

    public void SetBaseFireDelay(float newValue, bool setTrueValue)
    {
        fireDelay.baseValue = newValue;
        if (setTrueValue)
            trueFireDelay.baseValue = fireDelay.FinalValue;
    }

    public void SetBaseFireDelay(float newValue)
    {
        fireDelay.baseValue = newValue;
        trueFireDelay.baseValue = fireDelay.FinalValue;
    }

    //Reload Speed methods
    public void ModifyBaseReloadSpeed(StatModifier mod)
    {
        reloadSpeed.AddModifier(mod);
        trueReloadSpeed.baseValue = reloadSpeed.FinalValue;
    }

    public void SetBaseReloadSpeed(float newValue, bool setTrueValue)
    {
        reloadSpeed.baseValue = newValue;
        if (setTrueValue)
            trueReloadSpeed.baseValue = reloadSpeed.FinalValue;
    }

    public void SetBaseReloadSpeed(float newValue)
    {
        reloadSpeed.baseValue = newValue;
        trueReloadSpeed.baseValue = reloadSpeed.FinalValue;
    }

    //Shot Speed methods
    public void ModifyBaseShotSpeed(StatModifier mod)
    {
        shotSpeed.AddModifier(mod);
        trueShotSpeed.baseValue = shotSpeed.FinalValue;
    }

    public void SetBaseShotSpeed(float newValue, bool setTrueValue)
    {
        shotSpeed.baseValue = newValue;
        if (setTrueValue)
            trueShotSpeed.baseValue = shotSpeed.FinalValue;
    }

    public void SetBaseShotSpeed(float newValue)
    {
        shotSpeed.baseValue = newValue;
        trueShotSpeed.baseValue = shotSpeed.FinalValue;
    }

    //MagSize methods
    public void ModifyBaseMagSize(StatModifier mod)
    {
        magSize.AddModifier(mod);
        trueMagSize.baseValue = magSize.FinalValue;
    }

    public void SetBaseMagSize(float newValue, bool setTrueValue)
    {
        magSize.baseValue = newValue;
        if (setTrueValue)
            trueMagSize.baseValue = magSize.FinalValue;
    }

    public void SetBaseMagSize(float newValue)
    {
        magSize.baseValue = newValue;
        trueMagSize.baseValue = magSize.FinalValue;
    }

    //AmmoExpended methods
    public void ModifyBaseAmmoExpended(StatModifier mod)
    {
        ammoExpended.AddModifier(mod);
        trueAmmoExpended.baseValue = ammoExpended.FinalValue;
    }

    public void SetBaseAmmoExpended(float newValue, bool setTrueValue)
    {
        ammoExpended.baseValue = newValue;
        if (setTrueValue)
            trueAmmoExpended.baseValue = ammoExpended.FinalValue;
    }

    public void SetBaseAmmoExpended(float newValue)
    {
        ammoExpended.baseValue = newValue;
        trueAmmoExpended.baseValue = ammoExpended.FinalValue;
    }

    #endregion

    /**
     * Monobehaviour Methods:
     */

    protected override void Awake()
    {
        base.Awake();
        leftHandPivot = gameObject.transform.Find("leftHandPivot");
        rightHandPivot = gameObject.transform.Find("rightHandPivot");
        muzzleTransform = gameObject.transform.Find("shotPosition");
        pickupBox = gameObject.GetComponent<BoxCollider2D>();

        if (weaponType == null)
            throw new Exception("Error. Weapon " + baseName + " does not have a WeaponType!!");

        //Get random WeaponModifiers upon spawning, then apply
        //them along with Rarity damage buff etc.
        //Right now, only gets 1-2, but will later get more based on
        //level and/or other variables.
        WeaponRNG = new SeedRandom(LevelGenerator.Instance.RNGInitialValue);
        for(int numMods = 0; numMods <= WeaponRNG.State % 2; numMods++)
        {
            //TODO: Create RNG Manager Singleton that contains pools of items that will need to
            // be randomly instantiated at run time, including WeaponModifiers, Weapons, and Enemies.

            //WeaponModifier newMod = Resources.Load<WeaponModifier>("Weapon Modifiers")
        }
    }

    //virtual firing method that shoots gun based on fire rate,
    //assuming its value = shots per second. Possible override uses base.fire(), for example,
    //and adds other attributes of shooting the gun based on uniqueness or lack thereof.
    public virtual void FireWeapon(float shotSpeedMultiplier, Vector3 playerAimPosition)
    {
        if (LevelGenerator.Instance.WallsTilemapCollider.OverlapPoint(muzzleTransform.position)) return;
        Vector3 playerPosition = CurrentPlayer.transform.position;
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

        float finalShotSpeed = shotSpeedMultiplier * shotSpeed.FinalValue;
        float finalAccuracy = accuracy.FinalValue * currentPlayer.GetPlayerStats().ChangeAccuracy(0f);
        newBullet.OnShoot(finalShotSpeed, shootDir, finalAccuracy, this);
    }

    //Drops instance of weapon on ground that can be viewed.
    //Ideally, can work regardless of source of drop after stats have been determined.
    public virtual void DropWeapon()
    {

    }
    
    //TODO: REALLY IMPORTANT TO TRY WHEN THE TIME COMES:
    // Move players "aim" obj transform is lined up in world space (ignoring transform relative to parents)
    // is lined up with the weapon's shot position. This will be difficult to work out, but if
    // it this is fulfilled, shots will be accurate with the crosshair!!
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
        FakeHeight.SetShadowVisibility(false);

        OnWeaponEquipped();
    }

    public virtual void Unequip()
    {
        if (leftHandPivot != null)
            leftHandPivot.gameObject.SetActive(true);
        if (rightHandPivot != null)
            rightHandPivot.gameObject.SetActive(true);

        gameObject.SetActive(false);
        FakeHeight.SetShadowVisibility(true);

        OnWeaponUnequipped();
    }

    /**
     * UI Method implementations
     */
    
    //Returns colored full name of weapon instance based on rarity on prefab.
    public override string GetColoredName()
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(Rarity.RarityColor);
        return $"<color=#{hexColor}>{baseName}</color>\n";
    }

    //Returns string of all necessary info for stat section of tooltip UI.
    public override string GetTooltipStatsText()
    {
        StringBuilder text = new StringBuilder();

        text.Append("<align=left>\u2022  Damage: <line-height=0.001%>\n").Append("<align=right>").Append(TrueDamage.FinalValue.ToString("#.##")).Append("</line-height>\n");
        text.Append("<align=left>\u2022  Accuracy: <line-height=0.001%>\n").Append("<align=right>").Append(TrueAccuracy.FinalValue.ToString("#.#")).Append("</line-height>\n");
        text.Append("<align=left>\u2022  Reload Speed: <line-height=0.001%>\n").Append("<align=right>").Append(TrueReloadSpeed.FinalValue.ToString("#.#")).Append("</line-height>\n");
        text.Append("<align=left>\u2022  Fire Rate: <line-height=0.001%>\n").Append("<align=right>").Append((1/TrueFireDelay.FinalValue).ToString("#.##")).Append("</line-height>\n");
        text.Append("<align=left>\u2022  Magazine: <line-height=0.001%>\n").Append("<align=right>").Append(TrueMagSize.FinalValue).Append("</line-height>\n");

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

    public override void OnFakeHeightTriggerEnter(Collider2D collider)
    {
    }

    public override void OnFakeHeightTriggerExit(Collider2D collider)
    {
    }

    public override void OnFakeHeightTriggerStay(Collider2D collider)
    {
    }
}