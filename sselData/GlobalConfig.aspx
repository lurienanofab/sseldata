<%@ Page Title="" Language="C#" MasterPageFile="~/data.master" AutoEventWireup="true" CodeBehind="GlobalConfig.aspx.cs" Inherits="sselData.GlobalConfig" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <asp:Label runat="server" ID="lblConfigItem" Style="z-index: 101; left: 16px; position: absolute; top: 16px" Height="40" Width="560" CssClass="PageHeader">SSEL Config Item</asp:Label>
    <table id="Table1" style="z-index: 108; left: 16px; position: absolute; top: 64px" border="1">
        <tr>
            <td colspan="2">
                <asp:DataGrid runat="server" ID="dgGlobal" AllowSorting="true" AutoGenerateColumns="false" BorderColor="LightGray" BorderStyle="Ridge" BorderWidth="1" ShowFooter="true" HeaderStyle-Wrap="false" HeaderStyle-Font-Bold="true" HeaderStyle-BackColor="LightGrey" AlternatingItemStyle-BackColor="Linen" CellPadding="2" GridLines="Horizontal" OnItemCommand="dgGlobal_ItemCommand" OnItemDataBound="dgGlobal_ItemDataBound">
                    <EditItemStyle BackColor="#66FFFF" CssClass="GridText"></EditItemStyle>
                    <AlternatingItemStyle BackColor="Linen" CssClass="GridText"></AlternatingItemStyle>
                    <HeaderStyle CssClass="GridText" Font-Bold="True" Wrap="False" BackColor="LightGray"></HeaderStyle>
                    <FooterStyle CssClass="GridText" BackColor="LightGray"></FooterStyle>
                    <Columns>
                        <asp:TemplateColumn HeaderText="Make this dynamic">
                            <ItemTemplate>
                                <asp:Label ID="lblReqText" runat="server"></asp:Label>
                            </ItemTemplate>
                            <FooterTemplate>
                                <asp:TextBox runat="server" ID="tbReqTextF" Width="200" Text=""></asp:TextBox>
                                <asp:RequiredFieldValidator runat="server" ID="rfvtbReqTextF" Display="Dynamic" ErrorMessage="Please enter the required text" ControlToValidate="tbReqTextF" CssClass="WarningText"></asp:RequiredFieldValidator>
                            </FooterTemplate>
                            <EditItemTemplate>
                                <asp:TextBox runat="server" ID="tbReqText" Width="200"></asp:TextBox>
                                <asp:RequiredFieldValidator runat="server" ID="rfvtbReqText" Display="Dynamic" ErrorMessage="Please enter the required text" ControlToValidate="tbReqText" CssClass="WarningText"></asp:RequiredFieldValidator>
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
                                <asp:Button runat="server" ID="btnAddRow" Text="ADD" CommandName="AddANewRow" />
                            </FooterTemplate>
                        </asp:TemplateColumn>
                    </Columns>
                </asp:DataGrid>
            </td>
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
