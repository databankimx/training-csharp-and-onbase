# Ch03 Textbook Code: Student Class

## What This Is

A small standalone lab: the simplest possible class, a bare `Student` with public fields and no methods, just to demonstrate creating instances, incrementing a shared static counter, and reading fields back.

No functional bugs. The field names (`firstName`, `lastName`, `grade`) are camelCase rather than the PascalCase used elsewhere in this training set, that's left as-is on purpose: `TextbookCode.*` projects preserve the original download's naming even where it doesn't match our usual standard, so you're seeing exactly what shipped, casing included.

---

## Worth Noticing

`StudentCount` is `static`, which means it belongs to the `Student` type itself, not to any individual student. `firstStudent.StudentCount++` and `secondStudent.StudentCount++` would both be incrementing the exact same shared counter, there's only ever one `StudentCount` no matter how many `Student` instances exist. That's why the lab calls it as `Student.StudentCount++` instead of through an instance, the syntax is a hint about the underlying behavior: static members are accessed through the type name, not an object reference, precisely because there's nothing instance-specific about them.
