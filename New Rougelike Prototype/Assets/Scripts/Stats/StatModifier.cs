/**
 * This enum defines the way in which the modifier affects a stat
 * and in what order their modifications are applied. That order being
 * the same as the enum integer values, starting at Flat.
 * 
 * Flat: Add StatModifier value directly to the stat. These are done first.
 * PercentAdd: Percentage increases to the stat that can stack. For example,
 *      having two PercentAdd modifiers with a value of 20% will increase the
 *      value of the stat by +40%.
 * PercentMult: Percentage increases to the final value of the stat after all
 *      other modifiers have been applied. Usually for temporary increases
 *      to final values.
 * FlatEnd: Add StatModifier value directly to the stat after all other modifiers are
 *      made. This is more rare, but useful for things like fire rate, reload, and mag size.
 */
public enum StatModType
{
    Flat = 100,
    PercentAdd = 200,
    PercentMult = 300,
    FlatEnd = 400,
}

/**
 * This struct provides the data required to modify a stat for some entity in game.
 * Should be used add to a Stat struct's list of modifiers to properly and easily
 * apply the correct modification to the stat based on order and type. 
 * Can technically be used without a Stat struct being present but isn't recommended.
 */
public struct StatModifier
{
    public float value;                     //Raw value used to modify the stat.
    public readonly int order;              //Order in which stat is applied relative to others in a list
    public readonly StatModType type;       //Defines how modifier mathematically affects a Stat
    public readonly object source;          //Source object that applied the stat (optional, but simplifies process)

    private float lastValue;

    public StatModifier(float value, StatModType type, int order, object source = null)
    {
        this.value = value;
        this.lastValue = value;
        this.type = type;
        this.order = order;
        this.source = source;
    }

    //Order is automatically set to integer conversion of StatModType if not specified.
    public StatModifier(float value, StatModType type, object source) : this(value, type, (int)type, source) { }

    public void SetModifierValue(float newValue)
    {
        this.lastValue = value;
        this.value = newValue;
    }
}
