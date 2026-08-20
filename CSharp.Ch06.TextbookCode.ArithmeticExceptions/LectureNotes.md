# Ch06 Textbook Code: Arithmetic Exceptions

## What This Is

Four buttons, four arithmetic-exception scenarios, click and see the result immediately via `MessageBox`: integer overflow (unchecked), the same overflow inside a `checked` block, float overflow, and float divide-by-zero.

No bugs found. One cosmetic fix: the window's title was left as the generic `"Form1"` default in the original download, the only `TextbookCode.*` project in this chapter where that was true, changed to `"ArithmeticExceptions"` for consistency with every sibling project.

---

## The Same Contrast, Two Different Presentations

This is functionally the same demonstration as `ArithmeticExceptions()` in `CSharp.Ch06.Supplemental.05.ExceptionHandling`, integer overflow behaves differently depending on `checked`/`unchecked`, float overflow and divide-by-zero don't throw at all (`Infinity`, per IEEE 754), regardless of context. That project shows the results via console output, this one shows each result in its own popup as you click, worth trying both, seeing the *same* underlying behavior demonstrated two different ways can help make it stick.

---

## Worth Noticing: `Integer Overflow` and `Integers Checked` Are Nearly Identical Code

```csharp
private void integerOverflowButton_Click(object sender, EventArgs e)
{
    try
    {
        int a = 1000000000;
        int b = 1000000000;
        int c = a * b;
        MessageBox.Show("c = " + c.ToString());
    }
    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
}

private void integersCheckedButton_Click(object sender, EventArgs e)
{
    checked
    {
        try
        {
            int a = 1000000000;
            int b = 1000000000;
            int c = a * b;
            MessageBox.Show("c = " + c.ToString());
        }
        catch (Exception ex) { MessageBox.Show(ex.ToString()); }
    }
}
```

The *only* difference between these two handlers is the `checked { }` wrapper. Click "Integer Overflow" and you'll see a nonsensical wrapped-around number (`c = ` some negative value that isn't the real product). Click "Integers Checked" and you'll see an `OverflowException` message box instead, the catch block actually firing this time. Same multiplication, same numbers, one keyword's difference in outcome.
