<%@ Page Title="" Language="C#" MasterPageFile="~/data.master" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="sselData.Index" %>

<%@ Import Namespace="LNF.Data" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script>
        function logout(){
            var logoutUrl = '<%=GetLogoutUrl()%>';
            window.top.location = logoutUrl;
            return false;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="section">
        <div class="PageHeader">
            LNF Data Entry
        </div>
        <div class="LabelText" style="margin-top: 25px;">
            <strong>Organization:</strong>
            <asp:DropDownList ID="ddlOrg" runat="server" Height="24" CssClass="DDLText" AutoPostBack="True" OnSelectedIndexChanged="DdlOrg_SelectedIndexChanged">
            </asp:DropDownList>
        </div>
    </div>
    <div class="section">
        <table class="button-table">
            <tr>
                <td style="text-align: center;">
                    <span class="ButtonGroupHeader">Settings</span>
                </td>
                <td style="text-align: center;">
                    <span class="ButtonGroupHeader">Misc</span>
                </td>
                <td style="text-align: center;">
                    <span class="ButtonGroupHeader">Global Settings</span>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnAddModAccount" CssClass="CommandButton" Text="Add/Modify Account" OnCommand="Button_Command" CommandName="navigate" CommandArgument="Account.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnPassword" CssClass="CommandButton" Text="Reset Password" OnCommand="Button_Command" CommandName="navigate" CommandArgument="/data/admin/password-reset?app=sseldata" ToolTip="Reset user passwords to the UserName." />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnGlobalFundingSource" CssClass="CommandButton" Text="Funding Sources" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="FundingSource" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnAddModClient" CssClass="CommandButton" Text="Add/Modify Client" OnCommand="Button_Command" CommandName="navigate" CommandArgument="Client.aspx" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnAddModNews" CssClass="CommandButton" Text="Add/Modify News Items" OnCommand="Button_Command" CommandName="navigate" CommandArgument="//ssel-sched.eecs.umich.edu/news/" />
                </td>
                <td>
                    <asp:Button runat="server" ID="btnGlobalTechnicalField" CssClass="CommandButton" Text="Technical Fields" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="TechnicalField" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnAddModOrg" CssClass="CommandButton" Text="Add/Modify Organization" OnCommand="Button_Command" CommandName="navigate" CommandArgument="Organization.aspx" />
                </td>
                <td>&nbsp;</td>
                <td>
                    <asp:Button runat="server" ID="btnGlobalSpecialTopic" CssClass="CommandButton" Text="Special Topics" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="SpecialTopic" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnClientAccountAssign" CssClass="CommandButton" Text="Assign Clients to Accounts" OnCommand="Button_Command" CommandName="navigate-client-acct-assign" CommandArgument="/data/dispatch/assign-accounts?returnTo=/sseldata&OrgID={OrgID}" ToolTip="This page displays a matrix that allows assigning all of a manager's Clients to Any/all of his/her Accounts, including the setting of the Client's Primary Account" />
                </td>
                <td>&nbsp;</td>
                <td>
                    <asp:Button runat="server" ID="btnOrgType" CssClass="CommandButton" Text="Organization Types" OnCommand="Button_Command" CommandName="navigate" CommandArgument="OrgType.aspx" />
                </td>
            </tr>
            <tr>
                <td>&nbsp;</td>
                <td>&nbsp;</td>
                <td>
                    <asp:Button runat="server" ID="btnChargeType" CssClass="CommandButton" Text="Charge Type Categories" OnCommand="Button_Command" CommandName="navigate" CommandArgument="ChargeType.aspx" />
                </td>
            </tr>
            <tr>
                <td style="text-align: center">
                    <span class="ButtonGroupHeader">Application Control</span>
                </td>
                <td>&nbsp;</td>
                <td>
                    <asp:Button runat="server" ID="btnGlobalRole" CssClass="CommandButton" Text="Client Roles" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="Role" />
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Button runat="server" ID="btnLogout" CssClass="CommandButton" Text="Exit Application" OnClientClick="return logout();" />
                </td>
                <td>&nbsp;</td>
                <td>
                    <asp:Button ID="btnGlobalCommunity" runat="server" CssClass="CommandButton" Text="Communities" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="Community" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>