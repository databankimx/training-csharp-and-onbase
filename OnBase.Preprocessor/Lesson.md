# OnBase Preprocessor

## What This Is

Somewhere in a COLD/DIP import pipeline, OnBase is about to hand a text file to another program and ask it to clean the file up before import happens. That other program doesn't get a UI. It doesn't get a user sitting there to click through a dialog if something looks odd. It gets a file, a place to put the result, and exactly one chance to report back whether things went well, all of it expressed as a single integer.

This project is that other program. Nothing about it is complicated on its own, read a file one character at a time, decide what to do with each one, write the result somewhere else. What makes it worth building deliberately is the contract it has to honor, because OnBase is the one calling it, and OnBase doesn't negotiate.

## The Contract, Stated Plainly

Three rules, non-negotiable, because OnBase is the one enforcing them:

1. **No user interaction, ever.** Nobody's watching. A prompt waiting for a keypress is a prompt that waits forever.
2. **At least two command-line arguments.** An input file path and an output file path, `%I` and `%O` in OnBase's own configuration syntax. Anything less isn't a valid invocation.
3. **An integer exit code on the way out.** That single number is the entire vocabulary this program has for telling OnBase what happened. Choose the values deliberately, because "something went wrong" and "the wrong thing specifically went wrong" are not the same amount of information.

Everything else is negotiable. Those three aren't.

## How to Write This Program

### Step 1: Decide what actually needs cleaning, and give it names

Real text arriving from real sources tends to carry things OnBase would rather not see: control characters left over from whatever system generated the file, characters outside whatever encoding OnBase expects, the occasional em dash a word processor inserted without asking. Before writing a line of logic, give this program a vocabulary for what can go wrong on the way out, both while cleaning a file and while running at all:

```csharp
public enum CharacterSet
{
    Ansi = 127,
    Ascii = 255,
    Unicode = 65535
}
```

```csharp
public enum ExitStatus
{
    Success = 0,
    GeneralError = -1,
    MissingArgument = -2,
    InputFileNotFound = -3,
    OutputDirectoryNotFound = -4,
    ProcessingError = -5
}
```

Every distinct way this tool can fail gets its own code. Nobody's watching it run, so whoever reads the log afterward needs to be able to tell "the input file was missing" apart from "something broke while processing" without guessing.

### Step 2: Build the settings around a real config file, on purpose

It would be simpler to hardcode all of this. Don't. A real OnBase-invoked preprocessor gets configured exactly the way this one should be: through `App.config`, with a custom section defining the character set, the substitute character, whether form feeds become line breaks, and an explicit list of character replacements. A `ConfigurationSection` subclass gets you there:

```csharp
public class PreprocessorSettings : ConfigurationSection
{
    public const string SectionName = "preprocessorSettings";

    [ConfigurationProperty("debugMode", IsRequired = true)]
    public bool DebugMode
    {
        get => (bool)base["debugMode"];
        set => base["debugMode"] = value;
    }

    [ConfigurationProperty("interactive", IsRequired = true)]
    public bool Interactive
    {
        get => (bool)base["interactive"];
        set => base["interactive"] = value;
    }

    [ConfigurationProperty("charSet", IsRequired = true)]
    public CharacterSet CharSet
    {
        get => (CharacterSet)base["charSet"];
        set => base["charSet"] = value;
    }

    [ConfigurationProperty("invalidCharacterSubstitute", IsRequired = true)]
    public string InvalidCharacterSubstitute
    {
        get => (string)base["invalidCharacterSubstitute"];
        set => base["invalidCharacterSubstitute"] = value;
    }

    [ConfigurationProperty("replaceFormFeeds", IsRequired = true)]
    public bool ReplaceFormFeeds
    {
        get => (bool)base["replaceFormFeeds"];
        set => base["replaceFormFeeds"] = value;
    }

    [ConfigurationProperty("logFilePath", IsRequired = true)]
    public string LogFilePath
    {
        get => (string)base["logFilePath"];
        set => base["logFilePath"] = value;
    }

    [ConfigurationProperty("characterReplacements", IsRequired = true)]
    [ConfigurationCollection(typeof(CharacterElement), AddItemName = "character")]
    public CharacterCollection CharacterReplacements
    {
        get => (CharacterCollection)base["characterReplacements"];
        set => base["characterReplacements"] = value;
    }

    public override bool IsReadOnly() => false;
}
```

`LogFilePath` belongs in this same section rather than a separate one. Everything this program does happens with nobody watching, which means the log file is the only record anyone will ever have of a given run, so its destination is just as much a "setting" as the character set is.

