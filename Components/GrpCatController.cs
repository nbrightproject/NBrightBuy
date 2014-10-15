using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class GrpCatController
    {
        private NBrightBuyController _objCtrl;

        private String _lang = "";
        public List<GroupCategoryData> GrpCategoryList;
        public List<GroupCategoryData> CategoryList;
        public List<NBrightInfo> GroupList;
        public Dictionary<String, String> GroupsDictionary; 

        public GrpCatController(String lang,Boolean debugMode = false)
        {
            Load(lang, debugMode);
        }

        #region "base methods"

        public void Reload()
        {
            ClearCache();
            Load(_lang);
        }

        public void ClearCache()
        {
            foreach (var lang in DnnUtils.GetCultureCodeList(PortalSettings.Current.PortalId))
            {
                var strCacheKey = "NBS_GrpCategoryList_" + lang + "_" + PortalSettings.Current.PortalId;
                NBrightBuyUtils.RemoveCache(strCacheKey);
                strCacheKey = "NBS_CategoryList_" + lang + "_" + PortalSettings.Current.PortalId;
                NBrightBuyUtils.RemoveCache(strCacheKey);                
            }
        }

        public GroupCategoryData GetGrpCategory(int categoryid)
        {
            var lenum = from i in GrpCategoryList where i.categoryid == categoryid select i;
            var l = lenum.ToList();
            return l.Any() ? l[0] : null;
        }

        public GroupCategoryData GetCategory(int categoryid)
        {
            var lenum = from i in CategoryList where i.categoryid == categoryid select i;
            var l = lenum.ToList();
            return l.Any() ? l[0] : null;
        }

        public List<GroupCategoryData> GetGrpCategories(int parentcategoryid, string groupref = "")
        {
            IEnumerable<GroupCategoryData> lenum;
            if (groupref == "" || groupref == "cat")
                lenum = from i in GrpCategoryList where i.parentcatid == parentcategoryid select i;
            else
                lenum = from i in GrpCategoryList select i; // if we're getting group categories, the parentitemid is set to the group itemid + we only have the 1 level.

            if (groupref != "")
            {
                var lenum2 = from i2 in lenum where i2.grouptyperef == groupref select i2;
                return lenum2.ToList();
            }
            return lenum.ToList();
        }

        public List<GroupCategoryData> GetCategories(int parentcategoryid)
        {
            var lenum = from i in CategoryList where i.parentcatid == parentcategoryid select i;
            var l = lenum.ToList();
            return l;
        }

        public List<GroupCategoryData> GetCategoriesWithUrl(int parentcategoryid, int tabid)
        {
            var l = GetCategories(parentcategoryid);
            foreach (var c in l)
            {
                c.url = GetCategoryUrl(c, tabid);
            }
            return l;
        }

        public GroupCategoryData GetGrpCategoryByRef(string categoryref)
        {
            var lenum = from i in GrpCategoryList where i.categoryref == categoryref select i;
            var l = lenum.ToList();
            if (l.Count == 0) return null;
            return l[0];
        }

        public List<GroupCategoryData> GetSubCategoryList(List<GroupCategoryData> catList, int categoryid, int lvl = 0)
        {
            if (lvl > 50) return catList; // stop possible infinate loop

            var subcats = from i in GrpCategoryList where i.parentcatid == categoryid select i;
            foreach (var c in subcats)
            {
                catList.Add(c);
                GetSubCategoryList(catList, c.categoryid, lvl + 1);
            }
            return catList;
        }

        #endregion

        #region "Special methods"


        public string GetCategoryUrl(GroupCategoryData groupCategoryInfo, int tabid)
        {

            //set a default url
            var url = "?catid=" + groupCategoryInfo.categoryid.ToString("");
            // get friendly url if possible
                if (groupCategoryInfo.categoryname != "")
                {
                    var newBaseName = groupCategoryInfo.seoname;
                    if (newBaseName == "") newBaseName = groupCategoryInfo.categoryname;
                    var tab = CBO.FillObject<DotNetNuke.Entities.Tabs.TabInfo>(DotNetNuke.Data.DataProvider.Instance().GetTab(tabid));
                    if (tab != null)
                    {
                        url = DotNetNuke.Services.Url.FriendlyUrl.FriendlyUrlProvider.Instance().FriendlyUrl(tab, "~/Default.aspx?TabId=" + tab.TabID.ToString("") + "&catid=" + groupCategoryInfo.categoryid.ToString("") + "&language=" + Utils.GetCurrentCulture(), newBaseName.Replace(" ", "-") + ".aspx");
                        url = url.Replace("[catid]/", ""); // remove the injection token from the url, if still there. (Should be removed redirected to new page)
                    }
                }
            return url;
        }

        /// <summary>
        /// Get the default category for the product 
        /// </summary>
        /// <param name="productid"></param>
        /// <returns></returns>
        public int GetDefaultCatId(int productid)
        {
            var defId = 0;
            var objCtrl = new NBrightBuyController();
            var objQual = DotNetNuke.Data.DataProvider.Instance().ObjectQualifier;
            var dbOwner = DotNetNuke.Data.DataProvider.Instance().DatabaseOwner;
            var strFilter = " and NB1.parentitemid = " + productid.ToString("") + " ";
            var objInfo = objCtrl.Get(productid);

            if (objInfo != null)
            {

                var l = objCtrl.GetList(objInfo.PortalId, objInfo.ModuleId, "CATXREF", strFilter);

                var catIds = new HashSet<int>(CategoryList.Select(x => x.categoryid));
                l.RemoveAll(x => !catIds.Contains(x.XrefItemId));


                foreach (var e in l)
                {
                    if (e.GetXmlProperty("genxml/hidden/defaultcat").ToLower() == "true")
                    {
                        defId = e.XrefItemId;
                        break;
                    }
                }

                if (defId == 0)
                {
                    foreach (var e in l)
                    {
                        if ((e.GetXmlProperty("genxml/checkbox/chkishidden").ToLower() == "false") && (e.GetXmlProperty("genxml/checkbox/chkarchived").ToLower() == "false"))
                        {
                            defId = e.XrefItemId;
                            break;
                        }
                    }
                }


                if (defId == 0)
                {
                    if (l.Count > 0)
                    {
                        defId = l[0].XrefItemId;
                    }
                }
            }

            return defId;
        }

        public GroupCategoryData GetCurrentCategoryData(int portalId, System.Web.HttpRequest request, int entryId = 0, Dictionary<string, string> settings = null, String targetModuleKey = "")
        {

            var defcatid = 0;
            var qrycatid = Utils.RequestQueryStringParam(request, "catid");
            if (Utils.IsNumeric(entryId) && entryId > 0) defcatid = GetDefaultCatId(entryId);

            if (defcatid == 0 && Utils.IsNumeric(qrycatid)) defcatid = Convert.ToInt32(qrycatid);

            if (targetModuleKey != "")
            {
                var navigationdata = new NavigationData(portalId, targetModuleKey);
                if (Utils.IsNumeric(navigationdata.CategoryId) && navigationdata.FilterMode) defcatid = Convert.ToInt32(navigationdata.CategoryId);
            }

            if (defcatid == 0)
            {
                if (settings != null && settings["defaultcatid"] != null)
                {
                    var setcatid = settings["defaultcatid"];
                    if (Utils.IsNumeric(setcatid)) defcatid = Convert.ToInt32(setcatid);
                }
            }

            return GetCategory(defcatid);
        }

        public NBrightInfo GetCurrentCategoryInfo(int portalId, System.Web.HttpRequest request, int entryId = 0)
        {

            var defcatid = 0;
            var qrycatid = Utils.RequestQueryStringParam(request, "catid");
            if (Utils.IsNumeric(entryId) && entryId > 0) defcatid = GetDefaultCatId(entryId);

            if (defcatid == 0 && Utils.IsNumeric(qrycatid)) defcatid = Convert.ToInt32(qrycatid);

            var objCtrl = new NBrightBuyController();
            return objCtrl.GetData(defcatid,"CATEGORYLANG");
        }

        public List<GroupCategoryData> GetTreeCategoryList(List<GroupCategoryData> rtnList, int level, int parentid, string groupref,string breadcrumbseparator)
        {
            if (level > 20) return rtnList; // stop infinate loop

            var levelList = GetGrpCategories(parentid, groupref);
            foreach (GroupCategoryData tInfo in levelList)
            {
                var nInfo = tInfo;
                nInfo.breadcrumb = GetBreadCrumb(nInfo.categoryid, 50, breadcrumbseparator,false);
                nInfo.depth = level;
                rtnList.Add(nInfo);
                if (groupref == "" || groupref == "cat") GetTreeCategoryList(rtnList, level + 1, tInfo.categoryid, groupref, breadcrumbseparator);
            }

            return rtnList;
        }

        /// <summary>
        /// Select categories linked to product, by groupref
        /// </summary>
        /// <param name="productid"></param>
        /// <param name="groupref">groupref for select, "" = all, "cat"= Category only, "!cat" = all non-category, "{groupref}"=this group only</param>
        /// <param name="cascade">get all cascade records to get all parent categories</param>
        /// <returns></returns>
        public List<GroupCategoryData> GetProductCategories(int productid, String groupref = "", Boolean cascade = false)
        {
            var objCtrl = new NBrightBuyController();
            var catxrefList = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATXREF", " and NB1.[ParentItemId] = " + productid);

            if (cascade)
            {
                var catcascadeList = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATCASCADE", " and NB1.[ParentItemId] = " + productid);
                foreach (var c in catcascadeList)
                {
                    catxrefList.Add(c);
                }                
            }


            var notcat = "";
            if (groupref == "!cat")
            {
                groupref = "";
                notcat = "cat";
            }

            var joinItems = (from d1 in GrpCategoryList
                             join d2 in catxrefList on d1.categoryid equals d2.XrefItemId
                             where (d1.grouptyperef == groupref || groupref == "") && d1.grouptyperef != notcat
                             select d1).OrderBy(d1 => d1.grouptyperef).ThenBy(d1 => d1.breadcrumb).ToList<GroupCategoryData>();
            return joinItems;
        }

        #endregion

        #region "breadcrumbs"

        public String GetBreadCrumb(int categoryid, int shortLength, string separator, bool aslist)
        {
            var breadCrumb = "";
            var checkDic = new Dictionary<int, int>();
            while (true)
            {
                if (checkDic.ContainsKey(categoryid)) break; // jump out if we get data giving an infinate loop
                int itemid1 = categoryid;
                var lenum = from i in CategoryList where i.categoryid == itemid1 select i;
                var l = lenum.ToList();
                if (l.Any())
                {
                    var crumbText = l.First().categoryname;
                    if (crumbText != null)
                    {
                        if (shortLength > 0)
                        {
                            if (crumbText.Length > (shortLength + 1)) crumbText = crumbText.Substring(0, shortLength) + ".";
                        }

                        var strOut = "";
                        if (aslist)
                            strOut = "<li>" + separator + crumbText + "</li>" + breadCrumb;
                        else
                            strOut = separator + crumbText + breadCrumb;

                        checkDic.Add(categoryid, categoryid);
                        categoryid = l.First().parentcatid;
                        breadCrumb = strOut;
                        continue;
                    }
                }
                if (breadCrumb.StartsWith(separator)) breadCrumb = breadCrumb.Substring(separator.Length);
                if (aslist) breadCrumb = "<ul class='crumbs'>" + breadCrumb + "</ul>";
                return breadCrumb;
            }
            return "";
        }

        public String GetBreadCrumbWithLinks(int categoryid, int tabId, int shortLength, string separator, bool aslist)
        {
            var breadCrumb = "";
            var checkDic = new Dictionary<int, int>();
            while (true)
            {
                if (checkDic.ContainsKey(categoryid)) break; // jump out if we get data giving an infinate loop
                int itemid1 = categoryid;
                var lenum = from i in CategoryList where i.categoryid == itemid1 select i;
                var l = lenum.ToList();
                if (l.Any())
                {
                    var crumbText = l.First().categoryname;
                    if (crumbText != null)
                    {
                        if (shortLength > 0)
                        {
                            if (crumbText.Length > (shortLength + 1)) crumbText = crumbText.Substring(0, shortLength) + ".";
                        }

                        var strOut = "";
                        if (aslist)
                            strOut = "<li>" + separator + "<a href='" + GetCategoryUrl(l.First(), tabId) + "'>" + crumbText + "</a>" + "</li>" + breadCrumb;
                        else
                            strOut = separator + "<a href='" + GetCategoryUrl(l.First(), tabId) + "'>" + crumbText + "</a>" + breadCrumb;
                        
                        checkDic.Add(categoryid, categoryid);
                        categoryid = l.First().parentcatid;
                        breadCrumb = strOut;
                        continue;
                    }
                }
                if (breadCrumb.StartsWith(separator)) breadCrumb = breadCrumb.Substring(separator.Length);
                if (aslist) breadCrumb = "<ul class='crumbs'>" + breadCrumb + "</ul>"; 
                return breadCrumb;
            }
            return "";
        }

        #endregion

        #region "private methods"

        private void Load(String lang, Boolean debugMode = false)
        {
            _objCtrl = new NBrightBuyController();
            _lang = lang;

            // get groups
            GroupList = NBrightBuyUtils.GetCategoryGroups(_lang, true);

            GroupsDictionary = new Dictionary<String, String>();
            foreach (var g in GroupList)
            {
                if (!GroupsDictionary.ContainsKey(g.GetXmlProperty("genxml/textbox/groupref"))) GroupsDictionary.Add(g.GetXmlProperty("genxml/textbox/groupref"), g.GetXmlProperty("genxml/lang/genxml/textbox/groupname"));
            }

            // build group category list
            var strCacheKey = "NBS_GrpCategoryList_" + lang + "_" + PortalSettings.Current.PortalId;
            GrpCategoryList = (List<GroupCategoryData>)NBrightBuyUtils.GetModCache(strCacheKey);
            if (GrpCategoryList == null || debugMode)
            {
                GrpCategoryList = GetGrpCatListFromDatabase(lang);
                NBrightBuyUtils.SetModCache(-1, strCacheKey, GrpCategoryList);
            }

            // build cateogry list for navigation from group category list
            strCacheKey = "NBS_CategoryList_" + lang + "_" + PortalSettings.Current.PortalId;
            CategoryList = (List<GroupCategoryData>)NBrightBuyUtils.GetModCache(strCacheKey);
            if (CategoryList == null || debugMode)
            {
                var lenum = from i in GrpCategoryList where i.grouptyperef == "cat" select i;
                CategoryList = lenum.ToList();
                NBrightBuyUtils.SetModCache(-1, strCacheKey, CategoryList);
            }
        }

        private NBrightInfo GetLangData(List<NBrightInfo> langList,int categoryid)
        {
            var lenum = from i in langList where i.ParentItemId == categoryid select i;
            var l = lenum.ToList();
            return l.Any() ? l[0] : null;
        }

        private int GetEntryCount(List<NBrightInfo> xrefList, int categoryid)
        {
            var lenum = from i in xrefList where i.XrefItemId == categoryid select i;
            return lenum.Count();
        }


        private List<NBrightInfo> GetParentList(List<NBrightInfo> catList, int categoryid)
        {
            var rtnList = new List<NBrightInfo>();
            var startCat = from i in catList where i.ItemID == categoryid select i;
            if (startCat.Any())
            {
                categoryid = startCat.ToList()[0].ParentItemId;
                var c = 1;
                while (true)
                {
                    var l = from i in catList where i.ItemID == categoryid select i;
                    if (l.Any())
                    {
                        rtnList.Add(l.ToList()[0]);
                        categoryid = rtnList.Last().ParentItemId;
                    }
                    else
                        break;
                    c += 1;
                    if (c > 50) break; //stop possible infinate loop
                }
            }
            return rtnList;
        }

        private List<GroupCategoryData> GetGrpCatListFromDatabase(String lang = "")
        {

            var objCtrl = new NBrightBuyController();
            const string strOrderBy = " order by [XMLData].value('(genxml/hidden/recordsortorder)[1]','int') ";
            var grpcatList = new List<GroupCategoryData>();

            var l = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORY", "", strOrderBy, 0, 0, 0, 0, "", "");
            var lg = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORYLANG", "and NB1.lang = '" + lang + "'", "", 0, 0, 0, 0, "", "");
            var lx = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATCASCADE", "", "", 0, 0, 0, 0, "", "");
            var lx2 = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATXREF", "", "", 0, 0, 0, 0, "", "");
            lx.AddRange(lx2);
            foreach (var i in l)
            {
                var grpcat = new GroupCategoryData();
                grpcat.categoryid = i.ItemID;
                grpcat.recordsortorder = i.GetXmlPropertyInt("genxml/hidden/recordsortorder");
                grpcat.imageurl = i.GetXmlProperty("genxml/hidden/imageurl");
                grpcat.categoryref = i.GetXmlProperty("genxml/textbox/txtcategoryref");
                grpcat.archived = i.GetXmlPropertyBool("genxml/checkbox/chkarchived");
                grpcat.ishidden = i.GetXmlPropertyBool("genxml/checkbox/chkishidden");
                grpcat.grouptyperef = i.GetXmlProperty("genxml/dropdownlist/ddlgrouptype");
                grpcat.parentcatid = i.ParentItemId;
                grpcat.entrycount = GetEntryCount(lx, grpcat.categoryid);
                if (GroupsDictionary.ContainsKey(grpcat.grouptyperef)) grpcat.groupname = GroupsDictionary[grpcat.grouptyperef];

                // get the language data
                var langItem =  GetLangData(lg,grpcat.categoryid);
                if (langItem != null)
                {
                    grpcat.categoryname = langItem.GetXmlProperty("genxml/textbox/txtcategoryname");
                    grpcat.categorydesc = langItem.GetXmlProperty("genxml/textbox/txtcategorydesc");
                    grpcat.seoname = langItem.GetXmlProperty("genxml/textbox/txtseoname");
                    grpcat.metadescription = langItem.GetXmlProperty("genxml/textbox/txtmetadescription");
                    grpcat.metakeywords = langItem.GetXmlProperty("genxml/textbox/txtmetakeywords");
                    grpcat.seopagetitle = langItem.GetXmlProperty("genxml/textbox/txtseopagetitle");
                    grpcat.message = langItem.GetXmlProperty("genxml/edt/message");
                }

                //get parents
                var p = GetParentList(l,grpcat.categoryid);
                foreach (var pi in p)
                    grpcat.Parents.Add(pi.ItemID);

                grpcatList.Add(grpcat);
            }

            return grpcatList;

        }

        private void AddCatCascadeRecord(int categoryid,int productid)
        {
            var strGuid = categoryid.ToString("") + "x" + productid.ToString("");
            var nbi = _objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATCASCADE", strGuid);
            if (nbi == null)
            {
                nbi = new NBrightInfo();
                nbi.ItemID = -1;
                nbi.PortalId = PortalSettings.Current.PortalId;
                nbi.ModuleId = -1;
                nbi.TypeCode = "CATCASCADE";
                nbi.XrefItemId = categoryid;
                nbi.ParentItemId = productid;
                nbi.XMLData = null;
                nbi.TextData = null;
                nbi.Lang = null;
                nbi.GUIDKey = strGuid;
                _objCtrl.Update(nbi);
            }
        }

        #endregion

        #region "indexing"

        /// <summary>
        /// Reindex catcascade records for category and all parent categories 
        /// </summary>
        /// <param name="categoryid"></param>
        public void ReIndexCascade(int categoryid)
        {
            ReIndexSingleCascade(categoryid);
            var cat = GetCategory(categoryid);
            if (cat != null)
            {
                foreach (var p in cat.Parents)
                {
                    ReIndexSingleCascade(p);
                }                
            }
        }

        /// <summary>
        /// Rebuild the CATCASCADE index records for a single category
        /// </summary>
        /// <param name="categoryid"></param>
        private void ReIndexSingleCascade(int categoryid)
        {
            //get all category product ids from catxref sub category records.
            var xrefList = new List<NBrightInfo>();
            var prodItemIdList = xrefList.Select(r => r.ParentItemId).ToList();
            var catList = new List<GroupCategoryData>();
            var subCats = GetSubCategoryList(catList, categoryid);
            foreach (var c in subCats)
            {
                xrefList = _objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATXREF", " and xrefitemid = " + c.categoryid.ToString(""));
                prodItemIdList.AddRange(xrefList.Select(r => r.ParentItemId));
            }
            //Get the current catascade records
            xrefList = _objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATCASCADE", " and xrefitemid = " + categoryid.ToString(""));
            var casacdeProdItemIdList = xrefList.Select(r => r.ParentItemId).ToList();

            //Update the catcascade records.
            foreach (var prodId in prodItemIdList)
            {
                AddCatCascadeRecord(categoryid, prodId);
                casacdeProdItemIdList.RemoveAll(i => i == prodId);
            }
            //remove any cascade records that no longer exists
            foreach (var productid in casacdeProdItemIdList)
            {
                var strGuid = categoryid.ToString("") + "x" + productid.ToString("");
                var nbi = _objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "CATCASCADE", strGuid);
                if (nbi != null) _objCtrl.Delete(nbi.ItemID);
            }
        }


        #endregion

    }
}
