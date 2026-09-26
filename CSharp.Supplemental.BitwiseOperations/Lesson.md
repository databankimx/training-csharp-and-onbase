# Bitwise Operations

Nineteen topics, building from "what is a bit" up through real-world uses of every bitwise operator. Standalone in Supplementary rather than tied to a specific chapter - this material is too involved for how early in the curriculum the type system gets introduced (Chapter 3), and it doesn't map cleanly onto any single later chapter either.

`Program.cs` has a menu of eleven runnable demos, one per topic below that has actual code attached - pick the one that matches whichever section you're reading.

---

## 1. A Bit about Integers

> *"I see what you did there..."*<br>
> &nbsp;&nbsp;~Literally Everyone

A byte is comprised of 8 bits, each of which can be a one or a zero. Binary (base 2) is the basis of bitwise computation.

In most programming languages, the default integer type is four bytes (32 bits) long, but for simplicity, we'll work with 8-bit integers in this lesson.

### Position Values

In all of our numbering systems, the position values (1's place, e.g.) are computable. Starting from the rightmost position and traversing right-to-left, the place values are bᵖ where b is the base and p is the position relative to the rightmost number (how many digits away).

So, for any numbering system, the place values look like this.

|    |    |    |    |    |
|----|----|----|----|----|
| ...| b³ | b² | b¹ | b⁰ |

And reading the value of a number, where d is the digit in a given place, looks like this (sum of each digit times its position value):

|          |          |          |          |          |
|----------|----------|----------|----------|----------|
|    ...   | + d * b³ | + d * b² | + d * b¹ | + d * b⁰ |

---

## 2. Base 10 (Decimal)

Remember our rule: starting from the rightmost position and traversing right-to-left, the place values are bᵖ where b is the base and p is the position relative to the rightmost number (how many digits away).

So, in base 10, we have:

* 10⁰ = 1
* 10¹ = 10
* 10² = 100
* 10³ = 1000
* etc.

So the position values look like this:

|       |       |       |       |       |
|-------|-------|-------|-------|-------|
|  ...  | 1000s |  100s |  10s  |   1s  |

So 1234 is read as:

|            |            |            |            |            |
|------------|------------|------------|------------|------------|
|    ...     | + 1 * 1000 |  + 2 * 100 |  + 3 * 10  |   + 4 * 1  |

---

## 3. Base 2 (Binary)

Following the same logic we saw in base 10, in base 2, the position values are just powers of two instead of ten (although I will express their values as base 10 here for convenience):

* 2⁰ = 1
* 2¹ = 2
* 2² = 4
* 2³ = 8
* 2⁴ = 16
* 2⁵ = 32
* 2⁶ = 64
* 2⁷ = 128
* and so on

This means in our binary byte we have the following places:

|      |      |      |      |      |      |      |      |
|------|------|------|------|------|------|------|------|
| 128s |  64s |  32s |  16s |  8s  |  4s  |  2s  |  1s  |

Reading binary numbers is similar to decimal: the sum of the non-zero position values.

So:

* 0000 0000 = **0**
* 0000 0001 = **1** (1 * 1)
* 0000 0010 = **2** (1 * 2) + (0 * 1)
* 0000 0011 = **3** (1 * 2) + (1 * 1)
* ...
* 1111 1111 = **255** (128 + 64 + 32 + 16 + 8 + 4 + 2 + 1)

That means that there are 256 possible values available in an 8-bit byte, which makes perfect sense, since 2⁸ = 256

And for an unsigned 8-bit integer, these values range from 0 to 255

---

## 4. Negative Integers

But that begs the question of how do we handle negative numbers?

### Can we use a different value for negatives?

We don't have a separate value to indicate whether a number is negative or positive. If we were working in base 3, we could imagine a system in which we used the third possible value as the negative like this:

| base 3 | value  |
|--------|--------|
|    0   |    0   |
|    1   |    1   |
|    2   |   -1   |

When designing early computation devices, this was given serious consideration, but the necessity of dealing with three states (different voltages) instead of just on and off (0 and 1) made this an impractical choice.

### Can we make the byte itself contain an indicator for negatives?

The next thought was to borrow one of the bits as an indicator for the sign (positive or negative) of the number.

This is the practice still in use today. For a signed integer, we borrow the leftmost (largest value) bit to store the sign (storing a 0 for positive numbers and a 1 for negative numbers).

In this system, we can see that:

0000 0001 = 1<br>
The sign bit is zero (positive), and the value of the remaining bits is 1

1000 0001 = -1<br>
The sign bit is one (negative), and the value of the remaining bits is 1

... and so on

### So we lose half of our (absolute) values

The result of this is that where previously we had 2⁸ (or 256) values using 8 bits, now we have 2⁷ (128) values (since one bit has been borrowed), but every value (or *almost* every value) can be either positive or negative.

We still need to account for zero, and as we'll see later, it would be problematic to differentiate between positive and negative zero, so there is a built-in imbalance. We have 255 total non-zero numbers. If we divide these (such that half are positive and half negative) we wind up with 127 for each with one number left over.

In most programming languages, the extra value is a negative, resulting in a range for an 8-bit signed integer -128 to 127.

### Why can't the extra value be a positive number?

It would seem like giving the extra value to the negatives is a bit counter-intuitive, since we probably use positive numbers more frequently when we're modeling a counting world.

Believe it or not, there is a logical (and necessary) reason for the extra value being negative, and the next few topics will explain why this is so.

---

## 5. Integer Overflow

There is a general problem when working with a fixed number of bits.

What happens when we exceed the maximum value? That is to say, what happens when we run out of bits when performing an arithmetic operation?

This scenario is known as "overflow," and it can have odd (but predictable) behavior.

A Note about Python:
* Python implements integers with a dynamic number of bits.
* This means that it is not possible to overflow an integer in Python.
* The next several sections still apply to how Python represents integers in binary, but since the bit size (and therefore the position of the sign bit) can change dynamically, Python coders can ignore the effects of overflow.

### Overflowing an Unsigned Integer

With an unsigned integer, we can imagine a condition where the value stored in an integer variable `b` is already all ones when we try to add `1` to it:

```csharp
byte b = 255; // b = 1111 1111
b++;          // b = ?
```

If we add `1` to our integer `b`, the value (in real world numbers) would be 256

```
    1111 1111         255
 +  0000 0001       +   1
  -----------        ----
  1 0000 0000         256
```

But as you can see, that would require a ninth bit, which we don't have available in a fixed-size, 8-bit value.

In the computer, the binary math is computed the same way as real-world arithmetic, but when the carry value of 1 moves to the 9th bit, it is simply lost, the result of which is that we see 255 + 1 = 0.

```csharp
byte b = 255;           // b = 1111 1111
b++;                    // b = 0000 0000
Console.WriteLine(b);   // Outputs -> 0
```

### Overflowing a Signed Integer

The results of integer overflow can become even more surprising when we look at what happens with a signed integer.

We can again imagine a condition where the value stored in a signed integer variable `s` is already equal to the maximum value when we try to add `1` to it.

```csharp
               //     ⌄--- remember, this is the sign bit
sbyte s = 127; // s = 0111 1111
s++;           // s = ?
```

The arithmetic in the real world looks like this:

```
    0111 1111         127
 +  0000 0001       +   1
  -----------        ----
    1000 0000         128
```

But, what's happened in our integer is that while we haven't overflowed outside the 8-bit size, we ***have*** overflowed our value into the sign bit.

```csharp
sbyte s = 127;          // s = 0111 1111
s++;                    // s = 1000 0000
Console.WriteLine(s);   // Outputs -> -128
```

As expected, the result is a negative number. But the negative value isn't what we'd likely expect. It seems like the rest of the value after the sign bit should be zero. This is also explained over the next several topics.

**Try it**: menu option 1.

---

## 6. Binary Addition

In order to understand how binary representations of integers work, it's useful to first take a look at the way binary addition is handled by a computer.

### The Binary Adder

A binary adder (at its core) is a circuit. Optimally, all that circuit should do is:

1. Take in two bits A & B (plus a carry bit C)
2. Add them together where

```
           A   B   Sum Carry
        a. 0 + 0 =  0    0
        b. 0 + 1 =  1    0
        c. 1 + 0 =  1    0
        d. 1 + 1 =  0    1
```

3. Return the output and pass the carry bit to the next bit adder

### Addition with the Adder Circuit

Looking at the behavior of the circuit, we can see that there are eight possible scenarios to account for:

|   A   |   B   |  Cin  |       |   S   | Cout  |
|:-----:|:-----:|:-----:|:-----:|:-----:|:-----:|
|   0   |   0   |   0   |       |   0   |   0   |
|   0   |   0   |   1   |       |   1   |   0   |
|   0   |   1   |   0   |       |   1   |   0   |
|   0   |   1   |   1   |       |   0   |   1   |
|   1   |   0   |   0   |       |   1   |   0   |
|   1   |   0   |   1   |       |   0   |   1   |
|   1   |   1   |   0   |       |   0   |   1   |
|   1   |   1   |   1   |       |   1   |   1   |

Let's examine the case where A = 1, B = 1, and Cin = 0. This is equivalent to adding 1 + 1 with no previous carry coming in.

* A & B enter the first half-adder. Both are 1s, so:
  * SUM (S₁) = A XOR B = 1 XOR 1 = 0
  * CARRY (C₁) = A AND B = 1 AND 1 = 1
* S₁ and Cin enter the second half-adder. Both are 0s, so:
  * SUM (S) = S₁ XOR Cin = 0 XOR 0 = 0
  * CARRY (C₂) = S₁ AND Cin = 0 AND 0 = 0
* S is 0, so the output sum is 0
* C₁ + C₂ = 1 + 0 = 1, and since 1 ≥ 1, Cout = 1

So, binary 1 + 1 = 0, with a carry (to the next place) of 1. Hence 1 + 1 = 2 (or rather 10 in binary).

### Negatives in a Binary Adder

Looking at its minimal implementation, a circuit like this is not designed to know the difference between positive and negative numbers. All it does is add binary digits (and track the carry bit).

But, since there is no subtraction circuit, computers add negatives in order to handle subtraction as an operation.

Because of that, we need a system of numbering in which the results from the adder circuit are consistent for positive and negative numbers without requiring a different circuit for negative numbers (or subtraction).

We know that we can use a specific bit for the sign, but how do we create a binary numbering system where the results are consistent? The next topics will explain that...

---

## 7. An Intuitive (but Wrong) Implementation of Negative Integers

For these illustrations, we'll simplify beyond what the computer uses and imagine a 4-bit integer type.

Since this is a signed integer, we'll use the leftmost bit as our sign bit.

That means that for our positive values, we have the following set:

* 0000 = 0, 0001 = 1, 0010 = 2, 0011 = 3, 0100 = 4, 0101 = 5, 0110 = 6, 0111 = 7

If we implement negative numbers intuitively, we'd assume that all of the binary digits would be the same as the positive numbers, with only the sign bit flipped, like this:

* 1000 = -0, 1001 = -1, 1010 = -2, 1011 = -3, 1100 = -4, 1101 = -5, 1110 = -6, 1111 = -7

### What's wrong with that?

This approach introduces two major flaws:

1. There are two different values for zero. This is wasteful, since there is no mathematical difference between 0 and -0. It also results in the possibility that adding zero could flip the sign on the number it's being added to.
2. The way the adder circuit works, we would get incorrect answers, so this would necessitate a more complicated circuit, affecting performance.

Recall that `0 - 1` is the same as `0 + -1` so let's look at the result of that (using our intuitive system) in binary addition.

1 (as 4-bit binary) is 0001. -1 (as 4-bit binary) is 1001.

```
  0001
+ 1001
  ----
  1010
```

So according to our system, 1 - 1 = -2.

Other numbers can lose a carry bit and flip the sign. Consider `7 - 3` or `7 + -3`. 7 = 0111, -3 = 1011.

```
  0111
+ 1011
  ----
  0010
```

So according to our system, 7 - 3 = 2. So, this solution is clearly not going to work. The next idea is...

---

## 8. One's Complement

Another option that was looked at in early computer design was to implement negative numbers as the ***one's complement*** of their positive counterparts.

One's complement is simply the binary complement (opposite value for each bit) of the positive value.

So with our known positive values (in our imaginary 4-bit integer): 0000 = 0, 0001 = 1, 0010 = 2, 0011 = 3, 0100 = 4, 0101 = 5, 0110 = 6, 0111 = 7

The one's complement implementation would look like this: 1111 = -0, 1110 = -1, 1101 = -2, 1100 = -3, 1011 = -4, 1010 = -5, 1001 = -6, 1000 = -7

### OK - So what's wrong with this one?

We still have the problem of both positive and negative zeroes. Addition is also still a problem, though less so than in the previous example.

**1 - 1**:

```
  0001
+ 1110
------
  1111
```

This gives a result of `1 - 1 = -0`, which isn't perfect (because what even is -0), but if we add 1 to the result:

```
      1111
    + 0001
    ------
 1 <- 0000  *lost (overflowed) carry bit here
```

We end up with the expected value of `0`.

**7 - 3**:

```
      0111
    + 1100
    ------
 1 <- 0011
```

Which gives `7 - 3 = 3`, which is wrong, but again if we add 1 (which we can think of as wrapping the lost carry bit)...

```
      0011
    + 0001
    ------
      0100
```

We get the right answer of `7 - 3 = 4`.

### So Close!

This business of wrapping the lost carry bit doesn't make sense without extra hardware (and extra memory, since we don't have a fifth bit to hold it in).

