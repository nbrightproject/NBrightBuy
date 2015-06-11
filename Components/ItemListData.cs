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
        /// <param name="listName"></param>
        public ItemListData(String listName = "ItemList")
        {

            Exists = false;
            CookieName = listName;
            _request = HttpContext.Current.Request;
            _response = HttpContext.Current.Response;
            Get();
        }

        /// <summary>
        /// Save cookie to client
        /// </summary>
        public void Save(List<String> itemIdList)
        {
            ItemList = "";
            foreach (var i in itemIdList)
            {
                ItemList += i + "*";
            }
            ItemCount = itemIdList.Count;
            if (ItemList != "") _cookie["ItemList"] = ItemList;
            _cookie.Expires = DateTime.Now.AddDays(1d);
            _response.Cookies.Add(_cookie);
            Exists = true;
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
                    _cookie = new HttpCookie(CookieName);
                }
                else
                {
                    ItemList = _cookie.Value;
                }

            if (ItemList == "") // "Exist" property not used for paging data
                Exists = false;
            else
            {
                Exists = true;
                var l = GetItemList();
                ItemCount = l.Count;
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
                    ItemCount = 0;
                    ItemList = "";
                    Exists = false;
                }
        }
        /// <summary>
        /// Add Item to wishlist
        /// </summary>
        /// <param name="itemId"></param>
        public void Add(String itemId)
        {
            //set search cookie for the ref module
            var l = GetItemList();
            if (!l.Contains(itemId))
            {
                l.Add(itemId);
                Save(l);
            }
        }

        /// <summary>
        /// remove item from wishlist
        /// </summary>
        /// <param name="itemId"></param>
        public void Remove(String itemId)
        {
            var l = GetItemList();
            if (l.Contains(itemId))
            {
                l.Remove(itemId);
                Save(l);
            }
        }

        /// <summary>
        /// Return a generic list of itemid in the cookie
        /// </summary>
        /// <returns></returns>
        public List<String> GetItemList()
        {
            if (ItemList == "") return null;
            var l = ItemList.Split('*');
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
            if (ItemList == "") return false;
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
        /// List of itemids to be included in the list
        /// </summary>
        public string ItemList { get; set; }

        /// <summary>
        /// Count of itemids to be included in the list
        /// </summary>
        public int ItemCount { get; private set; }
    }


}
