<%@ Page Title="" Language="C#" MasterPageFile="~/data.master" AutoEventWireup="true" CodeBehind="Account.aspx.cs" Inherits="sselData.Account" %>

<%@ Register Src="~/controls/AddressManager.ascx" TagPrefix="uc" TagName="AddressManager" %>
<%@ Register Assembly="sselData" Namespace="sselData.Controls" TagPrefix="uc" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:HiddenField runat="server" ID="hidAccountID" Value="0" />
    <div class="section">
        <asp:Label runat="server" ID="lblHeader" CssClass="PageHeader">Account Information</asp:Label>
        <table id="Table1" style="margin-top: 10px;" border="1">
            <tr>
                <td>
                    <asp:Panel runat="server" ID="pIntAccount" Visible="false">
                        <table id="Table5" border="1" style="width: 100%;">
                            <tr>
                                <td colspan="2">University of Michigan Chart Fields</td>
                            </tr>
                            <tr>
                                <td style="width: 100px">Account:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtAccount"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Fund:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtFund"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Department</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtDepartment"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Program:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtProgram"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Class:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtClass"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Project/Grant</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtProject"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Short code</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtShortCode" MaxLength="6"></asp:TextBox>
                                    <span class="error-messsage-bgcolor" id="spanAjaxErrorMsg"></span><span style="display: none" id="spanEnableByShortcode">
                                        <asp:LinkButton runat="server" ID="lbtnReactivateByShortCode" OnClick="lbtnReactivateByShortCode_Click" Text="Yes"></asp:LinkButton>
                                    </span>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Panel runat="server" ID="pExtAccount" Visible="false">
                        <table id="Table6" border="1" style="width: 100%;">
                            <tr>
                                <td colspan="2">External Organization Account Number</td>
                            </tr>
                            <tr>
                                <td style="width: 200px">Account Number:</td>
                                <td>
                                    <asp:Label runat="server" ID="Label21">999999EX</asp:Label>
                                    <asp:TextBox runat="server" ID="txtNumber" Width="441"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Purchase order number:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtInvoiceNumber" Width="400"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Invoice - Line 1:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtInvoiceLine1" Width="400"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Invoice - Line 2:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtInvoiceLine2" Width="400"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Purchase order end date:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtPoEndDate" Width="400"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Purchase order initial funds:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtPoInitialFunds" Width="400"></asp:TextBox>
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Panel ID="pAddEdit" runat="server" Visible="false">
                        <table id="Table2" border="1" style="width: 100%;">
                            <tr>
                                <td style="width: 200px;">Account Name:</td>
                                <td>
                                    <asp:TextBox runat="server" ID="txtName" Width="406"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Funding Source:</td>
                                <td>
                                    <asp:DropDownList runat="server" ID="ddlFundingSource" Width="406">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>Technical Field:</td>
                                <td>
                                    <asp:DropDownList runat="server" ID="ddlTechnicalField" Width="406">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>Special Topic:</td>
                                <td>
                                    <asp:DropDownList runat="server" ID="ddlSpecialTopic" Width="406">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td>Account Type</td>
                                <td>
                                    <asp:RadioButtonList runat="server" ID="cklistAccountType" RepeatDirection="Horizontal" DataSourceID="odsAccountType" DataValueField="AccountTypeID" DataTextField="AccountTypeName">
                                    </asp:RadioButtonList>
                                    <asp:ObjectDataSource runat="server" ID="odsAccountType" SelectMethod="GetAllAccountTypes" TypeName="sselData.AppCode.DAL.AccountTypeDA"></asp:ObjectDataSource>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <uc:AddressManager runat="server" ID="AddressManager1" OnUpdateAddress="AddressManager1_UpdateAddress" OnCreateAddress="AddressManager1_CreateAddress" OnEditAddress="AddressManager1_EditAddress" OnDeleteAddress="AddressManager1_DeleteAddress">
                                        <AddressTypes>
                                            <uc:AddressType Column="BillAddressID" Name="Billing" />
                                            <uc:AddressType Column="ShipAddressID" Name="Shipping" />
                                        </AddressTypes>
                                    </uc:AddressManager>
