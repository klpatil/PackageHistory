using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Serialization;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Install;
using Sitecore.Zip;


public partial class sitecore_admin_PackageHistory_UninstallPackage : Sitecore.sitecore.admin.AdminPage
{

    private const string PACKAGEID = "PKGID";

    protected override void OnInit(EventArgs e)
    {
        base.CheckSecurity(true);
        base.OnInit(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(Request.QueryString[PACKAGEID]))
                {
                    ShowMessage("Please select the package to uninstall", "Error");

                }
                else
                {
                    LoadPackageInformation();
                }
            }
        }
        catch (Exception ex)
        {
            Sitecore.Diagnostics.Log.Error("An error occurred while loading package details", ex, this);
            ShowMessage("An error occurred while loading package details : " + ex.Message, "Error");
        }
    }

    private void LoadPackageInformation()
    {
        // Extract user given packagename and just validate it for simplicity!

        Database coreDatabase = Sitecore.Configuration.Factory.GetDatabase("core");

        Item selectedPacakgeItem = coreDatabase.GetItem(Sitecore.Data.ID.Parse(Request.QueryString[PACKAGEID]));

        if (selectedPacakgeItem == null)
        {
            ShowMessage("No package item found, it seems that it has been already deleted!", "Error");
        }
        else
        {
            litPackageName.Text = string.IsNullOrEmpty(selectedPacakgeItem["Package name"]) ? "NA" : selectedPacakgeItem["Package name"];
            litPackageVersion.Text = string.IsNullOrEmpty(selectedPacakgeItem["Package version"]) ? "NA" : selectedPacakgeItem["Package version"];

        }
    }


    /// <summary>
    /// This function will be used
    /// to show message
    /// </summary>
    /// <param name="message">Message to show</param>
    /// <param name="messageType">Message type to show</param>
    private void ShowMessage(string message, string messageType)
    {
        lblMessage.Text = message;
        divMessage.Visible = true;
        if (messageType == "Error")
        {
            lblMessage.CssClass = "text-danger";
            divMessage.Attributes["class"] = "alert alert-danger alert-dismissable";

        }
        else
        {
            lblMessage.CssClass = "text-success";
            divMessage.Attributes["class"] = "alert alert-success alert-dismissable";

        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="packageFileName"></param>
    /// <returns></returns>
    private project ExtractPackageAndGetProjectInformation(string packageFileName)
    {
        project projectInformation = null;
        ZipReader reader = null;
        string tempFileName = string.Empty;
        try
        {
            reader = new ZipReader(packageFileName, Encoding.UTF8);
            tempFileName = Path.GetTempFileName();
            ZipEntry entry = reader.GetEntry("package.zip");
            if (entry != null)
            {
                using (FileStream stream = File.Create(tempFileName))
                {
                    StreamUtil.Copy(entry.GetStream(), stream, 0x4000);
                }
                reader.Dispose();
                reader = new ZipReader(tempFileName, Encoding.UTF8);
            }

            // Get Project XML File
            entry = reader.GetEntry("installer/project");

            using (MemoryStream memoryStream = new MemoryStream())
            {
                StreamUtil.Copy(entry.GetStream(), memoryStream, 0x4000);

                // Let's go to start of stream
                memoryStream.Seek(0, SeekOrigin.Begin);


                // Convert XML to class

                // Package can contain list of 
                // Items
                // Files
                XmlSerializer serializer = new XmlSerializer(typeof(project));
                projectInformation = (project)serializer.Deserialize(memoryStream);

            }

        }
        catch (Exception ex)
        {
            projectInformation = null;
            Sitecore.Diagnostics.Log.Error("While extracing package and getting information something went wrong", ex, this);
        }
        finally
        {
            if (reader != null)
                reader.Dispose();
            if (!string.IsNullOrEmpty(tempFileName))
                File.Delete(tempFileName);
        }

        return projectInformation;
    }

    protected void btnViewDetails_Click(object sender, EventArgs e)
    {
        try
        {
            divMessage.Visible = false;
            ViewPackageDetails();
        }
        catch (Exception ex)
        {
            Sitecore.Diagnostics.Log.Error("An error occurred while viewing package details", ex, this);
            ShowMessage("An error occurred while viewing package details : " + ex.Message, "Error");
        }

    }

    private void ViewPackageDetails()
    {
        if (string.IsNullOrEmpty(txtPackageName.Text.Trim()))
        {
            ShowMessage("Package FileName is required.", "Error");
        }
        else
        {
            project projectInforation =
                ExtractPackageAndGetProjectInformation(Sitecore.Configuration.Settings.PackagePath + "\\" + txtPackageName.Text.Trim());
            if (projectInforation == null)
            {
                ShowMessage("Sorry, we are unable to extract package's project information. It might happen when package is corrupted or unavilable. Please try again later.", "Error");
            }
            else
            {

                string packageName = projectInforation.Metadata[0].PackageName;
                string packageVersion = projectInforation.Metadata[0].Version;

                if (packageName != litPackageName.Text && packageVersion != litPackageVersion.Text)
                {
                    ShowMessage("Provided package file is not of selected module. Please verify and try again later.", "Error");
                }
                else
                {
                    List<PackageDetailInfo> packageDetails = new List<PackageDetailInfo>();
                    PackageDetailInfo packageDetailInfo = null;


                    // Loop through items from ZIP file
                    foreach (EntriesXitem aItem in projectInforation.Sources[0].xitems[0].Entries)
                    {

                        string rawItemValue = aItem.Value;

                        // Sample value : /core/sitecore/content/Applications/Content Editor/Ribbons/Chunks/QuickItemSearchChunk/{404C9DC5-4FBB-41C1-BA3D-2846A5397183}/invariant/0
                        string database = rawItemValue.Substring(1, rawItemValue.IndexOf("/sitecore") - 1);

                        string itemPath = rawItemValue.Substring(rawItemValue.IndexOf("/sitecore"), rawItemValue.LastIndexOf("/{") - 4);

                        packageDetailInfo = new PackageDetailInfo();

                        // DATABASE
                        packageDetailInfo.DatabaseName = database;

                        // ITEMPATH
                        packageDetailInfo.ItemOrFilePath = itemPath;

                        packageDetailInfo.TypeOfItem = ItemType.Item;

                        packageDetails.Add(packageDetailInfo);


                    }

                    // Loop through files -- Just for display purpose as 
                    // We are not going to touch files
                    foreach (EntriesXitem aItem in projectInforation.Sources[0].xfiles[0].Entries)
                    {

                        packageDetailInfo = new PackageDetailInfo();

                        string rawFileValue = aItem.Value;

                        // DATABASE
                        packageDetailInfo.DatabaseName = "NA";

                        // FILEPATH
                        packageDetailInfo.ItemOrFilePath = rawFileValue;
                        if (rawFileValue.ToLower().Contains(".bin") ||
                            rawFileValue.ToLower().Contains("App_") ||
                            rawFileValue.ToLower().EndsWith(".dll") ||
                            rawFileValue.ToLower().EndsWith(".config") ||
                            rawFileValue.ToLower().EndsWith(".asax"))
                        {
                            // http://fuchangmiao.blogspot.in/2007/11/aspnet-application-restarts.html

                            packageDetailInfo.IsSensitive = true;


                        }

                        rptProjectInformation.DataSource = packageDetails;
                        rptProjectInformation.DataBind();

                        packageDetailInfo.TypeOfItem = ItemType.File;

                        packageDetails.Add(packageDetailInfo);

                    }


                    Page.ClientScript.RegisterStartupScript(this.GetType(), "next",
                        "$('#rootwizard').find(\"a[href*='tab2']\").trigger('click');", true);
                    pnlDetails.Visible = true;


                }
            }
        }
    }

    protected void btnUninstall_Click(object sender, EventArgs e)
    {
        try
        {
            divMessage.Visible = false;


            // Loop through table -- Don't touch files
            // Delete items one by one
            foreach (RepeaterItem rptItem in rptProjectInformation.Items)
            {
                if (rptItem.ItemType == ListItemType.Item || rptItem.ItemType == ListItemType.AlternatingItem)
                {
                    // Check whether it's file or not -- If item then only delete
                    ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType),
                        ((HiddenField)rptItem.FindControl("hfTypeOfItem")).Value, true);
                    if (itemType != null && itemType == ItemType.Item)
                    {

                        string database = ((Label)rptItem.FindControl("lblDatabaseName")).Text;
                        string itemPath = ((Label)rptItem.FindControl("lblItemOrFilePath")).Text;
                        // GET ITEM from It's DB
                        Item itemToDelete = Sitecore.Data.Database.GetDatabase(database).GetItem(itemPath);
                        if (itemToDelete != null)
                        {
                            // DELETE??
                            itemToDelete.Recycle();
                            // Audit Entry
                            Sitecore.Diagnostics.Log.Audit("Item has been recycled successfully using Uninstall package. Item Path : " + itemPath, this);
                        }
                        else
                        {
                            Sitecore.Diagnostics.Log.Info("Item is not found while locating using Uninstall package. Item ID : " + itemPath, this);
                        }

                    }
                }


            }

            pnlDetails.Visible = false;
            ShowMessage("Package Uninstalled successfully!", "Message");
        }
        catch (Exception ex)
        {
            Sitecore.Diagnostics.Log.Error("An error occurred while viewing package details", ex, this);
            ShowMessage("An error occurred while uninstalling package : " + ex.Message, "Error");
        }

    }
}


