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

// Records use 'init' setters, which require IsExternalInit. This type is built
// into .NET 5+ but is absent from .NET Framework 4.x. Declaring it here as an
// internal stub satisfies the compiler without any runtime cost.
namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
