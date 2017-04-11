<%@ Page Title="" Language="C#" MasterPageFile="~/data.master" AutoEventWireup="true" CodeBehind="Client.aspx.cs" Inherits="sselData.Client" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .client-dialog {
            padding: 10px;
            margin-left: 20px;
            border: solid 1px #AAA;
        }

            .client-dialog .dialog-buttons {
                margin-top: 10px;
            }

        .physical-access {
        }

            .physical-access > table {
                width: 100%;
            }

                .physical-access > table > tbody > tr > th {
                    text-align: left;
                    background-color: #eee;
                    text-align: right;
                }

                    .physical-access > table > tbody > tr > th[colspan="5"] {
                        text-align: left;
                        background-color: #d3d3d3;
                    }

                .physical-access > table > tbody > tr.disabled > td {
                    color: #aa0000;
                    background-color: #ffdddd;
                }

            .physical-access .no-data {
                padding: 5px;
                color: #808080;
            }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="edit-client-form" style="padding: 16px;">
        <asp:Label ID="lblHeader" runat="server" CssClass="PageHeader">Client Information</asp:Label>
        <asp:Panel runat="server" ID="panDisplay">
            <table id="Table1" border="1" style="margin-top: 16px; width: 300px;">
                <tr>
                    <td>
                        <asp:Panel runat="server" ID="panLDAPLookup" Visible="false">
                            <table border="1" style="width: 100%;">
                                <tr>
                                    <td style="width: 100px;">UM Unique ID:</td>
                                    <td>
                                        <asp:TextBox runat="server" ID="txtUniqueID" CssClass="umich-directory-search-textbox"></asp:TextBox>
                                        <input type="button" class="umich-directory-search-button" value="Fill in Form Using Unique ID" />
                                        <span class="umich-directory-search-message" style="color: #FF0000;"></span>
                                        <div style="color: #FF0000;">
                                            <asp:Label runat="server" ID="lblLDAPMsg" Text=""></asp:Label>
                                            <asp:Literal runat="server" ID="litDebug"></asp:Literal>
                                        </div>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Panel ID="pAddEdit" runat="server">
                            <table id="Table2" style="width: 300px;" border="1">
                                <tr>
                                    <td>Name (first, middle, last):</td>
                                    <td>
                                        <asp:TextBox ID="txtFName" TabIndex="1" runat="server" Width="184px" MaxLength="20" CssClass="fname-textbox"></asp:TextBox>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtMName" TabIndex="2" runat="server" Width="184px" MaxLength="20" CssClass="mname-textbox"></asp:TextBox>
                                    </td>
                                    <td>
                                        <asp:TextBox ID="txtLName" TabIndex="3" runat="server" Width="184px" MaxLength="30" CssClass="lname-textbox"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Username:</td>
                                    <td colspan="3">
                                        <asp:TextBox ID="txtUsername" TabIndex="5" runat="server" MaxLength="20" CssClass="username-textbox"></asp:TextBox>&nbsp;<span style="color: #808080; font-style: italic; font-family: Arial; font-size: 10pt;">(20 character limit)</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td>US citizen or perm. resident</td>
                                    <td colspan="3">
                                        <asp:RadioButtonList ID="rblCitizen" TabIndex="6" runat="server" RepeatDirection="Horizontal">
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Ethnicity:</td>
                                    <td colspan="3">
                                        <asp:RadioButtonList ID="rblEthnic" TabIndex="7" runat="server" RepeatDirection="Horizontal">
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Race:</td>
                                    <td colspan="3">
                                        <asp:RadioButtonList ID="rblRace" TabIndex="8" runat="server" RepeatDirection="Horizontal" CssClass="client-race-list" RepeatColumns="5">
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Gender:</td>
                                    <td colspan="3">
                                        <asp:RadioButtonList ID="rblGender" TabIndex="9" runat="server" RepeatDirection="Horizontal">
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Disability:</td>
                                    <td colspan="3">
                                        <asp:RadioButtonList ID="rblDisability" TabIndex="10" runat="server" RepeatDirection="Horizontal">
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="4" class="privs-cell">
                                        <asp:CheckBoxList ID="cblPriv" TabIndex="11" runat="server" RepeatDirection="Horizontal" RepeatColumns="4">
                                        </asp:CheckBoxList>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Communities</td>
                                    <td colspan="3">
                                        <asp:CheckBoxList ID="cblCommunities" runat="server" RepeatDirection="Horizontal">
                                        </asp:CheckBoxList>
                                    </td>
                                </tr>
                                <%--<tr>
                                    <td>
                                        Email Groups
                                    </td>
                                    <td colspan="3">
                                    </td>
                                </tr>--%>
                                <tr>
                                    <td>Technical Interest</td>
                                    <td colspan="3">
                                        <asp:DropDownList ID="ddlTechnicalInterest" TabIndex="13" runat="server" Width="248px">
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td>
                        <!-- Physical Access -->
                        <asp:Repeater runat="server" ID="rptPhysicalAccess" OnItemDataBound="rptPhysicalAccess_ItemDataBound">
                            <ItemTemplate>
                                <div class="physical-access">
                                    <table border="1">
                                        <tbody>
                                            <tr>
                                                <th colspan="5">Badge</th>
                                            </tr>
                                            <tr>
                                                <th style="width: 75px;">Issued</th>
                                                <td colspan="4"><%#Eval("IssueDate", "{0:MM/dd/yyyy hh:mm:ss tt}")%></td>
                                            </tr>
                                            <tr>
                                                <th>Expiration</th>
                                                <td colspan="4"><%#Eval("ExpireDate", "{0:MM/dd/yyyy hh:mm:ss tt}")%></td>
                                            </tr>
                                            <tr>
                                                <th colspan="5">Cards</th>
                                            </tr>
                                            <asp:Repeater runat="server" ID="rptCards">
                                                <HeaderTemplate>
                                                    <tr>
                                                        <th style="text-align: center;">Number</th>
                                                        <th style="text-align: center;">Status</th>
                                                        <th style="text-align: center;">Issued</th>
                                                        <th style="text-align: center;">Expiration</th>
                                                        <th style="text-align: center;">Last Access</th>
                                                    </tr>
                                                </HeaderTemplate>
                                                <ItemTemplate>
                                                    <tr class="<%#Eval("Status").ToString().ToLower()%>">
                                                        <td style="text-align: center;"><%#Eval("Number")%></td>
                                                        <td style="text-align: center;"><%#Eval("Status")%></td>
                                                        <td style="text-align: center;"><%#Eval("CardIssueDate", "{0:MM/dd/yyyy hh:mm:ss tt}")%></td>
                                                        <td style="text-align: center;"><%#Eval("CardExpireDate", "{0:MM/dd/yyyy hh:mm:ss tt}")%></td>
                                                        <td style="text-align: center;"><%#Eval("LastAccess", "{0:MM/dd/yyyy hh:mm:ss tt}")%></td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                            <asp:PlaceHolder runat="server" ID="phCardsNoData" Visible="false">
                                                <tr>
                                                    <td colspan="5"><em style="color: #808080;">This user has no cards yet. You must add cards manually in Prowatch.</em></td>
                                                </tr>
                                            </asp:PlaceHolder>
                                        </tbody>
                                    </table>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:PlaceHolder runat="server" ID="phPhysicalAccessNoData" Visible="false">
                            <div class="physical-access">
                                <div class="no-data"><em>Physical access has not been configured for this client. You must click the "Save Changes and Exit" button on the next page for any pending changes to take affect.</em></div>
                            </div>
                        </asp:PlaceHolder>
                        <!-- Existing -->
                        <asp:Panel ID="pExisting" runat="server" Visible="False">
                            <table id="Table4" runat="server">
                                <tr>
                                    <td>
                                        <div style="font-weight: bold;">Organization Filter</div>
                                        <div style="margin-bottom: 10px;">
                                            <asp:DropDownList runat="server" ID="ddlOrg" TabIndex="1" Width="300" CssClass="GridText" AutoPostBack="true" OnSelectedIndexChanged="ddlOrg_SelectedIndexChanged">
                                            </asp:DropDownList>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <div style="font-weight: bold;">Name Filter</div>
                                        <div style="margin-bottom: 10px;">
                                            <asp:TextBox runat="server" ID="txtName" TabIndex="2" Width="300" CssClass="GridText" MaxLength="50" AutoPostBack="true" OnTextChanged="txtName_TextChanged"></asp:TextBox>
                                        </div>
                                    </td>
                                </tr>
                                <tr style="height: 40px">
                                    <td>
                                        <div style="font-weight: bold;">Client to Add to Organization</div>
                                        <div style="margin-bottom: 10px;">
                                            <asp:DropDownList runat="server" ID="ddlClient" TabIndex="3" Width="536" CssClass="DDLText" AutoPostBack="true" OnSelectedIndexChanged="ddlClient_SelectedIndexChanged">
                                            </asp:DropDownList>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td>
                                        <asp:Button runat="server" ID="btnExistingQuit" CssClass="QuitStoreButton" Text="Abandon Restore" CausesValidation="false" OnClick="btnExistingQuit_Click" />
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Panel ID="pClientOrg" runat="server" Visible="false">
                            <table id="Table5" runat="server">
                                <tr>
                                    <td colspan="4">
                                        <asp:Label ID="lblClientOrg" runat="server" CssClass="SectionHeader">for University of Michigan</asp:Label>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Department:</td>
                                    <td>
                                        <asp:DropDownList ID="ddlDepartment" TabIndex="20" runat="server" Width="190px">
                                        </asp:DropDownList>
                                    </td>
                                    <td>Role:</td>
                                    <td>
                                        <asp:DropDownList ID="ddlRole" TabIndex="21" runat="server" Width="190px">
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td>E-mail address:</td>
                                    <td>
                                        <asp:TextBox ID="txtEmail" TabIndex="22" runat="server" Width="184px" MaxLength="50" CssClass="email-textbox"></asp:TextBox>
                                    </td>
                                    <td>Phone number:</td>
                                    <td>
                                        <asp:TextBox ID="txtPhone" TabIndex="23" runat="server" Width="184px" MaxLength="25" CssClass="phone-textbox"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2">
                                        <asp:CheckBox ID="chkManager" TabIndex="24" runat="server" Text="Technical Manager"></asp:CheckBox>
                                        <asp:CheckBox ID="chkFinManager" runat="server" Text="Financial Manager" />
                                    </td>
                                    <td colspan="2"></td>
                                </tr>
                                <tr>
                                    <td colspan="4">
                                        <asp:DataGrid runat="server" ID="dgAddress" TabIndex="25" ShowHeader="True" CellPadding="2" ShowFooter="True" DataKeyField="AddressID" AutoGenerateColumns="False" AllowSorting="True" HeaderStyle-Wrap="false" HeaderStyle-Font-Bold="true" HeaderStyle-BackColor="LightGrey" AlternatingItemStyle-BackColor="Linen" GridLines="Horizontal" BorderWidth="1px" BorderStyle="Ridge" BorderColor="LightGray" OnItemCommand="dgAddress_ItemCommand" OnItemDataBound="dgAddress_ItemDataBound">
                                            <FooterStyle CssClass="GridText" BackColor="LightGray"></FooterStyle>
                                            <EditItemStyle CssClass="GridText" BackColor="#66FFFF"></EditItemStyle>
                                            <ItemStyle CssClass="GridText" />
                                            <AlternatingItemStyle CssClass="GridText" BackColor="Linen"></AlternatingItemStyle>
                                            <HeaderStyle Font-Bold="True" Wrap="False" CssClass="GridText" BackColor="LightGray"></HeaderStyle>
                                            <Columns>
                                                <asp:TemplateColumn HeaderText="Attention Line">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblInternalAddress" runat="server" Width="100px"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:TextBox ID="txtInternalAddressF" runat="server" Width="100px" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                                    </FooterTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtInternalAddress" runat="server" Width="100px" MaxLength="45"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateColumn>
                                                <asp:TemplateColumn HeaderText="Street Address, Line 1">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblStrAddress1" runat="server" Width="160px"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:TextBox ID="txtStrAddress1F" runat="server" Width="160px" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                                    </FooterTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtStrAddress1" runat="server" Width="160px" MaxLength="45"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateColumn>
                                                <asp:TemplateColumn HeaderText="Street Address, Line 2">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblStrAddress2" runat="server" Width="160px"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:TextBox ID="txtStrAddress2F" runat="server" Width="160px" Text="" MaxLength="45"></asp:TextBox>&nbsp;
                                                    </FooterTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtStrAddress2" runat="server" Width="160px" MaxLength="45"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateColumn>
                                                <asp:TemplateColumn HeaderText="City">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblCity" runat="server" Width="100px"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:TextBox ID="txtCityF" runat="server" Width="100px" Text="" MaxLength="35"></asp:TextBox>&nbsp;
                                                    </FooterTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtCity" runat="server" Width="100px" MaxLength="35"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateColumn>
                                                <asp:TemplateColumn HeaderText="State">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblState" runat="server" Width="50px"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:TextBox ID="txtStateF" runat="server" Width="50px" Text="" MaxLength="2"></asp:TextBox>&nbsp;
                                                    </FooterTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtState" runat="server" Width="50px" MaxLength="2"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateColumn>
                                                <asp:TemplateColumn HeaderText="Zip">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblZip" runat="server" Width="50px"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:TextBox ID="txtZipF" runat="server" Width="50px" Text="" MaxLength="10"></asp:TextBox>&nbsp;
                                                    </FooterTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtZip" runat="server" Width="50px" MaxLength="10"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateColumn>
                                                <asp:TemplateColumn HeaderText="Country">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblCountry" runat="server" Width="50px"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:TextBox ID="txtCountryF" runat="server" Width="50px" Text="" MaxLength="50"></asp:TextBox>&nbsp;
                                                    </FooterTemplate>
                                                    <EditItemTemplate>
                                                        <asp:TextBox ID="txtCountry" runat="server" Width="50px" MaxLength="50"></asp:TextBox>
                                                    </EditItemTemplate>
                                                </asp:TemplateColumn>
                                                <asp:TemplateColumn>
                                                    <ItemTemplate>
                                                        <asp:ImageButton runat="server" ID="btnAddrEdit" ImageUrl="images/im_edit.gif" AlternateText="" CommandName="Edit" CausesValidation="false" />
                                                        <asp:ImageButton runat="server" ID="btnAddrDelete" ImageUrl="images/im_delete.gif" AlternateText="" CommandName="Delete" CausesValidation="false" />
                                                    </ItemTemplate>
                                                    <FooterStyle HorizontalAlign="Center"></FooterStyle>
                                                    <FooterTemplate>
                                                        <asp:Button ID="btnAddRow" runat="server" Text="ADD" CommandName="AddNewRow" />
                                                    </FooterTemplate>
                                                    <EditItemTemplate>
                                                        <asp:ImageButton runat="server" ID="btnAddrUpdate" ImageUrl="images/im_update.gif" AlternateText="Update" CommandName="Update" />
                                                        <asp:ImageButton runat="server" ID="btnAddrCancel" ImageUrl="images/im_cancel.gif" AlternateText="Cancel" CommandName="Cancel" CausesValidation="false" />
                                                    </EditItemTemplate>
                                                </asp:TemplateColumn>
                                            </Columns>
                                        </asp:DataGrid>
                                    </td>
                                </tr>
                                <tr>
                                    <td>Manager(s):</td>
                                    <td colspan="3">
                                        <asp:DataGrid runat="server" ID="dgClientManager" CellPadding="2" ShowFooter="true" DataKeyField="ManagerOrgID" AutoGenerateColumns="false" GridLines="Horizontal" BorderWidth="1" BorderStyle="Ridge" BorderColor="LightGray" AllowPaging="false" OnItemCommand="dgClientManager_ItemCommand" OnItemDataBound="dgClientManager_ItemDataBound" OnSortCommand="dgClient_SortCommand">
                                            <FooterStyle CssClass="GridText" BackColor="LightGray" />
                                            <EditItemStyle CssClass="GridText" BackColor="#66FFFF" />
                                            <ItemStyle CssClass="GridText" />
                                            <AlternatingItemStyle CssClass="GridText" BackColor="Linen"></AlternatingItemStyle>
                                            <HeaderStyle Font-Bold="True" Wrap="False" CssClass="GridText" BackColor="LightGray"></HeaderStyle>
                                            <Columns>
                                                <asp:TemplateColumn SortExpression="DisplayName" HeaderText="Manager">
                                                    <ItemStyle Width="200px"></ItemStyle>
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
                                                    <FooterStyle HorizontalAlign="Center" />
                                                    <ItemTemplate>
                                                        <asp:ImageButton runat="server" ID="btnMgrDelete" ImageUrl="~/images/im_delete.gif" AlternateText="delete" CommandName="Delete" />
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:Button ID="btnMgrAdd" runat="server" Text="Add New" CommandName="AddNew" CssClass="CmdButton" />
                                                    </FooterTemplate>
                                                </asp:TemplateColumn>
                                            </Columns>
                                        </asp:DataGrid>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="height: 30px">Account:</td>
                                    <td style="height: 30px" colspan="3">
                                        <asp:DropDownList runat="server" ID="ddlAccount" TabIndex="27" Width="384" AutoPostBack="true" OnSelectedIndexChanged="ddlAccount_SelectedIndexChanged" DataTextField="Text" DataValueField="Value">
                                        </asp:DropDownList>
                                        <div>
                                            <asp:RadioButtonList runat="server" ID="rblAcctDisplay" RepeatDirection="Horizontal" AutoPostBack="true" TextAlign="Left" OnSelectedIndexChanged="rblAcctDisplay_SelectedIndexChanged">
                                                <asp:ListItem Value="Name" Selected="True">Name</asp:ListItem>
                                                <asp:ListItem Value="Number">Number</asp:ListItem>
                                                <asp:ListItem Value="Project">Project</asp:ListItem>
                                                <asp:ListItem Value="ShortCode">Short Code</asp:ListItem>
                                            </asp:RadioButtonList>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="4">Subsidy Starting Date:
                                        <lnf:PeriodPicker runat="server" ID="pp1" StartPeriod="1/1/2009" />
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="4">New Faculty Starting Date:
                                        <lnf:PeriodPicker runat="server" ID="pp2" StartPeriod="1/1/2007" />
                                        &rarr; ONLY applies to UM + Executive
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="4">Billing Types:<br />
                                        <asp:RadioButtonList runat="server" ID="rdolistBillingType" RepeatDirection="Horizontal" RepeatColumns="4" DataTextField="BillingTypeName" DataValueField="BillingTypeID" DataSourceID="odsBillingType" OnDataBound="rdolistBillingType_DataBound">
                                        </asp:RadioButtonList>
                                        <asp:ObjectDataSource ID="odsBillingType" runat="server" TypeName="sselData.AppCode.DAL.BillingTypeDA" SelectMethod="GetAllBillingTypes"></asp:ObjectDataSource>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="text-align: left;" colspan="4">
                                        <asp:Button runat="server" ID="btnClientSave" TabIndex="28" CssClass="StoreButton" Text="Store New client" CausesValidation="false" OnClick="btnClientSave_Click" />
                                        <asp:Button runat="server" ID="btnClientQuit" CssClass="QuitStoreButton" Text="Abandon Changes" CausesValidation="false" OnClick="btnClientQuit_Click" />
                                    </td>
                                </tr>
                            </table>
                            <div class="error" style="padding: 5px;">
                                <asp:Literal runat="server" ID="litClientSaveError"></asp:Literal>
                            </div>
                        </asp:Panel>
                    </td>
                </tr>
                <tr>
                    <td>
                        <!-- Client List -->
                        <asp:Panel ID="pClientList" runat="server">
                            <table id="Table3" style="width: 300px;" border="1">
                                <tr>
                                    <td colspan="2">
                                        <asp:DataGrid runat="server" ID="dgClient" CellPadding="2" ShowFooter="true" DataKeyField="ClientID" AutoGenerateColumns="false" AllowSorting="true" GridLines="Horizontal" BorderWidth="1" BorderStyle="Ridge" BorderColor="LightGray" AllowPaging="true" PageSize="15" Width="100%" OnItemCommand="dgClient_ItemCommand" OnItemDataBound="dgClient_ItemDataBound">
                                            <FooterStyle CssClass="GridText" BackColor="LightGray" />
                                            <EditItemStyle CssClass="GridText" BackColor="#66FFFF" />
                                            <ItemStyle CssClass="GridText" />
                                            <AlternatingItemStyle CssClass="GridText" BackColor="Linen" />
                                            <HeaderStyle Font-Bold="True" Wrap="False" CssClass="GridText" BackColor="LightGray" />
                                            <PagerStyle Visible="False" HorizontalAlign="Right" Mode="NumericPages" />
                                            <Columns>
                                                <asp:TemplateColumn SortExpression="DisplayName" HeaderText="Client Name">
                                                    <ItemTemplate>
                                                        <asp:Label ID="lblClientName" runat="server"></asp:Label>
                                                    </ItemTemplate>
                                                    <FooterTemplate>
                                                        <asp:Button ID="btnAddExisting" runat="server" Text="Add Existing" CommandName="AddExisting" CssClass="CmdButton" />
                                                    </FooterTemplate>
                                                </asp:TemplateColumn>
                                                <asp:TemplateColumn>
                                                    <ItemStyle Width="65" HorizontalAlign="Left" />
                                                    <FooterStyle HorizontalAlign="Right" />
                                                    <ItemTemplate>
                                                        <asp:ImageButton runat="server" ID="btnEdit" ImageUrl="~/images/im_edit.gif" AlternateText="edit" CommandName="Edit" CausesValidation="false" />
                                                        <asp:ImageButton runat="server" ID="btnDelete" ImageUrl="~/images/im_delete.gif" AlternateText="delete" CommandName="Delete" CausesValidation="false" />
                                                        <asp:Literal runat="server" ID="litDryBoxMessage"></asp:Literal>
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
                                    <td style="height: 18px" colspan="2">Select page:&nbsp;
                                        <asp:DropDownList runat="server" ID="ddlPager" Width="200" AutoPostBack="true" OnSelectedIndexChanged="ddlPager_SelectedIndexChanged">
                                        </asp:DropDownList>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan="2" style="white-space: nowrap;">
                                        <asp:Button runat="server" ID="btnSave" Height="32" Width="220" CssClass="SaveButton" Text="Save Changes and Exit" CausesValidation="false" OnClick="btnSave_Click" />
                                        <asp:Button runat="server" ID="btnDiscard" Height="32" Width="220" CssClass="QuitButton" Text="Discard Changes and Quit" CausesValidation="false" OnClick="btnDiscard_Click" />
                                    </td>
                                </tr>
                            </table>
                        </asp:Panel>
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </div>
    <asp:Panel runat="server" ID="panDialogDuplicateName" CssClass="client-dialog" Width="600" Visible="false">
        <div class="dialog-message">
            <asp:Literal runat="server" ID="litDuplicateNameMsg"></asp:Literal>
        </div>
        <div class="dialog-buttons">
            <asp:Button runat="server" ID="btnDuplicateNameYes" Text="Yes" Width="60" OnCommand="DialogDuplicateName_Command" CommandName="yes" />
            <asp:Button runat="server" ID="btnDuplicateNameNo" Text="No" Width="60" OnCommand="DialogDuplicateName_Command" CommandName="no" />
        </div>
    </asp:Panel>
    <asp:Panel runat="server" ID="panDialogError" CssClass="client-dialog" Width="600" Visible="false">
        <div class="dialog-message">
            <asp:Literal runat="server" ID="litError"></asp:Literal>
        </div>
        <div class="dialog-buttons">
            <asp:Button runat="server" ID="btnErrorClient" Text="Return to Client Data Entry" Width="200" OnCommand="DialogError_Command" CommandName="client" />
            <asp:Button runat="server" ID="btnErrorHome" Text="Return to Data Entry Home" Width="200" OnCommand="DialogError_Command" CommandName="home" />
        </div>
    </asp:Panel>
</asp:Content>

<asp:Content runat="server" ID="Content3" ContentPlaceHolderID="scripts">
    <script src="scripts/jquery.uds.js"></script>
    <script>
        $('.edit-client-form').uds().find('.StoreButton').click(function (event) {
            var hid = !$('.privs-cell').is(':visible');
            var gtz = $('.privs-cell input[type="checkbox"]:checked').length > 0;

            var abort = true;

            if (hid || gtz)
                abort = false;
            else
                alert('You must enter at least one privilege.');

            if (abort)
                event.preventDefault();
        });
    </script>
</asp:Content>
