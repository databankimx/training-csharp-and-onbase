---
title: "A Real-World Class Hierarchy - Contact"
chapter: 5
index: 1
dependencies: []
---

```csharp
using System;
using System.IO;
using System.Text.RegularExpressions;

// ---------------------------------------------------------------------------
// Address hierarchy
// ---------------------------------------------------------------------------

public class Address
{
    public string StreetAddress { get; set; }
    public string City          { get; set; }
    public string State         { get; set; }
    public string ZipCode       { get; set; }
}

public class BusinessAddress : Address
{
    public string CompanyName { get; set; }
}

// ---------------------------------------------------------------------------
// Telephone with encapsulated formatting
// ---------------------------------------------------------------------------

public class Telephone
{
    private string _number;
    private static readonly Regex NonDigits = new Regex(@"\D");

    public string Number
    {
        get => FormatPhoneNumber(_number);
        set => _number = SetPhoneNumber(value);
    }

    private static string FormatPhoneNumber(string phoneNumber)
    {
        if (NonDigits.Match(phoneNumber).Success)
            throw new InvalidDataException($"Phone number {phoneNumber} contains non-digits!");
        if (phoneNumber.Length != 10)
            throw new InvalidDataException($"Phone number {phoneNumber} does not contain ten digits!");
        return $"({phoneNumber.Substring(0, 3)}) {phoneNumber.Substring(3, 3)}-{phoneNumber.Substring(6, 4)}";
    }

    private static string SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            throw new InvalidDataException("Phone number cannot be blank!");
        string temp = NonDigits.Replace(phoneNumber, "");
        if (temp.Length != 10)
            throw new InvalidDataException($"Phone number {phoneNumber} does not contain ten digits!");
        return temp;
    }
}

// ---------------------------------------------------------------------------
// Person base class with constructor chaining
// ---------------------------------------------------------------------------

public class Person
{
    public string FirstName  { get; set; }
    public string LastName   { get; set; }
    public string MiddleName { get; set; }

    public Person() { }

    public Person(string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(firstName))
            throw new ArgumentOutOfRangeException(nameof(firstName), "First name must not be null or blank!");
        if (string.IsNullOrEmpty(lastName))
            throw new ArgumentOutOfRangeException(nameof(lastName), "Last name must not be null or blank!");
        FirstName = firstName;
        LastName  = lastName;
    }

    public Person(string firstName, string middleName, string lastName) : this(firstName, lastName)
    {
        if (string.IsNullOrEmpty(middleName))
            throw new ArgumentOutOfRangeException(nameof(middleName), "Middle name must not be null or blank!");
        MiddleName = middleName;
    }

    public string FullName(bool reverse = false, bool includeMiddle = false)
    {
        if (string.IsNullOrEmpty(LastName))  throw new InvalidDataException("Last name is null or blank!");
        if (string.IsNullOrEmpty(FirstName)) throw new InvalidDataException("First name is null or blank!");
        if (includeMiddle && string.IsNullOrEmpty(MiddleName))
            throw new InvalidDataException("Middle name is null or blank!");

        string front = includeMiddle ? $"{FirstName} {MiddleName}" : FirstName;
        return reverse ? $"{LastName}, {front}" : $"{front} {LastName}";
    }

    // Extension method inlined: produces "FML" initials from First/Middle/Last.
    public string Initials()
    {
        if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
            throw new InvalidDataException("Unable to produce initials -- one or more required names are blank!");
        string mid = string.IsNullOrEmpty(MiddleName) ? "" : MiddleName.Substring(0, 1);
        return $"{FirstName.Substring(0, 1)}{mid}{LastName.Substring(0, 1)}".ToUpper();
    }
}

// ---------------------------------------------------------------------------
// Contact: Person + contact information
// ---------------------------------------------------------------------------

public class Contact : Person
{
    public Telephone      HomePhone    { get; set; }
    public Telephone      WorkPhone    { get; set; }
    public Telephone      MobilePhone  { get; set; }
    public string         Email        { get; set; }
    public Address        HomeAddress  { get; set; }
    public BusinessAddress WorkAddress { get; set; }
}

// ---------------------------------------------------------------------------
// Program
// ---------------------------------------------------------------------------

internal static class Program
{
    private static void Main()
    {
        var someone = new Contact
        {
            FirstName   = "Jordan",
            MiddleName  = "A",
            LastName    = "Rivera",
            Email       = "jrivera@databankimx.com",
            HomePhone   = new Telephone { Number = "2145550234" },
            WorkPhone   = new Telephone { Number = "2145550199" },
            MobilePhone = new Telephone { Number = "2145550172" },
            HomeAddress = new Address
            {
                StreetAddress = "123 Main St",
                City          = "Lewisville",
                State         = "TX",
                ZipCode       = "75067"
            },
            WorkAddress = new BusinessAddress
            {
                CompanyName   = "DataBank IMX",
                StreetAddress = "456 Corporate Dr",
                City          = "Lewisville",
                State         = "TX",
                ZipCode       = "75067"
            }
        };

        Console.WriteLine("Name");
        Console.WriteLine("----");
        Console.WriteLine($"FullName():                      {someone.FullName()}");
        Console.WriteLine($"FullName(reverse: true):         {someone.FullName(reverse: true)}");
        Console.WriteLine($"FullName(includeMiddle: true):   {someone.FullName(includeMiddle: true)}");
        Console.WriteLine($"Initials:                        {someone.Initials()}");
        Console.WriteLine();

        Console.WriteLine("Contact Information");
        Console.WriteLine("-------------------");
        Console.WriteLine($"Email:        {someone.Email}");
        Console.WriteLine($"Home Phone:   {someone.HomePhone.Number}");
        Console.WriteLine($"Work Phone:   {someone.WorkPhone.Number}");
        Console.WriteLine($"Mobile Phone: {someone.MobilePhone.Number}");
        Console.WriteLine();

        Console.WriteLine("Home Address");
        Console.WriteLine("------------");
        Console.WriteLine(someone.HomeAddress.StreetAddress);
        Console.WriteLine($"{someone.HomeAddress.City}, {someone.HomeAddress.State} {someone.HomeAddress.ZipCode}");
        Console.WriteLine();

        Console.WriteLine("Work Address");
        Console.WriteLine("------------");
        Console.WriteLine(someone.WorkAddress.CompanyName);
        Console.WriteLine(someone.WorkAddress.StreetAddress);
        Console.WriteLine($"{someone.WorkAddress.City}, {someone.WorkAddress.State} {someone.WorkAddress.ZipCode}");
    }
}
```
