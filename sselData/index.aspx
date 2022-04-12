<%@ Page Title="" Language="C#" MasterPageFile="~/data.master" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="sselData.Index" %>

<%@ Import Namespace="LNF.Data" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .container {
            display: flex;
            margin-bottom: 10px;
            margin-left: 5px;
        }

            .container > .col {
                margin-right: 10px;
                padding: 10px;
            }

            .container .CommandButton {
                display: block;
                margin-top: 15px;
            }

        .user-info {
            margin: 10px 0 20px 0;
        }
    </style>

    <script>
        function logout() {
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
        <div class="container">
            <div runat="server" id="divSettings" visible="true" class="col">
                <div class="ButtonGroupHeader">
                    <asp:Label runat="server">Settings</asp:Label>
                </div>
                <asp:Button runat="server" ID="btnAddModAccount" CssClass="CommandButton" Text="Add/Modify Account" OnCommand="Button_Command" CommandName="navigate" CommandArgument="Account.aspx" />
                <asp:Button runat="server" ID="btnAddModClient" CssClass="CommandButton" Text="Add/Modify Client" OnCommand="Button_Command" CommandName="navigate" CommandArgument="Client.aspx" />
                <asp:Button runat="server" ID="btnAddModOrg" CssClass="CommandButton" Text="Add/Modify Organization" OnCommand="Button_Command" CommandName="navigate" CommandArgument="Organization.aspx" />
                <asp:Button runat="server" ID="btnClientAccountAssign" CssClass="CommandButton" Text="Assign Clients to Accounts" OnCommand="Button_Command" CommandName="navigate-client-acct-assign" CommandArgument="/data/dispatch/assign-accounts?returnTo=/sseldata&OrgID={OrgID}" ToolTip="This page displays a matrix that allows assigning all of a manager's Clients to Any/all of his/her Accounts, including the setting of the Client's Primary Account" />
            </div>
            <div runat="server" id="divMisc" visible="true" class="col">
                <div class="ButtonGroupHeader">
                    <asp:Label runat="server">Misc</asp:Label>
                </div>
                <asp:Button runat="server" ID="btnPassword" CssClass="CommandButton" Text="Reset Password" OnCommand="Button_Command" CommandName="navigate" CommandArgument="/data/admin/password-reset?app=sseldata" ToolTip="Reset user passwords to the UserName." />
                <asp:Button runat="server" ID="btnAddModNews" CssClass="CommandButton" Text="Add/Modify News Items" OnCommand="Button_Command" CommandName="navigate" CommandArgument="//ssel-sched.eecs.umich.edu/news/" />
                <asp:Button runat="server" ID="btnAlerts" CssClass="CommandButton" Text="Add/Modify Alerts" OnCommand="Button_Command" CommandName="navigate" CommandArgument="/alerts/?ReturnTo=/sseldata" />
                <asp:Button runat="server" ID="btnFeeds" CssClass="CommandButton" Text="Add/Modify Feeds" OnCommand="Button_Command" CommandName="navigate" CommandArgument="/data/feed/list/?ReturnTo=/sseldata" />
            </div>
            <div runat="server" id="divGlobal" visible="true" class="col">
                <div class="ButtonGroupHeader">
                    <asp:Label runat="server">Global Settings</asp:Label>
                </div>
                <asp:Button runat="server" ID="btnGlobalFundingSource" CssClass="CommandButton" Text="Funding Sources" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="FundingSource" />
                <asp:Button runat="server" ID="btnGlobalTechnicalField" CssClass="CommandButton" Text="Technical Fields" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="TechnicalField" />
                <asp:Button runat="server" ID="btnGlobalSpecialTopic" CssClass="CommandButton" Text="Special Topics" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="SpecialTopic" />
                <asp:Button runat="server" ID="btnOrgType" CssClass="CommandButton" Text="Organization Types" OnCommand="Button_Command" CommandName="navigate" CommandArgument="OrgType.aspx" />
                <asp:Button runat="server" ID="btnChargeType" CssClass="CommandButton" Text="Charge Type Categories" OnCommand="Button_Command" CommandName="navigate" CommandArgument="ChargeType.aspx" />
                <asp:Button runat="server" ID="btnGlobalRole" CssClass="CommandButton" Text="Client Roles" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="Role" />
                <asp:Button runat="server" ID="btnGlobalCommunity" CssClass="CommandButton" Text="Communities" OnCommand="Button_Command" CommandName="navigate-global" CommandArgument="Community" />
            </div>
        </div>

        <div class="container">
            <div runat="server" id="divAppControl" visible="true" class="col">
                <div class="ButtonGroupHeader">
                    <asp:Label runat="server">Application Control</asp:Label>
                </div>
                <asp:Button runat="server" ID="btnLogout" CssClass="CommandButton" Text="Exit Application" OnClientClick="return logout();" />
            </div>
        </div>
    </div>
</asp:Content>
