---
title: "A Fuller Struct With Validation"
chapter: 3
index: 5
dependencies: []
---

```csharp
using System;

public struct Book
{
    public string Title;
    public string Category;
    public string Author;
    public int NumPages;
    public int CurrentPage;
    public double ISBN;
    public string CoverStyle;

    public Book(string title, string category, string author, int numPages, int currentPage, double isbn, string coverStyle)
    {
        Title = title;
        Category = category;
        Author = author;
        NumPages = numPages;
        CurrentPage = currentPage;
        if (CurrentPage < 1) CurrentPage = 1;
        if (CurrentPage > NumPages) CurrentPage = NumPages;
        ISBN = isbn;
        CoverStyle = coverStyle;
    }

    public void NextPage()
    {
        if (CurrentPage < NumPages)
        {
            CurrentPage++;
            Console.WriteLine("Current page is now " + CurrentPage);
        }
        else
        {
            Console.WriteLine("At end of book!");
        }
    }

    public void PrevPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            Console.WriteLine("Current page is now " + CurrentPage);
        }
        else
        {
            Console.WriteLine("At beginning of book!");
        }
    }
}

internal static class Program
{
    private static void Main()
    {
        var myBook = new Book(
            "MCSD Certification Toolkit (Exam 70-483)",
            "Certification",
            "Covaci, Tiberiu",
            648, 1, 81118612095, "Softcover");

        myBook.NextPage();
        myBook.PrevPage();
    }
}
```
