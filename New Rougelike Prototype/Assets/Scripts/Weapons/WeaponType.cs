using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item Data/Weapon Types", order = 1)]
public class WeaponType : ScriptableObject
{
    [SerializeField] private int _maxAmmoCap;
    [SerializeField] private string _name;
    [SerializeField] private Sprite _ammoSprite;

    public int MaxAmmoCap { get { return _maxAmmoCap; } }
    public string Name { get { return _name; } }
    public Sprite AmmoSprite { get { return _ammoSprite; } }
}