<%--                                    <asp:DataGrid runat="server" ID="dgAddress" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1" GridLines="Horizontal" AlternatingItemStyle-BackColor="Linen" HeaderStyle-BackColor="LightGrey" HeaderStyle-Font-Bold="true" HeaderStyle-Wrap="false" AllowSorting="true" AutoGenerateColumns="false" DataKeyField="AddressID" ShowFooter="true" CellPadding="2" ShowHeader="true" Width="100%" OnItemCommand="dgAddress_ItemCommand" OnItemDataBound="dgAddress_ItemDataBound" Visible="true">
                                        <FooterStyle CssClass="GridText" BackColor="LightGray" />
                                        <EditItemStyle CssClass="GridText" BackColor="#66ffff" />
                                        <ItemStyle CssClass="GridText" />
                                        <AlternatingItemStyle CssClass="GridText" BackColor="Linen" />
                                        <HeaderStyle Font-Bold="true" Wrap="false" CssClass="GridText" BackColor="LightGray" />
                                        <Columns>
                                            <asp:TemplateColumn HeaderText="Address Type">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lblType" Width="100"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:DropDownList runat="server" ID="ddlTypeF" Width="100" AutoPostBack="true" OnSelectedIndexChanged="ddlAddrType_Changed">
                                                        <asp:ListItem Value="BillAddressID">Billing</asp:ListItem>
                                                        <asp:ListItem Value="ShipAddressID">Shipping</asp:ListItem>
                                                    </asp:DropDownList>
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:DropDownList runat="server" ID="ddlType" Width="100" AutoPostBack="true" OnSelectedIndexChanged="ddlAddrType_Changed">
                                                        <asp:ListItem Value="BillAddressID">Billing</asp:ListItem>
                                                        <asp:ListItem Value="ShipAddressID">Shipping</asp:ListItem>
                                                    </asp:DropDownList>
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn HeaderText="Attention Line">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lblInternalAddress" Width="100"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox runat="server" ID="txtInternalAddressF" Width="100" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox runat="server" ID="txtInternalAddress" Width="100" MaxLength="45"></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn HeaderText="Street Address, Line 1">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lblStrAddress1" Width="160"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox runat="server" ID="txtStrAddress1F" Width="160" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox runat="server" ID="txtStrAddress1" Width="160" MaxLength="45"></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn HeaderText="Street Address, Line 2">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lblStrAddress2" Width="160"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox runat="server" ID="txtStrAddress2F" Width="160" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox runat="server" ID="txtStrAddress2" Width="160" MaxLength="45"></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn HeaderText="City">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lblCity" Width="100"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox runat="server" ID="txtCityF" Width="100" Text="" MaxLength="35"></asp:TextBox>&nbsp;
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox runat="server" ID="txtCity" Width="100" MaxLength="35"></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn HeaderText="State">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lblState" Width="50"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox runat="server" ID="txtStateF" Width="50" Text="" MaxLength="2"></asp:TextBox>&nbsp;
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox runat="server" ID="txtState" Width="50" MaxLength="2"></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn HeaderText="Zip">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lblZip" Width="50"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox runat="server" ID="txtZipF" Width="50" Text="" MaxLength="10"></asp:TextBox>&nbsp;
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox runat="server" ID="txtZip" Width="50" MaxLength="10"></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn HeaderText="Country">
                                                <ItemTemplate>
                                                    <asp:Label runat="server" ID="lblCountry" Width="50"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:TextBox runat="server" ID="txtCountryF" Width="50" Text="" MaxLength="50"></asp:TextBox>&nbsp;
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:TextBox runat="server" ID="txtCountry" Width="50" MaxLength="50"></asp:TextBox>
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn>
                                                <ItemStyle HorizontalAlign="Center" />
                                                <ItemTemplate>
                                                    <asp:ImageButton runat="server" ID="btnAddrEdit" ImageUrl="~/images/im_edit.gif" AlternateText="Edit" CommandName="Edit" CausesValidation="false" />
                                                    <asp:ImageButton runat="server" ID="btnAddrDelete" ImageUrl="~/images/im_delete.gif" AlternateText="Delete" CommandName="Delete" CausesValidation="false" />
                                                </ItemTemplate>
                                                <FooterStyle HorizontalAlign="Center" />
                                                <FooterTemplate>
                                                    <asp:Button ID="btnAddRow" runat="server" Text="ADD" CommandName="AddNewRow" />
                                                </FooterTemplate>
                                                <EditItemTemplate>
                                                    <asp:ImageButton runat="server" ID="btnAddrUpdate" ImageUrl="~/images/im_update.gif" AlternateText="save/update" CommandName="Update" />
                                                    <asp:ImageButton runat="server" ID="btnAddrCancel" ImageUrl="~/images/im_cancel.gif" AlternateText="cancel" CommandName="Cancel" CausesValidation="false" />
                                                </EditItemTemplate>
                                            </asp:TemplateColumn>
                                        </Columns>
                                    </asp:DataGrid>--%>
                                </td>
                            </tr>
                            <tr>
                                <td>Manager(s):</td>
                                <td>
                                    <asp:DataGrid runat="server" ID="dgAccountManager" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1px" GridLines="Horizontal" AlternatingItemStyle-BackColor="Linen" HeaderStyle-BackColor="LightGrey" HeaderStyle-Font-Bold="true" HeaderStyle-Wrap="false" AutoGenerateColumns="False" DataKeyField="ClientOrgID" ShowFooter="True" CellPadding="2" AllowPaging="false" OnItemCommand="dgAccountManager_ItemCommand" OnItemDataBound="dgAccountManager_ItemDataBound">
                                        <FooterStyle CssClass="GridText" BackColor="LightGray"></FooterStyle>
                                        <EditItemStyle CssClass="GridText" BackColor="#66FFFF"></EditItemStyle>
                                        <ItemStyle CssClass="GridText" />
                                        <AlternatingItemStyle CssClass="GridText" BackColor="Linen"></AlternatingItemStyle>
                                        <HeaderStyle Font-Bold="True" Wrap="False" CssClass="GridText" BackColor="LightGray"></HeaderStyle>
                                        <Columns>
                                            <asp:TemplateColumn SortExpression="DisplayName" HeaderText="Manager">
                                                <ItemStyle Width="200" />
                                                <ItemTemplate>
                                                    <asp:Label ID="lblMgr" runat="server"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:DropDownList ID="ddlMgr" runat="server" Width="200px">
                                                    </asp:DropDownList>
                                                </FooterTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn>
                                                <ItemStyle HorizontalAlign="Center" />
                                                <FooterStyle HorizontalAlign="Right" />
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="lbutMgrDelete" runat="server" Text="<img border=0 src=images/im_delete.gif alt=delete>" CommandName="Delete" CausesValidation="false"></asp:LinkButton>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:Button runat="server" ID="btnMgrAdd" Text="Add New" CommandName="AddNew" CssClass="CmdButton" />
                                                </FooterTemplate>
                                            </asp:TemplateColumn>
                                        </Columns>
                                        <PagerStyle HorizontalAlign="Right" Mode="NumericPages" />
                                    </asp:DataGrid>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Button runat="server" ID="btnAccountStore" CssClass="StoreButton" Text="Store New Account" CausesValidation="false" OnClick="btnAccountStore_Click" />
                                    <asp:Button runat="server" ID="btnAccountStoreQuit" CssClass="QuitStoreButton" Text="Abandon Changes" CausesValidation="false" OnClick="btnAccountStoreQuit_Click" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Panel ID="pExisting" runat="server" Visible="False">
                        <table runat="server" id="Table4">
                            <tr>
                                <td colspan="2">
                                    <asp:RadioButtonList ID="rblAcctDisplay" runat="server" AutoPostBack="True" RepeatDirection="Horizontal" TextAlign="Left" OnSelectedIndexChanged="rblAcctDisplay_SelectedIndexChanged">
                                        <asp:ListItem Value="Name" Selected="True">Name</asp:ListItem>
                                        <asp:ListItem Value="Number">Number</asp:ListItem>
                                        <asp:ListItem Value="Project">Project</asp:ListItem>
                                        <asp:ListItem Value="ShortCode">Short Code</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                            <tr>
                                <td>Record Filter:</td>
                                <td>
                                    <asp:TextBox ID="txtSearch" runat="server" CssClass="GridText" Width="300" AutoPostBack="True" MaxLength="50"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td>Select Account to Restore:</td>
                                <td>
                                    <asp:DropDownList ID="ddlAccount" runat="server" CssClass="DDLText" Width="536px" OnPreRender="ddlAccount_PreRender">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Button runat="server" ID="btnAccountReenable" CssClass="StoreButton" Text="Restore Account" CausesValidation="False" OnClick="btnAccountReenable_Click" />
                                    <asp:Button runat="server" ID="btnAccountReenableQuit" CssClass="QuitStoreButton" Text="Abandon Restore" CausesValidation="False" OnClick="btnAccountReenableQuit_Click" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Panel ID="pAccountList" runat="server">
                        <table id="Table3" border="1">
                            <tr>
                                <td colspan="2">
                                    <asp:DataGrid runat="server" ID="dgAccount" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1" GridLines="Horizontal" AllowSorting="True" AutoGenerateColumns="False" DataKeyField="AccountID" ShowFooter="True" CellPadding="2" AllowPaging="True" PageSize="15" OnItemCommand="dgAccount_ItemCommand" OnItemDataBound="dgAccount_ItemDataBound" OnSortCommand="dgAccount_SortCommand">
                                        <FooterStyle CssClass="GridText" BackColor="LightGray" />
                                        <EditItemStyle CssClass="GridText" BackColor="#66FFFF" />
                                        <ItemStyle CssClass="GridText" />
                                        <AlternatingItemStyle CssClass="GridText" BackColor="Linen" />
                                        <HeaderStyle Font-Bold="True" Wrap="False" CssClass="GridText" BackColor="LightGray" />
                                        <PagerStyle Visible="False" HorizontalAlign="Right" Mode="NumericPages" />
                                        <Columns>
                                            <asp:TemplateColumn SortExpression="Name" HeaderText="Account Name">
                                                <HeaderStyle Width="250" />
                                                <ItemStyle Width="250" />
                                                <FooterStyle HorizontalAlign="Left" />
                                                <ItemTemplate>
                                                    <asp:Label ID="lblName" runat="server"></asp:Label>
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:Button ID="btnAddExisting" runat="server" Text="Add Existing" CommandName="AddExisting" CssClass="CmdButton" />
                                                </FooterTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn SortExpression="Number" HeaderText="Account Number">
                                                <HeaderStyle Width="350" />
                                                <ItemStyle Width="350" />
                                                <ItemTemplate>
                                                    <asp:Label ID="lblNumber" runat="server"></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn SortExpression="Project" HeaderText="Project/Grant">
                                                <HeaderStyle HorizontalAlign="Center" Width="80" />
                                                <ItemStyle HorizontalAlign="Center" Width="80" />
                                                <ItemTemplate>
                                                    <asp:Label ID="lblProject" runat="server"></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn SortExpression="ShortCode" HeaderText="Short Code">
                                                <HeaderStyle Width="80" />
                                                <ItemStyle Width="80" />
                                                <ItemTemplate>
                                                    <asp:Label ID="lblShortCode" runat="server"></asp:Label>
                                                </ItemTemplate>
                                            </asp:TemplateColumn>
                                            <asp:TemplateColumn>
                                                <ItemStyle HorizontalAlign="Center" />
                                                <FooterStyle HorizontalAlign="Right" />
                                                <ItemTemplate>
                                                    <asp:ImageButton runat="server" ID="btnEdit" ImageUrl="~/images/im_edit.gif" AlternateText="edit" CommandName="Edit" CausesValidation="false" />
                                                    <asp:ImageButton runat="server" ID="btnDelete" ImageUrl="~/images/im_delete.gif" AlternateText="delete" CommandName="Delete" CausesValidation="false" />
                                                </ItemTemplate>
                                                <FooterTemplate>
                                                    <asp:Button ID="btnAddNew" runat="server" Text="Add New" CommandName="AddNew" CssClass="CmdButton" />
                                                </FooterTemplate>
                                            </asp:TemplateColumn>
                                        </Columns>
                                    </asp:DataGrid>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">Select page:
                                    <asp:DropDownList runat="server" ID="ddlPager" Width="283px" AutoPostBack="True" OnSelectedIndexChanged="ddlPager_SelectedIndexChanged">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Button runat="server" ID="btnSave" CssClass="SaveButton" Width="220" Height="32" Text="Save Changes and Exit" CausesValidation="false" OnClick="btnSave_Click" />
                                    <asp:Button runat="server" ID="btnDiscard" CssClass="QuitButton" Width="220" Height="32" Text="Discard Changes and Quit" CausesValidation="false" OnClick="btnDiscard_Click" />
                                </td>
                            </tr>
                        </table>
                    </asp:Panel>
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
