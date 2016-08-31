using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace Nevoweb.DNN.NBrightBuy.Providers.PromoProvider
{
    public static class PromoUtils
    {

        public static string CalcGroupPromo(int portalId)
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
                var lastcalculated = p.GetXmlProperty("genxml/hidden/lastcalculated");

                if (!disabled)
                {
                    var runcalc = true;
                    if (Utils.IsDate(lastcalculated))
                    {
                        if (Convert.ToDateTime(lastcalculated) >= p.ModifiedDate) runcalc = false; // don't run if no change.
                    }
                    if (Utils.IsDate(validuntil))
                    {
                        if (DateTime.Now.Date > Convert.ToDateTime(validuntil)) runcalc = true; // need to disable the promo if passed date
                    }
                    if ((runcalc) && Utils.IsDate(validfrom) && Utils.IsDate(validuntil))
                    {
                        var dteF = Convert.ToDateTime(validfrom).Date;
                        var dteU = Convert.ToDateTime(validuntil).Date;
                        CategoryData gCat;
                        var groupid = catgroupid;
                        if (typeselect != "cat") groupid = propgroupid;

                        gCat = CategoryUtils.GetCategoryData(groupid, Utils.GetCurrentCulture());
                        var prdList = gCat.GetAllArticles();

                        foreach (var prd in prdList)
                        {
                            if (DateTime.Now.Date >= dteF && DateTime.Now.Date <= dteU)
                            {
                                // CALC Promo
                                CalcProductSalePrice(prd.ParentItemId, amounttype, amount, promoname, p.ItemID, overwrite);
                            }
                            if (DateTime.Now.Date > dteU)
                            {
                                // END Promo
                                EndProductSalePrice(prd.ParentItemId, amounttype, amount, promoname, p.ItemID, overwrite);
                                p.SetXmlProperty("genxml/checkbox/disabled", "True");
                                objCtrl.Update(p);
                            }
                            ProductUtils.RemoveProductDataCache(portalId, prd.ParentItemId);
                        }

                        p.SetXmlProperty("genxml/hidden/lastcalculated", DateTime.Now.AddSeconds(10).ToString("O")); // Add 10 sec to time so we don't get exact clash with update time.
                        objCtrl.Update(p);
                    }
                }
            }
            return "OK";
        }

        public static string RemoveGroupProductPromo(int promoid)
        {
            var objCtrl = new NBrightBuyController();
            var p = objCtrl.GetData(promoid);

            var typeselect = p.GetXmlProperty("genxml/radiobuttonlist/typeselect");
            var catgroupid = p.GetXmlProperty("genxml/dropdownlist/catgroupid");
            var propgroupid = p.GetXmlProperty("genxml/dropdownlist/propgroupid");
            var promoname = p.GetXmlProperty("genxml/textbox/name");
            var amounttype = p.GetXmlProperty("genxml/radiobuttonlist/amounttype");
            var amount = p.GetXmlPropertyDouble("genxml/textbox/amount");
            var overwrite = p.GetXmlPropertyBool("genxml/checkbox/overwrite");

            CategoryData gCat;
            var groupid = catgroupid;
            if (typeselect != "cat") groupid = propgroupid;

            gCat = CategoryUtils.GetCategoryData(groupid, Utils.GetCurrentCulture());
            var prdList = gCat.GetAllArticles();

            foreach (var prd in prdList)
            {
                // END Promo
                EndProductSalePrice(prd.ParentItemId, amounttype, amount, promoname, p.ItemID, overwrite);
                ProductUtils.RemoveProductDataCache(prd.PortalId, prd.ParentItemId);
            }
            return "OK";
        }

        private static void EndProductSalePrice(int productId, string amounttype, double amount, string promoname, int promoid, bool overwrite)
        {
            var prdData = ProductUtils.GetProductData(productId, Utils.GetCurrentCulture(), false);
            var nodList = prdData.DataRecord.XMLDoc.SelectNodes("genxml/models/genxml");
            if (nodList != null)
            {
                var currentpromoid = prdData.DataRecord.GetXmlPropertyInt("genxml/hidden/promoid");
                if (currentpromoid == promoid)
                {
                    prdData.DataRecord.RemoveXmlNode("genxml/hidden/promoname");
                    prdData.DataRecord.RemoveXmlNode("genxml/hidden/promoid");
                    prdData.DataRecord.RemoveXmlNode("genxml/hidden/promocalcdate");
                    var lp = 1;
                    foreach (XmlNode nod in nodList)
                    {
                        prdData.DataRecord.SetXmlPropertyDouble("genxml/models/genxml[" + lp + "]/textbox/txtsaleprice", 0);
                        lp += 1;
                    }
                }
                prdData.Save();
            }
        }

        private static void CalcProductSalePrice(int productId, string amounttype, double amount, string promoname, int promoid, bool overwrite)
        {
            var prdData = ProductUtils.GetProductData(productId, Utils.GetCurrentCulture(), false);
            var nodList = prdData.DataRecord.XMLDoc.SelectNodes("genxml/models/genxml");
            if (nodList != null)
            {

                var currentpromoid = prdData.DataRecord.GetXmlPropertyInt("genxml/hidden/promoid");
                if (currentpromoid == promoid || overwrite)
                {
                    prdData.DataRecord.SetXmlPropertyDouble("genxml/hidden/promoname", promoname);
                    prdData.DataRecord.SetXmlProperty("genxml/hidden/promoid", promoid.ToString());
                    prdData.DataRecord.SetXmlProperty("genxml/hidden/promocalcdate", DateTime.Now.ToString("O"));
                }

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

                    var currentprice = prdData.DataRecord.GetXmlPropertyDouble("genxml/models/genxml[" + lp + "]/textbox/txtsaleprice");

                    if (!overwrite)
                    {
                        if (currentprice == 0) overwrite = true;
                        if (currentpromoid == promoid) overwrite = true;
                    }
                    if (overwrite)
                    {
                        prdData.DataRecord.SetXmlPropertyDouble("genxml/models/genxml[" + lp + "]/textbox/txtsaleprice", newamt);
                    }
                    lp += 1;
                }
                prdData.Save();
            }

        }

    }
}
