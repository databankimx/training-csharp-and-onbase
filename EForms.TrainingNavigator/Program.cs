#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
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

#region Using Directives
using Microsoft.EntityFrameworkCore;
using Serilog;
using EForms.TrainingNavigator.Data;
using EForms.TrainingNavigator.Models;
#endregion

#region Main
// DataBank standard: Serilog for logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Register the Entity Framework Core DbContext with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

var app = builder.Build();

// Helper function to create a DbContext with optional custom connection string
ApplicationDbContext CreateDbContext(string? customConnectionString = null)
{
    var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>();
    
    if (!string.IsNullOrWhiteSpace(customConnectionString))
    {
        contextOptions.UseSqlServer(customConnectionString);
    }
    else
    {
        contextOptions.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
    
    return new ApplicationDbContext(contextOptions.Options);
}

// Initialize the database on startup - creates it if it doesn't exist and applies migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseSerilogRequestLogging();
app.UseDefaultFiles();
app.UseStaticFiles();

// Location lookup endpoint - queries the ZipCodes database table
app.MapPost("/api/location/lookup", async (LocationLookupRequest request, ApplicationDbContext db, ILogger<Program> logger, HttpContext context) =>
{
    var response = new LocationLookupResponse { RequestId = request.RequestId };

    try
    {
        // Check if a custom connection string was provided in the request header
        ApplicationDbContext queryDb = db;
        string? customConnectionString = null;
        
        if (context.Request.Headers.TryGetValue("X-Connection-String", out var headerValue))
        {
            customConnectionString = headerValue.ToString();
            queryDb = CreateDbContext(customConnectionString);
            logger.LogInformation("Using custom connection string from request header");
        }

        // Validate the zip code
        if (string.IsNullOrWhiteSpace(request.ZipCode))
        {
            response.Errors.Add("ZipCode is required");
            return Results.BadRequest(response);
        }

        // Remove dashes and validate length
        var zipCode = request.ZipCode.Replace("-", "").Trim();
        if (zipCode.Length < 5 || zipCode.Length > 9 || !int.TryParse(zipCode, out _))
        {
            response.Errors.Add($"Value [{request.ZipCode}] is not a valid Zip code!");
            return Results.BadRequest(response);
        }

        // Use only the first 5 digits
        zipCode = zipCode[..5];

        // Query the database using Entity Framework Core
        var locations = await queryDb.ZipCodes
            .Where(z => z.Zip == zipCode)
            .ToListAsync();

        foreach (var location in locations)
        {
            response.Data.Add(new Location
            {
                State = location.State,
                County = location.County,
                City = location.City,
                ZipCode = location.Zip
            });
        }

        if (response.Data.Count == 0)
        {
            response.Errors.Add($"No location found for zip code {zipCode}");
        }

        // Dispose the custom context if one was created
        if (queryDb != db)
        {
            await queryDb.DisposeAsync();
        }

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error looking up location for zip code {ZipCode}", request.ZipCode);
        response.Errors.Add($"Error: {ex.Message}");
        return Results.Json(response, statusCode: StatusCodes.Status500InternalServerError);
    }
});

app.MapGet("/api/download/externaldata-backup", (IWebHostEnvironment env, HttpContext context) =>
{
    var backupPath = Path.Combine(env.ContentRootPath, "Resources", "ExternalData.bak");
    
    if (!File.Exists(backupPath))
    {
        return Results.NotFound("Backup file not found");
    }
    
    var fileStream = File.OpenRead(backupPath);
    var fileInfo = new FileInfo(backupPath);
    
    context.Response.ContentType = "application/octet-stream";
    context.Response.ContentLength = fileInfo.Length;
    context.Response.Headers.ContentDisposition = "attachment; filename=ExternalData.bak";
    
    return Results.Stream(fileStream, "application/octet-stream");
});

await app.RunAsync();
#endregion

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
