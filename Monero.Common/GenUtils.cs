﻿using Org.BouncyCastle.Utilities;

namespace Monero.Common
{
    public class GenUtils
    {
        public static T? Reconcile<T>(T? val1, T? val2, bool? resolveDefined = null, bool? resolveTrue = null, bool? resolveMax = null)
        {
            // check for same reference
            if (object.ReferenceEquals(val1, val2)) return val1;

            int? comparison = null;

            // check for BigInteger equality
            if (val1 is ulong b1 && val2 is ulong b2)
            {
                comparison = b1.CompareTo(b2);
                if (comparison == 0) return val1;
            }

            // resolve one value null
            if (val1 == null || val2 == null)
            {
                if (resolveDefined == false) return default!;
                return val1 == null ? val2 : val1!;
            }

            // resolve different booleans
            if (resolveTrue.HasValue && val1 is bool && val2 is bool)
            {
                return (T)(object)resolveTrue.Value;
            }

            // resolve different numbers
            if (resolveMax.HasValue)
            {
                if (val1 is ulong b1Num && val2 is ulong b2Num)
                {
                    return (T)(object)(resolveMax.Value
                        ? (comparison < 0 ? b2Num : b1Num)
                        : (comparison < 0 ? b1Num : b2Num));
                }

                if (val1 is int i1 && val2 is int i2)
                {
                    return (T)(object)(resolveMax.Value ? Math.Max(i1, i2) : Math.Min(i1, i2));
                }

                if (val1 is uint l1 && val2 is uint l2)
                {
                    return (T)(object)(resolveMax.Value ? Math.Max(l1, l2) : Math.Min(l1, l2));
                }

                throw new Exception("Need to resolve primitives and object versions");
            }

            // assert deep equality
            if (!Equals(val1, val2))
            {
                throw new Exception($"Cannot Reconcile values {val1} and {val2} with config: [{resolveDefined}, {resolveTrue}, {resolveMax}]");
            }

            return val1;
        }

        public static byte[] ReconcileByteArrays(byte[] arr1, byte[] arr2)
        {

            // check for same reference or null
            if (arr1 == arr2) return arr1;

            // resolve one value defined
            if (arr1 == null || arr2 == null)
            {
                return arr1 == null ? arr2 : arr1;
            }

            // assert deep equality
            if (!Arrays.Equals(arr1, arr2)) throw new MoneroError("Cannot Reconcile arrays");
            return arr1;
        }

        public static byte[]? ListToByteArray(List<byte>? list)
        {
            if (list == null) return null;
            byte[] bytes = new byte[list.Count];
            for (int i = 0; i < list.Count; i++) bytes[i] = list[i];
            return bytes;
        }
    }
}
