<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.Providers.PromoProvider.DiscountCodes" AutoEventWireup="true"  Codebehind="DiscountCodes.ascx.cs" %>
<asp:Repeater ID="rpData" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
<asp:PlaceHolder ID="phPaging" runat="server"></asp:PlaceHolder>

