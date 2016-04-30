using System;
using System.Collections.Generic;

namespace PrimeWithFile
{
  public static class Integers
  {

    #region Prime Numbers <100
    private static readonly int[] Primes =
    { 2, 3, 5, 7, 11, 13, 17, 19, 23,
      29, 31, 37, 41, 43, 47, 53, 59,
      61, 67, 71, 73, 79, 83, 89, 97 };

    #endregion
    // starting number for iterative factorization
    private const int StartNum = 101;
    
    #region IsPrime: primality Check
    /// <summary>
    /// Check if the number is Prime
    /// </summary>
    /// <param name="num">Int64</param>
    /// <returns>bool</returns>
    public static bool IsPrime(Int64 num)
    {
      Int64 _upMargin = (Int64)Math.Sqrt(num) + 1;
      // Check if number is in Prime Array
      /*
      if (Primes.Any(t => num == t))
      {
        return true;
      }
      */
      for (int i = 0; i < Primes.Length; i++)
      {
        if (num == Primes[i]) { return true; }
      }

      // Check divisibility w/Prime Array
      for (int i = 0; i < Primes.Length; i++)
      {
        if (num % Primes[i] == 0) return false;
      }
      // Main iteration for Primality check
      _upMargin = (Int64)Math.Sqrt(num) + 1;
      var j = StartNum;
      var ret = true;
      while (j <= _upMargin)
      {
        if (num % j == 0) { ret = false; break; }
        j++; j++;
      }

      return ret;
    }

    /// <summary>
    /// Check if number-string is Prime
    /// </summary>
    /// <param name="stringNum">string</param>
    /// <returns>bool</returns>
    public static bool IsPrime(string stringNum)
    {
      return IsPrime(Int64.Parse(stringNum));
    }

    #endregion
    #region Fast Factorization

    /// <summary>
    /// Factorize string converted to long integers
    /// </summary>
    /// <param name="stringNum">string</param>
    /// <returns>Int64[]</returns>
    public static Int64[] FactorizeFast(string stringNum)
    {
      return FactorizeFast(Int64.Parse(stringNum));
    }

    /// <summary>
    /// Factorize long integers: speed optimized
    /// </summary>
    /// <param name="num">Int64</param>
    /// <returns>Int64[]</returns>
    public static Int64[] FactorizeFast(Int64 num)
    {
      #region vars
      // list of Factors
      List<Int64> arrFactors = new List<Int64>();
      // temp variable
      Int64 _num = num;
      #endregion

      #region Check if the number is Prime (<100)
      for (int k = 0; k < Primes.Length; k++)
      {
        if (_num == Primes[k])
        {
          arrFactors.Add(Primes[k]);
          return arrFactors.ToArray();
        }
      }
      #endregion

      #region Try to factorize using Primes Array
      for (int k = 0; k < Primes.Length; k++)
      {
        int m = Primes[k];
        if (_num < m) break;
        while (_num % m == 0)
        {
          arrFactors.Add(m);
          _num = _num / m;
        }
      }
      if (_num < StartNum)
      {
        arrFactors.Sort();
        return arrFactors.ToArray();
      }
      #endregion
      #region Main Factorization Algorithm
      Int64 upMargin = (Int64)Math.Sqrt(_num) + 1;
      Int64 i = StartNum;
      while (i <= upMargin)
      {
        if (_num % i == 0)
        {
          arrFactors.Add(i);
          _num = _num / i;
          upMargin = (Int64)Math.Sqrt(_num) + 1;
          i = StartNum;
        }
        else { i++; i++; }
      }
      arrFactors.Add(_num);
      arrFactors.Sort();
      return arrFactors.ToArray();
      #endregion
    }
    #endregion

    public static string FactorizeToString(Int64 number)
    {
      // Linq: string result = FactorizeFast(number).Aggregate(string.Empty, (current, item) => current + (item + " "));
      string result = string.Empty;
      foreach (Int64 item in FactorizeFast(number))
      {
        result += item + " * ";
      }

      result += "1";
      return string.Format("{0} = {1}", number, result);
    }
  }
}