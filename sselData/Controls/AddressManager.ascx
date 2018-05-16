<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AddressManager.ascx.cs" Inherits="sselData.Controls.AddressManager" %>

<div class="address-manager">
    <asp:HiddenField runat="server" ID="hidEditItemIndex" Value="-1" />
    <asp:HiddenField runat="server" ID="hidShowFooter" Value="true" />
    <asp:Repeater runat="server" ID="rptAddressManager" OnItemDataBound="rptAddressManager_ItemDataBound">
        <HeaderTemplate>
            <table class="address-table">
                <thead>
                    <tr>
                        <th>Type</th>
                        <th>Attention</th>
                        <th>Address Line 1</th>
                        <th>Address Line 2</th>
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
                    <asp:Label runat="server" ID="TypeLabel" Text='<%#Eval("Type.Name") %>'></asp:Label>
                    <asp:DropDownList runat="server" ID="TypeDropDownList" DataTextField="Name" DataValueField="Column" Visible="false"></asp:DropDownList>
                </td>
                <td>
                    <asp:Label runat="server" ID="AttentionLabel" Text='<%#Eval("AttentionLine") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="AttentionTextBox" Text='<%#Eval("AttentionLine") %>' Width="100" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="AddressLine1Label" Text='<%#Eval("StreetAddressLine1") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="AddressLine1TextBox" Text='<%#Eval("StreetAddressLine1") %>' Width="160" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="AddressLine2Label" Text='<%#Eval("StreetAddressLine2") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="AddressLine2TextBox" Text='<%#Eval("StreetAddressLine2") %>' Width="160" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="CityLabel" Text='<%#Eval("City") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="CityTextBox" Text='<%#Eval("City") %>' Width="100" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="StateLabel" Text='<%#Eval("State") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="StateTextBox" Text='<%#Eval("State") %>' Width="50" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="ZipLabel" Text='<%#Eval("Zip") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="ZipTextBox" Text='<%#Eval("Zip") %>' Width="50" Visible="false"></asp:TextBox>
                </td>
                <td>
                    <asp:Label runat="server" ID="CountryLabel" Text='<%#Eval("Country") %>'></asp:Label>
                    <asp:TextBox runat="server" ID="CountryTextBox" Text='<%#Eval("Country") %>' Width="50" Visible="false"></asp:TextBox>
                </td>
                <td style="text-align: center;">
                    <asp:HiddenField runat="server" ID="AddressHiddenField" Value='<%#Eval("AddressID") %>' />
                    <asp:ImageButton runat="server" ID="EditButton" ImageUrl="~/images/im_edit.gif" OnCommand="Item_Command" CommandName="edit" CommandArgument='<%#Container.ItemIndex %>' />
                    <asp:ImageButton runat="server" ID="DeleteButton" ImageUrl="~/images/im_delete.gif" OnCommand="Item_Command" CommandName="delete" CommandArgument='<%#Container.ItemIndex %>' />
                    <asp:ImageButton runat="server" ID="UpdateButton" ImageUrl="~/images/im_update.gif" OnCommand="Item_Command" CommandName="update" CommandArgument='<%#Container.ItemIndex %>' Visible="false" />
                    <asp:ImageButton runat="server" ID="CancelButton" ImageUrl="~/images/im_cancel.gif" OnCommand="Item_Command" CommandName="cancel" CommandArgument='<%#Container.ItemIndex %>' Visible="false" />
                </td>
            </tr>
        </ItemTemplate>
    </asp:Repeater>
</div>
