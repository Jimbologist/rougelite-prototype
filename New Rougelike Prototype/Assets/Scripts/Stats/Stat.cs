using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

[Serializable]
public class Stat
{
    public float baseValue;

    public float FinalValue 
    {   
        get 
        {
            if (_isDirty || baseValue != _lastBaseValue)
            {
                _lastBaseValue = baseValue;
                _finalValue = CalculateFinalValue();
                _isDirty = false;
            }
            return _finalValue;
        } 
    }
    public int FinalValueInt { get => Mathf.RoundToInt(FinalValue); }

    [SerializeField] protected float _finalValue;
    protected float _lastBaseValue = float.MinValue;
    protected bool _isDirty = true;

    protected readonly List<StatModifier> _statModifiers;
    public readonly ReadOnlyCollection<StatModifier> StatModifiers;

    public Stat()
    {
        this.baseValue = 0f;
        this._finalValue = baseValue;
        _statModifiers = new List<StatModifier>();
        StatModifiers = _statModifiers.AsReadOnly();
    }

    public Stat(float baseValue)
    {
        this.baseValue = baseValue;
        this._finalValue = baseValue;
        _statModifiers = new List<StatModifier>();
        StatModifiers = _statModifiers.AsReadOnly();
    }

    public void AddModifier(StatModifier mod)
    {
        _isDirty = true;
        _statModifiers.Add(mod);
        _statModifiers.Sort(CompareModifierOrder);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        if(_statModifiers.Remove(mod))
        {
            _isDirty = true;
            return true;
        }
        return false;
    }

    public bool EditModifier(StatModifier mod, float newValue)
    {
        if (!_statModifiers.Contains(mod))
            return false;

        mod.SetModifierValue(newValue);
        _isDirty = true;
        return true;
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        //Remove all items with same reference as source. Return true if any were removed.
        return _statModifiers.RemoveAll(statModifier => statModifier.source == source) > 0;
    }

    public List<StatModifier> GetModifiersFromSource(object source)
    {
        return _statModifiers.FindAll(statModifier => statModifier.source == source);
    }

    public void RemoveAllModifiers()
    {
        _statModifiers.Clear();
    }

    protected int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.order < b.order)
            return -1;
        else if (a.order > b.order)
            return 1;
        return 0;
    }

    protected float CalculateFinalValue()
    {
        float finalValue = baseValue;
        float sumPercentAdd = 0;

        for(int i = 0; i < _statModifiers.Count; i++)
        {
            StatModifier mod = _statModifiers[i];

            if (mod.Nullified) continue;

            switch (mod.type)
            {
                case StatModType.Flat:
                    finalValue += mod.value;
                    break;
                case StatModType.PercentAdd:
                    sumPercentAdd += mod.value;

                    if (i + 1 >= _statModifiers.Count || _statModifiers[i + 1].type != StatModType.PercentAdd)
                    {
                        finalValue *= (1 + sumPercentAdd);
                        sumPercentAdd = 0;
                    }
                    break;
                case StatModType.PercentMult:
                    finalValue *= mod.value;
                    break;
                case StatModType.FlatEnd:
                    finalValue += mod.value;    //Still adds at end, since list is ordered to do these last.
                    break;
                default:
                    Debug.Log("Error. A StatModType is unnaccounted for!");
                    break;
            }
        }

        //Round to 4 decimal places to avoid floating point comparison errors.
        return (float)Math.Round(finalValue, 4);
    }
}
