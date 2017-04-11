<%@ Page Title="" Language="C#" MasterPageFile="~/data.master" AutoEventWireup="true" CodeBehind="Organization.aspx.cs" Inherits="sselData.Organization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label ID="lblHeader" Style="z-index: 101; left: 16px; position: absolute; top: 16px" runat="server" Height="40px" Width="888px" CssClass="PageHeader">
        Organization Configuration
    </asp:Label>
    <table id="Table1" style="z-index: 102; left: 16px; position: absolute; top: 56px;" border="1">
        <tr>
            <td>
                <asp:Panel ID="pAddEdit" runat="server" Visible="False">
                    <table id="Table2" border="0" runat="server">
                        <tr>
                            <td style="width: 150px;">
                                <asp:Label ID="Label1" runat="server" CssClass="LabelText">Organization Name</asp:Label></td>
                            <td>
                                <asp:TextBox ID="txtOrgName" runat="server" CssClass="GridText" Width="300px" MaxLength="50"></asp:TextBox></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="Label2" runat="server" CssClass="LabelText">Organization Type</asp:Label></td>
                            <td>
                                <asp:DropDownList ID="ddlOrgType" runat="server" CssClass="GridText" Width="300px">
                                </asp:DropDownList></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="Label3" runat="server" CssClass="LabelText">Is NNIN Organization?</asp:Label></td>
                            <td>
                                <asp:CheckBox ID="chkNninOrg" runat="server"></asp:CheckBox></td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:DataGrid runat="server" ID="dgAddress" ShowHeader="true" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1" GridLines="Horizontal" AllowSorting="true" AutoGenerateColumns="false" DataKeyField="AddressID" ShowFooter="True" CellPadding="2" OnItemCommand="dgAddress_ItemCommand" OnItemDataBound="dgAddress_ItemDataBound">
                                    <EditItemStyle BackColor="#66ffff" CssClass="GridText" />
                                    <AlternatingItemStyle BackColor="Linen" CssClass="GridText" />
                                    <ItemStyle CssClass="GridText" />
                                    <HeaderStyle CssClass="GridText" Font-Bold="true" Wrap="false" BackColor="LightGray" />
                                    <FooterStyle CssClass="GridText" BackColor="LightGray" />
                                    <Columns>
                                        <asp:TemplateColumn HeaderText="Address Type">
                                            <ItemTemplate>
                                                <asp:Label ID="lblType" runat="server" Width="100"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:DropDownList ID="ddlTypeF" runat="server" Width="100">
                                                    <asp:ListItem Value="DefClientAddressID">Client</asp:ListItem>
                                                    <asp:ListItem Value="DefBillAddressID">Billing</asp:ListItem>
                                                    <asp:ListItem Value="DefShipAddressID">Shipping</asp:ListItem>
                                                </asp:DropDownList>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:DropDownList ID="ddlType" runat="server" Width="100">
                                                    <asp:ListItem Value="DefClientAddressID">Client</asp:ListItem>
                                                    <asp:ListItem Value="DefBillAddressID">Billing</asp:ListItem>
                                                    <asp:ListItem Value="DefShipAddressID">Shipping</asp:ListItem>
                                                </asp:DropDownList>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="Attention Line">
                                            <ItemTemplate>
                                                <asp:Label ID="lblInternalAddress" runat="server" Width="100"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtInternalAddressF" runat="server" Width="100" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtInternalAddress" runat="server" Width="100" MaxLength="45"></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="Street Address, Line 1">
                                            <ItemTemplate>
                                                <asp:Label ID="lblStrAddress1" runat="server" Width="160"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtStrAddress1F" runat="server" Width="160" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtStrAddress1" runat="server" Width="160" MaxLength="45"></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="Street Address, Line 2">
                                            <ItemTemplate>
                                                <asp:Label ID="lblStrAddress2" runat="server" Width="160"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtStrAddress2F" runat="server" Width="160" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtStrAddress2" runat="server" Width="160" MaxLength="45"></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="City">
                                            <ItemTemplate>
                                                <asp:Label ID="lblCity" runat="server" Width="100"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtCityF" runat="server" Width="100" Text="" MaxLength="35"></asp:TextBox>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtCity" runat="server" Width="100" MaxLength="35"></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="State">
                                            <ItemTemplate>
                                                <asp:Label ID="lblState" runat="server" Width="50"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtStateF" runat="server" Width="50" Text="" MaxLength="2"></asp:TextBox>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtState" runat="server" Width="50" MaxLength="2"></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="Zip">
                                            <ItemTemplate>
                                                <asp:Label ID="lblZip" runat="server" Width="50"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtZipF" runat="server" Width="50" Text="" MaxLength="10"></asp:TextBox>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtZip" runat="server" Width="50" MaxLength="10"></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn HeaderText="Country">
                                            <ItemTemplate>
                                                <asp:Label ID="lblCountry" runat="server" Width="50"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtCountryF" runat="server" Width="50" Text="" MaxLength="50"></asp:TextBox>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtCountry" runat="server" Width="50" MaxLength="50"></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn>
                                            <ItemTemplate>
                                                <asp:ImageButton runat="server" ID="btnAddrEdit" ImageUrl="~/images/im_edit.gif" AlternateText="edit" CommandName="Edit" CausesValidation="false" />
                                                <asp:ImageButton runat="server" ID="btnAddrDelete" ImageUrl="~/images/im_delete.gif" AlternateText="delete" CommandName="Delete" CausesValidation="false" />
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:ImageButton runat="server" ID="btnAddrUpdate" ImageUrl="~/images/im_update.gif" AlternateText="save/update" CommandName="Update" />
                                                <asp:ImageButton runat="server" ID="btnAddrCancel" ImageUrl="~/images/im_cancel.gif" AlternateText="cancel" CommandName="Cancel" CausesValidation="false" />
                                            </EditItemTemplate>
                                            <FooterStyle HorizontalAlign="Center" />
                                            <FooterTemplate>
                                                <asp:Button ID="btnAddRow" runat="server" Text="ADD" CommandName="AddNewRow" />
                                            </FooterTemplate>
                                        </asp:TemplateColumn>
                                    </Columns>
                                </asp:DataGrid>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:DataGrid runat="server" ID="dgDepartment" ShowHeader="true" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1" GridLines="Horizontal" AllowSorting="true" AutoGenerateColumns="false" DataKeyField="DepartmentID" ShowFooter="True" CellPadding="2" OnItemCommand="dgDepartment_ItemCommand" OnItemDataBound="dgDepartment_ItemDataBound" OnSortCommand="dgDepartment_SortCommand">
                                    <FooterStyle CssClass="GridText" BackColor="#cdcdde" />
                                    <EditItemStyle CssClass="GridText" BackColor="#66ffff" />
                                    <ItemStyle CssClass="GridText" />
                                    <AlternatingItemStyle CssClass="GridText" BackColor="Linen" />
                                    <HeaderStyle Font-Bold="true" Wrap="false" CssClass="GridText" BackColor="#cdcdde" />
                                    <Columns>
                                        <asp:TemplateColumn SortExpression="Department" HeaderText="Department">
                                            <ItemTemplate>
                                                <asp:Label ID="lblDepartment" runat="server" Width="250"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:TextBox ID="txtDepartmentF" runat="server" Width="250" Text=""></asp:TextBox>&nbsp;
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:TextBox ID="txtDepartment" runat="server" Width="250"></asp:TextBox>
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn>
                                            <ItemTemplate>
                                                <asp:ImageButton runat="server" ID="btnDeptEdit" ImageUrl="~/images/im_edit.gif" AlternateText="edit" CommandName="Edit" CausesValidation="false" />
                                            </ItemTemplate>
                                            <FooterStyle HorizontalAlign="Center" />
                                            <FooterTemplate>
                                                <asp:Button ID="btnAddDeptRow" runat="server" Text="ADD" CommandName="AddNewRow" />
                                            </FooterTemplate>
                                            <EditItemTemplate>
                                                <asp:ImageButton runat="server" ID="btnDeptUpdate" ImageUrl="~/images/im_update.gif" AlternateText="save/update" CommandName="Update" />
                                                <asp:ImageButton runat="server" ID="btnDeptCancel" ImageUrl="~/images/im_cancel.gif" AlternateText="cancel" CommandName="Cancel" CausesValidation="false" />
                                            </EditItemTemplate>
                                        </asp:TemplateColumn>
                                    </Columns>
                                </asp:DataGrid>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="white-space: nowrap;">
                                <asp:Button runat="server" ID="btnOrgStore" CssClass="StoreButton" CausesValidation="false" Text="Store New Organization" OnClick="btnOrgStore_Click" />
                                <asp:Button runat="server" ID="btnOrgStoreQuit" CssClass="QuitStoreButton" CausesValidation="false" Text="Abandon Changes" OnClick="btnOrgStoreQuit_Click" />
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Panel ID="pExisting" runat="server" Visible="False">
                    <table id="Table3" runat="server">
                        <tr>
                            <td>
                                <asp:Label ID="Label5" runat="server" CssClass="LabelText">Name filter</asp:Label>
                            </td>
                            <td>
                                <asp:TextBox runat="server" ID="txtName" CssClass="GridText" Width="300" MaxLength="50" AutoPostBack="true" OnTextChanged="txtName_TextChanged"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Label ID="Label6" runat="server" CssClass="LabelText">Select Organization</asp:Label>
                            </td>
                            <td>
                                <asp:DropDownList ID="ddlOrg" runat="server" CssClass="DDLText" Width="300">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="white-space: nowrap;">
                                <asp:Button runat="server" ID="btnOrgReenable" CssClass="StoreButton" Text="Restore Organization" OnClick="btnOrgReenable_Click" />
                                <asp:Button runat="server" ID="btnOrgReenableQuit" CssClass="QuitStoreButton" Text="Abandon Restore" OnClick="btnOrgReenableQuit_Click" />
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Panel ID="pOrgList" runat="server" Visible="False">
                    <table id="Table4" runat="server" border="1">
                        <tr>
                            <td colspan="2">
                                <asp:DataGrid runat="server" ID="dgOrg" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1" GridLines="Horizontal" AllowSorting="True" AutoGenerateColumns="False" DataKeyField="OrgID" ShowFooter="true" CellPadding="2" AllowPaging="true" PageSize="15" Width="100%" OnSortCommand="dgOrg_SortCommand" OnItemCommand="dgOrg_ItemCommand" OnItemDataBound="dgOrg_ItemDataBound">
                                    <HeaderStyle BackColor="LightGray" Font-Bold="true" Wrap="false" />
                                    <FooterStyle CssClass="GridText" BackColor="LightGray" />
                                    <EditItemStyle CssClass="GridText" BackColor="#66FFFF" />
                                    <ItemStyle CssClass="GridText" />
                                    <AlternatingItemStyle CssClass="GridText" BackColor="Linen" />
                                    <HeaderStyle Font-Bold="True" Wrap="False" CssClass="GridText" BackColor="LightGray" />
                                    <PagerStyle Visible="False" HorizontalAlign="Right" Mode="NumericPages" />
                                    <Columns>
                                        <asp:TemplateColumn SortExpression="OrgName" HeaderText="Organization Name">
                                            <FooterStyle HorizontalAlign="Left" />
                                            <ItemTemplate>
                                                <asp:Label ID="lblOrgName" runat="server"></asp:Label>
                                            </ItemTemplate>
                                            <FooterTemplate>
                                                <asp:Button ID="btnAddExisting" runat="server" Text="Add Existing" CommandName="AddExisting" CssClass="CmdButton" />
                                            </FooterTemplate>
                                        </asp:TemplateColumn>
                                        <asp:TemplateColumn>
                                            <ItemStyle HorizontalAlign="Center" Width="60" />
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
                            <td style="height: 18px" colspan="2">Select page:
                                <asp:DropDownList runat="server" ID="ddlPager" Width="200" AutoPostBack="true" OnSelectedIndexChanged="ddlPager_SelectedIndexChanged">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="2" style="white-space: nowrap;">
                                <asp:Button runat="server" ID="btnSave" TabIndex="6" CssClass="SaveButton" Width="220" Height="32" CausesValidation="false" Text="Save Changes and Exit" OnClick="btnSave_Click" />
                                <asp:Button runat="server" ID="btnDiscard" CssClass="QuitButton" Width="220" Height="32" CausesValidation="false" Text="Discard Changes and Quit" OnClick="btnDiscard_Click" />
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
    </table>
</asp:Content>
