<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.ProductAjaxView" AutoEventWireup="true"  Codebehind="ProductAjaxView.ascx.cs" %>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
<asp:PlaceHolder ID="phPaging" runat="server"></asp:PlaceHolder>

<div id="nbs_productajaxview" style="display: none;">
    <!-- Parameter fields -->
    <!-- These fields will be initialized from the _head.cshtml -->
    <input id="userid" type="hidden" value="" />
    <input id="moduleid" type="hidden" value="" />
    <input id="tabid" type="hidden" value="" />
    <input id="currentlang" type="hidden" value="" />
    <input id="lang" type="hidden" value="" />
    <input id="pagesize" type="hidden" value="" />
    <input id="pagenumber" type="hidden" value="" />
    <input id="catid" type="hidden" value="" />
    <input id="catref" type="hidden" value="" />

    <input id="modkey" type="hidden" value="" />
    <input id="pagemid" type="hidden" value="" />
    <input id="page" type="hidden" value="" />
    <input id="pagesize" type="hidden" value="" />
    <input id="orderby" type="hidden" value="" />

</div>
<div id="nbs_ajaxproducts"></div>