The explicit replacement list needs its own small pair of classes, a `ConfigurationElement` for one replacement and a `ConfigurationElementCollection` to hold them:

```csharp
public class CharacterElement : ConfigurationElement
{
    [ConfigurationProperty("original", IsRequired = true)]
    public char Original
    {
        get => (char)base["original"];
        set => base["original"] = value;
    }

    [ConfigurationProperty("replacement", IsRequired = true)]
    public char Replacement
    {
        get => (char)base["replacement"];
        set => base["replacement"] = value;
    }

    public override bool IsReadOnly() => false;
}
```

```csharp
public class CharacterCollection : ConfigurationElementCollection
{
    protected override ConfigurationElement CreateNewElement() => new CharacterElement();

    protected override object GetElementKey(ConfigurationElement element)
        => ((CharacterElement)element).Original;

    // Explicit (object) cast is required here, not decorative: char converts implicitly to
    // int, which C# prefers over boxing to object during overload resolution. Without the
    // cast, this silently resolves to BaseGet(int index) instead of the intended
    // BaseGet(object key) - treating each character's numeric code point as a collection
    // index rather than a key, and throwing "index out of range" for any character whose
    // code point is >= the collection's actual size. With only a couple of replacement
    // entries configured, that's nearly every character this tool will ever process.
    public CharacterElement this[char original] => (CharacterElement)BaseGet((object)original);

    public override bool IsReadOnly() => false;
}
```

That comment on the indexer isn't decoration, it's the single easiest mistake to make in this whole project. Write `BaseGet(original)` without the cast and the code compiles cleanly, looks correct, and fails on almost every character you throw at it, because `ConfigurationElementCollection` has *two* `BaseGet` overloads, one by index and one by key, and the implicit `char`-to-`int` conversion wins the overload resolution unless you force it otherwise.

And the `App.config` this reads:

```xml
<configuration>
  <configSections>
    <section name="preprocessorSettings" type="OnBase.Preprocessor.Models.Configuration.PreprocessorSettings, OnBase.Preprocessor"/>
  </configSections>
  <preprocessorSettings debugMode="true"
                        interactive="true"
                        charSet="Ascii"
                        invalidCharacterSubstitute="?"
                        replaceFormFeeds="true"
                        logFilePath=".\logs\preprocessor.log">
    <characterReplacements>
      <character original="–" replacement="-"/>
      <character original="—" replacement="-"/>
    </characterReplacements>
  </preprocessorSettings>
</configuration>
```

### Step 3: Wire up logging before anything else can fail

Set up logging as the very first thing initialization does, before argument parsing, before file checks, before anything that could throw, because if initialization itself fails (and a missing or malformed config section will make sure it can), you still want a log entry describing what happened, even when the thing that broke is the logging configuration's own home:

```csharp
public static class LogWriter
{
    public static bool DebugMode { get; set; }
    public static bool Interactive { get; set; }

    private static Logger logger;

    public static void Initialize(string logFilePath)
    {
        logger = new LoggerConfiguration()
            .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    public static void Log(string message, bool isError = false, bool forceLog = false)
    {
        if (Interactive) Console.WriteLine(message);
        if (!isError)
        {
            if (DebugMode || forceLog) logger?.Debug(message);
            return;
        }
        logger?.Error(message);
    }

    public static void Log(this Exception ex)
    {
        while (ex != null)
        {
            string message = $"{ex.GetType().Name}: {ex.Message}" +
                (DebugMode ? $"\n\nStack Trace:\n{ex.StackTrace}" : "");
            Log(message, true);
            ex = ex.InnerException;
        }
    }
}
```

Two entry points cover everything the rest of the program needs: `Log()` for ordinary messages, and an `Exception` extension that walks the full inner-exception chain, so a wrapped exception doesn't hide the original cause from whoever reads the log later.

### Step 4: Write the character-cleaning loop itself

This is the part the whole project exists to do, and it's genuinely just a loop:

```csharp
public void CleanFile(string inputFile, string outputFile)
{
    using var reader = new StreamReader(inputFile);
    using var writer = new StreamWriter(outputFile);

    int i = reader.Read();
    while (i > -1)
    {
        string c = ((char)i).ToString();

        // Outside the configured character set, or a known control character
        if (i > (int)Settings.CharSet || Array.IndexOf(invalidCharacters, i) > -1)
            c = Settings.InvalidCharacterSubstitute;

        // An explicit replacement rule always wins over the generic substitution above -
        // this check runs second on purpose
        if (Settings.CharacterReplacements[(char)i] != null)
            c = Settings.CharacterReplacements[(char)i].Replacement.ToString();

        if (i == FormFeed && Settings.ReplaceFormFeeds)
            c = Environment.NewLine;

        if (!string.IsNullOrEmpty(c)) writer.Write(c);

        i = reader.Read();
    }
}
```

