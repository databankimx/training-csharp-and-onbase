#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using System.IO;
using System.Text;
#endregion

namespace CSharp.Supplemental.BitwiseOperations
{
    // Eleven small, independent demos, one per numbered topic in Lesson.md that has runnable
    // code attached. Menu-driven rather than run-them-all-in-sequence, since these are meant
    // to be picked one at a time alongside whichever part of Lesson.md is being discussed,
    // not sat through all eleven every time.
    internal static class Program
    {
        #region Main
        private static void Main()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Bitwise Operations - Demos");
                Console.WriteLine("===========================");
                Console.WriteLine();
                Console.WriteLine("  1.  Integer Overflow (Lesson 05)");
                Console.WriteLine("  2.  Bitwise AND (Lesson 11)");
                Console.WriteLine("  3.  Checking Even/Odd with AND (Lesson 11)");
                Console.WriteLine("  4.  Bit-Flags with AND (Lessons 12 & 14)");
                Console.WriteLine("  5.  Bitwise OR (Lesson 13)");
                Console.WriteLine("  6.  File Permissions with OR (Lesson 13)");
                Console.WriteLine("  7.  Bitwise NOT (Lesson 15)");
                Console.WriteLine("  8.  Bitwise XOR (Lesson 16)");
                Console.WriteLine("  9.  XOR Encryption (Lesson 17)");
                Console.WriteLine("  10. Bit Shifts (Lesson 18)");
                Console.WriteLine("  11. Iterating a Bit-Flag with Shifts (Lesson 18)");
                Console.WriteLine("  12. Reconstructing Integers (Lesson 19)");
                Console.WriteLine();
                Console.WriteLine("  X.  Exit");
                Console.WriteLine();
                Console.Write("Choice: ");

                string choice = Console.ReadLine()?.Trim() ?? "";

                if (string.Equals(choice, "X", StringComparison.OrdinalIgnoreCase)) return;

                Console.WriteLine();

