<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.OrderAdmin" AutoEventWireup="true"  Codebehind="OrderAdmin.ascx.cs" %>
<asp:Repeater ID="rpDataH" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpData" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpItemH" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpItem" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpItemF" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpDataF" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>

