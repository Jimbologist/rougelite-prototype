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
 */
public abstract class WeaponModifier : MonoBehaviour
{
    [SerializeField] private Weapon _appliedWeapon;
    [SerializeField] private List<WeaponType> _applicableTypes;
    [SerializeField] private List<StatModifier> _statModifiers;
    [SerializeField] private WeaponModifierType _modifierType;
    [SerializeField] private PlayerControl _equippedPlayer;
    [SerializeField] private string _modifierDescription;

    public List<WeaponType> ApplicableTypes { get => _applicableTypes; }
    public WeaponModifierType ModifierType { get => _modifierType; }

    //Helps for listening for certain actions/applying player stat upgrades!
    public PlayerControl EquippedPlayer { get => _equippedPlayer; } 
    public string ModifierDescription { get => _modifierDescription; }

    //Adds modifier only if weapon type of given weapon is applicable.
    //return false if not.
    public virtual bool AddModifier(Weapon newWeapon)
    {
        if (_applicableTypes.Contains(newWeapon.WeaponType))
        {
            _appliedWeapon = newWeapon;
            newWeapon.WeaponModifiers.Add(this);
            newWeapon.WeaponModifiers.Sort(CompareTooltipPriority);
            return true;
        }
        return false;
    }

    //Removes modifier and weapon references from respective lists.
    public virtual bool RemoveModifier(Weapon newWeapon)
    {
        _appliedWeapon = null;
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

    public abstract void ApplyMainEffects();

    public abstract void RemoveMainEffects();
}
