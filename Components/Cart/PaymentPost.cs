namespace Nevoweb.DNN.NBrightBuy.Components.Cart
{
    public class PaymentPost
    {
        private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();
        public string Url = "";
        public string Method = "post";
        public string FormName = "form";
        public void Add(string name, string value)
        {
            Inputs.Add(name, value);
        }

        public string GetPostHtml(string Url)
        {
            string sipsHtml = "";

            sipsHtml = "<html><head>";
            sipsHtml += "</head><body onload=\"document." + FormName + ".submit()\">";
            sipsHtml += "<form name=\"" + FormName + "\" method=\"" + Method + "\" action=\"" + Url + "\">";
            int i = 0;
            for (i = 0; i <= Inputs.Keys.Count - 1; i += 1)
            {
                sipsHtml += "<input type=\"hidden\" name=\"" + Inputs.Keys[i] + "\" value=\"" + Inputs[Inputs.Keys[i]] + "\" />";
            }
            sipsHtml += "</form>";

            sipsHtml += "  <table border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\" height=\"100%\">";
            sipsHtml += "<tr><td width=\"100%\" height=\"100%\" valign=\"middle\" align=\"center\">";
            sipsHtml += "<font style=\"font-family: Trebuchet MS, Verdana, Helvetica;font-size: 14px;letter-spacing: 1px;font-weight: bold;\">";
            sipsHtml += "Processing...";
            sipsHtml += "</font><br /><br /><img src='' /> LOADING IMG     ";
            sipsHtml += "</td></tr>";
            sipsHtml += "</table>";

            sipsHtml += "</body></html>";

            return sipsHtml;

        }

    }

}
