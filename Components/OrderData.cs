using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using NEvoWeb.Modules.NB_Store;

namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class OrderData : PurchaseData
    {

        public OrderData(int portalId, CartData cartData)
        {
            PurchaseTypeCode = "ORDER";
            PortalId = portalId;
            PopulatePurchaseData(cartData.GetInfo().ItemID); // move cart from "CART" type to "ORDER" (Cart no loanger exists at this point, it becomes an order.)
        }

        public OrderData(int portalId, int entryid)
        {
            PurchaseTypeCode = "ORDER";
            PortalId = portalId;
            PopulatePurchaseData(entryid);
        }

        public void ConvertToCart(Boolean debugMode = false, String storageType = "Cookie", string nameAppendix = "")
        {
            PurchaseTypeCode = "CART";
            var cartId = base.Save();
            var cartData = new CartData(PortalId, StoreSettings.Current.StorageTypeClient, "", cartId.ToString("")); //create the client record (cookie)
            if (debugMode) OutputDebugFile("debug_convertedorder.xml");
        }

        public void CopyToCart(Boolean debugMode = false, String storageType = "Cookie", string nameAppendix = "")
        {
            PurchaseTypeCode = "CART";
            var cartId = base.Save(true);
            var cartData = new CartData(PortalId, StoreSettings.Current.StorageTypeClient, "", cartId.ToString("")); //create the client record (cookie)
            if (debugMode) OutputDebugFile("debug_copytocart.xml");
        }


        //TODO: these need to persist to the DB
        public bool Status { get; set; }        
        public DateTime ShippedDate { get; set; }
        public DateTime OrderPlacedDate { get; set; }
        public String TrackingCode { get; set; }


    }
}
