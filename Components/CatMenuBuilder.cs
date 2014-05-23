using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightCore.render;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    /// <summary>
    /// Syntax class to keep logic and syntax simple in CategoryMenu.ascx.cs
    /// </summary>
    public class CatMenuBuilder
    {

        private readonly GrpCatController _catGrpCtrl;
        private readonly NBrightBuyController _ctrlObj;
        private readonly Boolean _debugMode;
        private readonly ModSettings _modSettings;
        private readonly List<string> _templateBody;

        public CatMenuBuilder(String templateBody, ModSettings modSettings, Boolean debugMode)
        {
            _catGrpCtrl = new GrpCatController(Utils.GetCurrentCulture());
            _ctrlObj = new NBrightBuyController();
            _modSettings = modSettings;
            _debugMode = debugMode;
            _templateBody = GetMenuTemplates(templateBody);

        }

        public string GetTreeCatList(int displaylevels = 20, int parentid = 0, int tabid = 0, String identClass = "nbrightbuy_catmenu", String styleClass = "", String activeClass = "active")
        {
            if (tabid == 0) tabid = PortalSettings.Current.ActiveTab.TabID;
            var rtnList = "";
            var strCacheKey = "NBrightBuy_GetTreeCatList" + PortalSettings.Current.PortalId + "*" + displaylevels + "*" + parentid + "*" + Utils.GetCurrentCulture() + "*" + StoreSettings.Current.ActiveCatId;
            var objCache = NBrightBuyUtils.GetModCache(strCacheKey);
            if (objCache == null | StoreSettings.Current.DebugMode)
            {
                rtnList = BuildTreeCatList(rtnList, 0, parentid, "cat", tabid, displaylevels,identClass,styleClass,activeClass);
                NBrightBuyUtils.SetModCache(-1, strCacheKey, rtnList);
            }
            else
            {
                rtnList = (string)objCache;
            }
            return rtnList;
        }


        private String BuildTreeCatList(String rtnList, int level, int parentid, string groupref, int tabid, int displaylevels = 50, String identClass = "nbrightbuy_catmenu", String styleClass = "", String activeClass = "active")
        {
            if (level > displaylevels) return rtnList; // stop infinate loop

            // header
            if (level == 0)
                rtnList += "<ul class='" + identClass + " " + styleClass + "'>";
            else
                rtnList += "<ul>";

            var activeCat = _catGrpCtrl.GetCategory(StoreSettings.Current.ActiveCatId);
            if (activeCat == null) activeCat = new GroupCategoryData();
            var depth = 0;
            var levelList = _catGrpCtrl.GetGrpCategories(parentid, groupref);
            foreach (GroupCategoryData grpcat in levelList)
            {
                if (grpcat.isvisible)
                {
                    // update cat info
                    grpcat.url = _catGrpCtrl.GetCategoryUrl(grpcat, tabid);
                    grpcat.depth = level; //make base 1, to pick up the

                    var openClass = "";
                    if (activeCat.Parents.Contains(grpcat.categoryid) || grpcat.categoryid == StoreSettings.Current.ActiveCatId) openClass = " open ";

                    if (StoreSettings.Current.ActiveCatId == grpcat.categoryid)
                        rtnList += "<li class='" + activeClass + openClass + "'>";
                    else
                    {
                        if (openClass == "")
                            rtnList += "<li>";
                        else 
                            rtnList += "<li class='" + openClass + "'>";
                    }

                    //body 
                    if (_templateBody.Count > grpcat.depth) depth = grpcat.depth;
                    rtnList += GenXmlFunctions.RenderRepeater(grpcat, _templateBody[depth], "", "XMLData", "", StoreSettings.Current.Settings());

                    rtnList = BuildTreeCatList(rtnList, level + 1, grpcat.categoryid, groupref, tabid, displaylevels);

                    rtnList += "</li>";

                }
            }

            //footer
            rtnList += "</ul>";

            return rtnList;
        }

        private List<string> GetMenuTemplates(string basetempl)
        {
            var rtnL = new List<string>();
            var templ = _ctrlObj.GetTemplateData(_modSettings, basetempl, Utils.GetCurrentCulture(), _debugMode);
            rtnL.Add(templ);

            var lp = 1;
            templ = _ctrlObj.GetTemplateData(_modSettings, basetempl.Replace(".html", lp.ToString("") + ".html"), Utils.GetCurrentCulture(), _debugMode);
            while (!String.IsNullOrEmpty(templ))
            {
                lp += 1;
                rtnL.Add(templ);
                templ = _ctrlObj.GetTemplateData(_modSettings, basetempl.Replace(".html", lp.ToString("") + ".html"), Utils.GetCurrentCulture(), _debugMode);
            }

            return rtnL;
        }


    }
}
