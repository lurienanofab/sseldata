<%@ Page Title="" Language="C#" MasterPageFile="~/data.master" AutoEventWireup="true" CodeBehind="ChargeType.aspx.cs" Inherits="sselData.ChargeType" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
<div class="section">
        <asp:Label ID="lblConfigItem" Style="z-index: 102; left: 16px; position: absolute; top: 16px" runat="server" CssClass="PageHeader" Width="560px" Height="40px">Configure Charge Type Categories</asp:Label>
        <table id="Table1" style="z-index: 101; left: 16px; position: absolute; top: 64px" border="1">
            <tr>
                <td colspan="2">
                    <asp:DataGrid runat="server" ID="dgChargeType" GridLines="Horizontal" CellPadding="2" AlternatingItemStyle-BackColor="Linen" HeaderStyle-BackColor="LightGrey" HeaderStyle-Font-Bold="true" HeaderStyle-Wrap="false" ShowFooter="true" BorderWidth="1" BorderStyle="Ridge" BorderColor="LightGray" AutoGenerateColumns="false" AllowSorting="true" OnItemCommand="dgChargeType_ItemCommand" OnItemDataBound="dgChargeType_ItemDataBound">
                        <FooterStyle CssClass="GridText" BackColor="LightGray"></FooterStyle>
                        <EditItemStyle CssClass="GridText" BackColor="#66FFFF"></EditItemStyle>
                        <ItemStyle CssClass="GridText" />
                        <AlternatingItemStyle CssClass="GridText" BackColor="Linen"></AlternatingItemStyle>
                        <HeaderStyle Font-Bold="True" Wrap="False" CssClass="GridText" BackColor="LightGray"></HeaderStyle>
                        <Columns>
                            <asp:TemplateColumn HeaderText="CTC ID">
                                <ItemTemplate>
                                    <asp:Label ID="lblCTCId" runat="server" Width="100px"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox runat="server" ID="txtCTCIdF" Width="100" MaxLength="3"></asp:TextBox>&nbsp;
                                        <asp:RequiredFieldValidator ID="rfvtxtCTCIdF" runat="server" Display="Dynamic" ErrorMessage="ID is required"
                                            ControlToValidate="txtCTCIdF" CssClass="WarningText"></asp:RequiredFieldValidator>
                                </FooterTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox runat="server" ID="txtCTCId" Width="100" MaxLength="3"></asp:TextBox>&nbsp;
                                        <asp:RequiredFieldValidator ID="rfvtxtCTCId" runat="server" Display="Dynamic" ErrorMessage="ID is required"
                                            ControlToValidate="txtCTCId" CssClass="WarningText"></asp:RequiredFieldValidator>
                                </EditItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Charge Type">
                                <ItemTemplate>
                                    <asp:Label ID="lblCTC" runat="server" Width="150px"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:TextBox ID="txtCTCF" runat="server" Width="150px" Text=""></asp:TextBox>&nbsp;
                                        <asp:RequiredFieldValidator ID="rvftxtCTCF" runat="server" Display="Dynamic" ErrorMessage="Charge type is required"
                                            ControlToValidate="txtCTCIdF" CssClass="WarningText"></asp:RequiredFieldValidator>
                                </FooterTemplate>
                                <EditItemTemplate>
                                    <asp:TextBox ID="txtCTC" runat="server" Width="150px"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="rfvtxtCTC" runat="server" Display="Dynamic" ErrorMessage="Charge type is required"
                                        ControlToValidate="txtCTC" CssClass="WarningText"></asp:RequiredFieldValidator>
                                </EditItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn HeaderText="Dept Ref Acct">
                                <ItemTemplate>
                                    <asp:Label ID="lblAccountID" runat="server" Width="150px"></asp:Label>
                                </ItemTemplate>
                                <FooterTemplate>
                                    <asp:DropDownList ID="ddlAccountIDF" runat="server" Width="150px">
                                    </asp:DropDownList>&nbsp;
                                        <asp:RequiredFieldValidator ID="rvftxtAccountIDF" runat="server" Display="Dynamic"
                                            ErrorMessage="Please select an account" ControlToValidate="ddlAccountIDF" CssClass="WarningText"></asp:RequiredFieldValidator>
                                </FooterTemplate>
                                <EditItemTemplate>
                                    <asp:DropDownList ID="ddlAccountID" runat="server" Width="150px">
                                    </asp:DropDownList>&nbsp;
                                        <asp:RequiredFieldValidator ID="rvftxtAccountID" runat="server" Display="Dynamic"
                                            ErrorMessage="Please select an account" ControlToValidate="ddlAccountID" CssClass="WarningText"></asp:RequiredFieldValidator>
                                </EditItemTemplate>
                            </asp:TemplateColumn>
                            <asp:TemplateColumn>
                                <ItemTemplate>
                                    <asp:ImageButton runat="server" ID="btnEdit" ImageUrl="~/images/im_edit.gif" AlternateText="edit" CommandName="Edit" CausesValidation="false" />
                                    <asp:ImageButton runat="server" ID="btnDelete" ImageUrl="~/images/im_delete.gif" AlternateText="delete" CommandName="Delete" CausesValidation="false" />
                                </ItemTemplate>
                                <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                <FooterTemplate>
                                    <asp:Button ID="btnAddRow" runat="server" Text="ADD" CommandName="AddANewRow" />
                                </FooterTemplate>
                                <EditItemTemplate>
                                    <asp:ImageButton runat="server" ID="btnUpdate" ImageUrl="~/images/im_update.gif" AlternateText="save/update" CommandName="Update" />
                                    <asp:ImageButton runat="server" ID="btnCancel" ImageUrl="~/images/im_cancel.gif" AlternateText="cancel" CommandName="Cancel" CausesValidation="false" />
                                </EditItemTemplate>
                            </asp:TemplateColumn>
                        </Columns>
                    </asp:DataGrid>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <asp:Button runat="server" ID="btnSave" CssClass="SaveButton" Width="220" Height="32" CausesValidation="false" Text="Save Changes and Exit" OnClick="btnSave_Click" />
                    <asp:Button runat="server" ID="btnDiscard" CssClass="QuitButton" Width="220" Height="32" CausesValidation="false" Text="Discard Changes and Quit" OnClick="btnDiscard_Click" />
                </td>
            </tr>
        </table>
    </div>
</asp:Content>