public class PackageDetailInfo
{
    public string DatabaseName { get; set; }
    public string ItemOrFilePath { get; set; }
    public bool IsSensitive { get; set; }
    public ItemType TypeOfItem { get; set; }
}

public enum ItemType
{
    Item,
    File
}


//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.17929.
// 

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class Entries
{

    private EntriesXitem[] xitemField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("x-item", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
    public EntriesXitem[] xitem
    {
        get
        {
            return this.xitemField;
        }
        set
        {
            this.xitemField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class EntriesXitem
{

    private string valueField;

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public string Value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class Converter
{

    private ConverterItemToEntryConverter[] itemToEntryConverterField;

    private ConverterFileToEntryConverter[] fileToEntryConverterField;

    private ConverterTrivialConverter[] trivialConverterField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ItemToEntryConverter", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ConverterItemToEntryConverter[] ItemToEntryConverter
    {
        get
        {
            return this.itemToEntryConverterField;
        }
        set
        {
            this.itemToEntryConverterField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("FileToEntryConverter", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ConverterFileToEntryConverter[] FileToEntryConverter
    {
        get
        {
            return this.fileToEntryConverterField;
        }
        set
        {
            this.fileToEntryConverterField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("TrivialConverter", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public ConverterTrivialConverter[] TrivialConverter
    {
        get
        {
            return this.trivialConverterField;
        }
        set
        {
            this.trivialConverterField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ConverterItemToEntryConverter
{

    private string transformsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Transforms
    {
        get
        {
            return this.transformsField;
        }
        set
        {
            this.transformsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ConverterFileToEntryConverter
{

    private string rootField;

    private string transformsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Root
    {
        get
        {
            return this.rootField;
        }
        set
        {
            this.rootField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Transforms
    {
        get
        {
            return this.transformsField;
        }
        set
        {
            this.transformsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ConverterTrivialConverter
{

    private string transformsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Transforms
    {
        get
        {
            return this.transformsField;
        }
        set
        {
            this.transformsField = value;
        }
    }
}


[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class project
{

    private string saveProjectField;

    private string includeField;

    private string excludeField;

    private string nameField;

    private projectMetadataMetadata[] metadataField;

    private projectSources[] sourcesField;

    private Converter[] converterField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string SaveProject
    {
        get
        {
            return this.saveProjectField;
        }
        set
        {
            this.saveProjectField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Include
    {
        get
        {
            return this.includeField;
        }
        set
        {
            this.includeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Exclude
    {
        get
        {
            return this.excludeField;
        }
        set
        {
            this.excludeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    [System.Xml.Serialization.XmlArrayItemAttribute("metadata", typeof(projectMetadataMetadata), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    public projectMetadataMetadata[] Metadata
    {
        get
        {
            return this.metadataField;
        }
        set
        {
            this.metadataField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Sources", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public projectSources[] Sources
    {
        get
        {
            return this.sourcesField;
        }
        set
        {
            this.sourcesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Converter")]
    public Converter[] Converter
    {
        get
        {
            return this.converterField;
        }
        set
        {
            this.converterField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class projectMetadataMetadata
{

    private string packageNameField;

    private string authorField;

    private string versionField;

    private string revisionField;

    private string licenseField;

    private string commentField;

    private string attributesField;

    private string readmeField;

    private string publisherField;

    private string postStepField;

    private string packageIDField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string PackageName
    {
        get
        {
            return this.packageNameField;
        }
        set
        {
            this.packageNameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Author
    {
        get
        {
            return this.authorField;
        }
        set
        {
            this.authorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Revision
    {
        get
        {
            return this.revisionField;
        }
        set
        {
            this.revisionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string License
    {
        get
        {
            return this.licenseField;
        }
        set
        {
            this.licenseField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Comment
    {
        get
        {
            return this.commentField;
        }
        set
        {
            this.commentField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Attributes
    {
        get
        {
            return this.attributesField;
        }
        set
        {
            this.attributesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Readme
    {
        get
        {
            return this.readmeField;
        }
        set
        {
            this.readmeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Publisher
    {
        get
        {
            return this.publisherField;
        }
        set
        {
            this.publisherField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string PostStep
    {
        get
        {
            return this.postStepField;
        }
        set
        {
            this.postStepField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string PackageID
    {
        get
        {
            return this.packageIDField;
        }
        set
        {
            this.packageIDField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class projectSources
{

    private projectSourcesXitems[] xitemsField;

    private projectSourcesXfiles[] xfilesField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("xitems", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public projectSourcesXitems[] xitems
    {
        get
        {
            return this.xitemsField;
        }
        set
        {
            this.xitemsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("xfiles", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public projectSourcesXfiles[] xfiles
    {
        get
        {
            return this.xfilesField;
        }
        set
        {
            this.xfilesField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class projectSourcesXitems
{

    private string skipVersionsField;

    private string includeField;

    private string excludeField;

    private string nameField;

    private EntriesXitem[] entriesField;

    private Converter[] converterField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string SkipVersions
    {
        get
        {
            return this.skipVersionsField;
        }
        set
        {
            this.skipVersionsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Include
    {
        get
        {
            return this.includeField;
        }
        set
        {
            this.includeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Exclude
    {
        get
        {
            return this.excludeField;
        }
        set
        {
            this.excludeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("x-item", typeof(EntriesXitem), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public EntriesXitem[] Entries
    {
        get
        {
            return this.entriesField;
        }
        set
        {
            this.entriesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Converter")]
    public Converter[] Converter
    {
        get
        {
            return this.converterField;
        }
        set
        {
            this.converterField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class projectSourcesXfiles
{

    private string includeField;

    private string excludeField;

    private string nameField;

    private EntriesXitem[] entriesField;

    private Converter[] converterField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Include
    {
        get
        {
            return this.includeField;
        }
        set
        {
            this.includeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Exclude
    {
        get
        {
            return this.excludeField;
        }
        set
        {
            this.excludeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("x-item", typeof(EntriesXitem), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    public EntriesXitem[] Entries
    {
        get
        {
            return this.entriesField;
        }
        set
        {
            this.entriesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Converter")]
    public Converter[] Converter
    {
        get
        {
            return this.converterField;
        }
        set
        {
            this.converterField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class NewDataSet
{

    private object[] itemsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Converter", typeof(Converter))]
    [System.Xml.Serialization.XmlElementAttribute("Entries", typeof(Entries))]
    [System.Xml.Serialization.XmlElementAttribute("project", typeof(project))]
    public object[] Items
    {
        get
        {
            return this.itemsField;
        }
        set
        {
            this.itemsField = value;
        }
    }
}

