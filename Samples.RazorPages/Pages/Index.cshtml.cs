#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Samples.RazorPages.Data;
using Samples.RazorPages.Models;

namespace Samples.RazorPages.Pages
{
    /// <summary>
    /// One page handles both the search form AND the results, a genuinely idiomatic Razor
    /// Pages pattern (the page's own state naturally varies based on whether ZipCode is
    /// bound), where Samples.MvcWebPortal(.Core) needed two separate controller+view pairs
    /// (HomeController for the form, LocationLookupController for the results) to do the
    /// same job. See LectureNotes.md.
    /// </summary>
    public class IndexModel(LocationLookupContext db) : PageModel
    {
        // [BindProperty(SupportsGet = true)] is what lets this bind directly from the query
        //   string on a plain GET request, the idiomatic Razor Pages way to accept page
        //   input without a separate action-method parameter list.
        [BindProperty(SupportsGet = true)]
        public string? ZipCode { get; set; }

        public IReadOnlyList<ZipCode> Locations { get; private set; } = [];

        public bool HasSearched { get; private set; }

        public async Task OnGetAsync()
        {
            if (string.IsNullOrWhiteSpace(ZipCode)) return;

            HasSearched = true;
            Locations = await db.ZipCodes.Where(z => z.ZipCode1 == ZipCode).ToListAsync();
        }
    }
}
