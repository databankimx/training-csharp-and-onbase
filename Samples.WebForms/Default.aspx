<%@ Page Title="Location Lookup" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Samples.WebForms.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!--
    *Migration Note: no legacy source to port here, this is a fresh sample, built to
    demonstrate the genuinely different Web Forms execution model directly: postback, server
    controls, and automatic ViewState (txtZipCode's value survives the postback below with NO
    code written anywhere to make that happen, contrast against Samples.RazorPages'
    [BindProperty(SupportsGet = true)] or Samples.MvcWebPortal.Core's query-string binding,
    both of which are explicit). See LectureNotes.md.
    -->
    <div class="p-5 mb-4 bg-light rounded-3">
        <h1>Location Lookup</h1>
        <p class="lead">Enter a Zip Code to Search:</p>
        <p>
            <asp:TextBox ID="txtZipCode" runat="server" Text="75067" MaxLength="5" />
        </p>
        <p>
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary btn-lg" OnClick="btnSearch_Click" />
        </p>
        <asp:Label ID="lblError" runat="server" CssClass="text-danger" Visible="false" />
    </div>

    <asp:Panel ID="pnlResults" runat="server" Visible="false">
        <h2>Location Lookup - <asp:Literal ID="litZipCode" runat="server" /></h2>
        <asp:GridView ID="gridResults" runat="server" CssClass="table" AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="State" HeaderText="State" />
                <asp:BoundField DataField="County" HeaderText="County" />
                <asp:BoundField DataField="City" HeaderText="City" />
                <asp:BoundField DataField="ZipCode1" HeaderText="Zip Code" />
            </Columns>
        </asp:GridView>
    </asp:Panel>

</asp:Content>
