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
            EditMode = "E";
            var cartId = base.SavePurchaseData();
            var cartData = new CartData(PortalId,  "", cartId.ToString("")); //create the client record (cookie)
            cartData.ValidateCart();
            cartData.Save();
            if (debugMode) OutputDebugFile("debug_convertedorder.xml");
        }

        public void CopyToCart(Boolean debugMode = false, String storageType = "Cookie", string nameAppendix = "")
        {
            PurchaseTypeCode = "CART";
            EditMode = "R";
            var cartId = base.SavePurchaseData(true);
            var cartData = new CartData(PortalId,  "", cartId.ToString("")); //create the client record (cookie)
            cartData.ValidateCart();
            cartData.Save();
            if (debugMode) OutputDebugFile("debug_copytocart.xml");
        }


        public String OrderStatus { 
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/textbox/orderstatus");
            } 
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/textbox/orderstatus", value);
                PurchaseInfo.GUIDKey = value;
            }  
        }

        public String ShippedDate
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/textbox/shippingdate");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/textbox/shippingdate", value, TypeCode.DateTime);
            }
        }
        public String OrderPlacedDate
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/textbox/orderplaceddate");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/textbox/orderplaceddate", value, TypeCode.DateTime);
            }
        }
        public String TrackingCode
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/textbox/trackingcode");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/textbox/trackingcode", value);
            }
        }

        public String InvoiceFilePath
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefilepath");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilepath", value);
            }
        }
        public String InvoiceFileName
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefilename");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefilename", value);
            }
        }
        public String InvoiceFileExt
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/hidden/invoicefileext");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/hidden/invoicefileext", value);
            }
        }

    }
}
