using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WeaponBuffZany : WeaponModifier
{
    [SerializeField] private StatModifier reloadAndAccBuff;

    public override void ApplyPermEffects()
    {
        _appliedWeapon.TrueAccuracy.AddModifier(reloadAndAccBuff);
        _appliedWeapon.TrueReloadSpeed.AddModifier(reloadAndAccBuff);
    }

    public override void RemovePermEffects()
    {
        _appliedWeapon.TrueAccuracy.RemoveModifier(reloadAndAccBuff);
        _appliedWeapon.TrueReloadSpeed.RemoveModifier(reloadAndAccBuff);
    }

    public override void ApplyEquipEffects()
    {
        _appliedWeapon.CurrentPlayer.GetPlayerStats().ChangeMoveSpeed(1f);
    }

    public override void RemoveEquipEffects()
    {
        _appliedWeapon.CurrentPlayer.GetPlayerStats().ChangeMoveSpeed(-1f);
    }

    protected override void ChacheStatModifiers()
    {
        _statModifiers.Add(reloadAndAccBuff);
    }

    public override string GetDescription()
    {
        StringBuilder str = new StringBuilder();

        str.Append("Reload Speed and Accuracy ");
        str.Append(reloadAndAccBuff.ToString());

        return str.ToString();
    }
}
