# Trie Examples

## What This Is

Ported from the loose `training-trie-examples` repo - already a clean, complete C# implementation (Insert/Search/PrefixExists/Delete), no functional changes needed.

## What Changed From the Original

- `internal class Program` → `internal static class Program` - the class has no instance members, matching the console-app convention used throughout this solution.
- Namespace changed from the original's bare `Trie` to `CSharp.Supplemental.TrieExamples`, matching the project's own name - consistent with how the other newly-ported Supplementary lessons are namespaced.
- `data/words.txt` (4.2 MB) wasn't copied through directly - text-editing tools aren't a safe way to move a file that size without risking silent corruption. Copy it manually:
  ```
  Copy-Item "C:\Development\training-trie-examples\csharp\data\words.txt" "C:\Development\training\developer-training\CSharp.Supplemental.TrieExamples\data\words.txt"
  ```
  (create the `data` folder first if it doesn't already exist)
- Added a cross-reference to the new `CSharp.Supplemental.DataStructureFundamentals` lesson, for anyone who wants the tree-structure background before diving into a Trie specifically.

## Not Yet Verified

Can't actually run this myself to confirm the word list loads and searches behave as expected - that's worth a quick check once `words.txt` is in place.
