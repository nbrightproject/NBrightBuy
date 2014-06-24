<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.CartView" AutoEventWireup="true"  Codebehind="CartView.ascx.cs" %>
<asp:Repeater ID="rpDetailDisplay" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpDataH" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpData" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
<asp:Repeater ID="rpDataF" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:PlaceHolder ID="phPaging" runat="server"></asp:PlaceHolder>
<asp:Repeater ID="rpAddrListH" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpAddrListB" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpAddrListF" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpAddr" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpShip" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpPromo" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpTax" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>
<asp:Repeater ID="rpExtra" runat="server" OnItemCommand="CtrlItemCommand"></asp:Repeater>