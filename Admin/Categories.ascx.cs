using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Web;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Base;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Admin
{

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The EditNBrightIndex class is used to manage content
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class Categories : NBrightBuyAdminBase
    {

        private GenXmlTemplate _templSearch;
        private String _entryid = "";
        private String _templatType = "list";
        private int _openid = 0;

        public String Edittype { get; set; }
        //NOTE:  This code is dual use: by Categories.ascx and PropertiesValue.ascx.  The "Edittype" attr identifies this.
        //eg: if (!String.IsNullOrEmpty(Edittype) && Edittype.ToLower() == "group")

        private const string NotifyRef = "categoryaction";

        #region Load Event Handlers

        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);

            _entryid = Utils.RequestParam(Context, "eid");
            if (_entryid != "") _templatType = "detail";

            // create different templates for properties
            if (!String.IsNullOrEmpty(Edittype) && Edittype.ToLower() == "group")
            {
                _templatType = "properties" + _templatType;
                // Get Search
                var rpSearchTempl = ModCtrl.GetTemplateData(ModSettings, "propertiessearch.html", Utils.GetCurrentCulture(), DebugMode);
                _templSearch = NBrightBuyUtils.GetGenXmlTemplate(rpSearchTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
                rpSearch.ItemTemplate = _templSearch;                                
            }
            else
                _templatType = "category" + _templatType;
            
            var t1 = _templatType + "header.html";
            var t2 = _templatType + "body.html";
            var t3 = _templatType + "footer.html";

            // Get Display Header
            var rpDataHTempl = ModCtrl.GetTemplateData(ModSettings, t1, Utils.GetCurrentCulture(), DebugMode);
            rpDataH.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataHTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
            // Get Display Body
            var rpDataTempl = ModCtrl.GetTemplateData(ModSettings, t2, Utils.GetCurrentCulture(), DebugMode);
            rpData.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);
            // Get Display Footer
            var rpDataFTempl = ModCtrl.GetTemplateData(ModSettings, t3, Utils.GetCurrentCulture(), DebugMode);
            rpDataF.ItemTemplate = NBrightBuyUtils.GetGenXmlTemplate(rpDataFTempl, ModSettings.Settings(), PortalSettings.HomeDirectory);

            // get selected open catid
            var stropenid = Utils.RequestParam(Context, "catid");
            if (Utils.IsNumeric(stropenid)) _openid = Convert.ToInt32(stropenid);

        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                if (Page.IsPostBack == false)
                {
                    PageLoad();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                //display the error on the template (don;t want to log it here, prefer to deal with errors directly.)
                var l = new Literal();
                l.Text = exc.ToString();
                phData.Controls.Add(l);
            }
        }

        private void PageLoad()
        {

            #region "Data Repeater"

            if (UserId > 0) // only logged in users can see data on this module.
            {

                if (Utils.IsNumeric(_entryid) && _entryid != "0")
                {
                    var categoryData = CategoryUtils.GetCategoryData(Convert.ToInt32(_entryid), EditLanguage);
                    base.DoDetail(rpData, categoryData.Info);
                }
                else
                {

                    var navigationData = new NavigationData(PortalId, "CategoryAdmin");

                    // get search data
                    var sInfo = new NBrightInfo();
                    sInfo.XMLData = navigationData.XmlData;

                    // display search
                    base.DoDetail(rpSearch, sInfo);

                    var grpCats = new List<NBrightInfo>();
                    if (!String.IsNullOrEmpty(Edittype) && Edittype.ToLower() == "group")
                    {
                        var selgroup = GenXmlFunctions.GetGenXmlValue(navigationData.XmlData, "genxml/dropdownlist/groupsel");
                        if (selgroup == "") selgroup = GenXmlFunctions.GetField(rpSearch, "groupsel");
                        grpCats = NBrightBuyUtils.GetCatList(_openid, selgroup,EditLanguage);
                    }
                    else
                        grpCats = NBrightBuyUtils.GetCatList(_openid, "cat", EditLanguage);

                    rpData.DataSource = grpCats;
                    rpData.DataBind();

                }
            }

            #endregion

            // display header (Do header after the data return so the productcount works)
            base.DoDetail(rpDataH);

            // display footer
            base.DoDetail(rpDataF);

        }

        #endregion

        #region "Event handlers"

        protected void CtrlItemCommand(object source, RepeaterCommandEventArgs e)
        {
            var cArg = e.CommandArgument.ToString();
            var param = new string[3];

            var navigationData = new NavigationData(PortalId, "CategoryAdmin");
            switch (e.CommandName.ToLower())
            {
                case "entrydetail":
                    SaveAll();
                    param[1] = "eid=" + cArg;
                    param[2] = "catid=" + _openid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "return":
                    param[1] = "";
                    param[2] = "catid=" + _openid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "search":
                    var strXml = GenXmlFunctions.GetGenXml(rpSearch, "", "");
                    navigationData.XmlData = strXml;
                    if (StoreSettings.Current.DebugModeFileOut)
                    {
                        strXml = "<root><sql><![CDATA[" + navigationData.Criteria + "]]></sql>" + strXml + "</root>";
                        var xmlDoc = new System.Xml.XmlDocument();
                        xmlDoc.LoadXml(strXml);
                        xmlDoc.Save(PortalSettings.HomeDirectoryMapPath + "debug_search.xml");
                    }   
                    navigationData.Save();
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "resetsearch":
                    // clear cookie info
                    navigationData.Delete();
                    param[2] = "catid=0";
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "addnew":
                    var strXml2 = GenXmlFunctions.GetGenXml(rpSearch, "", "");
                    navigationData.XmlData = strXml2;
                    navigationData.Save();
                    var categoryData = CategoryUtils.GetCategoryData(-1, EditLanguage);
                    if (!String.IsNullOrEmpty(Edittype) && Edittype.ToLower() == "group")
                    {
                        categoryData.GroupType = GenXmlFunctions.GetGenXmlValue(navigationData.XmlData, "genxml/dropdownlist/groupsel");
                        if (categoryData.GroupType == "") categoryData.GroupType = "cat";
                        var grpCtrl = new GrpCatController(Utils.GetCurrentCulture());
                        var grp = grpCtrl.GetGrpCategoryByRef(categoryData.GroupType);
                        if (grp != null) categoryData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", grp.categoryid.ToString(""));
                        categoryData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", "False"); // don't hide property groups by default
                    }
                    categoryData.ParentItemId = _openid; 
                    categoryData.Save();
                    NBrightBuyUtils.RemoveModCachePortalWide(PortalId);
                    
                    param[2] = "catid=" + _openid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "delete":
                    if (Utils.IsNumeric(cArg))
                    {
                        var catid = Convert.ToInt32(cArg);
                        if (catid > 0)
                        {
                            var delCatData = CategoryUtils.GetCategoryData(catid, EditLanguage);
                            if (delCatData.Exists && delCatData.GetDirectChildren().Count == 0) // only delete end leaf
                            {
                                var productidlist = new ArrayList();
                                foreach (var dc in delCatData.GetDirectArticles())
                                {
                                    productidlist.Add(dc.ParentItemId);
                                }

                                var parentCatList = new List<CategoryData>();
                                var loopCat = CategoryUtils.GetCategoryData(catid, EditLanguage);
                                while (loopCat.Exists && loopCat.ParentItemId > 0)
                                {
                                    loopCat = CategoryUtils.GetCategoryData(loopCat.ParentItemId, EditLanguage);
                                    parentCatList.Add(loopCat);
                                }

                                foreach (var pCat in parentCatList)
                                {
                                    foreach (var prodxref in pCat.GetCascadeArticles())
                                    {
                                        if (productidlist.Contains(prodxref.ParentItemId))
                                        {
                                            // delete CATCASCADE record
                                            if (prodxref.TypeCode == "CATCASCADE") // just check we have correct record. (stop nasty surprises)
                                            {
                                                ModCtrl.Delete(prodxref.ItemID);
                                            }
                                        }                                        
                                    }
                                    
                                }

                                foreach (var dc in delCatData.GetDirectArticles())
                                {
                                    // delete CATXREF record
                                    if (dc.TypeCode == "CATXREF") // just check we have correct record. (stop nasty surprises)
                                    {
                                        ModCtrl.Delete(dc.ItemID);
                                    }
                                }
                                // delete CATEGORY record (constrants remove LANG records.)
                                ModCtrl.Delete(catid);
                            }
                            else
                            {
                                NBrightBuyUtils.SetNotfiyMessage(ModuleId, "onlyleafcat", NotifyCode.fail);
                            }
                        }
                    }
                    param[2] = "catid=" + _openid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "saveall":
                    SaveAll();
                    NBrightBuyUtils.RemoveModCachePortalWide(PortalId); //clear any cache
                    param[2] = "catid=" + _openid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "move":
                    SaveAll();
                    if (Utils.IsNumeric(cArg))
                    {
                        MoveRecord(Convert.ToInt32(cArg));
                    }
                    param[2] = "catid=" + _openid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "open":
                    param[1] = "catid=" + cArg;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "close":
                    var catData = CategoryUtils.GetCategoryData(_openid, EditLanguage);
                    if (catData.DataRecord == null)
                        param[1] = "catid=0";
                    else
                        param[1] = "catid=" + catData.DataRecord.ParentItemId.ToString("");
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "save":
                    UpdateRecord();
                    param[1] = "eid=" + cArg;
                   param[2] = "catid=" + _openid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;
                case "saveexit":
                    UpdateRecord();
                    param[2] = "catid=" + _openid;
                    Response.Redirect(NBrightBuyUtils.AdminUrl(TabId, param), true);
                    break;

            }

        }


        #endregion

        private void SaveAll()
        {
            foreach (RepeaterItem rtnItem in rpData.Items)
            {
                var isdirty = GenXmlFunctions.GetField(rtnItem, "isdirty");
                var itemid = GenXmlFunctions.GetField(rtnItem, "itemid");
                if (isdirty == "true" && Utils.IsNumeric(itemid))
                {
                    var catData = CategoryUtils.GetCategoryData(Convert.ToInt32(itemid), StoreSettings.Current.EditLanguage);
                    if (catData.Exists)
                    {
                        var chkishidden = GenXmlFunctions.GetField(rtnItem, "chkishidden");
                        var catname = GenXmlFunctions.GetField(rtnItem, "txtcategoryname");
                        catData.DataRecord.SetXmlProperty("genxml/checkbox/chkishidden", chkishidden);

                        if (!String.IsNullOrEmpty(Edittype) && Edittype.ToLower() == "group")
                        {
                            var propertyref = GenXmlFunctions.GetField(rtnItem, "propertyref");
                            if (propertyref != "")
                            {
                                catData.DataRecord.SetXmlProperty("genxml/textbox/propertyref", propertyref);
                                catData.DataRecord.SetXmlProperty("genxml/textbox/txtcategoryref", propertyref);
                            }
                            var grptype = catData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
                            var grp = ModCtrl.GetByGuidKey(PortalSettings.PortalId, -1, "GROUP", grptype);
                            if (grp != null)
                            {
                                catData.ParentItemId = grp.ItemID;
                                ModCtrl.Update(catData.DataRecord);
                            }
                        }
                        else
                        {
                            // the base category ref cannot have language dependant refs, we therefore just use a unique key
                            var catref = catData.DataRecord.GetXmlProperty("genxml/textbox/txtcategoryref");
                            if (catref == "")
                            {
                                catref = Utils.GetUniqueKey().ToLower();
                                catData.DataRecord.SetXmlProperty("genxml/textbox/txtcategoryref", catref);
                                catData.DataRecord.GUIDKey = catref;
                            }
                            ModCtrl.Update(catData.DataRecord);
                        }

                        catData.DataLangRecord.SetXmlProperty("genxml/textbox/txtcategoryname", catname);
                        ModCtrl.Update(catData.DataLangRecord);
                        if (catname != "")
                        {
                            // update all language records that have no name
                            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
                            {
                                var catLangUpd = CategoryUtils.GetCategoryData(Convert.ToInt32(itemid), lang);
                                if (catLangUpd.DataLangRecord != null && catLangUpd.Info.GetXmlProperty("genxml/lang/genxml/textbox/txtcategoryname") == "")
                                {
                                    catLangUpd.DataLangRecord.SetXmlProperty("genxml/textbox/txtcategoryname", catname);
                                    ModCtrl.Update(catLangUpd.DataLangRecord);
                                }
                            }
                        }

                        catData.Save();
                        NBrightBuyUtils.RemoveModCachePortalWide(PortalId);  // clear cache before validate lang, so we pickup new changes to name.
                        CategoryUtils.ValidateLangaugeRef(PortalId, Convert.ToInt32(itemid)); // do validate so we update all refs and children refs
                    }
                }
            }
        }

        private void UpdateRecord()
        {
            var xmlData = GenXmlFunctions.GetGenXml(rpData, "", StoreSettings.Current.FolderImagesMapPath);
            var objInfo = new NBrightInfo();
            objInfo.ItemID = -1;
            objInfo.TypeCode = "POSTDATA";
            objInfo.XMLData = xmlData;
            var settings = objInfo.ToDictionary(); // put the fieds into a dictionary, so we can get them easy.

            // check we don't have an invalid parentitemid
            var parentitemid = objInfo.GetXmlPropertyInt("genxml/dropdownlist/ddlparentcatid");
            var strOut = "No Category ID ('itemid' hidden fields needed on input form)";
            if (settings.ContainsKey("itemid"))
            {
                if (parentitemid != Convert.ToInt32(settings["itemid"]))
                {

                    var catData = CategoryUtils.GetCategoryData(Convert.ToInt32(settings["itemid"]), StoreSettings.Current.EditLanguage);

                    // check we've not put a category under it's child
                    if (IsParentInChildren(catData, parentitemid))
                    {
                        NBrightBuyUtils.SetNotfiyMessage(ModuleId, "categoryactionsave", NotifyCode.fail);
                    }
                    else
                    {

                        catData.Update(objInfo);

                        if (!String.IsNullOrEmpty(Edittype) && Edittype.ToLower() == "group")
                        {
                            var grptype = objInfo.GetXmlProperty("genxml/dropdownlist/ddlparentcatid");
                            var grp = ModCtrl.GetByGuidKey(PortalSettings.PortalId, -1, "GROUP", grptype);
                            if (grp != null)
                            {
                                var newGuidKey = objInfo.GetXmlProperty("genxml/textbox/propertyref");
                                if (newGuidKey != "") newGuidKey = GetUniqueGuidKey(catData.CategoryId, Utils.UrlFriendly(newGuidKey));
                                catData.DataRecord.GUIDKey = newGuidKey;
                                catData.DataRecord.SetXmlProperty("genxml/textbox/txtcategoryref", newGuidKey);
                                catData.DataRecord.ParentItemId = grp.ItemID;
                                // list done using ddlgrouptype, in  GetCatList 
                                catData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", grptype);
                                catData.Save();
                                NBrightBuyUtils.RemoveModCachePortalWide(PortalId);
                            }
                        }
                        else
                        {
                            // the base category ref cannot have language dependant refs, we therefore just use a unique key
                            var catref = catData.DataRecord.GetXmlProperty("genxml/textbox/txtcategoryref");
                            if (catref == "")
                            {
                                if (catData.DataRecord.GUIDKey == "")
                                {
                                    catref = Utils.GetUniqueKey().ToLower();
                                    catData.DataRecord.SetXmlProperty("genxml/textbox/txtcategoryref", catref);
                                    catData.DataRecord.GUIDKey = catref;
                                }
                                else
                                {
                                    catData.DataRecord.SetXmlProperty("genxml/textbox/txtcategoryref", catData.DataRecord.GUIDKey);
                                }
                            }
                            catData.Save();
                            CategoryUtils.ValidateLangaugeRef(PortalId, Convert.ToInt32(settings["itemid"])); // do validate so we update all refs and children refs
                            NBrightBuyUtils.RemoveModCachePortalWide(PortalId);
                        }
                        NBrightBuyUtils.SetNotfiyMessage(ModuleId, "categoryactionsave", NotifyCode.ok);
                    }
                }
                else
                {
                    NBrightBuyUtils.SetNotfiyMessage(ModuleId, "categoryactionsave", NotifyCode.fail);
                }
            }
            NBrightBuyUtils.RemoveModCachePortalWide(PortalId); //clear any cache
        }

        private Boolean IsParentInChildren(CategoryData catData,int parentItemId)
        {
            foreach (var ch in catData.GetDirectChildren())
            {
                if (ch.ItemID == parentItemId) return true;
                var catChildData = CategoryUtils.GetCategoryData(ch.ItemID, StoreSettings.Current.EditLanguage);
                if (IsParentInChildren(catChildData, parentItemId)) return true;
            }
            return false;
        }

        private void MoveRecord(int itemId)
        {

            var selecteditemid = GenXmlFunctions.GetField(rpDataH, "selecteditemid");
            if (Utils.IsNumeric(selecteditemid))
            {
                var movData = CategoryUtils.GetCategoryData(itemId, StoreSettings.Current.EditLanguage);
                var selData = CategoryUtils.GetCategoryData(Convert.ToInt32(selecteditemid), StoreSettings.Current.EditLanguage);
                var fromParentItemid = selData.DataRecord.ParentItemId;
                var toParentItemid = movData.DataRecord.ParentItemId;
                var reindex = toParentItemid != fromParentItemid;

                var objGrpCtrl = new GrpCatController(StoreSettings.Current.EditLanguage);
                var movGrp = objGrpCtrl.GetGrpCategory(movData.Info.ItemID);
                if (!movGrp.Parents.Contains(selData.Info.ItemID)) // cannot move a category into itself (i.e. move parent into sub-category)
                {
                    selData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlparentcatid", toParentItemid.ToString(""));
                    selData.DataRecord.ParentItemId = toParentItemid;
                    selData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", movData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype"));
                    var strneworder = movData.DataRecord.GetXmlProperty("genxml/hidden/recordsortorder");
                    var selorder = selData.DataRecord.GetXmlProperty("genxml/hidden/recordsortorder");
                    if (!Utils.IsNumeric(strneworder)) strneworder = "1";
                    if (!Utils.IsNumeric(selorder)) selorder = "1";
                    var neworder = Convert.ToDouble(strneworder, CultureInfo.GetCultureInfo("en-US"));
                    if (Convert.ToDouble(strneworder, CultureInfo.GetCultureInfo("en-US")) < Convert.ToDouble(selorder, CultureInfo.GetCultureInfo("en-US")))
                        neworder = neworder - 0.5;
                    else
                        neworder = neworder + 0.5;
                    selData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder", neworder.ToString(""), TypeCode.Double);
                    ModCtrl.Update(selData.DataRecord);

                    FixRecordSortOrder(toParentItemid.ToString("")); //reindex all siblings (this is so we get a int on the recordsortorder)
                    FixRecordGroupType(selData.Info.ItemID.ToString(""), selData.DataRecord.GetXmlProperty("genxml/dropdownlist/ddlgrouptype"));

                    if (reindex)
                    {
                        objGrpCtrl.ReIndexCascade(fromParentItemid); // reindex from parent and parents.
                        objGrpCtrl.ReIndexCascade(selData.Info.ItemID); // reindex select and parents
                    }
                    NBrightBuyUtils.RemoveModCachePortalWide(PortalId); //clear any cache
                }
            }
        }

        private void FixRecordGroupType(String parentid, String groupType)
        {
            if (Utils.IsNumeric(parentid) && Convert.ToInt32(parentid) > 0)
            {
                // fix any incorrect sort orders
                var strFilter = " and NB1.ParentItemId = " + parentid + " ";
                var levelList = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLanguage, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
                foreach (NBrightInfo catinfo in levelList)
                {
                    var grouptype = catinfo.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
                    var catData = CategoryUtils.GetCategoryData(catinfo.ItemID, StoreSettings.Current.EditLanguage);
                    if (grouptype != groupType)
                    {
                        catData.DataRecord.SetXmlProperty("genxml/dropdownlist/ddlgrouptype", groupType);
                        ModCtrl.Update(catData.DataRecord);
                    }
                    FixRecordGroupType(catData.Info.ItemID.ToString(""), groupType);
                }
            }
        }

        private void FixRecordSortOrder(String parentid)
        {
            if (!Utils.IsNumeric(parentid)) parentid = "0";
            // fix any incorrect sort orders
            Double lp = 1;
            var strFilter = " and NB1.ParentItemId = " + parentid + " ";
            var levelList = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLanguage, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
            foreach (NBrightInfo catinfo in levelList)
            {
                var recordsortorder = catinfo.GetXmlProperty("genxml/hidden/recordsortorder");
                if (!Utils.IsNumeric(recordsortorder) || Convert.ToDouble(recordsortorder, CultureInfo.GetCultureInfo("en-US")) != lp)
                {
                    var catData = CategoryUtils.GetCategoryData(catinfo.ItemID, StoreSettings.Current.EditLanguage);
                    catData.DataRecord.SetXmlProperty("genxml/hidden/recordsortorder", lp.ToString(""));
                    ModCtrl.Update(catData.DataRecord);
                }
                lp += 1;
            }
        }


        private List<NBrightInfo> GetTreeCatList(List<NBrightInfo> rtnList, int level = 0, int parentid = 0, string groupref = "", string parentlist = "", int displaylevels = 50)
        {
            if (level > displaylevels) return rtnList; // stop infinate loop

            var strFilter = "";
            if (groupref == "" || groupref == "0") // Because we've introduced Properties (for non-category groups) we will only display these if cat is not selected.
                strFilter += " and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') != 'cat' ";
            else
            {
                if (groupref == "cat") strFilter = " and NB1.ParentItemId = " + parentid + " "; // only category have multipel levels.
                strFilter += " and [XMLData].value('(genxml/dropdownlist/ddlgrouptype)[1]','nvarchar(max)') = '" + groupref + "' ";
            }

            if (parentid > 0 ) parentlist += parentid.ToString("") + ";";

            var levelList = ModCtrl.GetDataList(PortalSettings.Current.PortalId, -1, "CATEGORY", "CATEGORYLANG", EditLanguage, strFilter, " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','decimal(10,2)') ", true);
            foreach (NBrightInfo catinfo in levelList)
            {
                var str = new string('.', level);
                str = str.Replace(".", "....");
                catinfo.SetXmlProperty("genxml/hidden/levelprefix",str);
                rtnList.Add(catinfo);
                catinfo.SetXmlProperty("genxml/parentlist",parentlist);
                if (groupref == "cat") rtnList = GetTreeCatList(rtnList, level + 1, catinfo.ItemID, groupref, parentlist, displaylevels); // only category have multipel levels.
            }

            return rtnList;
        }


        private string GetUniqueGuidKey(int categoryId, string newGUIDKey)
        {
            // make sure we have a unique guidkey
            var doloop = true;
            var lp = 1;
            var testGUIDKey = newGUIDKey.ToLower();
            while (doloop)
            {
                var obj = ModCtrl.GetByGuidKey(PortalSettings.PortalId, -1, "CATEGORY", testGUIDKey);
                if (obj != null && obj.ItemID != categoryId)
                {
                    testGUIDKey = newGUIDKey + lp.ToString();
                }
                else
                    doloop = false;

                lp += 1;
                if (lp > 999) doloop = false; // make sure we never get a infinate loop
            }
            return testGUIDKey;
        }


    }

}