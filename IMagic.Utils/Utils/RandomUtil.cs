using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class RandomUtil
{
    private static string AlphabetUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static string AlphabetLower = "abcdefghijklmnopqrstuvwxyz";
    public static Random Random = new Random();
    public static int Next(int max)
    {
        return Next(0, max);
    }

    public static int Next(int min, int max)
    {
        return Random.Next(min, max);
    }

    public static T RandomEnumValue<T>()
    {
        var v = Enum.GetValues(typeof(T));
        return (T)v.GetValue(Random.Next(v.Length));
    }


    public static string GetRandomString(int length = 8, bool lowercase = true, bool upperCase = true, bool numbers = true)
    {
        List<char> chars = new List<char> { };
        if (upperCase)
        {
            chars.AddRange(AlphabetUpper.ToArray().RandomElements(length));
        }

        if (lowercase)
        {
            chars.AddRange(AlphabetLower.ToArray().RandomElements(length));
        }

        if (numbers)
        {
            for (int i = 0; i < length; i++)
            {
                int random = RandomUtil.Next(9);
                chars.Add(Convert.ToChar(random.ToString()));
            }
        }

        List<char> outputChars = chars.RandomElements(length).ToList();
        string output = new string(outputChars.ToArray());
        return output;
    }

}
