using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Web.DDRMenu;
using NBrightCore.common;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class DdrMenuInterface : INodeManipulator
    {
        #region Implementation of INodeManipulator
        String _catid = "";
		String _tabid = "";
		String _catguidkey = "";

        public List<MenuNode> ManipulateNodes(List<MenuNode> nodes, DotNetNuke.Entities.Portals.PortalSettings portalSettings)
        {
            // jump out if we don't have [CAT] token in nodes
            if (nodes.Count(x => x.Text.ToUpper() == "[CAT]") == 0) return nodes;

            var nameValueCollection = HttpUtility.ParseQueryString(HttpContext.Current.Request.Url.Query);
            _catid = nameValueCollection["catid"];
			_catguidkey = nameValueCollection["category"];
            
            _tabid = PortalSettings.Current.ActiveTab.TabID.ToString("");

            var defaultListPage = "";
            defaultListPage = StoreSettings.Current.Get("productlisttab");

            var catNodeList = GetCatNodeXml(_tabid, 0, true, 0, null, defaultListPage);

            // [TODO: We want to cache this data, but doing so brakes the menu, becuase the cache is by Reference to the object, which is changed on display to fit into the menu.  We can't easily clone the object because MenuNode doesn't support IClone, need to find a way around this.]
            //const string cacheListKey = "catnodesxml_list"; // keep a list of menu nodes so we can remove them
            //var cacheKey = "catnodesxml_" + _tabid;
            //var catNodeList = (List<MenuNode>)Utils.GetCache(cacheKey);
            //if (catNodeList == null)
            //{
            //    catNodeList = GetCatNodeXml(_tabid);
            //    Utils.SetCache(cacheKey, catNodeList);
            //    var keylist = (List<String>)Utils.GetCache(cacheListKey) ?? new List<string>();
            //    keylist.Add(cacheKey);
            //    Utils.SetCache(cacheListKey, keylist);
            //}             
            //if (catNodeList.Count >= 1) catNodeList[0].Parent = null;
            //Utils.RemoveCache(cacheKey);

            // see if we need to merge into the current pages, by searching for marker page [cat]
            int idx = 0;
            var catNods = new Dictionary<int,MenuNode>();
            foreach (var n in nodes)
            {
                if (n.Text.ToLower() == "[cat]")
                {
                    catNods.Add(idx,n);
					break;
                }
                idx += 1;
            }


            if (catNods.Count > 0)
            {
				foreach (var catNod in catNods)
				{
					// remove marker page [cat]
					nodes.Remove(catNod.Value);
				}
                var insidx = idx;
                foreach (var n2 in catNodeList)
                {
                    if (n2.Parent == null)
                    {
                        nodes.Insert(insidx, n2);
                    }
                    insidx += 1;
                }                
            }

            return nodes;
        }


        private List<MenuNode> GetCatNodeXml(string currentTabId, int parentItemId = 0, bool recursive = true, int depth = 0, MenuNode pnode = null, string defaultListPage = "")
        {
            
            var nodes = new List<MenuNode>();
            //[TODO: Add images onto DDRMenu]
            //var objS = objCtrl.GetByGuidKey(PortalSettings.Current.PortalId, -1, "SETTINGS", "NBrightBuySettings");
            //var imgFolder = objS.GetXmlProperty("genxml/textbox/txtuploadfolder");
            //var defimgsize = objS.GetXmlProperty("genxml/textbox/txtsmallimgsize");

            //var l = objCtrl.GetList(PortalSettings.Current.PortalId, -1, "CATEGORY", strFilter, strOrderBy, 0, 0, 0, 0, "CATEGORYLANG", Utils.GetCurrentCulture());

            var grpCatCtrl = new GrpCatController(Utils.GetCurrentCulture());

            var l = grpCatCtrl.GetCategories(parentItemId);

            foreach (var obj in l)
            {
                if (!obj.ishidden)
                {

                    var n = new MenuNode();

                    n.Parent = pnode;

                    n.TabId = obj.categoryid;
                    n.Text = obj.categoryname;
                    n.Title = obj.categorydesc;

                    var tabid = "";
                    if (Utils.IsNumeric(defaultListPage)) tabid = defaultListPage;
                    if (tabid == "") tabid = currentTabId;
                    if (Utils.IsNumeric(tabid)) n.Url = grpCatCtrl.GetCategoryUrl(obj, Convert.ToInt32((tabid)));

                    n.Enabled = true;
                    if (obj.disabled) n.Enabled = false;
                    n.Selected = false;
                    if (_catid == obj.categoryid.ToString("")) n.Selected = true;
                    n.Breadcrumb = false;
                    if (_catid == obj.categoryid.ToString("")) n.Breadcrumb = true;
                    n.Separator = false;
                    n.LargeImage = "";
                    n.Icon = "";
                    var img = obj.imageurl;
                    if (img != "")
                    {
                        n.LargeImage = img;
                        n.Icon = StoreSettings.NBrightBuyPath() + "/NBrightThumb.ashx?w=50&h=50&src=/" + img.TrimStart('/');
                    }
                    n.Keywords = obj.metakeywords;
                    n.Description = obj.metadescription;
                    n.CommandName = "";
                    //n.CommandArgument = string.Format("entrycount={0}|moduleid={1}", obj.GetXmlProperty("genxml/hidden/entrycount"), obj.ModuleId.ToString(""));
                    n.CommandArgument = obj.entrycount.ToString(""); // not used, so we use it to store the entry count

                    if (recursive && depth < 50) //stop infinate loop, only allow 50 sub levels
                    {
                        depth += 1;
                        var childrenNodes = GetCatNodeXml(tabid, obj.categoryid, true, depth, n, defaultListPage);
                        if (childrenNodes.Count > 0)
                        {
                            n.Children = childrenNodes;
                        }
                    }

                    nodes.Add(n);
                }

            }

            return nodes;

        }


        #endregion
    }

   
}
