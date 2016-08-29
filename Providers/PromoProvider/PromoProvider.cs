using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Xml;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers.PromoProvider
{
    public class GroupPromoProvider : Components.Interfaces.PromoInterface
    {
        public override string ProviderKey { get; set; }
        public override string SchedulerPromotionCalc(int portalId)
        {
            var objCtrl = new NBrightBuyController();
            var l = objCtrl.GetList(portalId, -1, "CATEGORYPROMO", "", "", 0, 0, 0, 0, Utils.GetCurrentCulture());
            foreach (var p in l)
            {
                var typeselect = p.GetXmlProperty("genxml/radiobuttonlist/typeselect");
                var catgroupid = p.GetXmlProperty("genxml/dropdownlist/catgroupid");
                var propgroupid = p.GetXmlProperty("genxml/dropdownlist/propgroupid");
                var promoname = p.GetXmlProperty("genxml/textbox/name");
                var amounttype = p.GetXmlProperty("genxml/radiobuttonlist/amounttype");
                var amount = p.GetXmlPropertyDouble("genxml/textbox/amount");
                var validfrom = p.GetXmlProperty("genxml/textbox/validfrom");
                var validuntil = p.GetXmlProperty("genxml/textbox/validuntil");
                var overwrite = p.GetXmlPropertyBool("genxml/checkbox/overwrite");
                var disabled = p.GetXmlPropertyBool("genxml/checkbox/disabled");

                if (!disabled)
                {
                    if (Utils.IsDate(validfrom) && Utils.IsDate(validuntil))
                    {
                        var dteF = Convert.ToDateTime(validfrom);
                        var dteU = Convert.ToDateTime(validuntil);
                        if (DateTime.Now > dteU)
                        {
                            p.SetXmlProperty("genxml/checkbox/disabled","True");
                            objCtrl.Update(p);
                        }
                        else
                        {
                            if (DateTime.Now >= dteF && DateTime.Now <= dteU)
                            {
                                CategoryData gCat;
                                var groupid = catgroupid;
                                if (typeselect != "cat") groupid = propgroupid;

                                gCat = CategoryUtils.GetCategoryData(groupid, Utils.GetCurrentCulture());
                                var prdList = gCat.GetCascadeArticles();

                                foreach (var prd in prdList)
                                {
                                    CalcProductSalePrice(prd.ItemID, amounttype, amount,promoname ,overwrite);
                                }

                                p.SetXmlProperty("genxml/hidden/lastcalculated", DateTime.UtcNow.ToString("u"));
                                objCtrl.Update(p);
                            }
                        }
                    }

                }

            }
            return "OK";
        }

        public override string ProductPromotionCalc(int portalId, int productId)
        {
            throw new NotImplementedException();
        }


        private void CalcProductSalePrice(int productId,string amounttype,double amount, string promoname, bool overwrite)
        {
            var prdData = ProductUtils.GetProductData(productId, Utils.GetCurrentCulture(), false);
            var nodList = prdData.DataRecord.XMLDoc.SelectNodes("genxml/models/genxml");
            if (nodList != null)
            {
                var lp = 1;
                foreach (XmlNode nod in nodList)
                {
                    var nbi = new NBrightInfo();
                    nbi.XMLData = nod.OuterXml;
                    var unitcost = nbi.GetXmlPropertyDouble("genxml/textbox/txtunitcost");
                    Double newamt = 0;
                    if (amounttype == "1")
                    {
                        newamt = unitcost - amount;
                    }
                    else
                    {
                        newamt = unitcost - ((unitcost / 100) * amount);
                    }

                    if (newamt < 0) newamt = 0;

                    prdData.DataRecord.SetXmlPropertyDouble("genxml/modles/genxml[" + lp + "]/textbox/txtsaleprice", newamt);
                    lp += 1;
                }
                prdData.Save();
            }

        }

    }

    public class MultiBuyPromoProvider : Components.Interfaces.PromoInterface
    {
        public override string ProviderKey { get; set; }
        public override string SchedulerPromotionCalc(int portalId)
        {
            throw new NotImplementedException();
        }

        public override string ProductPromotionCalc(int portalId, int productId)
        {
            throw new NotImplementedException();
        }
    }

}
