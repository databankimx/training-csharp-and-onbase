# Supplemental: Bitwise Operations

## What This Is

A menu-driven reference for twelve bitwise concepts, each with a small runnable demo. These aren't meant to be run all at once -- pick the demo that matches whatever section of this file you're reading and step through it.

Bitwise operations work directly on the binary representation of integers, flipping, masking, or shifting individual bits rather than treating the value as a whole number. They show up in flag enums, embedded-protocol parsing, XOR-based tricks, and anywhere else a program needs to pack or inspect individual bits in a value.

---

## Background: Binary and Overflow

### How Binary Works

Every integer in a computer is stored as a sequence of bits, each either 0 or 1. A `byte` has 8 bits; an `int` has 32. The rightmost bit represents 1, the next represents 2, then 4, 8, 16, and so on -- each position is a power of two. `01001001` in binary is 64 + 8 + 1 = 73.

The `0b` prefix in C# is a binary literal. `0b01001001` and `73` are the same value.

### Integer Overflow (Demo 1)

```csharp
byte b = 255;
b++;
Console.WriteLine(b); // 0

sbyte s = 127;
s++;
Console.WriteLine(s); // -128
```

`byte` is 8 bits, maximum 255 (`11111111`). Adding 1 would need a ninth bit -- which doesn't exist -- so the result wraps around to 0 (`00000000`). No exception, no warning.

`sbyte.MaxValue` is 127 because signed types reserve the leftmost bit for the sign. Incrementing past it flips that bit and the value wraps to -128.

---

## The Operators

### Bitwise AND -- `&` (Demo 2)

Compares two values bit by bit. A result bit is `1` only where **both** inputs have `1`.

```
10011100 (156)
00110100 ( 52)
---------
00010100 ( 20)
```

```csharp
int a = 0b10011100; // 156
int b = 0b00110100; // 52
Console.WriteLine(a & b); // 20
```

### Checking Even/Odd With AND (Demo 3)

The lowest bit of any integer is 1 if the number is odd, 0 if it's even. `n & 1` isolates that bit -- faster than modulus and produces the same result:

```csharp
bool isEvenMod = n % 2 == 0;
bool isEvenAnd = (n & 1) == 0; // same result, no division
```

### Bit-Flags With AND (Demo 4)

`[Flags]` enums pack multiple true/false values into a single integer, one bit per flag. Use `&` to test whether a specific flag is set:

```csharp
[Flags]
enum ProductLicenses : byte
{
    WordProcessing = 0b0000_0001,
    Spreadsheets   = 0b0000_0010,
    Presentations  = 0b0000_0100,
    // ...
    Personal = WordProcessing | EmailClient,
    Work     = Personal | Spreadsheets | Presentations | Notebook | Collaboration
}

byte licenses = (byte)ProductLicenses.Work;

// Test one flag:
bool hasWordProcessing = (licenses & (byte)ProductLicenses.WordProcessing) == (byte)ProductLicenses.WordProcessing;
```

Each member of a `[Flags]` enum should be a distinct power of two so that no two members share a bit. Combined values like `Work` and `Personal` are `|`-ed together at declaration -- see Bitwise OR below.

### Bitwise OR -- `|` (Demos 5 and 6)

A result bit is `1` where **either** input has `1`.

```
10011100 (156)
00110100 ( 52)
---------
10111100 (188)
```

```csharp
Console.WriteLine(156 | 52); // 188
```

Practical use: adding a flag to a value without disturbing the others:

```csharp
FileAccess permissions = FileAccess.Read;
if (needToWrite) permissions |= FileAccess.Write;
```

`|=` is the compound-assignment form -- sets `Write` without clearing `Read`.

### Bitwise NOT -- `~` (Demo 7)

Flips every bit. The `byte` gotcha: `~` always promotes the operand to a signed 32-bit `int` before flipping. Cast back to `byte` or you'll get a very large negative number instead of the small complement you expected:

```csharp
byte b = 0b10011100; // 156
byte n = (byte)~b;   // 99 -- correct, with the cast
```

Also the mechanism behind clearing a specific bit:

```csharp
private static int SetBit(byte word, byte pos, byte value)
{
    int mask = 1 << pos;
    if (value == 0) return word & ~mask; // clear the bit at pos
    if (value == 1) return word | mask;  // set the bit at pos
    return word;
}
```

`~mask` flips the mask so that `&` clears exactly that one bit while leaving everything else alone.

### Bitwise XOR -- `^` (Demo 8)

A result bit is `1` where the inputs **differ**.

```
10011100 (156)
00110100 ( 52)
---------
10101000 (168)
```

```csharp
Console.WriteLine(156 ^ 52); // 168
```

XOR has a useful property: `a ^ a == 0` and `a ^ 0 == a`. This makes it possible to swap two variables without a temporary:

```csharp
a ^= b;
b ^= a;
a ^= b;
```

After three XORs, the values are exchanged. Not faster than a temporary in modern C#, but a well-known pattern worth recognizing.

### XOR Encryption (Demo 9)

XOR encryption is symmetric and self-reversing: applying the same key twice restores the original. Not cryptographically secure, but illustrates the concept cleanly:

```csharp
private static string EncryptDecrypt(string text, int key)
{
    var sb = new StringBuilder(text.Length);
    foreach (char c in text)
        sb.Append((char)(c ^ key));
    return sb.ToString();
}

string cipher = EncryptDecrypt("Hello World!", 30);
string plain  = EncryptDecrypt(cipher, 30); // back to "Hello World!"
```

### Bit Shifts -- `<<` and `>>` (Demos 10 and 11)

`<<` shifts bits left (multiplying by powers of two). `>>` shifts right (dividing). Bits shifted off the end are lost:

```csharp
byte x = 128;
Console.WriteLine((byte)(x >> 3)); // 16
x = 16;
Console.WriteLine((byte)(x << 3)); // 128
x = 64;
Console.WriteLine((byte)(x << 3)); // 0 -- overflowed, bits lost
```

Practical use: iterating the set bits of a flag value by repeatedly shifting right by 1 and testing the lowest bit:

```csharp
byte flag = 0b01010101;
byte current = 1;
while (flag > 0)
{
    if ((flag & 1) == 1) Console.WriteLine($"Bit worth {current} is set");
    flag >>= 1;
    current <<= 1;
}
```

### Reconstructing Integers From Bytes (Demo 12)

Bytes arriving from a binary protocol or file can be reassembled into integers by shifting each byte into its correct position:

```csharp
private static int ReconstructInteger(byte[] data, int size = 4)
{
    int num = 0;
    for (int p = 0; p < data.Length; p++)
        num += data[p] << (p * 8);
    return num;
}
```

Byte 0 occupies bits 0-7 (no shift), byte 1 occupies bits 8-15 (shift left 8), and so on. This is little-endian byte order -- the least significant byte first.

---

## Takeaways

- `&` masks: result bits are 1 only where both inputs are 1. Use it to test or clear flags.
- `|` combines: result bits are 1 where either input is 1. Use it to set flags.
- `^` differs: result bits are 1 where inputs differ. Self-inverse: `a ^ key ^ key == a`.
- `~` complements every bit. Always casts the operand to `int` first -- cast back to `byte` explicitly.
- `<<` / `>>` multiply or divide by powers of two. Bits shifted off the end are lost.
- `[Flags]` enum members must be distinct powers of two for bit testing to work correctly.
- Integer overflow wraps silently by default. Wrap in `checked { }` to throw instead.