So, how do we address the problem of getting results that are off by one? And is there a way to get rid of that pesky second zero?

---

## 9. Two's Complement

Enter the solution that is implemented for numbers in modern computing: **two's complement**.

Two's complement is just one's complement plus one.

So with our known positive values: 0000 = 0, 0001 = 1, 0010 = 2, 0011 = 3, 0100 = 4, 0101 = 5, 0110 = 6, 0111 = 7

The two's complement implementation would look like this:
* 0000 =  0, 1111 = -1, 1110 = -2, 1101 = -3, 1100 = -4, 1011 = -5, 1010 = -6, 1001 = -7<br>
  ... which leaves a wasted bit, so 1000 = -8

... and this is why we have an extra negative value in integer data types.

We can check ourselves by looking at the same examples we've been using:

**1 - 1**:

```
      0001
    + 1111
    ------
 1 <- 0000
```

The previously lost carry bit doesn't matter any more, and we get the correct answer: `1 - 1 = 0`

**7 - 3**:

```
      0111
    + 1101
    ------
 1 <- 0100
```

Again, although we lose a carry bit, we end up with the right answer: `7 - 3 = 4`

### Problem Solved!

Implementation of a two's complement negatives system ensures that we always get the correct answer when performing binary addition with our adder circuits, even if the signs don't match.

