<%@ Page Language="C#" AutoEventWireup="true"
    Inherits="Web.sitecore.admin.PackageHistory" CodeFile="PackageHistory.aspx.cs" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <meta http-equiv="content-type" content="text/html; charset=utf-8" />
    <title>Sitecore Package History</title>
    <link rel="stylesheet" type="text/css" href="//cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/2.3.2/css/bootstrap.min.css" />

    <link rel="stylesheet" type="text/css" href="//cdnjs.cloudflare.com/ajax/libs/bootstrap-modal/2.1.0/bootstrap-modal.min.css" />

    <link rel="stylesheet" type="text/css" href="Resources/DT_bootstrap.css">
    <script type="text/javascript" charset="utf-8" language="javascript" src="Resources/jquery.js"></script>
    <script type="text/javascript" charset="utf-8" language="javascript" src="Resources/jquery.dataTables.js"></script>
    <script type="text/javascript" charset="utf-8" language="javascript" src="Resources/DT_bootstrap.js"></script>
    <script type="text/javascript" charset="utf-8" language="javascript" src="Resources/PackageHistory.js"></script>
    <script type="text/javascript" src="//cdnjs.cloudflare.com/ajax/libs/bootstrap-modal/2.1.0/bootstrap-modal.pack.min.js"></script>
    <style type="text/css">
        body .modal
        {
            /* new custom width */
            width: 750px;
            /* must be half of the width, minus scrollbar on the left (30px) */
            margin-left: -375px;
            overflow-y: auto;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container" style="margin-top: 10px">

            <center>                
            <asp:Label ID="lblMessage" runat="server" Text="" CssClass="text-info" Font-Bold="false" Visible="false"/>
                </center>

            <asp:Table ID="tblPackageDetails"
                CellPadding="0" CellSpacing="0" runat="server" CssClass="table table-striped table-bordered" Visible="false">
                <asp:TableHeaderRow TableSection="TableHeader">
                    <asp:TableHeaderCell ID="thPackageName">Name</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="thPackageVersion">Version</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="thPackageAuthor">Author</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="thPackagePublisher">Publisher</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="thPackageReadMe">Readme</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="thPackageRevision">Revision</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="thPackageInstalledBy">Installed by</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="thPackageCreatedDate">Installed on</asp:TableHeaderCell>
                    <asp:TableHeaderCell ID="thPackageUnInstall">Uninstall &nbsp;<sup><span class="label label-warning">BETA</span></sup></asp:TableHeaderCell>
                </asp:TableHeaderRow>
            </asp:Table>

            <!-- Modal -->
            <div id="myModal" class="modal hide fade" tabindex="-1" role="dialog">

                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">×</button>
                    <h5>Uninstall Package Dialog &nbsp; <sup><span class="label label-warning">BETA</span></sup></h5>
                </div>
                <div class="modal-body">
                    <iframe src="" width="700" height="410" frameborder="0" allowtransparency="true"></iframe>
                </div>
                <div class="modal-footer">
                    <button class="btn" data-dismiss="modal">Close</button>
                </div>
            </div>
    </form>
</body>
</html>
