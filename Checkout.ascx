<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.Checkout" AutoEventWireup="true"  Codebehind="Checkout.ascx.cs" %>
<asp:Repeater ID="rpDetailDisplay" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpPaymentGateways" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>

