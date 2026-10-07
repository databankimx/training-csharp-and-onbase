---
title: "Matrix Exponentiation - O(log n)"
chapter: 0
index: 5
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private const int N = 40;
    private const int KnownF40 = 102_334_155;

    private static void Main()
    {
        int count = 0;
        var timer = Stopwatch.StartNew();
        int result = Fib(N, ref count);
        timer.Stop();

        Console.WriteLine($"f({N}) = {result:#,0}");
        Console.WriteLine(result == KnownF40 ? "Result verified." : "Result INCORRECT.");
        PrintReport("Matrix exponentiation", N, count, timer.Elapsed);
    }

    // Fibonacci numbers satisfy a matrix identity: [[1,1],[1,0]]^n gives F(n).
    // Matrix multiplication for a fixed 2x2 size is O(1). Exponentiation by squaring
    // computes the nth power in O(log n) multiplications - genuinely better than any O(n) approach.
    private static int Fib(int n, ref int count)
    {
        if (n < 2) return n;
        long[,] baseMatrix = { { 1, 1 }, { 1, 0 } };
        long[,] result = MatrixPower(baseMatrix, n - 1, ref count);
        return (int)result[0, 0];
    }

    private static long[,] MatrixPower(long[,] matrix, int power, ref int count)
    {
        long[,] result = { { 1, 0 }, { 0, 1 } }; // Identity
        long[,] b = matrix;
        while (power > 0)
        {
            count++;
            if ((power & 1) == 1) result = Multiply(result, b);
            b = Multiply(b, b);
            power >>= 1;
        }
        return result;
    }

    private static long[,] Multiply(long[,] a, long[,] b) => new long[,]
    {
        { a[0,0]*b[0,0] + a[0,1]*b[1,0], a[0,0]*b[0,1] + a[0,1]*b[1,1] },
        { a[1,0]*b[0,0] + a[1,1]*b[1,0], a[1,0]*b[0,1] + a[1,1]*b[1,1] }
    };

    private static void PrintReport(string name, int n, int ops, TimeSpan elapsed)
    {
        Console.WriteLine($"{name}: {ops:#,0} operations for n={n}");
        Console.WriteLine($"Elapsed: {elapsed.TotalMilliseconds:F3} ms");
    }
}
```
