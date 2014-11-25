using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;
using NBrightCore.common;
using NBrightCore.render;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components.Interfaces;


namespace Nevoweb.DNN.NBrightBuy.Components
{
    public class OrderData : PurchaseData
    {

        public OrderData(int entryid)
        {
            PurchaseTypeCode = "ORDER";
            PopulatePurchaseData(entryid);
            PortalId = PurchaseInfo.PortalId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="portalId">Left to ensure backward compatiblity</param>
        /// <param name="entryid"></param>
        public OrderData(int portalId, int entryid)
        {
            PurchaseTypeCode = "ORDER";
            PortalId = portalId;
            PopulatePurchaseData(entryid);
        }

        public void ConvertToCart(Boolean debugMode = false, String storageType = "Cookie", string nameAppendix = "")
        {
            // only magers and editors allowed to edit orders
            if (UserController.GetCurrentUserInfo().IsInRole(StoreSettings.ManagerRole) ||
                UserController.GetCurrentUserInfo().IsInRole(StoreSettings.EditorRole) ||
                UserController.GetCurrentUserInfo().IsInRole("Administrators")) 
            {
                PurchaseTypeCode = "CART";
                EditMode = "E";
                var cartId = base.SavePurchaseData();
                var cartData = new CartData(PortalId, "", cartId.ToString("")); //create the client record (cookie)
                cartData.Save();
                if (debugMode) OutputDebugFile("debug_convertedorder.xml");
            }
        }

        public void CopyToCart(Boolean debugMode = false, String storageType = "Cookie", string nameAppendix = "")
        {
            PurchaseTypeCode = "CART";
            EditMode = "R";

            // reset order fields
            ShippedDate = "";
            OrderStatus = "010";
            TrackingCode = "";
            InvoiceFileExt = "";
            InvoiceFileName = "";
            InvoiceFilePath = "";

            var cartId = base.SavePurchaseData(true);
            var cartData = new CartData(PortalId,  "", cartId.ToString("")); //create the client record (cookie)

            cartData.Save();
            if (debugMode) OutputDebugFile("debug_copytocart.xml");
        }

        /// <summary>
        /// Order status
        /// 010,020,030,040,050,060,070,080,090,100,110
        /// Incomplete,Waiting for Bank,Cancelled,Payment OK,Payment Not Verified,Waiting for Payment,Waiting for Stock,Waiting,Shipped,Closed,Archived
        /// </summary>
        public String OrderStatus { 
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/dropdownlist/orderstatus");
            } 
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/dropdownlist/orderstatus", value);
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
        public String InvoiceDownloadName
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/hidden/invoicedownloadname");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/hidden/invoicedownloadname", value);
            }
        }

        public String OrderNumber
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/ordernumber");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/ordernumber", value);
            }
        }

        public String CreatedDate
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/createddate");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/createddate", value,TypeCode.DateTime);
            }
        }

        /// <summary>
        /// Save the internal key for to identify whcih payment provider is processing the order
        /// </summary>
        public String PaymentProviderKey
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/paymentproviderkey");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/paymentproviderkey", value);
            }
        }

        /// <summary>
        /// A payment passkey can be link to the order for security
        /// </summary>
        public String PaymentPassKey
        {
            get
            {
                return PurchaseInfo.GetXmlProperty("genxml/paymentpasskey");
            }
            set
            {
                PurchaseInfo.SetXmlProperty("genxml/paymentpasskey", value);
            }
        }

        public void PaymentOk(String orderStatus = "040", Boolean sendEmails = true)
        {
            if (OrderStatus == "020") // only process this on waiting for bank.  On return to cart this could be triggered more than once, but IPN and return.
            {
                var discountprov = DiscountCodeInterface.Instance();
                if (discountprov != null)
                {
                    PurchaseInfo = discountprov.UpdatePercentUsage(PortalId, UserId, PurchaseInfo);
                    PurchaseInfo = discountprov.UpdateVoucherAmount(PortalId, UserId, PurchaseInfo);                    
                }

                CreatedDate = DateTime.Now.ToString("O");
                ApplyModelTransQty();
                OrderStatus = orderStatus;
                SavePurchaseData();

                // Send emails
                if (sendEmails)
                {
                    NBrightBuyUtils.SendEmailOrderToClient("ordercreatedclientemail.html", PurchaseInfo.ItemID, "ordercreatedemailsubject");
                    NBrightBuyUtils.SendEmailToManager("ordercreatedemail.html", PurchaseInfo, "ordercreatedemailsubject");
                }                
            }

        }

        public void PaymentFail(String orderStatus = "010")
        {
            ReleaseModelTransQty();
            OrderStatus = orderStatus;
            PurchaseTypeCode = "CART";
            SavePurchaseData();
        }

    }
}