### Wait a minute! Now integer overflow makes sense too!

Now that we understand that the number system uses two's complement for negatives, the overflow behavior we saw earlier makes sense.

If we go back to using an 8-bit integer, and look at our 127 + 1 problem:

```csharp
sbyte s = 127;          // s = 0111 1111
s++;                    // s = 1000 0000
Console.WriteLine(s);   // Outputs -> -128
```

We can predict that this is the expected overflow behavior by doing the binary addition:

```
  0111 1111
+ 0000 0001
-----------
  1000 0000
```

And we see the previously inexplicable behavior of `127 + 1 = -128` for an 8-bit signed integer emerge as a result of overflowing into the sign bit.

Just to verify that this is the expected behavior:

* 128 in binary is `1000 0000`
* One's complement of that is `0111 1111`
* Two's complement (one's complement plus one) then is `1000 0000`

So `-128` is the expected result.

---

## 10. Truth Tables

### Refresher: Logic Operators

When working with Boolean (logical) operators, we compare the values `true` and `false`. As a refresher, here are the truth tables for the common logical operations:

**Negation (NOT)**

| ! | p |
|:--|--:|
| F | t |
| T | f |

**Conjunction (AND)**

| p | &&| q |
|:--|:-:|--:|
| t | T | t |
| t | F | f |
| f | F | t |
| f | F | f |

