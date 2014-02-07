<%@ Page Language="C#" AutoEventWireup="true" CodeFile="UninstallPackage.aspx.cs" Inherits="sitecore_admin_PackageHistory_UninstallPackage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="content-type" content="text/html; charset=utf-8" />
    <title>Uninstall Package</title>
    <!-- Latest compiled and minified CSS -->
    <link rel="stylesheet" href="//netdna.bootstrapcdn.com/bootstrap/3.1.0/css/bootstrap.min.css">
    <script type="text/javascript" charset="utf-8" language="javascript" src="Resources/jquery.js"></script>
    <!-- Latest compiled and minified JavaScript -->
    <script src="//netdna.bootstrapcdn.com/bootstrap/3.1.0/js/bootstrap.min.js"></script>
    <script src="Resources/bootbox.min.js"></script>
    <script src="Resources/jquery.bootstrap.wizard.min.js"></script>
    <script src="Resources/PackageHistory.js"></script>
</head>
<body>
    <div class="panel panel-default">
        <div class="panel-heading">
            <h3 class="panel-title">Uninstall Package</h3>
        </div>
        <div class="panel-body">
            <form id="frmUninstall" runat="server" class="form-horizontal" role="form">
                <div id="divMessage" class="alert alert-danger alert-dismissable" runat="server" visible="false">
                    <button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>
                    <strong>
                        <asp:Label ID="lblMessage" runat="server" />
                    </strong>
                </div>



                <%--Wizard Start--%>
                <div id="rootwizard">
                    <div class="navbar">
                        <div class="navbar-inner">
                            <div class="container">
                                <ul>
                                    <li><a href="#tab1" data-toggle="tab">Step 1 : Package Info</a></li>
                                    <li><a href="#tab2" data-toggle="tab">Step 2 : Package Uninstall</a></li>
                                </ul>
                            </div>
                        </div>
                    </div>
                    <div class="tab-content">
                        <div class="tab-pane" id="tab1">

                            <div>
                                <div class="form-group">
                                    <label class="col-xs-2 control-label">Name</label>
                                    <div class="col-xs-10">
                                        <asp:Literal ID="litPackageName" runat="server" />
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-xs-2 control-label">Version</label>
                                    <div class="col-xs-10">

                                        <asp:Literal ID="litPackageVersion" runat="server" />

                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-xs-2 control-label">FileName</label>
                                    <div class="col-xs-10">
                                        <%--<input class="form-control" id="focusedInput" type="text" value="Yourpackagename.zip">--%>
                                        <asp:TextBox ID="txtPackageName" runat="server" CssClass="form-control"></asp:TextBox>
                                        <span class="help-block">Please provide package zip file name (e.g. Quick_Item_Search_6.zip), and make sure this package is availble under your packages folder.</span>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <div class="col-xs-offset-2 col-xs-10">
                                        
                                        <asp:Button ID="btnViewDetails" runat="server" CssClass="btn btn-default" OnClick="btnViewDetails_Click" Text="View Details" />
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="tab-pane" id="tab2">
                            <%--TABLE--%>
                            <asp:Panel ID="pnlDetails" runat="server" Visible="false">

                                <asp:Repeater ID="rptProjectInformation" runat="server">
                                    <HeaderTemplate>
                                        <table class="table table-bordered">
                                            <tr>
                                                <th>Database
                                                </th>
                                                <th>Item Path/File Path
                                                </th>
                                            </tr>
                                    </HeaderTemplate>
                                    <ItemTemplate>
                                        <tr class="<%# ((bool)Eval("IsSensitive")) ? "danger" : ""%>"
                                            title='<%# ((bool)Eval("IsSensitive")) ? "Deleting a file may cause ASP.NET Restart. So, we are not going to touch it. You have to delete it manually." : ""%>'>
                                            <td>
                                                <asp:HiddenField ID="hfTypeOfItem" runat="server" Value='<%# Eval("TypeOfItem") %>' />
                                                <asp:Label ID="lblDatabaseName" runat="server" Text='<%# Eval("DatabaseName") %>' />
                                            </td>
                                            <td>
                                                <asp:Label ID="lblItemOrFilePath" runat="server" Text='<%# Eval("ItemOrFilePath") %>' />

                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    <FooterTemplate>
                                        </table>
                                    </FooterTemplate>
                                </asp:Repeater>

                                <div class="alert alert-warning">
                                    Clicking on Uninstall will recycle all package related database items from their respective database (That's why please review above given list cautiously).
                                     As deleting file may cause ASP.NET pool recycle, this tool won’t touch them.
                                     And if you wish you can delete them manually. Are you sure you would like to uninstall?
                                </div>
                                <%--<button type="button" class="btn btn-primary btn-lg btn-danger">Uninstall</button>--%>
                                <asp:Button ID="btnUninstall" runat="server" Text="Uninstall" CssClass="btn btn-primary btn-lg btn-danger"
                                    OnClientClick="return ShowConfirm(this.id);"
                                    OnClick="btnUninstall_Click" />

                            </asp:Panel>
                        </div>
                    </div>
                </div>

                <%--Wizard End--%>
            </form>
        </div>
    </div>
</body>
</html>
