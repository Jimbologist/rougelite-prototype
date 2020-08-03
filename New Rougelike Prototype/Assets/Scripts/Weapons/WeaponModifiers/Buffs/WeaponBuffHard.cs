using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WeaponBuffHard : WeaponModifier
{
    [SerializeField] private StatModifier damageBuff;

    public override void ApplyPermEffects()
    {
        _appliedWeapon.TrueDamage.AddModifier(damageBuff);
    }

    public override void RemovePermEffects()
    {
        _appliedWeapon.TrueDamage.RemoveModifier(damageBuff);
    }


    public override void ApplyEquipEffects()
    {
        
    }
    public override void RemoveEquipEffects()
    {

    }

    protected override void ChacheStatModifiers()
    {
        _statModifiers.Add(damageBuff);
    }

    public override string GetDescription()
    {
        StringBuilder str = new StringBuilder();

        str.Append("Damage ");
        str.Append(damageBuff.ToString());

        return str.ToString();
    }
}
