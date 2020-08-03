using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Base class for any weapon modifier (buff/debuff)
 * Inherits from monobehaviour because Scriptable object isn't necessarily
 * going to work here. Since any new type of WeaponModifier will have it's
 * own specific functionality, it will have to inherit from this class. 
 * However, each instance has its own specific scripting that will listen to
 * player events (or not) while ALSO having certain data that is used in the same
 * way as all other modifiers (applicableTypes, description, etc.) that will never change!
 * 
 * So, to make a new modifier, inherit from this class, then attach the new script to 
 * any empty prefab that ONLY contains the script. Then, use Resources.Load() to
 * retreive the specific class WITH the predifined data that was given via the
 * inspector. Kinda hacky, but its a nice middle ground since we don't want to have
 * to hard code each individual piece of data as constant, and we'll still be able to 
 * creaet specific code for the inherited methods AND assign values via the inspector
 * (for example, dragging and dropping the WeaponType ScriptableObjects).
 * 
 * Finally, no mechanic is planned to be able to copy modifiers due to Weapon Rarity and
 * WeaponType restrictions. Only will be able to get new Modifiers specifically from Resources.Load,
 * and each one will start with its default values, effects, and modifiers
 */
public abstract class WeaponModifier : MonoBehaviour
{
    protected Weapon _appliedWeapon;

    //If weapon rarity and type doesn't match these lists, then don't apply modifier.
    //However, if list is empty, all types apply!
    [SerializeField] protected List<WeaponType> _applicableTypes;
    [SerializeField] protected List<Rarity> _applicableRarities;
    [SerializeField] protected List<StatModifier> _statModifiers;

    [SerializeField] protected WeaponModifierType _modifierType;
    [SerializeField] protected string _modifierDescription;
    [SerializeField] protected string _modifierPrefix;

    //Helps for listening for certain actions/applying player stat upgrades!
    public PlayerControl EquippedPlayer { get; set; }
    public List<WeaponType> ApplicableTypes { get => _applicableTypes; }
    public List<Rarity> ApplicableRarites { get => _applicableRarities; }
    public List<StatModifier> StatModifiers { get => _statModifiers; }
    public WeaponModifierType ModifierType { get => _modifierType; }
    public string ModifierDescription { get => _modifierDescription; }

    protected void Awake()
    {
        Debug.Log("Weapon Modifiers Awoke!!!!");
        ChacheStatModifiers();
        foreach(var stat in StatModifiers)
        {
            stat.source = this;
        }

        _modifierDescription = GetDescription();
    }

    //Adds modifier only if weapon type of given weapon is applicable.
    //return false if not.
    public virtual bool AddWeaponModifier(Weapon newWeapon)
    {
        if (IsWeaponTypeApplicable(newWeapon) && IsRarityApplicable(newWeapon))
        {
            _appliedWeapon = newWeapon;
            _appliedWeapon.OnWeaponEquipped += ApplyEquipEffects;
            _appliedWeapon.OnWeaponUnequipped += RemoveEquipEffects;
            newWeapon.WeaponModifiers.Add(this);
            newWeapon.WeaponModifiers.Sort(CompareTooltipPriority);
            return true;
        }
        return false;
    }

    //Removes modifier and weapon references from respective lists.
    public virtual bool RemoveWeaponModifier(Weapon newWeapon)
    {
        _appliedWeapon = null;
        _appliedWeapon.OnWeaponEquipped -= ApplyEquipEffects;
        _appliedWeapon.OnWeaponUnequipped -= RemoveEquipEffects;
        return (newWeapon.WeaponModifiers.Remove(this));
    }

    public static int CompareTooltipPriority(WeaponModifier a, WeaponModifier b)
    {
        if (a.ModifierType.TooltipPriority < b.ModifierType.TooltipPriority)
            return -1;
        else if (a.ModifierType.TooltipPriority > b.ModifierType.TooltipPriority)
            return 1;
        return 0;
    }

    private bool IsRarityApplicable(Weapon newWeapon)
    {
        if (_applicableRarities.Count == 0) return true;
        return (_applicableRarities.Contains(newWeapon.Rarity));
    }

    private bool IsWeaponTypeApplicable(Weapon newWeapon)
    {
        if (_applicableTypes.Count == 0) return true;
        return (_applicableTypes.Contains(newWeapon.WeaponType));
    }

    //Apply effects/modifiers upon weapon creation.
    public abstract void ApplyPermEffects();

    //Remove ALL effects given from ApplyPermModifiers.
    public abstract void RemovePermEffects();

    //Main effects/modifiers to apply to weapon upon weapon creation.
    public abstract void ApplyEquipEffects();
    
    //Remove main effects applied from ApplyMainEffects();
    public abstract void RemoveEquipEffects();

    //Add StatModifiers to list to allow source to be applied
    protected abstract void ChacheStatModifiers();

    //Generate the description string of the modifier. Preferably with
    //StringBuilder and using ToString representations of all StatModifiers
    public abstract string GetDescription();
}
