using System;

namespace MidiPro.Core.GP.Files
{
    public class GpBase
    {
        public const int BendPosition = 60;
        public const int BendSemitone = 25;
        public static int Pointer = 0;
        public static byte[] Data;

        public static void Skip(int count)
        {
            Pointer += count;
        }

        public static byte[] ReadByte(int count = 1)
        {
            return Extract(Pointer, count, true);
        }

        public static sbyte[] ReadSignedByte(int count = 1)
        {
            byte[] unsigned = Extract(Pointer, count, true);
            sbyte[] retVal = new sbyte[unsigned.Length];
            for (int x = 0; x < unsigned.Length; x++)
            {
                retVal[x] = (sbyte)unsigned[x];
            }
            return retVal;
        }

        public static bool[] ReadBool(int count = 1)
        {
            byte[] vals = Extract(Pointer, count, true);
            bool[] retVal = new bool[vals.Length];
            for (int x = 0; x < vals.Length; x++)
            {
                retVal[x] = (vals[x] != 0x0);
            }
            return retVal;
        }

        public static short[] ReadShort(int count = 1)
        {
            byte[] vals = Extract(Pointer, count * 2, true);
            short[] retVal = new short[count];
            for (int x = 0; x < vals.Length; x += 2)
            {
                retVal[x / 2] = (short)(vals[x] + (vals[x + 1] << 8));
            }
            return retVal;
        }
        public static int[] ReadInt(int count = 1)
        {
            byte[] vals = Extract(Pointer, count * 4, true);
            int[] retVal = new int[count];
            for (int x = 0; x < vals.Length; x += 4)
            {
                retVal[x / 4] = (int)(vals[x] + (vals[x + 1] << 8) + (vals[x + 2] << 16) + (vals[x + 3] << 24));
            }
            return retVal;
        }

        public static float[] ReadFloat(int count = 1)
        {
            byte[] vals = Extract(Pointer, count * 4, true);
            float[] retVal = new float[count];
            for (int x = 0; x < vals.Length; x += 4)
            {
                retVal[x / 4] = System.BitConverter.ToSingle(vals, x);
            }
            return retVal;
        }

        public static double[] ReadDouble(int count = 1)
        {
            byte[] vals = Extract(Pointer, count * 8, true);
            double[] retVal = new double[count];
            for (int x = 0; x < vals.Length; x += 8)
            {
                retVal[x / 8] = System.BitConverter.ToDouble(vals, x);
            }
            return retVal;
        }

        public static string ReadString(int size, int length = 0)
        {
            if (length == 0)
            {
                length = size;
            }
            int count = (size > 0) ? size : length;
            byte[] ss = (length >= 0) ? Extract(Pointer, length, true) : Extract(Pointer, size, true);
            Skip(count - ss.Length);
            return System.Text.Encoding.Default.GetString(ss);
        }

        public static string ReadByteSizeString(int size)
        {
            return ReadString(size, (int)ReadByte()[0]);
        }

        public static string ReadIntSizeString()
        {
            return ReadString(ReadInt()[0]);
        }
        public static string ReadIntByteSizeString()
        {
            //Read length of the string increased by 1 and stored in 1 integer
            //followed by length of the string in 1 byte and finally followed by
            //character bytes.

            int d = ReadInt()[0] - 1;

            return ReadByteSizeString(d);
        }

        public static byte[] Extract(int start, int length, bool advancePointer)
        {
            if (length <= 0)
            {
                return new byte[Math.Max(0, length)];
            }
            if (length + start > Data.Length)
            {
                return new byte[Math.Max(0, length)];
            }

            byte[] ret = new byte[length];
            for (int x = start; x < start + length; x++)
            {
                ret[x - start] = Data[x];
            }
            if (advancePointer) Pointer += length;

            return ret;
        }
    }
}