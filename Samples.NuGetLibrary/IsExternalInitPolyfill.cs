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

#if NETFRAMEWORK
namespace System.Runtime.CompilerServices
{
    // *Migration Note: record types (Location.cs) and init-only properties are lowered by
    // the compiler using this marker type, System.Runtime.CompilerServices.IsExternalInit,
    // which shipped in the BCL starting with .NET 5. net48's own BCL has no such type at
    // all, the compiler doesn't care WHERE the type comes from though, only that a type
    // with this exact name and namespace exists somewhere visible to the compilation, so
    // defining an empty one here satisfies it. #if NETFRAMEWORK (a symbol the SDK defines
    // automatically for any .NET Framework target, net48 included, and NOT for net10.0)
    // is what keeps this from colliding with the real type net10.0 already has natively,
    // this file compiles into the net48 build of this project only.
    internal static class IsExternalInit
    {
    }
}
#endif

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
