/* 
 * SHGG
 * More info in SHGG.cs file 
 * 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace HAKGERSoft {

    internal sealed class BinBuffer<K, V> {
        private Dictionary<K, V> innerBuff = new Dictionary<K, V>();

        internal void pushSave(K key, V val) {
            if (innerBuff.ContainsKey(key))
                innerBuff[key] = val;
            else
                innerBuff.Add(key, val);
        }

        internal V popSave(K key) {
            V val;
            if (!innerBuff.TryGetValue(key, out val))
                return default(V);
            innerBuff.Remove(key);
            return val;
        }
    }

    public sealed partial class sHGG {

        internal static int SplitPacket(int count, int div) {
            if(div >= count)
                return -1;
            int i = count;
            while (i > 0) {
                if (i % div == 0 && i < count)
                    return i - 1;
                i--;
            }
            return -1;
        }

        internal static byte[] RawSerialize(object anything) {
            int rawSize = Marshal.SizeOf(anything);
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.StructureToPtr(anything, buffer, false);
            byte[] RawData = new byte[rawSize];
            Marshal.Copy(buffer, RawData, 0, rawSize);
            Marshal.FreeHGlobal(buffer);
            return RawData;
        }

        /*
        private static object RawDeserialize(byte[] rawData, Type anytype) {
            int rawSize = Marshal.SizeOf(anytype);
            if (rawSize > rawData.Length)
                return null;
            IntPtr buffer = Marshal.AllocHGlobal(rawSize);
            Marshal.Copy(rawData, 0, buffer, rawSize);
            object retObj = Marshal.PtrToStructure(buffer, anytype);
            Marshal.FreeHGlobal(buffer);
            return retObj;
        }
        */

        internal static long Hash(string password, uint seed) {
            uint x, y, z;
            y = seed;
            x = 0;
            for (int i = 0; i < password.Length; i++) {
                x = (x & 0xffffff00) | password[i];
                y ^= x;
                y += x;
                x <<= 8;
                y ^= x;
                x <<= 8;
                y -= x;
                x <<= 8;
                y ^= x;
                z = y & 0x1f;
                y = (y << Convert.ToInt32(z) | (y >> Convert.ToInt32(32 - z)));
            }
            return Convert.ToUInt32(y);
        }

        internal static uint StatusCode(GGStatusType status, string description) {
            uint result = STATUS_NOT_AVAILABLE;
            bool noDesc = (description.Length == 0);
            switch (status) {
                case GGStatusType.NotAvailable:
                    result = noDesc ? STATUS_NOT_AVAILABLE : STATUS_NOT_AVAILABLE_DESC;
                    break;
                case GGStatusType.Available:
                    result = noDesc ? STATUS_AVAILABLE : STATUS_AVAILABLE_DESC;
                    break;
                case GGStatusType.Busy:
                    result = noDesc ? STATUS_BUSY : STATUS_BUSY_DESC;
                    break;
                case GGStatusType.Invisible:
                    result = noDesc ? STATUS_INVISIBLE : STATUS_INVISIBLE_DESC;
                    break;
            }
            return result;
        }

        internal static GGStatusType StatusDecode(uint code) {
            switch (code) {
                case STATUS_AVAILABLE:
                case STATUS_AVAILABLE_DESC:
                    return GGStatusType.Available;
                case STATUS_BUSY:
                case STATUS_BUSY_DESC:
                    return GGStatusType.Busy;
                case STATUS_INVISIBLE:
                case STATUS_INVISIBLE_DESC:
                    return GGStatusType.Invisible;
                case STATUS_BLOCKED:
                    return GGStatusType.Blocked;
                case STATUS_NOT_AVAILABLE:
                case STATUS_NOT_AVAILABLE_DESC:
                case 0:
                    return GGStatusType.NotAvailable;
                default:
                    return GGStatusType.NotAvailable;
            }
        }

        internal static int GetStatusOrder(GGUser user) {
            switch (user.GGStatus) {
                case GGStatusType.Available:
                case GGStatusType.Busy:
                    return 3;
                case GGStatusType.Invisible:
                    return 2;
                case GGStatusType.Blocked:
                    return 1;
                case GGStatusType.NotAvailable:
                    return 0;
            }
            return 0;
        }

        internal string GGClientVersionDecode(byte code) {
            return (string)GG_VERSIONS[(int)code] ?? string.Empty;
        }

        internal string IPDecode() { // todo


            return "";
        }

        internal static int HttpHash(string[] hashParams) {
            int b = -1, i, j;
            uint a, c;
            if (hashParams.Length == 0)
                return -1;
            for (i = 0; i < hashParams.Length; i++) {
                for (j = 0; j < hashParams[i].Length; j++) {
                    c = hashParams[i][j];
                    a = (uint)(c ^ b) + (c << 8);
                    b = (int)(a >> 24) | (int)(a << 8);
                }
            }
            return System.Math.Abs(b);
        }

        internal static byte[] StrToByteArray(string str) {
            return Encoding.GetEncoding(DEFAULT_ENCODING).GetBytes(str);
        }

        internal static string ByteArray2Str(byte[] array) {
            return Encoding.GetEncoding(DEFAULT_ENCODING).GetString(array);
        }

        internal static T[] ToArray<T>(ICollection<T> list) {
            T[] dest = new T[list.Count];
            list.CopyTo(dest, 0);
            return dest;
        }

        internal static T[] ToArray<T>(IEnumerable<T> list) {
            ICollection<T> coll = list as ICollection<T>;
            if (coll != null) 
                return ToArray(coll);
            return new List<T>(list).ToArray();
        }

        internal static T[][] ArrayChop<T>(T[] array, int split) {
            //if (array.Length <= split)
            //    return new T[][] { array };
            T[][] outerArr = new T[][] { };
            T[] innerArr = new T[] { };
            for (int i = 0; i < array.Length; i++) {
                if (i % split == 0 && i != 0) {
                    Array.Resize<T[]>(ref outerArr, outerArr.Length + 1);
                    outerArr[outerArr.Length - 1] = innerArr;
                    innerArr = new T[] { };
                } 
                Array.Resize<T>(ref innerArr, innerArr.Length + 1);
                innerArr[innerArr.Length - 1] = array[i];
            }
            if (innerArr.Length > 0) {
                Array.Resize<T[]>(ref outerArr, outerArr.Length + 1);
                outerArr[outerArr.Length - 1] = innerArr;
            }
            return outerArr;
        }

        internal static T[] ConcatArray<T>(T[] arr1, T[]arr2) {
            T[] dest = new T[arr1.Length + arr2.Length];
            arr1.CopyTo(dest, 0);
            arr2.CopyTo(dest, arr1.Length);
            return dest;
        }

        internal static byte[] Stream2Array(MemoryStream stream) {
            byte[] data = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }



 
    }
}