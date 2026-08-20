# Ch06 Textbook Code: Factorials (Real-World Scenario 2)

## What This Is

An interactive factorial calculator demonstrating three distinct failure modes and how each gets handled differently:

```csharp
try
{
    resultTextBox.Clear();

    long n;
    if (!long.TryParse(nTextBox.Text, out n))
    {
        MessageBox.Show("The number must be an integer.");
        return;
    }
    resultTextBox.Text = Factorial(n).ToString();
}
catch (ArgumentOutOfRangeException) { MessageBox.Show("The number must be at least 0."); }
catch (OverflowException) { MessageBox.Show("This number is too big to calculate its factorial."); }
catch (Exception ex) { MessageBox.Show(ex.ToString()); }
```

One pre-existing typo fixed: `ArgumentOutOfRangeException`'s message read `"...to calcualte n!"`, corrected to `"...to calculate n!"`. No other bugs.

---

## Three Failure Modes, Three Different Handling Strategies

Type `abc` into the field, and `long.TryParse` catches it *before* an exception is ever thrown, that's not an exception path at all, `TryParse` returns `false` and the code shows a message and returns early. This is worth noticing on its own: not every kind of "bad input" needs to be handled with exceptions, `TryParse` exists specifically so parsing failures don't have to be.

Type `-5`, and `Factorial()` deliberately throws:

```csharp
if (n < 0) throw new ArgumentOutOfRangeException(
    "n", "The number n must be at least 0 to calculate n!");
```

Type something large, like `25`, and the `checked` block inside `Factorial()` throws `OverflowException` on its own, `long` simply can't hold `25!` (that's roughly 1.55 × 10²⁵, while `long.MaxValue` is only about 9.2 × 10¹⁸).

Three different problems, three different `catch` blocks, each showing a message tailored to what actually went wrong, rather than one generic "something went wrong" catch-all. Worth comparing this directly to `CSharp.Ch06.Supplemental.05.ExceptionHandling`'s `SpecificToGeneral()`, which covers the same "order your catches from specific to general" principle with a different scenario.

---

## Try It Yourself

Try `20` (large but valid, `20!` fits comfortably in a `long`), then `21` (still fits), then keep increasing until you find the exact boundary where `OverflowException` starts firing instead. Then try `-1` and an empty/non-numeric input, and confirm each produces its own distinct message.
