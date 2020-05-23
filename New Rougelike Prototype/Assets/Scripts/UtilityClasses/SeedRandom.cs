using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/**
 * Seeded randomness class used to retrieve random numbers from a new
 * seed/subseed created upon creation of an instance of this class.
 * Method of creating PRNG uses a version of XORshift64 algorithm.
 * Intended for events that should be repeatable given the same seed.
 * Non-run intrusive events DO NOT use this.
 * 
 * Initial seed can either be from system clock at construction,
 * or based on a previous seed in order to become a "subseed"
 */

//Note to self -> create static instances of this each time a new "category"
//of randomness is required, using the initial seed from the GameManager
//and a new set of a, b, and c values for each category.
public class SeedRandom
{
    private readonly int a;
    private readonly int b;
    private readonly int c;

    //Seeded value determined by system clock or previous state.
    private long state;

    //List of all SeedRandom instances, EXCEPT main seed.
    private static List<SeedRandom> allRandoms;

    /**
     * Default constructor; seed based on current Unix Epoch time
     * Does NOT add to list of all current SeedRandoms; meant for MAIN SEED
     */
    public SeedRandom()
    {
        state = (long)new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
        allRandoms = new List<SeedRandom>();

        //Aribtary shift values of good quality
        //taken from https://www.javamex.com/tutorials/random_numbers/xorshift.shtml#.XptDuMhKhhG
        this.a = 21;
        this.b = 35;
        this.c = 4;
    }

    /**
     * Constructor can set seed for this instance using default a, b, and c values.
     * This and the next constructor are for SeedRandoms separate fromt the main one.
     */
    public SeedRandom(long newSeed)
    {
        state = newSeed;

        //Aribtary shift values of good quality
        //taken from https://www.javamex.com/tutorials/random_numbers/xorshift.shtml#.XptDuMhKhhG
        this.a = 21;
        this.b = 35;
        this.c = 4;
        allRandoms.Add(this);
    }

    /**
     * Constructor can set a new seed using new arbitrary a, b, and c values.
     * Ideally, these values should be constants from this class that are known
     * to be of high quality, separated by category of randomness.
     */
    public SeedRandom(long newSeed, int a, int b, int c)
    {
        state = newSeed;
        this.a = a;
        this.b = b;
        this.c = c;
        allRandoms.Add(this);
    }

    //Set seed of current instance. Ideally, this should be used after construction to 
    //shift seeds based on IDs of Room, Item, Character, etc.
    public void SetSeed(long newSeed)
    {
        state = newSeed;
    }

    //Returns next state as uint64
    public ulong Random64U()
    {
        state ^= state << a;
        state ^= state >> b;
        state ^= state << c;
        return (ulong)state;
    }

    //Returns next state as uint32 (only positive values)
    public uint Rand32U()
    {
        state ^= state << a;
        state ^= state >> b;
        state ^= state << c;
        return (uint)state;
    }

    //Returns next state as signed int32 (can be negative)
    public int Rand32S()
    {
        state ^= state << a;
        state ^= state >> b;
        state ^= state << c;
        return (int)state;
    }

    //Returns next state as float value in [0, 1)
    public float RandFloat()
    {
        state ^= state << a;
        state ^= state >> b;
        state ^= state << c;
        return (float)state * (1.0f / ulong.MaxValue);
    }

    public float RandRangeFloat(float min, float max)
    {
        state ^= state << a;
        state ^= state >> b;
        state ^= state << c;
        return ((float)state % ((max + 1.0f) - min)) + min;
    }

    public uint RandRangeUInt(uint min, uint max)
    {
        state ^= state << a;
        state ^= state >> b;
        state ^= state << c;
        return ((uint)state % ((max + 1) - min)) + min;
    }
}
