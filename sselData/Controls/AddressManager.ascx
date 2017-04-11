<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddressManager.ascx.cs" Inherits="sselData.Controls.AddressManager" %>
<div class="address-manager">
    <asp:HiddenField runat="server" ID="hidEditItemIndex" Value="-1" />
    <asp:HiddenField runat="server" ID="hidShowFooter" Value="true" />
    <asp:Repeater runat="server" ID="rptAddressManager" OnItemDataBound="rptAddressManager_ItemDataBound">
        <HeaderTemplate>
            <table class="address-table">
                <thead>
                    <tr>
                        <th>Address Type</th>
                        <th>Attention Line</th>
                        <th>Street Address, Line 1</th>
                        <th>Street Address, Line 2</th>
                        <th>City</th>
                        <th>State</th>
                        <th>Zip</th>
                        <th>Country</th>
                        <th>&nbsp;</th>
                    </tr>
                </thead>
                <tbody>
        </HeaderTemplate>
        <ItemTemplate>
            <tr runat="server" id="trItem">
                <td>
                    <asp:Label runat="server" ID="lblType" Text='<%#Eval("Type.Name") %>'></asp:Label>
                    <asp:DropDownList runat="server" ID="ddlType" DataTextField="Name" DataValueField="Column" Visible="false"></asp:DropDownList>
                </td>
                <td>
                    <asp:Label runat="server" ID="lblAttentionLine" Text='<%#Eval("AttentionLine") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="txtAttentionLine" Text='<%#Eval("AttentionLine") %>' Width="100" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="lblStreetAddressLine1" Text='<%#Eval("StreetAddressLine1") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="txtStreetAddressLine1" Text='<%#Eval("StreetAddressLine1") %>' Width="160" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="lblStreetAddressLine2" Text='<%#Eval("StreetAddressLine2") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="txtStreetAddressLine2" Text='<%#Eval("StreetAddressLine2") %>' Width="160" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="lblCity" Text='<%#Eval("City") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="txtCity" Text='<%#Eval("City") %>' Width="100" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="lblState" Text='<%#Eval("State") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="txtState" Text='<%#Eval("State") %>' Width="50" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="lblZip" Text='<%#Eval("Zip") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="txtZip" Text='<%#Eval("Zip") %>' Width="50" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="lblCountry" Text='<%#Eval("Country") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="txtCountry" Text='<%#Eval("Country") %>' Width="50" Visible="false"></asp:TextBox>
                </td>
                <td style="text-align: center;">
                    <asp:HiddenField runat="server" ID="hidAddressID" Value='<%#Eval("AddressID") %>' />
                    <asp:ImageButton runat="server" ID="btnEdit" ImageUrl="~/images/im_edit.gif" OnCommand="Item_Command" CommandName="edit" CommandArgument='<%#Container.ItemIndex %>' />
                    <asp:ImageButton runat="server" ID="btnDelete" ImageUrl="~/images/im_delete.gif" OnCommand="Item_Command" CommandName="delete" CommandArgument='<%#Container.ItemIndex %>' />
                    <asp:ImageButton runat="server" ID="btnUpdate" ImageUrl="~/images/im_update.gif" OnCommand="Item_Command" CommandName="update" CommandArgument='<%#Container.ItemIndex %>' Visible="false" />
                    <asp:ImageButton runat="server" ID="btnCancel" ImageUrl="~/images/im_cancel.gif" OnCommand="Item_Command" CommandName="cancel" CommandArgument='<%#Container.ItemIndex %>' Visible="false" />
                </td>
            </tr>
        </ItemTemplate>
        <FooterTemplate>
            </tbody>
            <tfoot>
                <tr runat="server" id="trFooter">
                    <td>
                        <asp:DropDownList runat="server" ID="ddlTypeF" DataTextField="Name" DataValueField="Column" Width="100%"></asp:DropDownList>
                    </td>
                    <td>
                        <asp:TextBox runat="server" ID="txtAttentionLineF" Width="100"></asp:TextBox>
                    </td>
                    <td>
                        <asp:TextBox runat="server" ID="txtStreetAddressLine1F" Width="160"></asp:TextBox>
                    </td>
                    <td>
                        <asp:TextBox runat="server" ID="txtStreetAddressLine2F" Width="160"></asp:TextBox>
                    </td>
                    <td>
                        <asp:TextBox runat="server" ID="txtCityF" Width="100"></asp:TextBox>
                    </td>
                    <td>
                        <asp:TextBox runat="server" ID="txtStateF" Width="50"></asp:TextBox>
                    </td>
                    <td>
                        <asp:TextBox runat="server" ID="txtZipF" Width="50"></asp:TextBox>
                    </td>
                    <td>
                        <asp:TextBox runat="server" ID="txtCountryF" Width="50"></asp:TextBox>
                    </td>
                    <td>
                        <asp:Button runat="server" ID="btnAdd" Text="ADD" CssClass="add-button" OnClick="btnAdd_Click" />
                    </td>
                </tr>
            </tfoot>
            </table>
        </FooterTemplate>
    </asp:Repeater>
</div>
