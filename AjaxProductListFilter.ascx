<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.AjaxProductListFilter" AutoEventWireup="true"  Codebehind="AjaxProductListFilter.ascx.cs" %>
<script type="text/javascript">
    $(document).ready(function () {
        // propertyFilterClicked method is in AjaxDisplayProductList_head
        $(".nbs-ajaxfilter input[type='checkbox']").change(propertyFilterClicked); 

    });    
</script>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