**Disjunction (OR)**

| p | ││| q |
|:--|:-:|--:|
| t | T | t |
| t | T | f |
| f | T | t |
| f | F | f |

**Exclusive Disjunction (XOR)**

Note: I'll use ⊕ as the XOR symbol, since this operator typically doesn't exist in most programming languages.

| p | ⊕ | q |
|:--|:-:|--:|
| t | F | t |
| t | T | f |
| f | T | t |
| f | F | f |

### Bitwise Operators

When performing bitwise operations, we compare `0` and `1` at each bit position instead of `true` and `false`.

**Complement (Negation)**

| ~ | b |
|:--|--:|
| 0 | 1 |
| 1 | 0 |

**Conjunction (AND)**

| b1 | & | b2 |
|:---|:-:|---:|
| 1  | 1 |  1 |
| 1  | 0 |  0 |
| 0  | 0 |  1 |
| 0  | 0 |  0 |

**Disjunction (OR)**

| b1 | │ | b2 |
|:---|:-:|---:|
| 1  | 1 |  1 |
| 1  | 1 |  0 |
| 0  | 1 |  1 |
| 0  | 0 |  0 |

**Exclusive Disjunction (XOR)**

| b1 | ^ | b2 |
|:---|:-:|---:|
| 1  | 0 |  1 |
| 1  | 1 |  0 |
| 0  | 1 |  1 |
| 0  | 0 |  0 |