                try
                {
                    switch (choice)
                    {
                        case "1": DemoOverflow(); break;
                        case "2": DemoBitwiseAnd(); break;
                        case "3": DemoEvenNumbers(); break;
                        case "4": DemoBitFlags(); break;
                        case "5": DemoBitwiseOr(); break;
                        case "6": DemoFilePermissions(); break;
                        case "7": DemoBitwiseNot(); break;
                        case "8": DemoBitwiseXor(); break;
                        case "9": DemoEncryptDecrypt(); break;
                        case "10": DemoBitShifts(); break;
                        case "11": DemoIterateFlags(); break;
                        case "12": DemoReconstructIntegers(); break;
                        default:
                            Console.WriteLine("That's not a valid choice.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    while (ex != null)
                    {
                        Console.WriteLine(ex);
                        ex = ex.InnerException;
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to return to the menu...");
                Console.ReadKey();
            }
        }
        #endregion

        #region Lesson 05: Integer Overflow
        private static void DemoOverflow()
        {
            byte b = 255;
            b++;
            Console.WriteLine($"(byte)  255 + 1 = {b}");

            sbyte s = 127;
            s++;
            Console.WriteLine($"(sbyte) 127 + 1 = {s}");
        }
        #endregion

        #region Lesson 11: Bitwise AND
        private static void DemoBitwiseAnd()
        {
            int a = 0b10011100; // 156
            int b = 0b00110100; // 52
            Console.WriteLine(a & b); // --> 20

            int x = 156;
            int y = 52;
            Console.WriteLine(x & y); // --> 20
        }

        private static void DemoEvenNumbers()
        {
            foreach (int n in new[] { 42, 73 })
            {
                Console.WriteLine($"Using 'n % 2', {n} is {(IsEvenMod(n) ? "even" : "odd")}");
                Console.WriteLine($"Using 'n & 1', {n} is {(IsEvenAnd(n) ? "even" : "odd")}");
            }
        }

        private static bool IsEvenMod(int n) => n % 2 == 0;
        private static bool IsEvenAnd(int n) => (n & 1) == 0;
        #endregion

        #region Lessons 12 & 14: Bit-Flags with AND (and OR, for combined package flags)
        private static void DemoBitFlags()
        {
            byte licenses = (byte)ProductLicenses.Work;
            string name = Enum.GetName(typeof(ProductLicenses), licenses);
            var values = Enum.GetValues(typeof(ProductLicenses));
            foreach (int l in values)
            {
                if (l == (int)ProductLicenses.None) continue;
                if ((licenses & l) == l)
                {
                    string subName = Enum.GetName(typeof(ProductLicenses), l);
                    Console.WriteLine($"{subName} = {l}");
                }
            }
            if (HasFlag(licenses, (byte)ProductLicenses.WordProcessing))
            {
                Console.WriteLine($"{name} - Includes Word Processing");
            }
        }

        private static bool HasFlag(byte value, byte flag) => (value & flag) == flag;
        #endregion

        #region Lesson 13: Bitwise OR
        private static void DemoBitwiseOr()
        {
            int a = 0b10011100; // 156
            int b = 0b00110100; // 52
            Console.WriteLine(a | b); // --> 188

            int x = 156;
            int y = 52;
            Console.WriteLine(x | y); // --> 188
        }

        private static void DemoFilePermissions()
        {
            bool needToWrite = true;

            FileAccess permissions = FileAccess.Read;

            Console.WriteLine($"{permissions} = {(int)permissions}");
            Console.WriteLine($"{FileAccess.Write} = {(int)FileAccess.Write}");

            if (needToWrite) permissions |= FileAccess.Write;

            Console.WriteLine($"{permissions} = {(int)permissions}");
        }
        #endregion

        #region Lesson 15: Bitwise NOT
        private static void DemoBitwiseNot()
        {
            byte b = 0b10011100;  // 156   10011100
            byte n = (byte)~b;    //  99   01100011
            Console.WriteLine(n); // --> 99

            byte word = 0;
            byte pos = 0;
            while (pos < 8)
            {
                Console.WriteLine(SetBit(word, pos, 1));
                pos++;
            }
        }

        private static int SetBit(byte word, byte pos, byte value)
        {
            int mask = 1 << pos;
            // Here, we're using `~` to flip the mask bits
            if (value == 0) return word & ~mask;
            else if (value == 1) return word | mask;
            else return word;
        }
        #endregion

        #region Lesson 16: Bitwise XOR
        private static void DemoBitwiseXor()
        {
            int a = 0b10011100; // 156
            int b = 0b00110100; // 52
            Console.WriteLine(a ^ b); // --> 168

            int x = 156;
            int y = 52;
            Console.WriteLine(x ^ y); // --> 168

            a = 5;
            b = 8;
            Console.WriteLine($"a = {a}, b = {b}");
            SwapValues(ref a, ref b);
            Console.WriteLine($"a = {a}, b = {b}");
        }

        // Exchange the values between two variables without creating a temp variable
        private static void SwapValues(ref int a, ref int b)
        {
            a ^= b;
            b ^= a;
            a ^= b;
        }
        #endregion

        #region Lesson 17: XOR Encryption
        private static void DemoEncryptDecrypt()
        {
            int key = 30;

            string text = "Hello World!";
            Console.WriteLine($"Plain Text:     {text}");

            string cipherText = EncryptDecrypt(text, key);
            Console.WriteLine($"Encrypted Text: {cipherText}");

            string plainText = EncryptDecrypt(cipherText, key);
            Console.WriteLine($"Decrypted Text: {plainText}");
        }

        // Encrypt or decrypt a string using a symmetric key and XOR. Reversible - running the
        // same text through this again with the same key restores the original.
        private static string EncryptDecrypt(string text, int key)
        {
            var inString = new StringBuilder(text);
            var outString = new StringBuilder(text.Length);

            for (int i = 0; i < text.Length; i++)
            {
                outString.Append((char)(inString[i] ^ key));
            }

            return outString.ToString();
        }
        #endregion

        #region Lesson 18: Bit Shifts
        private static void DemoBitShifts()
        {
            byte x = 128;
            byte b = (byte)(x >> 3);
            Console.WriteLine($"{x} >> 3 = {b}");

            x = 16;
            b = (byte)(x << 3);
            Console.WriteLine($"{x} << 3 = {b}");

            // Overflow
            x = 64;
            b = (byte)(x << 3);
            Console.WriteLine($"{x} << 3 = {b}  <- overflowed a ninth bit, lost");

            // Underflow
            x = 4;
            b = (byte)(x >> 3);
            Console.WriteLine($"{x} >> 3 = {b}  <- shifted past the ones place, lost");

            byte flag = 0b01010101;
            Console.WriteLine($"In value [{flag}], the following bits are set:");
            byte currentValue = 1;
            while (flag > 0)
            {
                if ((flag & 1) == 1) Console.WriteLine($"{currentValue}");
                flag >>= 1;
                currentValue <<= 1;
            }
        }

        private static void DemoIterateFlags()
        {
            // Not a security-sensitive use of randomness - this just needs a plausible value to
            // iterate over to demonstrate the bit-shift technique below, which is the actual
            // point of this demo. System.Random is the right tool for that; pulling in
            // RandomNumberGenerator here would be noise that obscures the lesson, not a
            // meaningful improvement.
#pragma warning disable S2245
            int settings = new Random().Next(0, 15);
#pragma warning restore S2245
            Console.WriteLine($"With Settings: {settings}, the following bits are set:");
            int currentValue = 1;
            while (settings > 0)
            {
                if ((settings & 1) == 1)
                {
                    Console.WriteLine($"- {Enum.GetName(typeof(Settings), currentValue)}");
                }
                settings >>= 1;
                currentValue <<= 1;
            }
        }
        #endregion

        #region Lesson 19: Reconstructing Integers
        private static void DemoReconstructIntegers()
        {
            byte[] bytes =
            {
                0b1101_0010, 0b0000_0010, 0b1001_0110, 0b0100_1001,
                0b0001_0101, 0b1100_1101, 0b0101_1011, 0b0000_0111
            };
            int[] values = ReconstructIntegers(bytes);
            foreach (int value in values)
            {
                Console.WriteLine($"{value:n0}");
            }
        }

        // Process byte array into list of integers of the specified size
        private static int[] ReconstructIntegers(byte[] bytes, int size = 4)
        {
            if (bytes.Length % size != 0) throw new ArgumentException("Invalid data length!", nameof(bytes));

            int numInts = bytes.Length / size;

            int[] integers = new int[numInts];

            for (int n = 0; n < numInts * size; n += size)
            {
                byte[] data = new byte[size];

                for (int i = 0; i < size; i++)
                {
                    data[i] = bytes[n + i];
                }

                integers[n / size] = ReconstructInteger(data, size);
            }

            return integers;
        }

        // Reconstruct an integer from a byte array
        private static int ReconstructInteger(byte[] data, int size = 4)
        {
            if (data.Length != size) throw new ArgumentException("Invalid data length!", nameof(data));

            int num = 0;

            for (int p = 0; p < data.Length; p++)
            {
                num += data[p] << (p * 8);
            }

            return num;
        }
        #endregion
    }

    // Bit-Flag values for product licenses - Lessons 12 & 14
    [Flags]
    internal enum ProductLicenses
    {
        None = 0b0000_0000,
        WordProcessing = 0b0000_0001,
        Spreadsheets = 0b0000_0010,
        Presentations = 0b0000_0100,
        EmailClient = 0b0000_1000,
        Notebook = 0b0001_0000,
        Collaboration = 0b0010_0000,
        ProjectManagement = 0b0100_0000,
        Publishing = 0b1000_0000,
        Personal = WordProcessing | EmailClient,
        Work = Personal | Spreadsheets | Presentations | Notebook | Collaboration,
        UltraDeluxe = Work | ProjectManagement | Publishing
    }

    // Bit-flag "settings" enumeration - Lesson 18
    [Flags]
    internal enum Settings
    {
        DebugMode = 1,
        InteractiveMode = 2,
        RememberSettings = 4,
        Optimize = 8
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
