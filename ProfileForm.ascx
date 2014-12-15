<%@ Control language="C#" Inherits="Nevoweb.DNN.NBrightBuy.ProfileForm" AutoEventWireup="true"  Codebehind="ProfileForm.ascx.cs" %>
<asp:PlaceHolder ID="notifymsg" runat="server"></asp:PlaceHolder>
<asp:Repeater ID="rpInp" runat="server" OnItemCommand="CtrlItemCommand" ></asp:Repeater>
<asp:PlaceHolder ID="phData" runat="server"></asp:PlaceHolder>