### Remember

These truth tables just show what would happen at a single bit position. When we implement these processes using byte or multiple-byte sized types, any of these operations will be performed for every bit position across both operands.

---

## 11. Using Bitwise AND (`&`)

The bitwise AND operator (`&`) is used to compare two values and return only the intersection of their set (`1`) bits.

At each binary position (i), the result of the operation is equivalent to multiplying the bits together: `(a & b)ᵢ = aᵢ × bᵢ`

Since 1 × 1 is 1, and any number n × 0 is 0, we can see that this matches the bitwise truth table we laid out for this operator, where a 1 is returned only in bit positions where both of the compared values contain 1.

Consider this scenario:

```
    a = 10011100₂ = 156₁₀
    b = 00110100₂ =  52₁₀
a & b = 00010100₂ =  20₁₀
```

Only the positions where the bit is 1 in both a and b return a 1 from the `&` operation. As long as we understand that what's being compared are the individual bits, we can express the values in any base with the same results.

**Try it**: menu option 2.

### A Simple Example: Checking for an Even Number

There are many places where it's useful to check if a number is odd or even. Typically, most of us have been taught to do that using a modulo (remainder) computation:

```csharp
bool IsEven(int n) => n % 2 == 0;
```

Another approach would be to perform a bitwise check on the ones-place bit:

```csharp
bool IsEven(int n) => (n & 1) == 0;
```

Any even number must have 0 in the 1's place bit, so a bitwise AND against the 1 bit will always return 0 if even and 1 if odd.

The bitwise comparison is inherently faster than a modulo computation, because division is a cumbersome process in a computer. Of course, in modern programming languages, the compilers are optimized for well-known scenarios like this one, so `n % 2` will compile to the same assembly language instructions as `n & 1`, which means there isn't a practical reason to choose one over the other.

**Try it**: menu option 3.

---

## 12. Bitwise AND - Real-World Example: Bit-Flags

### Scenario

You are employed by a large company called MacroWare. One of the company's flagship software products is an office product suite that contains several different programs. The installation package installs all of the programs, regardless of which have been purchased by the user, and you're tasked with checking each program's licensed state before allowing it to run.

### An Obvious Approach

You might intuitively default to using boolean values to enable/disable each feature - a bool property per product. This works, but each boolean value consumes one byte in memory. If you're storing this in a database, that's eight columns (minimum one byte apiece) per user.

### An Alternative Approach: Bit-Flags

Suppose, instead of using one byte per product, you used one bit. One approach is to define each product as a binary bit position value:

```csharp
[Flags]
enum ProductLicenses
{
    None              = 0b0000_0000,
    WordProcessing    = 0b0000_0001,
    Spreadsheets      = 0b0000_0010,
    Presentations     = 0b0000_0100,
    EmailClient       = 0b0000_1000,
    Notebook          = 0b0001_0000,
    Collaboration     = 0b0010_0000,
    ProjectManagement = 0b0100_0000,
    Publishing        = 0b1000_0000
}
```

Then, when you need to check if a product is licensed, you use bitwise AND to validate a specific bit position:

```csharp
byte licenses = someValueFromDatabase;
int wp = (int)ProductLicenses.WordProcessing;
if ((licenses & wp) != wp)
{
    throw new ApplicationException("Not licensed for word processing.");
}
```

Since each enum value only has a single bit set, that bit is the only one for which a 1 could possibly be returned by the AND. If the compared value doesn't have that bit set, the result is 0; if it does, the result is the bit's value.

This technique works with any collection of settings where the values are enumerated as powers of two, and the integer size determines how many values you can store.

**Try it**: menu option 4.

---

## 13. Using Bitwise OR (`|`)

The bitwise OR operator (`|`) is used to compare two values and return the union of their set (`1`) bits.

This is an *inclusive* OR: the result bit is set to 1 any time a 1 is found in either value, irrespective of the other value.

Consider this scenario:

```
    a = 10011100₂ = 156₁₀
    b = 00110100₂ =  52₁₀
a | b = 10111100₂ = 188₁₀
```

**Try it**: menu option 5.

### A Simple Example: Adding a Permission

