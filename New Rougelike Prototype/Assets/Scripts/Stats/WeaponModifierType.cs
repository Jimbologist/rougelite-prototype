using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item Data/Weapon Modifier Types", order = 2)]
public class WeaponModifierType : ScriptableObject
{
    [SerializeField] private Color _toolTipNoteColor;
    [SerializeField] private string _modifierTypeName;
    [SerializeField] private int _tooltipPriority;
    public Color TooltipNoteColor { get => _toolTipNoteColor; }
    public string ModifierTypeName { get => _modifierTypeName; }
    public int TooltipPriority { get => _tooltipPriority; }
}
