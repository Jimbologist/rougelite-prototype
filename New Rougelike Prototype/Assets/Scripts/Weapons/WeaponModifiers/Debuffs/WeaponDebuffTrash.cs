using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WeaponDebuffTrash : WeaponModifier
{
    [SerializeField] private StatModifier damageDown;
    [SerializeField] private StatModifier reloadDown;
    public override void ApplyEquipEffects()
    {

    }

    public override void ApplyPermEffects()
    {
        _appliedWeapon.TrueDamage.AddModifier(damageDown);
        _appliedWeapon.TrueReloadSpeed.AddModifier(reloadDown);
    }

    public override void RemoveEquipEffects()
    {

    }

    public override void RemovePermEffects()
    {
        _appliedWeapon.TrueDamage.RemoveModifier(damageDown);
        _appliedWeapon.TrueReloadSpeed.RemoveModifier(reloadDown);
    }

    protected override void ChacheStatModifiers()
    {
        _statModifiers.Add(damageDown);
        _statModifiers.Add(reloadDown);
    }

    public override string GetDescription()
    {
        StringBuilder str = new StringBuilder();

        str.Append("Damage ");
        str.Append(damageDown.ToString());
        str.Append(", and Reload Speed ");
        str.Append(reloadDown.ToString());

        return str.ToString();
    }
}