In C#, the `File.Open()` method accepts a `FileAccess` argument, where Read = 1, Write = 2, ReadWrite = 3. We can use bitwise OR (as a combined assignment operator) to apply the permissions needed:

```csharp
FileAccess permissions = FileAccess.Read;      // 0000_0001 (1 = Read)
if (needToWrite) permissions |= FileAccess.Write;  // 0000_0010 (2 = Write) gets added
// If write was added, permissions is 0000_0011 (3 = ReadWrite)
```

**Try it**: menu option 6.

---

## 14. Bitwise OR - Real-World Example: Bit-Flags Redux

Sometimes it's useful to have a particular value represented by more than one bit in a bit-flag. Bitwise OR can be used to simplify this.

### Scenario

Back at MacroWare, you're asked to handle license checks for packages that bundle several programs together: Personal (Word Processing + Email), Work (Personal + Spreadsheets + Presentations + Notebook + Collaboration), and Ultra Deluxe (everything).

### An Obvious (but Clunky) Approach

You could add new enum values for each package and check for multiple conditions everywhere a product is checked - but this decouples the package values from the products they actually include, and any future change to what's in a package means hunting down every place that checks it.

### An Alternative Approach: Combined Bit-Flags

Using bitwise OR lets you combine multiple bit values, since OR returns the union rather than the intersection. WordProcessing (1 = 0000 0001) OR'd with EmailClient (8 = 0000 1000) gives 9 (0000 1001) - both bits set at once. This is sometimes called a bit-mask.

Rather than hardcoding that 9, you can express the relationship directly in the enum:

```csharp
[Flags]
enum ProductLicenses
{
    None              = 0b0000_0000,
    WordProcessing    = 0b0000_0001,
    Spreadsheets      = 0b0000_0010,
    Presentations     = 0b0000_0100,
    EmailClient       = 0b0000_1000,
    Notebook          = 0b0001_0000,
    Collaboration     = 0b0010_0000,
    ProjectManagement = 0b0100_0000,
    Publishing        = 0b1000_0000,
    Personal    = WordProcessing | EmailClient,
    Work        = Personal | Spreadsheets | Presentations | Notebook | Collaboration,
    UltraDeluxe = Work | ProjectManagement | Publishing
}
```

Now your original, simple licensing check (`(licenses & wp) != wp`) keeps working unmodified regardless of which package a user owns - the Work package's value has the Word Processing bit set among its several bits, so ANDing against just that bit still gives the same right answer it always did. Package composition changes only ever touch the enum, never the check.

**Try it**: this is the same demo as Lesson 12 (menu option 4) - `Program.cs`'s `ProductLicenses` enum already includes the `Personal`/`Work`/`UltraDeluxe` composite values described here.

---

## 15. Using Bitwise NOT (`~`)

The bitwise NOT operator (`~`) is used with a single value and returns the one's complement of the original value (reverses each bit).

Consider this scenario:

```
  a = 10011100₂ = 156₁₀
  -------------
 ~a = 01100011₂ =  99₁₀
```

**Try it**: menu option 7.

### A Word of Warning

In Python, all integers are signed and of arbitrary size, so a bitwise NOT operation there yields unexpected results if you're treating the value as unsigned - you have to mask the result against `(1 << num_bits) - 1` to lose the sign bit and get the answer you'd expect from a fixed-size unsigned value.

### Real-World Applications

Outside of systems-level programming, bitwise NOT doesn't have a ton of uses, but it shows up (under the covers) in several encryption algorithms, high-performance mathematical algorithms, and bit-masking for IP address computations, among others.

One example is setting an arbitrary bit in an integer "word":

```csharp
static int SetBit(byte word, byte pos, byte value)
{
    int mask = 1 << pos;
    // Here, we're using `~` to flip the mask bits
    if (value == 0) return word & ~mask;
    else if (value == 1) return word | mask;
    else return word;
}
```

---

## 16. Using Bitwise XOR (`^`)

The bitwise XOR operator (`^`) compares two values and returns 1 in each position where the bits are not equal across the two values - one or the other (but not both) contains a 1.

Consider this scenario:

```
    a = 10011100₂ = 156₁₀
    b = 00110100₂ =  52₁₀
a ^ b = 10101000₂ = 168₁₀
```

**Try it**: menu option 8.

### A Simple Example: Swapping Two Values

A simple example of using bitwise XOR is swapping two values without creating a temp variable:

```csharp
static void SwapValues(ref int a, ref int b)
{
    a ^= b;
    b ^= a;
    a ^= b;
}
```

