# Ch04 Textbook Code: Short Path Names

## What This Is

The interactive WinForms original of the `GetShortPathName` P/Invoke demo already ported into `ImportedComDll()` in `CSharp.Ch04.UsingTypes`. A path text box, a Go button, and a read-only field showing the converted 8.3 short path.

---

## The Bug That Was Here

```csharp
private void Form1_Load(object sender, EventArgs e)
{
    fileTextBox.Text = Application.ExecutablePath;
}
```

That method is defined and correctly pre-fills `fileTextBox` with this program's own executable path, a sensible default so there's something to click "Go" on immediately. Except the original `Form1.Designer.cs` never actually subscribes it to the form's `Load` event, `InitializeComponent()` had no `this.Load += ...` line at all. The method just sat there, fully written, never called. The field started blank every time, and anyone running the raw download would have had to type or paste in a path by hand before the demo did anything.

Fixed by adding the missing wire-up:

```csharp
this.Load += new System.EventHandler(this.Form1_Load);
```

This is a real bug, not a style choice, an event handler that's defined but never subscribed is dead code no matter how correct its body is. Worth knowing as its own category of mistake: it doesn't throw, it doesn't warn, it just silently does nothing, and the only way to notice is realizing a feature you expected to happen never did.

---

## Worth Actually Using This One

No debugger required. Launch it, the path field now starts pre-filled with this program's own `.exe` path (once you've built it), click Go, and watch the short 8.3-style path come back, spaces and long segment names collapsed into the classic `~1`-style abbreviations.
