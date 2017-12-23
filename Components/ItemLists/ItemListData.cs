using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;

namespace Nevoweb.DNN.NBrightBuy.Components
{

    /// <summary>
    /// Class to deal with itemlist cookie data.
    /// </summary>
    public class ItemListData
    {

        private HttpCookie _cookie;

        public Dictionary<string, string> listnames;
        public string products;
        public Dictionary<string, string> productsInList;
        public string listkeys;

        public int UserId;

        /// <summary>
        /// Populate class with cookie data
        /// </summary>
        /// <param name="listName"></param>
        public ItemListData()
        {
            UserId = UserController.Instance.GetCurrentUserInfo().UserID;
            CookieName = "NBSShoppingList";
            Exists = false;
            ItemCount = 0;
            listnames = new Dictionary<string, string>();
            products = "";
            productsInList = new Dictionary<string, string>();

            if (UserId > 0)
            {
                Get();
                SaveCookie();
            }
        }

        /// <summary>
        /// Save cookie to client
        /// </summary>
        public void SaveAll()
        {
            // if user then get from clientdata
            var currentUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            if (currentUserId > 0)
            {
                var clientData = new ClientData(PortalSettings.Current.PortalId, currentUserId);
                if (clientData.Exists)
                {
                    products = "";
                    listkeys = "";
                    foreach (var lname in listnames)
                    {
                        listkeys += lname.Key + "*";
                        clientData.UpdateItemList(lname.Key, productsInList[lname.Key]);
                        var l = clientData.GetItemList(lname.Key);
                        products += l;
                    }
                    clientData.Save();
                    SaveCookie();
                }
            }
        }

        public void SaveList(string listkey)
        {
            // if user then get from clientdata
            var currentUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            if (currentUserId > 0)
            {
                var clientData = new ClientData(PortalSettings.Current.PortalId, currentUserId);
                if (clientData.Exists)
                {
                    clientData.UpdateItemList(listkey, productsInList[listkey]);
                    clientData.Save();
                    products = "";
                    listkeys = "";
                    foreach (var list in listnames)
                    {
                        listkeys += list.Key + "*";
                        var l = clientData.GetItemList(list.Key);
                        products += l;
                    }
                    SaveCookie();
                }
            }
        }

        public void SaveCookie()
        {
            if (products.Length > 0)
            {
                Exists = true;
            }

            _cookie = HttpContext.Current.Request.Cookies[CookieName];
            if (_cookie == null)
            {
                _cookie = new HttpCookie(CookieName);
            }
            _cookie[CookieName] = products;
            _cookie.Expires = DateTime.MinValue;
            HttpContext.Current.Response.Cookies.Add(_cookie);
        }

        /// <summary>
        /// Get the cookie data from the client.
        /// </summary>
        /// <returns></returns>
        public ItemListData Get()
        {
                // if user then get from clientdata
            var currentUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            if (currentUserId > 0)
            {
                products = "";
                var clientData = new ClientData(PortalSettings.Current.PortalId, currentUserId);
                if (clientData.Exists)
                {
                    listkeys = "";
                    listnames = clientData.GetItemListNames();
                    foreach (var list in listnames)
                    {
                        listkeys += list.Key + "*";
                        var l = clientData.GetItemList(list.Key);
                        productsInList.Add(list.Key, l);
                        products += l;
                    }
                }
            }


            if (products.Length == 0)
            {
                Exists = false;
            }
            else
            {
                Exists = true;
                ItemCount = products.Length;
            }

            return this;
        }

        /// <summary>
        /// Delete cookie from client
        /// </summary>
        public void DeleteList(string listkey)
        {
            if (productsInList.ContainsKey(listkey))
            {
                productsInList.Remove(listkey);
                SaveAll();
            }
        }

        public void Add(string listkey, string itemId)
        {
            if (productsInList.ContainsKey(listkey))
            {
                productsInList[listkey] = productsInList[listkey] + itemId + "*";
            }
            else
            {
                productsInList.Add(listkey, itemId + "*");
                listnames.Add(listkey, listkey.Replace(" ","-"));
            }
            SaveList(listkey);
        }

        /// <summary>
        /// remove item from wishlist
        /// </summary>
        /// <param name="itemId"></param>
        public void Remove(string listkey, string itemId)
        {
            if (productsInList.ContainsKey(listkey))
            {
                productsInList[listkey] = productsInList[listkey].Replace(itemId + "*", "");
                SaveList(listkey);
            }
        }

        /// <summary>
        /// Return a generic list of itemid in the cookie
        /// </summary>
        /// <returns></returns>
        public List<String> GetItemList()
        {
            if (products == "") return null;
            var l = products.Split('*');
            var gl = new List<String>();
            foreach (var s in l)
            {
                if (s != "") gl.Add(s);
            }
            if (gl.Count == 0) return null;
            return gl;
        }

        public Boolean IsInList(String itemid)
        {
            if (products.Length == 0) return false;
            return GetItemList().Contains(itemid);
        }

        /// <summary>
        /// Cookie name
        /// </summary>
        public string CookieName { get; private set; }

        /// <summary>
        /// Set to true if cookie exists
        /// </summary>
        public bool Exists { get; private set; }

        /// <summary>
        /// Count of itemids to be included in the list
        /// </summary>
        public int ItemCount { get; private set; }



    }


}
