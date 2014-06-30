<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.AddressAdmin" AutoEventWireup="true"  Codebehind="AddressAdmin.ascx.cs" %>
<asp:Repeater ID="rpDataH" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpData" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpDataF" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:Repeater ID="rpAddr" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
