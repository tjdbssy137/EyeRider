using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class Stat
{
    public float DefaultValue { get; private set; }

    private Dictionary<int, StatModifier> _flatModifierValueDic = new Dictionary<int, StatModifier>();
    private Dictionary<int, StatModifier> _percentageModifierValueDic = new Dictionary<int, StatModifier>();

    public Stat(float defaultValue)
    {
        DefaultValue = defaultValue;
    }

    public float Value
    {
        get
        {
            float flatValue = DefaultValue;
            foreach (var value in _flatModifierValueDic.Values)
            {
                flatValue += value.Value;
            }

            float percentageValue = 0.0f;
            foreach (var value in _percentageModifierValueDic.Values)
            {
                percentageValue += value.Value;
            }

            float totalPercentageValue = flatValue * (percentageValue / 100.0f);
            return Mathf.Clamp(flatValue + totalPercentageValue, 0.0f, Mathf.Infinity);
        }
    }

    public void AddStatModifier(StatModifier StatModifier)
    {
        if(StatModifier.ModifierType == EStatModifierType.Flat)
        {
            if (!_flatModifierValueDic.ContainsKey(StatModifier.Id))
            {
                _flatModifierValueDic.Add(StatModifier.Id, StatModifier);
            }
        }
        else if(StatModifier.ModifierType == EStatModifierType.Percentage)
        {
            if (!_percentageModifierValueDic.ContainsKey(StatModifier.Id))
            {
                _percentageModifierValueDic.Add(StatModifier.Id, StatModifier);
            }
        }
    }

    public void RemoveStatModifier(int statModifierId)
    {
        if (_flatModifierValueDic.ContainsKey(statModifierId))
        {
            _flatModifierValueDic.Remove(statModifierId);
        }

        if (_percentageModifierValueDic.ContainsKey(statModifierId))
        {
            _percentageModifierValueDic.Remove(statModifierId);
        }
    }

    public void ClearModifiers()
    {
        _flatModifierValueDic.Clear();
        _percentageModifierValueDic.Clear();
    }
}

public class StatModifier
{
    public int Id { get; private set; }
    public EStatModifierKind ModifierKind { get; private set; }
    public EStatModifierType ModifierType { get; private set; }
    public float Value { get; private set; }
    private static int _idGenerator = 1;

    public StatModifier(EStatModifierKind modifierKind, EStatModifierType modifierType, float value)
    {
        Id = _idGenerator++;
        ModifierKind = modifierKind;
        ModifierType = modifierType;
        Value = value;
    }
}


public class Stats
{
    // public Dictionary<StatType, Stat> StatDic { get; private set; } = new Dictionary<StatType, Stat>();
    public Dictionary<EStatType, int> StatDic { get; set; } = new Dictionary<EStatType, int>();
    public float Hp 
    {
        get => _hp;
        set
        {
            var newHp = Mathf.Clamp(value, 0, StatDic[EStatType.MaxHp]);
            _hp = newHp;
        }
    }
    private float _hp;
    public Stats()
    {
    }
}

public enum EStatModifierKind
{
    Item,
    Pet,
    Summon,
    Passive,
    Buff,
    Relic,
}

public enum EStatModifierType
{
    Flat,
    Percentage,
}

