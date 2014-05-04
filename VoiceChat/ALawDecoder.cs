﻿using System;
using System.Collections.Generic;
using System.Text;

namespace g711
{
    // Turns 8-bit A-law bytes back into 16-bit PCM values.
    public static class ALawDecoder
    {

        // An array where the index is the a-law input, and the value is the 16-bit PCM result.
        private static short[] aLawToPcmMap;

        static ALawDecoder()
        {
            aLawToPcmMap = new short[256];
            for (byte i = 0; i < byte.MaxValue; i++)
                aLawToPcmMap[i] = Decode(i);
        }
       
        // Returns a short containing the 16-bit  decoded result
        private static short Decode(byte alaw)
        {
            //Invert every other bit, and the sign bit (0xD5 = 1101 0101)
            alaw ^= 0xD5;
            //Pull out the value of the sign bit
            int sign = alaw & 0x80;
            //Pull out and shift over the value of the exponent
            int exponent = (alaw & 0x70) >> 4;
            //Pull out the four bits of data
            int data = alaw & 0x0f;

            //Shift the data four bits to the left
            data <<= 4;
            //Add 8 to put the result in the middle of the range (like adding a half)
            data += 8;

            //If the exponent is not 0, then we know the four bits followed a 1,
            //and can thus add this implicit 1 with 0x100.
            if (exponent != 0)
                data += 0x100;
            /* Shift the bits to where they need to be: left (exponent - 1) places
             * Why (exponent - 1) ?
             * 1 2 3 4 5 6 7 8 9 A B C D E F G
             * . 7 6 5 4 3 2 1 . . . . . . . . <-- starting bit (based on exponent)
             * . . . . . . . Z x x x x 1 0 0 0 <-- our data (Z is 0 only when exponent is 0)
             * We need to move the one under the value of the exponent,
             * which means it must move (exponent - 1) times
             * It also means shifting is unnecessary if exponent is 0 or 1.
             */
            if (exponent > 1)
                data <<= (exponent - 1);

            return (short)(sign == 0 ? data : -data);
        }

        
        public static short ALawDecode(byte alaw)
        {
            return aLawToPcmMap[alaw];
        }

        public static short[] ALawDecode(byte[] data)
        {
            int size = data.Length;
            short[] decoded = new short[size];
            for (int i = 0; i < size; i++)
                decoded[i] = aLawToPcmMap[data[i]];
            return decoded;
        }

        public static void ALawDecode(byte[] data, out short[] decoded)
        {
            int size = data.Length;
            decoded = new short[size];
            for (int i = 0; i < size; i++)
                decoded[i] = aLawToPcmMap[data[i]];
        }

        /// Returns An array of bytes in Little-Endian format containing the results
        public static void ALawDecode(byte[] data, out byte[] decoded)
        {
            int size = data.Length;
            decoded = new byte[size * 2];
            for (int i = 0; i < size; i++)
            {
                //First byte is the less significant byte
                decoded[2 * i] = (byte)(aLawToPcmMap[data[i]] & 0xff);
                //Second byte is the more significant byte
                decoded[2 * i + 1] = (byte)(aLawToPcmMap[data[i]] >> 8);
            }
        }
    }
}
