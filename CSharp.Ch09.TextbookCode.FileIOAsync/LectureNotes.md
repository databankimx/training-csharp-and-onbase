# Ch09 Textbook Code: File I/O Async

## What This Is

A genuinely runnable WinForms lab (unlike `Chapter9`'s dead-code collection): one button, click it and the app asynchronously searches every `.txt` file in a folder for a substring, writes the matching filenames out to a results file, then opens that file automatically. No bugs found, `button1.Click` is correctly wired (unlike the missing-wiring bug found in `CSharp.Ch07.TextbookCode.WinFormApp`).

---

## Two Hardcoded Paths, Both Required to Actually Exist

```csharp
string outputFileName = @"c:\Test\FoundFiles.txt";
await SearchDirectory(@"c:\Chapter9Samples", "A", outputFileName);
```

Unlike `Chapter9.IOSamples`'s hardcoded paths (dead code, never actually reached), this one **is** the live entry point, clicking the button really does try to touch both of these paths. `Directory.GetFiles(@"c:\Chapter9Samples")` throws `DirectoryNotFoundException` if that folder doesn't exist, and `File.CreateText(@"c:\Test\FoundFiles.txt")` throws the same if `c:\Test\` doesn't exist (`CreateText()` creates the *file*, not any missing parent directories). To actually see this lab do something, create `C:\Chapter9Samples\` with a few `.txt` files inside (at least one containing the letter "A" to get a match) and create `C:\Test\` as an empty folder, before clicking the button. Preserved exactly as downloaded rather than fixed, matching this training set's policy for raw textbook content, but worth knowing before you click the button expecting output.

---

## The Async Pattern Itself: Worth Reading Closely

```csharp
private static async Task FindTextInFilesAsync(string[] fileNames, string searchString, StreamWriter outputFile)
{
    foreach (string fileName in fileNames)
    {
        if (fileName.ToLower().EndsWith(".txt"))
        {
            StreamReader streamReader = new StreamReader(fileName);
            string textOfFile = await streamReader.ReadToEndAsync();
            streamReader.Close();

            if (textOfFile.Contains(searchString))
            {
                await outputFile.WriteLineAsync(fileName);
            }
        }
    }
}
```

This is a clean, genuinely instructive example of `async`/`await` applied to file I/O specifically: `button1_Click` itself is `async void` (the one legitimate use of `async void`, a UI event handler, see `CSharp.Ch06.Supplemental.02.LambdaExpressions`'s lecture notes for why `async void` is otherwise discouraged), and it `await`s `SearchDirectory()`, which in turn `await`s `FindTextInFilesAsync()`, which `await`s both `ReadToEndAsync()` and `WriteLineAsync()` inside a loop. The UI thread stays responsive (the window's title bar updates to "Searching..." and is actually paintable) while every file read and the final write happen asynchronously. Worth comparing directly against `CSharp.Ch09.Supplemental.04.FileIO`'s `UsingAsyncIo()`, which covers the same `StreamReader.ReadToEndAsync()`/`StreamWriter`-async-write techniques in a more curated, step-by-step way; this lab shows the same ideas woven into a complete, realistic multi-file search feature.

---

## Style Note: No `using` Statements

`StreamWriter`/`StreamReader` here are manually `.Close()`d rather than wrapped in `using` blocks. This isn't a resource leak (`.Close()` is called in every path this code actually takes), just an older style choice, `using` (or the more modern `await using` for async-disposable types) is the safer default in new code, since it guarantees cleanup even if an exception is thrown partway through, something manual `.Close()` calls don't protect against. Left exactly as downloaded.
