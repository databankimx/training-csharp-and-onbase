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

namespace LessonRunner.Models
{
    /// <summary>
    /// A single runnable lesson: a display name for the menu, and the folder/project
    /// name it lives in. The project name is used to locate both the folder and the
    /// .csproj inside it, since every project in this solution names its folder,
    /// .csproj, and AssemblyName identically.
    /// </summary>
    public class Lesson
    {
        #region Properties
        /// <summary>
        /// Text shown in the lesson menu
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// Folder name and .csproj base name, e.g. "CSharp.Ch01.HelloWorld"
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        /// True if this project uses a &lt;COMReference&gt; (tlbimp-based COM interop, e.g. Excel).
        /// The "dotnet" SDK CLI's bundled MSBuild cannot process the ResolveComReference task
        /// (MSB4803), only the full .NET Framework MSBuild that ships with Visual Studio can, so
        /// lessons flagged true here get built and launched differently than "dotnet run".
        /// </summary>
        public bool RequiresFullFrameworkMsBuild { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Lesson class
        /// </summary>
        /// <param name="displayName">Text shown in the lesson menu</param>
        /// <param name="projectName">Folder name and .csproj base name</param>
        /// <param name="requiresFullFrameworkMsBuild">True if this project uses a &lt;COMReference&gt;</param>
        public Lesson(string displayName, string projectName, bool requiresFullFrameworkMsBuild = false)
        {
            DisplayName = displayName;
            ProjectName = projectName;
            RequiresFullFrameworkMsBuild = requiresFullFrameworkMsBuild;
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
