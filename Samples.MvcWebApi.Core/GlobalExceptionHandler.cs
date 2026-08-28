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

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Samples.MvcWebApi.Core
{
    /// <summary>
    /// The modern, built-in equivalent of Samples.MvcWebApi's [ExceptionFilter] +
    /// DatabankException/ErrorHandling.HandleException() combination. IExceptionHandler (added
    /// in .NET 8) is ASP.NET Core's own centralized exception-handling extension point, wired
    /// up once in Program.cs (app.UseExceptionHandler()) rather than applied per-controller via
    /// an attribute. See LectureNotes.md for the fuller "why this differs from the classic
    /// project" discussion, including why DatabankException itself couldn't be reused here at
    /// all (CSharp.SharedLibrary targets net48, which a net10.0 project cannot reference).
    /// </summary>
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

            // ProblemDetails (RFC 7807) is ASP.NET Core's built-in structured error response
            //   format, a genuinely modern replacement for the classic API's "always return
            //   HTTP 200, check the Errors array" pattern, real HTTP status codes here, a
            //   real, standard error body shape any HTTP client already knows how to parse.
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = exception.Message
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
