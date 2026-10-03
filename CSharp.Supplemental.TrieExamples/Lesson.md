# Supplemental: Trie Examples

## What This Is

A trie (pronounced "try," from the middle syllable of "retrieval") is a tree where each node represents a single character, and a path from the root to a marked node spells out a word. The structure is purpose-built for prefix operations: finding all words that start with a given sequence, checking whether a prefix exists, or confirming an exact word is in the set - all in time proportional to the length of the word being looked up, regardless of how many words are in the dictionary.

This project loads a real word file into a trie and runs an interactive console lookup against it. `CSharp.Supplemental.DataStructureFundamentals` covers arrays, linked lists, and binary search trees if you want the tree-structure background before diving in here.

---

## The Structure

### TrieNode

```csharp
internal class TrieNode
{
    internal TrieNode[] Children { get; set; }
    internal uint WordCount { get; set; } = 0;

    internal TrieNode(int size = 26) // 26 slots, one per letter
    {
        Children = new TrieNode[size];
    }
}
```

Each node holds an array of 26 child slots (one per letter of the alphabet) and a `WordCount`. `WordCount > 0` at a node means a complete word ends at that position in the tree. Children that haven't been used yet are `null`.

### Trie

The `Trie` class holds the root node and provides four operations: `InsertKey`, `PrefixExists`, `Search`, and `Delete`.

**Inserting a word:**

```csharp
internal bool InsertKey(string key)
{
    var current = Root;
    foreach (char c in key.ToLower())
    {
        int loc = c - 'a'; // 'a' maps to 0, 'b' to 1, etc.
        if (current.Children[loc] == null)
            current.Children[loc] = new TrieNode(Size);
        current = current.Children[loc];
    }
    current.WordCount++;
    return true;
}
```

Each character maps to an index by subtracting `'a'`. If the child node at that index doesn't exist yet, it's created. After the last character, `WordCount` is incremented to mark that a complete word ends here.

**Searching for a word:**

```csharp
internal bool Search(string key)
{
    var current = Root;
    foreach (char c in key.ToLower())
    {
        int loc = c - 'a';
        if (current.Children[loc] == null) return false;
        current = current.Children[loc];
    }
    return current.WordCount > 0;
}
```

Follows the path for each character. If any child is missing, the word isn't in the trie. After the last character, checks `WordCount > 0` - a node existing at that path means a prefix was inserted, but only `WordCount > 0` means a complete word was explicitly added.

**The distinction between search and prefix:** `PrefixExists` returns `true` for any string that's a prefix of an inserted word, even if it's not a word itself. `Search` requires `WordCount > 0` at the end node. The word `"pre"` being in the dictionary doesn't mean `"pr"` is - `PrefixExists("pr")` would return `true`, `Search("pr")` would return `false`.

---

## Running the Program

The program loads `data/words.txt` (a plain text file with one word per line) into the trie, then prompts for input:

```
Enter a word to search (letters only) or press <ENTER> to quit:
> hello
'hello' is in the dictionary...
```

The lookup time is proportional to the length of the word you typed, regardless of how many words are in the dictionary. That's the trie's defining characteristic.

---

## Performance Characteristics

| Operation | Time | Why |
|---|---|---|
| Insert | O(m) | Walk one node per character |
| Search (exact) | O(m) | Walk one node per character, check WordCount |
| Prefix exists | O(m) | Walk one node per character, check node exists |
| Delete | O(m) | Walk to the node, decrement WordCount |

m is the length of the word being operated on. Compare this to a `HashSet<string>`, which has O(m) insert and O(m) lookup (computing the hash requires reading all m characters). The trie doesn't have a meaningful asymptotic advantage over a hash set for exact-word lookups - the difference is the prefix operation. A hash set cannot tell you whether any word in the set starts with "pre" without scanning every entry. A trie answers that in O(m) - follow the path for "pre" and check whether any child node exists.

**Memory tradeoff.** Each node allocates an array of 26 slots regardless of how many are actually used. A trie holding a sparse vocabulary wastes significant memory on null slots. A compressed variant (a "Patricia trie" or "radix trie") merges chains of single-child nodes into a single edge, dramatically reducing memory at the cost of more complex implementation.

---

## Takeaways

- A trie maps paths from root to node to strings, one character per level.
- `WordCount > 0` at a node means a complete word ends there. A node existing without `WordCount > 0` means only a prefix was inserted.
- Insert, search, and prefix-check are all O(m) where m is the string length - independent of dictionary size.
- Tries are the right structure specifically when prefix operations matter. For pure exact-match lookup, a `HashSet<string>` is simpler and uses far less memory.
- The 26-child array wastes memory on sparse vocabularies. Compressed tries address this at the cost of implementation complexity.
