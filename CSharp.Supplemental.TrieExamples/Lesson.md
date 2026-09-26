# Trie Examples

A [Trie](https://en.wikipedia.org/wiki/Trie) (pronounced "try," from re**trie**val) is a tree built specifically for fast prefix-based lookups over a set of strings - a dictionary word list, for example. Each node represents one character position, and its children represent the possible next characters. New to trees generally? `CSharp.Supplemental.DataStructureFundamentals` covers arrays, linked lists, and trees at a foundational level first - worth a look before this one if the branching-node structure below feels unfamiliar.

## How It's Built Here

`TrieNode` holds a fixed-size array of child references - 26 by default, one slot per letter of the alphabet - plus a `WordCount`, which is how a node marks "a real word ends here" (as opposed to just being partway through a longer word). `Trie` wraps a root node and exposes the actual operations:

- **`InsertKey`** walks the trie one character at a time, creating child nodes as needed, and increments `WordCount` on the final node.
- **`Search`** walks the same path and checks whether the final node's `WordCount` is greater than zero - present, but only as a real inserted word, not just as a prefix of something else.
- **`PrefixExists`** is the same walk, but doesn't check `WordCount` at all - it only cares whether the path exists, so it also matches prefixes that were never inserted as complete words themselves.
- **`Delete`** is the most involved of the four - it has to find the deepest node it can safely remove without breaking any *other* word that shares that same prefix, which is why it tracks the last branching point (a node with more than one child) along the way.

## Try It Yourself

`Program.cs` loads `data/words.txt` (a large real word list) into a trie at startup, then prompts for words to search one at a time, reporting whether each one is in the dictionary. Try a real word, a prefix of a real word that isn't itself a complete word (`PrefixExists` would say yes, `Search` says no), and a string that isn't a word at all.