Walking through it with a = 5 (0101), b = 8 (1000): `a = a ^ b` gives a = 1101; `b = a ^ b` gives b = 0101 (a's original value); `a = a ^ b` gives a = 1000 (b's original value). Both values have swapped, no temp variable needed.

---

## 17. Bitwise XOR - Real-World Example: Encryption

One particularly interesting behavior of XOR is that it is reversible. If `z = x ^ y`, then `z ^ y` is equivalent to `x`.

This ability to XOR against a known value twice - once to obfuscate, once to restore - is the basis for a simple encryption/decryption algorithm. Since the same key is used both ways, this is *symmetric key encryption*.

> Note: As presented here, this is **not** a secure algorithm and should not be used for any production applications. This is just an example of a simple procedure to encrypt and decrypt text.

```csharp
static string EncryptDecrypt(string text, int key)
{
    var inString = new StringBuilder(text);
    var outString = new StringBuilder(text.Length);
    for (int i = 0; i < text.Length; i++)
    {
        outString.Append((char)(inString[i] ^ key));
    }
    return outString.ToString();
}
```

With key = 30 and text "Hello World!", this yields cipherText "V{rrq>Iqlrz?" - and running that cipherText back through the same function with the same key restores "Hello World!" exactly.

**Try it**: menu option 9.

### Why This Isn't Secure

* A single-byte integer key means only 256 possible keys to brute-force.
* It requires both parties to share the same private key, unlike modern public-key algorithms.

That said, the concept of XOR with a symmetric key is used widely as a step in real encryption algorithms like DES and AES, and single-byte XOR encryption is genuinely fast. Using a random key per character (a one-time pad) makes even this simple mechanism immune to common cryptanalysis attacks like frequency analysis.

The NSA has called [Vernam's Patent](https://patents.google.com/patent/US1310719A/en), a circuit-based implementation of XOR, "perhaps one of the most important in the history of cryptography."

---

## 18. Using Bit-Shifts (`<<` and `>>`)

A bit-shift operation shifts the bit values in a field a specified number of positions to the left (`<<`) or right (`>>`).

Right-shifting a value is equivalent to dividing by two to the power of the number of positions shifted: `n >> p = n / 2ᵖ`. So `128 >> 3 = 16`.

Left-shifting is equivalent to multiplying by two to the power of the number of positions shifted: `n << p = n * 2ᵖ`. So `16 << 3 = 128`.

**Try it**: menu option 10.

### Overflow and Underflow Can Result in Data Loss

Shifting `64 << 3` would need a ninth bit to hold the result (256) in an 8-bit value - that bit is simply lost, and the result is 0. The same thing happens shifting right past the ones place. Note: bit-shift overflow does not occur in Python, since Python integers aren't fixed-size.

### A Simple Example: Iterating Over a Bit-Flag

Given a `[Flags]` enum, you can cycle through every set bit with a shift-based loop:

```csharp
int settings = new Random().Next(0, 15);
int currentValue = 1;
while (settings > 0)
{
    if ((settings & 1) == 1)
    {
        Console.WriteLine($"- {Enum.GetName(typeof(Settings), currentValue)}");
    }
    settings >>= 1;      // move to the next bit in `settings`
    currentValue <<= 1;  // move to the next value to check against
}
```

**Try it**: menu option 11.

---

## 19. Bit-Shift - Real-World Example: Reconstructing Integers

### Scenario

You have an incoming byte stream representing 32-bit integers, where within each set of four bytes, the least significant byte comes first.

The first byte (bits 0-7) can be taken as-is. But the second byte represents bits 8-15 - naively adding it directly gives a wrong, too-small result, since bits 8-15 have values from 256 upward. You need to multiply it by 256 first (or, equivalently, bit-shift it left by 8):

```csharp
int num = bytes[0];
num += bytes[1] << 8;   // not bytes[1] * 256 directly, though the result is the same
```

|Byte|Low Bit|Shift|Value|
|:-:|:-:|:-:|-:|
|0|2⁰|none|1|
|1|2⁸|<< 8|256|
|2|2¹⁶|<< 16|65,536|
|3|2²⁴|<< 24|16,777,216|

A loop handles each four-byte set:

```csharp
int num = 0;
for (int p = 0; p < 4; p++)
{
    num += bytes[p] << (p * 8);
}
```

**Try it**: menu option 12.
