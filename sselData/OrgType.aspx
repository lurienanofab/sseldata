<%@ Page Title="" Language="C#" MasterPageFile="~/data.master" AutoEventWireup="true" CodeBehind="OrgType.aspx.cs" Inherits="sselData.OrgType" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label ID="lblConfigItem" Style="z-index: 101; left: 16px; position: absolute; top: 16px"
        runat="server" Height="40px" Width="560px" CssClass="PageHeader">Configure Organization Type</asp:Label>
    <table id="Table1" style="z-index: 108; left: 16px; position: absolute; top: 64px" border="1">
        <tr>
            <td colspan="2">
                <asp:DataGrid runat="server" ID="dgOrgType" AutoGenerateColumns="false" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1" ShowFooter="true" HeaderStyle-Wrap="false" HeaderStyle-Font-Bold="true" HeaderStyle-BackColor="LightGrey" AlternatingItemStyle-BackColor="Linen" CellPadding="2" GridLines="Horizontal" DataKeyField="OrgTypeID" OnItemCommand="dgOrgType_ItemCommand" OnItemDataBound="dgOrgType_ItemDataBound">
                    <EditItemStyle BackColor="#66FFFF" CssClass="GridText"></EditItemStyle>
                    <AlternatingItemStyle BackColor="Linen" CssClass="GridText"></AlternatingItemStyle>
                    <HeaderStyle CssClass="GridText" Font-Bold="True" Wrap="False" BackColor="LightGray"
                        HorizontalAlign="Center"></HeaderStyle>
                    <FooterStyle CssClass="GridText" BackColor="LightGray"></FooterStyle>
                    <Columns>
                        <asp:TemplateColumn HeaderText="Organization Type">
                            <ItemStyle Width="150"></ItemStyle>
                            <ItemTemplate>
                                <asp:Label ID="lblOrgType" runat="server"></asp:Label>
                            </ItemTemplate>
                            <FooterStyle Width="150"></FooterStyle>
                            <FooterTemplate>
                                <asp:TextBox ID="txtOrgTypeF" runat="server" Width="150px" Text=""></asp:TextBox>&nbsp;
                                        <asp:RequiredFieldValidator ID="rfvtxtOrgTypeF" runat="server" Display="Dynamic"
                                            ErrorMessage="Please enter the required text" ControlToValidate="txtOrgTypeF"
                                            CssClass="WarningText"></asp:RequiredFieldValidator>
                            </FooterTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtOrgType" runat="server" Width="150px"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvtxtOrgType" runat="server" Display="Dynamic" ErrorMessage="Please enter the required text"
                                    ControlToValidate="txtOrgType" CssClass="WarningText"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                        </asp:TemplateColumn>
                        <asp:TemplateColumn HeaderText="Charge Type Category">
                            <ItemStyle Width="200"></ItemStyle>
                            <ItemTemplate>
                                <asp:Label ID="lblCTC" runat="server"></asp:Label>
                            </ItemTemplate>
                            <FooterStyle Width="200"></FooterStyle>
                            <FooterTemplate>
                                <asp:DropDownList ID="ddlCTCF" runat="server" Width="200px">
                                </asp:DropDownList>&nbsp;
                                        <asp:RequiredFieldValidator ID="rfvddlCTCF" runat="server" Display="Dynamic" ErrorMessage="Please enter a CTC"
                                            ControlToValidate="ddlCTCF" CssClass="WarningText"></asp:RequiredFieldValidator>
                            </FooterTemplate>
                            <EditItemTemplate>
                                <asp:DropDownList ID="ddlCTC" runat="server" Width="200">
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="rfvddlCTC" runat="server" Display="Dynamic" ErrorMessage="Please enter a CTC" ControlToValidate="ddlCTC" CssClass="WarningText"></asp:RequiredFieldValidator>
                            </EditItemTemplate>
                        </asp:TemplateColumn>
                        <asp:TemplateColumn>
                            <ItemTemplate>
                                <asp:ImageButton runat="server" ID="btnEdit" ImageUrl="~/images/im_edit.gif" AlternateText="edit" CommandName="Edit" CausesValidation="false" />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:ImageButton runat="server" ID="btnUpdate" ImageUrl="~/images/im_update.gif" AlternateText="save/update" CommandName="Update" />
                                <asp:ImageButton runat="server" ID="btnCancel" ImageUrl="~/images/im_cancel.gif" AlternateText="cancel" CommandName="Cancel" CausesValidation="false" />
                            </EditItemTemplate>
                            <FooterStyle HorizontalAlign="Center" />
                            <FooterTemplate>
                                <asp:Button ID="btnAddRow" runat="server" Text="ADD" CommandName="AddANewRow" />
                            </FooterTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                </asp:DataGrid></td>
        </tr>
        <tr>
            <td>
                <asp:Button runat="server" ID="btnSave" Height="32" Width="220" Text="Save Changes and Exit" CausesValidation="false" CssClass="SaveButton" OnClick="btnSave_Click" />
            </td>
            <td>
                <asp:Button runat="server" ID="btnDiscard" Height="32" Width="220" Text="Discard Changes and Quit" CausesValidation="false" CssClass="QuitButton" OnClick="btnDiscard_Click" />
            </td>
        </tr>
    </table>
</asp:Content>
