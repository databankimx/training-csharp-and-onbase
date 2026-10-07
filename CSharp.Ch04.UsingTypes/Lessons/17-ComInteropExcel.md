---
title: "COM Interop - Driving Excel"
chapter: 4
index: 17
dependencies: []
launchMode: external
notes: "Requires Microsoft Excel to be installed. Launches a visible Excel process and drives it live."
---

```csharp
using System;
using Excel = Microsoft.Office.Interop.Excel;

internal static class Program
{
    private static void Main()
    {
        Excel._Application excelApp = new Excel.Application();
        Excel.Workbook workbook = excelApp.Workbooks.Add();
        dynamic sheet = workbook.Worksheets[1];

        excelApp.Visible = true;

        sheet.Cells[1, 1].Value = "Value";
        sheet.Cells[1, 2].Value = "Value Squared";

        for (int i = 1; i <= 10; i++)
        {
            sheet.Cells[i + 1, 1].Value = i;
            sheet.Cells[i + 1, 2].Value = (i * i).ToString();
        }

        sheet.Columns[1].AutoFit();
        sheet.Columns[2].AutoFit();

        Console.WriteLine("Excel workbook populated. Close Excel to continue.");
    }
}
```
