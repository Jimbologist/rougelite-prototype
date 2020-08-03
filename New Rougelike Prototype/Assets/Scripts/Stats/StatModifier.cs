using System;
using System.Text;
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
 * Can technically be used without a Stat being present but isn't recommended.
 */
 [Serializable]
public class StatModifier
{
    public float value;                              //Raw value used to modify the stat.
    public readonly int order;                       //Order in which stat is applied relative to others in a list
    public StatModType type;                         //Defines how modifier mathematically affects a Stat
    public object source;                            //Source object that applied the stat (optional, but simplifies process)

    public bool Nullified { get; set; }               //Should modifier be ignored regardless?
    public float LastValidValue { get; private set; } //Last value the modifier had before changed.

    public StatModifier(float value, StatModType type, int order, object source = null)
    {
        this.Nullified = false;
        this.LastValidValue = value;
        this.value = value;
        this.type = type;
        this.order = order;
        this.source = source;
    }

    //Order is automatically set to integer conversion of StatModType if not specified.
    public StatModifier(float value, StatModType type, object source) : this(value, type, (int)type, source) { }

    //Clone a StatModifier as a new instance.
    public StatModifier CloneModifier()
    {
        return new StatModifier(value, type, order, source);
    }

    //Do not use this method from any other class besides an associated Stat,
    //unless you don't want the new value to immediately take effect, since
    //Stats with this modifier won't be set to dirty after calling this.
    public void SetModifierValue(float newValue)
    {
        this.LastValidValue = value;
        this.value = newValue;
    }

    //Prints string representation of StatModifier using its own value
    public override string ToString()
    {
        StringBuilder str = new StringBuilder();

        //Add "x" if multiplier, else add "+" if positive,
        //then print value and % sign if percentAdd.
        
        if(value >= 0)
            str.Append("+");
        str.Append(value);

        switch (type)
        {
            case StatModType.Flat:
                str.Append(" (root)");
                break;
            case StatModType.PercentAdd:
                str.Append("%");
                break;
            case StatModType.PercentMult:
                str.Append("x");
                break;
            case StatModType.FlatEnd:
                str.Append(" (final)");
                break;
            default:
                throw new Exception("Invalid StatMod Type!");
        }

        return str.ToString();
    }

    //Prints string representation of StatModifier using a custom value.
    //Use this when printing stackable modifiers in order to print the
    //value that is the sum of the other modifiers!!
    public string ToString(float stackedValues)
    {
        StringBuilder str = new StringBuilder();

        //Add "x" if multiplier, else add "+" if positive,
        //then print value and % sign if percentAdd.

        if (stackedValues >= 0)
            str.Append("+");
        str.Append(stackedValues);

        switch (type)
        {
            case StatModType.Flat:
                str.Append(" (root)");
                break;
            case StatModType.PercentAdd:
                str.Append("%");
                break;
            case StatModType.PercentMult:
                str.Append("x");
                break;
            case StatModType.FlatEnd:
                str.Append(" (final)");
                break;
            default:
                throw new Exception("Invalid StatMod Type!");
        }

        return str.ToString();
    }
}
