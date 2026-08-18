using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace cmdNet.Services;

public static class PasswordGenerator
{
   
    
    public static  string lower = "abcdefghjkmnpqstuvwxyz";
    public static  string upper = "ABCDEFGHJKMNPQRSTUVWXYZ";
    public static  string numbers = "23456789";
    public static string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    public static string Generate(int length = 16)
    {
        

        string all = lower + upper + numbers;

        var chars = new List<char>
        {
            lower[RandomNumberGenerator.GetInt32(lower.Length)],
            upper[RandomNumberGenerator.GetInt32(upper.Length)],
            numbers[RandomNumberGenerator.GetInt32(numbers.Length)]
        };

        while (chars.Count < length)
        {
            chars.Add(
                all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        return new string(
            chars.OrderBy(_ =>
                RandomNumberGenerator.GetInt32(Int32.MaxValue))
            .ToArray());
    }

  


}