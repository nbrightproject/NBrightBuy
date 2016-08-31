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
        #region "Group promo"

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
                                EndProductSalePrice(portalId,prd.ParentItemId, p.ItemID);
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

        public static string RemoveGroupProductPromo(int portalId,int promoid)
        {
            var objCtrl = new NBrightBuyController();
            var p = objCtrl.GetData(promoid);

            var typeselect = p.GetXmlProperty("genxml/radiobuttonlist/typeselect");
            var catgroupid = p.GetXmlProperty("genxml/dropdownlist/catgroupid");
            var propgroupid = p.GetXmlProperty("genxml/dropdownlist/propgroupid");

            CategoryData gCat;
            var groupid = catgroupid;
            if (typeselect != "cat") groupid = propgroupid;

            gCat = CategoryUtils.GetCategoryData(groupid, Utils.GetCurrentCulture());
            var prdList = gCat.GetAllArticles();

            foreach (var prd in prdList)
            {
                // END Promo
                EndProductSalePrice(portalId,prd.ParentItemId, p.ItemID);
                ProductUtils.RemoveProductDataCache(prd.PortalId, prd.ParentItemId);
            }
            return "OK";
        }

        private static void EndProductSalePrice(int portalId,int productId, int promoid)
        {
            var prdData = ProductUtils.GetProductData(productId, Utils.GetCurrentCulture(), false);
            var nodList = prdData.DataRecord.XMLDoc.SelectNodes("genxml/models/genxml");
            if (nodList != null)
            {
                var currentpromoid = prdData.DataRecord.GetXmlPropertyInt("genxml/hidden/promoid");
                if (currentpromoid == promoid)
                {
                    var lp = 1;
                    foreach (XmlNode nod in nodList)
                    {
                        prdData.DataRecord.SetXmlPropertyDouble("genxml/models/genxml[" + lp + "]/textbox/txtsaleprice", 0);
                        lp += 1;
                    }
                    prdData.Save();
                    RemoveProductPromoData(portalId, productId, promoid);
                }
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
                    prdData.DataRecord.SetXmlPropertyDouble("genxml/hidden/promotype", "PROMOGROUP");
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
                var objCtrl = new NBrightBuyController();
                if (currentpromoid == promoid || overwrite)
                {
                    foreach (var lang in DnnUtils.GetCultureCodeList(prdData.Info.PortalId))
                    {
                        var p = objCtrl.GetDataLang(promoid,lang);
                        var prdData2 = ProductUtils.GetProductData(productId, lang, false);
                        prdData2.DataLangRecord.SetXmlProperty("genxml/hidden/promodesc",p.GetXmlProperty("genxml/textbox/description"));
                        prdData2.Save();
                    }
                }
            }

        }

        #endregion

        #region "Multi-Buy"

        public static string CalcMultiBuyPromo(int portalId)
        {
            var objCtrl = new NBrightBuyController();
            var l = objCtrl.GetList(portalId, -1, "MULTIBUYPROMO", "", "", 0, 0, 0, 0, Utils.GetCurrentCulture());
            foreach (var p in l)
            {
                var propgroupid = p.GetXmlProperty("genxml/dropdownlist/propbuy");
                var promoname = p.GetXmlProperty("genxml/textbox/name");
                var validfrom = p.GetXmlProperty("genxml/textbox/validfrom");
                var validuntil = p.GetXmlProperty("genxml/textbox/validuntil");
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
                        var groupid = propgroupid;

                        gCat = CategoryUtils.GetCategoryData(groupid, Utils.GetCurrentCulture());
                        var prdList = gCat.GetAllArticles();

                        foreach (var prd in prdList)
                        {
                            if (DateTime.Now.Date >= dteF && DateTime.Now.Date <= dteU)
                            {
                                // CALC Promo
                                FlagProductMultiBuy(portalId,prd.ParentItemId, promoname, p.ItemID);
                            }
                            if (DateTime.Now.Date > dteU)
                            {
                                // END Promo
                                RemoveProductPromoData(portalId, prd.ParentItemId, p.ItemID);
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

        private static void FlagProductMultiBuy(int portalid,int productId, string promoname, int promoid)
        {
            var cultureList = DnnUtils.GetCultureCodeList(portalid);
            var validlang = cultureList.First();

            var prdData = ProductUtils.GetProductData(productId, validlang, false);
            var nodList = prdData.DataRecord.XMLDoc.SelectNodes("genxml/models/genxml");
            if (nodList != null)
            {

                var currentpromoid = prdData.DataRecord.GetXmlPropertyInt("genxml/hidden/promoid");
                if (currentpromoid == promoid)
                {
                    prdData.DataRecord.SetXmlPropertyDouble("genxml/hidden/promotype", "PROMOMULTIBUY");
                    prdData.DataRecord.SetXmlPropertyDouble("genxml/hidden/promoname", promoname);
                    prdData.DataRecord.SetXmlProperty("genxml/hidden/promoid", promoid.ToString());
                    prdData.DataRecord.SetXmlProperty("genxml/hidden/promocalcdate", DateTime.Now.ToString("O"));
                }

                prdData.Save();
                var objCtrl = new NBrightBuyController();
                if (currentpromoid == promoid)
                {
                    foreach (var lang in cultureList)
                    {
                        var p = objCtrl.GetDataLang(promoid, lang);
                        var prdData2 = ProductUtils.GetProductData(productId, lang, false);
                        prdData2.DataLangRecord.SetXmlProperty("genxml/hidden/promodesc", p.GetXmlProperty("genxml/textbox/description"));
                        prdData2.Save();
                    }
                }
            }

        }


        #endregion


        #region "Shared"

        private static void RemoveProductPromoData(int portalid, int productId, int promoid)
        {
            var cultureList = DnnUtils.GetCultureCodeList(portalid);
            var validlang = cultureList.First();

            var prdData = ProductUtils.GetProductData(productId, validlang, false);
            var currentpromoid = prdData.DataRecord.GetXmlPropertyInt("genxml/hidden/promoid");
            if (currentpromoid == promoid)
            {
                prdData.DataRecord.RemoveXmlNode("genxml/hidden/promotype");
                prdData.DataRecord.RemoveXmlNode("genxml/hidden/promoname");
                prdData.DataRecord.RemoveXmlNode("genxml/hidden/promoid");
                prdData.DataRecord.RemoveXmlNode("genxml/hidden/promocalcdate");
                prdData.Save();
                foreach (var lang in cultureList)
                {
                    var prdData2 = ProductUtils.GetProductData(productId, lang, false);
                    prdData2.DataLangRecord.RemoveXmlNode("genxml/hidden/promodesc");
                    prdData2.Save();
                }
            }
        }

        #endregion
    }
}
