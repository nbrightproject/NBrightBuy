using System;
using System.Collections.Generic;
using System.Web;
using System.Xml;
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

        private readonly HttpRequest _request;
        private readonly HttpResponse _response;
        private HttpCookie _cookie;

        /// <summary>
        /// Populate class with cookie data
        /// </summary>
        /// <param name="request"> </param>
        /// <param name="response"> </param>
        /// <param name="moduleid"> </param>
        /// <param name="tabid"> </param>
        public ItemListData(HttpRequest request, HttpResponse response, int moduleid, String listName = "ItemList")
        {
            Exists = false;
            _request = request;
            _response = response;
            CookieName = "NBrightBuy_" + moduleid.ToString("") + "_" + listName;

            Get();
        }

        /// <summary>
        /// Save cookie to client
        /// </summary>
        public void Save()
        {
            if (ItemList != "") _cookie["ItemList"] = ItemList;
            if (ItemCount != "") _cookie["ItemCount"] = ItemCount;

            Exists = true;

            _cookie.Expires = DateTime.Now.AddDays(1d);
            _response.Cookies.Add(_cookie);

        }

        /// <summary>
        /// Get the cookie data from the client.
        /// </summary>
        /// <returns></returns>
        public ItemListData Get()
        {
            ItemList = "";

            _cookie = _request.Cookies[CookieName];
            if (_cookie == null)
            {
                Exists = false;
                _cookie = new HttpCookie(CookieName);
            }
            else
            {
                if (_cookie["ItemList"] != null) ItemList = _cookie["ItemList"];
                if (_cookie["ItemCount"] != null) ItemCount = _cookie["ItemCount"];
                Exists = true;
            }

            return this;
        }

        /// <summary>
        /// Delete cookie from client
        /// </summary>
        public void Delete()
        {
            if (_cookie != null)
            {
                _cookie.Expires = DateTime.Now.AddDays(-1d);
                _response.Cookies.Add(_cookie);
                ItemCount = "";
                ItemList = "";
                Exists = false;
            };
        }
        /// <summary>
        /// Add Item to wishlist
        /// </summary>
        /// <param name="itemId"></param>
        public void Add(String itemId)
        {
            //set search cookie for the ref module
            if (ItemList == "") ItemList = ","; // start with a delimeter, so we can make sure we can search for the full itemid
            if (!ItemList.Contains("," + itemId + ","))
            {
                ItemList = ItemList + itemId + ",";
                ItemCount = (ItemList.Split(',').Length - 2).ToString("");
                Save();
            }
        }

        /// <summary>
        /// remove item from wishlist
        /// </summary>
        /// <param name="itemId"></param>
        public void Remove(String itemId)
        {
            //set search cookie for the ref module
            if (ItemList.Contains("," + itemId + ","))
            {
                ItemList = ItemList.Replace("," + itemId + ",", ",");
                ItemCount = (ItemList.Split(',').Length - 2).ToString("");
                Save();
            }
        }

        /// <summary>
        /// Return a generic list of itemid in the cookie
        /// </summary>
        /// <returns></returns>
        public List<String> GetItemList()
        {
            if (ItemList == "") return null;
            var l = ItemList.Split(',');
            var gl = new List<String>();
            foreach (var s in l)
            {
                if (s != "") gl.Add(s);
            }
            if (gl.Count == 0) return null;
            return gl;
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
        /// List of itemids to be included in the wishlist
        /// </summary>
        public string ItemList { get; set; }

        /// <summary>
        /// Count of itemids to be included in the wishlist
        /// </summary>
        public string ItemCount { get; set; }
        /// <summary>
        /// wishlist is active for bi-view modules
        /// </summary>
        public bool Active { get; set; }
    }


}
