using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu] 
public class Rarity : ScriptableObject
{
    [SerializeField] private new string name;
    [SerializeField] private Color rarityColor;

    public string Name { get { return name; } }
    public Color RarityColor { get {return rarityColor; } }
}
