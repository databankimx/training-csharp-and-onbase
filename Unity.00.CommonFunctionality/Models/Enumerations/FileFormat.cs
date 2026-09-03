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

namespace Unity._00.CommonFunctionality.Models.Enumerations
{
    /// <summary>
    /// Maps to the file format types supported by the Unity system.
    /// These values are used to identify and handle different file formats within the application.
    /// </summary>
    public enum FileFormat
    {
        /// <summary>
        /// Unknown or undefined file format.
        /// </summary>
        Undefined = -1,

        /// <summary>
        /// Plain text file format.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Image file format (e.g., JPEG, PNG, BMP, GIF, TIFF).
        /// </summary>
        Image = 2,

        /// <summary>
        /// Printer Command Language file format.
        /// </summary>
        Pcl = 10,

        /// <summary>
        /// MS Word document file format. (e.g., .doc, .docx)
        /// </summary>
        Word = 12,

        /// <summary>
        /// MS Excel spreadsheet file format. (e.g., .xls, .xlsx)
        /// </summary>
        Excel = 13,

        /// <summary>
        /// MS PowerPoint presentation file format. (e.g., .ppt, .pptx)
        /// </summary>
        PowerPoint = 14,

        /// <summary>
        /// Rich Text Format file format. (e.g., .rtf)
        /// </summary>
        RichText = 15,

        /// <summary>
        /// PDF file format. (e.g., .pdf)
        /// </summary>
        Pdf = 16,

        /// <summary>
        /// HTML file format. (e.g., .html, .htm)
        /// </summary>
        Html = 17,

        /// <summary>
        /// AVI video file format. (e.g., .avi)
        /// </summary>
        Avi = 18,

        /// <summary>
        /// QuickTime video file format. (e.g., .mov)
        /// </summary>
        QuickTime = 19,

        /// <summary>
        /// WAV audio file format. (e.g., .wav)
        /// </summary>
        Wav = 20,

        /// <summary>
        /// EForm file format.
        /// </summary>
        EForm = 24,

        /// <summary>
        /// Virtual Form file format.
        /// </summary>
        VirtualForm = 27,

        /// <summary>
        /// XML format. (e.g., .xml)
        /// </summary>
        Xml = 32,

        /// <summary>
        /// MS Outlook file format. (e.g., .msg)
        /// </summary>
        Outlook = 35,

        /// <summary>
        /// Lotus Notes file format. (e.g., .nsf)
        /// </summary>
        LotusNotes = 40,

        /// <summary>
        /// Unicode HTML data format. (e.g., .html, .htm)
        /// </summary>
        UnicodeHtml = 43,

        /// <summary>
        /// HL7 file format. (e.g., .hl7)
        /// </summary>
        Hl7 = 48,

        /// <summary>
        /// Meditech file format. (e.g., .meditech)
        /// </summary>
        Meditech = 54,

        /// <summary>
        /// Unity Form file format.
        /// </summary>
        UnityForm = 57,

        /// <summary>
        /// Image Form file format.
        /// </summary>
        ImageForm = 58,

        /// <summary>
        /// Image-Only PDF file format.
        /// </summary>
        ImagePdf = 59,

        /// <summary>
        /// Email file format. (e.g., .eml)
        /// </summary>
        Email = 63,

        /// <summary>
        /// AutoCAD file format. (e.g., .dwg, .dxf)
        /// </summary>
        Cad = 69,

        /// <summary>
        /// Zip file format. (e.g., .zip)
        /// </summary>
        Zip = 70,

        /// <summary>
        /// OnBase Export file format. (e.g., .obx)
        /// </summary>
        OnBaseExport = 71
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
