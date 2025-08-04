using System.Collections.Generic;

namespace System;

public static class RandomExtensions
{
    public static byte[] NextBytes(this Random random, int count)
    {
        var buffer = new byte[count];
        random.NextBytes(buffer);
        return buffer;
    }

    /// <summary>
    /// Chooses a random element from a given list.
    /// </summary>
    public static T PickRandom<T>(this Random r, IList<T> list) =>
        list[r.Next(list.Count)];

    /// <summary>
    /// Chooses a random element from a given array or parameters.
    /// </summary>
    public static T PickRandom<T>(this Random r, params T[] items) =>
        items[r.Next(items.Length)];

    /// <summary>
    /// Gets random boolean.
    /// </summary>
    /// <param name="random"></param>
    /// <returns>Random boolean.</returns>
    public static bool NextBoolean(this Random random) =>
        random.Next(2) == 0;

    /// <summary>
    /// Gets random value from between 0 and 1.
    /// </summary>
    /// <returns>Random number between 0 and 1.</returns>
    public static float NextFloat(this Random random) =>
        random.Next() * (1.0f / int.MaxValue);

    /// <summary>
    /// Gets random value from inclusive SByte.MinValue to inclusive SByte.MaxValue.
    /// </summary>
    /// <returns>Random number from -128 to 127 inclusive.</returns>
    public static sbyte NextSByte(this Random random) =>
        (sbyte)random.Next(sbyte.MinValue, sbyte.MaxValue + 1);

    /// <summary>
    /// Gets random value from inclusive Byte.MinValue to inclusive Byte.MaxValue.
    /// </summary>
    /// <returns>Random number from 0 to 255 inclusive.</returns>
    public static byte NextByte(this Random random) =>
        (byte)random.Next(byte.MinValue, byte.MaxValue + 1);

    /// <summary>
    /// Gets random value from inclusive Int16.MinValue to inclusive Int16.MaxValue.
    /// ()
    /// </summary>
    /// <returns>Random number from -32_768 to 32_767 inclusive.</returns>
    public static short NextShort(this Random random) =>
        (short)random.Next(short.MinValue, short.MaxValue + 1);

    /// <summary>
    /// Gets random value from inclusive UInt16.MinValue to inclusive UInt16.MaxValue.
    /// </summary>
    /// <returns>Random number from 0 to 65_535 inclusive.</returns>
    public static ushort NextUShort(this Random random) =>
        (ushort)random.Next(ushort.MinValue, ushort.MaxValue + 1);

    /// <summary>
    /// Gets random value from inclusive int.MinValue to inclusive int.MaxValue.
    /// </summary>
    /// <returns>Random number from -2_147_483_648 to 2_147_483_647 inclusive.</returns>
    // Due to second param being "exclusive" the max value can never be reached
    // We solve this by removing the value there and or'ing in another random value where we pick only that one bit
    public static int NextInt(this Random random) =>
        (int)((random.Next(int.MinValue, int.MaxValue) & ~1) | (random.Next() & 1));

    /// <summary>
    /// Gets random value from inclusive Uint.MinValue to inclusive Uint.MaxValue.
    /// </summary>
    /// <returns>Random number from 0 to 4_294_967_295 inclusive.</returns>
    public static uint NextUInt(this Random random) =>
        (uint)((random.Next(int.MinValue, int.MaxValue) & ~1) | (random.Next() & 1));

    /// <summary>
    /// Gets random value from inclusive long.MinValue to inclusive long.MaxValue.
    /// </summary>
    /// <returns>Random number from -9_223_372_036_854_775_808 to 9_223_372_036_854_775_807 inclusive.</returns>
    public static long NextLong(this Random random) =>
        (((long)random.Next(int.MinValue, int.MaxValue) & ~1) << 31) | (((long)random.Next(int.MinValue, int.MaxValue) & ~1) << 2) | (long)(random.Next() & 1);

    /// <summary>
    /// Gets random value from inclusive ulong.MinValue to inclusive ulong.MaxValue.
    /// </summary>
    /// <returns>Random number from 0 to 18_446_744_073_709_551_615 inclusive.</returns>
    public static ulong NextULong(this Random random) =>
        (ulong)((((long)random.Next(int.MinValue, int.MaxValue) & ~1) << 31) | (((long)random.Next(int.MinValue, int.MaxValue) & ~1) << 2) | (long)(random.Next() & 1));

}
