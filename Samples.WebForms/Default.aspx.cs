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

#region Training Notes
/*
 * ASP.NET Web Forms is the OLDEST server-side UI technology demonstrated in this training
 *   set (2002, part of .NET's very first release), and the execution model is genuinely
 *   different from everything else here:
 *
 * - POSTBACK: every button click submits the ENTIRE page back to itself (the same URL), the
 *   framework then re-runs the page lifecycle (Page_Load, control events) to figure out what
 *   happened and re-render. There's no separate "action method" or "handler" the way MVC/
 *   Razor Pages have, the whole page IS the handler.
 *
 * - VIEWSTATE: server controls (asp:TextBox, asp:GridView, etc.) automatically persist their
 *   own state across postbacks via a hidden, encoded __VIEWSTATE field, WITHOUT any code
 *   written to make that happen. Notice txtZipCode.Text below is read directly in
 *   btnSearch_Click with no re-binding step, ASP.NET already restored it from ViewState
 *   before this method even runs.
 *
 * - SINGLE SERVER FORM: an entire Web Forms page can have only ONE <form runat="server">
 *   element (see Site.Master), every postback-capable control anywhere on the page shares it.
 */
#endregion

using System;
using System.Linq;
using CSharp.SharedLibrary.Models;
using Samples.WebForms.Models;

namespace Samples.WebForms
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // IsPostBack is how a Web Forms page tells "first load" apart from "postback",
            //   there is no separate route/action for those two cases the way MVC/Razor Pages
            //   would have, it's all the same Page_Load.
        }

#pragma warning disable IDE1006 // Naming Styles
        protected void btnSearch_Click(object sender, EventArgs e)
#pragma warning restore IDE1006 // Naming Styles
        {
            try
            {
                lblError.Visible = false;
                pnlResults.Visible = false;

                // txtZipCode.Text already reflects whatever the user typed, restored from
                //   ViewState automatically, no model binding step needed here at all.
                string zipCode = txtZipCode.Text?.Trim();
                if (string.IsNullOrEmpty(zipCode)) return;

                using (var db = new ExternalDataEntities())
                {
                    var results = db.ZipCodes.Where(z => z.ZipCode1 == zipCode).ToList();

                    litZipCode.Text = System.Web.HttpUtility.HtmlEncode(zipCode);
                    gridResults.DataSource = results;
                    gridResults.DataBind();
                    pnlResults.Visible = true;
                }
            }
            catch (Exception ex)
            {
                var wrapped = new DatabankException("Error looking up locations!", ex);
#pragma warning disable S6667
                Global.Logger?.Error(wrapped, "Error in btnSearch_Click");
#pragma warning restore S6667
                lblError.Text = wrapped.Message;
                lblError.Visible = true;
            }
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