Read the ordering of those three checks carefully, because it's the entire design. The generic "outside the character set" substitution runs first and sets a default. The explicit replacement list runs *second*, specifically so a deliberate rule for a specific character always overrides the generic fallback, rather than the generic fallback replacing the character before your specific rule ever gets a look at it. Reverse that order and every explicit replacement becomes unreachable.

### Step 5: Validate the arguments, and be precise about what "invalid" means

```csharp
private static void Initialize(string[] args)
{
    Status = ExitStatus.Success;
    settings = (PreprocessorSettings)ConfigurationManager.GetSection(PreprocessorSettings.SectionName);
    LogWriter.DebugMode = settings.DebugMode;
    LogWriter.Interactive = settings.Interactive;
    LogWriter.Initialize(settings.LogFilePath);

    if (args.Length < 2)
    {
        Status = ExitStatus.MissingArgument;
        throw new ArgumentNullException(nameof(args), "Command line must include input and output file paths!");
    }

    inputFile = args[0];
    if (!File.Exists(inputFile))
    {
        Status = ExitStatus.InputFileNotFound;
        throw new FileNotFoundException($"Cannot find input file [{inputFile}]!");
    }

    outputFile = args[1];
    string outputDirectory = Path.GetDirectoryName(outputFile);

    // An empty outputDirectory just means "no directory given" - i.e. the current
    // directory, which always exists. Only a non-empty directory that doesn't exist is
    // actually an error. A bare output filename with no path at all ("cleaned.txt" rather
    // than "C:\somewhere\cleaned.txt") is a completely normal thing to want, and it should
    // never be treated the same as a genuinely missing directory.
    if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
    {
        Status = ExitStatus.OutputDirectoryNotFound;
        throw new DirectoryNotFoundException($"Cannot find output directory [{outputDirectory}]!");
    }

    processor = new FileCleaner(settings);
}
```

`Path.GetDirectoryName()` returning an empty string for a bare filename is the trap here. Treat that the same as a missing directory, and the single most ordinary way anyone would ever invoke this tool, writing the output right next to the input, becomes a hard failure.

### Step 6: Tie it together in `Main()`, and let it run unattended

```csharp
private static int Main(string[] args)
{
    try
    {
        Initialize(args);
        LogWriter.Log("Preprocessor started...", forceLog: true);
        processor.CleanFile(inputFile, outputFile);
    }
    catch (Exception ex)
    {
        if (Status == ExitStatus.Success) Status = ExitStatus.GeneralError;
        ex.Log();
    }
    finally
    {
        if (settings != null && settings.Interactive)
        {
            LogWriter.Log($"Preprocessor completed with exit status [{Status}]", forceLog: true);
        }
    }

    return (int)Status;
}
```

No prompts anywhere on this path, success or failure. The exit code is set before the method returns no matter which branch got taken, and the log carries a full record of what happened either way, which is the whole contract from the top of this document, now actually enforced in code.

## Try It Yourself

`sample-input.txt` ships with the project, three rows of CSV-ish data carrying an en dash, an em dash, and a curly apostrophe, none of them plain ASCII. `App.config` has explicit replacement rules for both dashes (they become plain hyphens); the curly apostrophe has no explicit rule, so it falls through to the generic out-of-range substitute, `?`.

Build the project, then run it from a command line with an input and a destination path:

```
OnBase.Preprocessor.exe sample-input.txt cleaned-output.txt
```

Open `cleaned-output.txt` afterward and compare it against the original: the dashes should read as plain hyphens, the curly apostrophes should have become `?`. Check `.\logs\preprocessor.log` for a full record of the run, exactly the kind of record you'd want if this were running unattended somewhere and something needed investigating later.

Try renaming or deleting `sample-input.txt` and running it again to see the missing-input-file exit code, or run it with no arguments at all to see the missing-argument path. Both are exactly what OnBase itself would encounter if either situation happened during a real import.

This project isn't wired into `LessonRunner`'s menu, and that's deliberate: every other lesson there is meant to be picked and simply run, but this one needs two real file paths to do anything meaningful, and a menu has no way to prompt for those. Running it by hand, from a command line, with explicit arguments, is a closer match to how a real OnBase-invoked preprocessor actually gets exercised anyway.
