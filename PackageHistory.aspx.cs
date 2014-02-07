using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Web.sitecore.admin
{
    public partial class PackageHistory : Sitecore.sitecore.admin.AdminPage
    {
        private const string PACKAGE_INSTALLATION_HISTORY_PATH = "/sitecore/system/Packages/Installation history";
        private const string PACKAGE_REGISTRATION_TEMPLATE = "/sitecore/templates/System/Packages/Package registration";
        private const string PACKAGE_REGISTRATION_TEMPLATE_ID = "{22A11D20-5F1D-4216-BF3F-18C016F1F98E}";

        protected override void OnInit(EventArgs e)
        {
            base.CheckSecurity(true);
            base.OnInit(e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    LoadPackagesHistory();
                }
            }
            catch (Exception ex)
            {
                Sitecore.Diagnostics.Log.Error("Error while loading packages history", ex, this);
                ShowMessage(ex.Message, "Error");
            }

        }

        /// <summary>
        ///  This method will be used to load packages
        ///  history from it's source
        /// </summary>
        private void LoadPackagesHistory()
        {

            // Load Package
            Database coreDatabase = Sitecore.Configuration.Factory.GetDatabase("core");

            Item installationHistoryItem = coreDatabase.GetItem(PACKAGE_INSTALLATION_HISTORY_PATH);

            if (installationHistoryItem == null)
            {
                ShowMessage("No package installation history found, it seems that you haven't installed any package yet!", "Error");
                lblMessage.Visible = true;
                tblPackageDetails.Visible = false;
            }
            else
            {
                Item[] installationItems = installationHistoryItem.Axes.GetDescendants();

                foreach (Item aItem in installationItems)
                {
                    // We need to ensure that we only show package registration type template 
                    if (aItem != null && aItem.Template.ID.ToString()
                        == PACKAGE_REGISTRATION_TEMPLATE_ID)
                    {
                        TableRow tableRow = new TableRow();
                        tableRow.ID = "row" + aItem.ID.ToShortID().ToString();
                        tableRow.TableSection = TableRowSection.TableBody;

                        
                        // Package name
                        AddTableCell(tableRow, aItem.Fields["Package name"], FieldTypes.Text);

                        // Package version
                        AddTableCell(tableRow, aItem.Fields["Package version"], FieldTypes.Text);

                        // Package author
                        AddTableCell(tableRow, aItem.Fields["Package author"], FieldTypes.Text);

                        // Package publisher
                        AddTableCell(tableRow, aItem.Fields["Package publisher"], FieldTypes.Text);

                        // Package readme
                        AddTableCell(tableRow, aItem.Fields["Package readme"], FieldTypes.Text);

                        // Package revision
                        AddTableCell(tableRow, aItem.Fields["Package revision"], FieldTypes.Text);

                        // Package Installed By
                        AddTableCell(tableRow, aItem.Fields[Sitecore.FieldIDs.CreatedBy], FieldTypes.Text);

                        // Package Installed Date
                        AddTableCell(tableRow, aItem.Fields[Sitecore.FieldIDs.Created], FieldTypes.DateTime);

                        // Package UnInstall Link -- thPackageUnInstall                       

                        TableCell tableCell1 = new TableCell();                        
                        string valueToPrint = string.Format("<a href='#' class='btn btn-default btn-lg' role='button' onclick=\"ShowUninstallDialog('{0}');\">Uninstall (BETA)</a>",
                            aItem.ID.ToString());
                        
                        tableCell1.Text = valueToPrint;
                        tableRow.Cells.Add(tableCell1);

                        tblPackageDetails.Rows.Add(tableRow);
                    }

                }

                lblMessage.Visible = false;
                tblPackageDetails.Visible = true;

            }

        }

        /// <summary>
        /// This function will be used to add
        /// table cell
        /// </summary>
        /// <param name="tableRow">Table Row</param>
        /// <param name="aField">Field</param>
        /// <param name="fieldType">Type of field</param>
        /// <returns></returns>
        private static TableCell AddTableCell(TableRow tableRow, Sitecore.Data.Fields.Field aField, FieldTypes fieldType)
        {
            TableCell tableCell1 = new TableCell();

            string valueToPrint = "NA";

            switch (fieldType)
            {
                case FieldTypes.DateTime:
                    if ((aField != null) && !string.IsNullOrEmpty(aField.Value))
                    {
                        string dateFormat = "r";
                        DateTime createdDate = DateTime.Now;
                        createdDate = Sitecore.DateUtil.IsoDateToDateTime(aField.Value);
                        valueToPrint = createdDate.ToString(dateFormat);
                    }
                    else
                    {
                        valueToPrint = "NA";
                    }
                    break;
                case FieldTypes.Text:
                    valueToPrint = ((aField != null) && !string.IsNullOrEmpty(aField.Value)) ? aField.Value : "NA";
                    break;
                default:
                    valueToPrint = ((aField != null) && !string.IsNullOrEmpty(aField.Value)) ? aField.Value : "NA";
                    break;
            }

            tableCell1.Text = valueToPrint;
            tableRow.Cells.Add(tableCell1);
            return tableCell1;


        }

        /// <summary>
        /// This function will be used
        /// to show message
        /// </summary>
        /// <param name="message">Message to show</param>
        /// <param name="messageType">Message type to show</param>
        private void ShowMessage(string message, string messageType)
        {
            lblMessage.Visible = true;
            lblMessage.Text = message;
            if (messageType == "Error")
            {
                lblMessage.CssClass = "text-danger";
                
            }
            else
            {
                lblMessage.CssClass = "text-success";
            }
        }

    }

    enum FieldTypes
    {
        DateTime,
        Text

    }
}