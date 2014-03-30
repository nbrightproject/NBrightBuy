<%@ Control language="vb" CodeBehind="~/admin/Skins/skin.vb" AutoEventWireup="false" Explicit="true" Inherits="DotNetNuke.UI.Skins.Skin" %>
<%@ Register TagPrefix="dnn" TagName="LANGUAGE" Src="~/Admin/Skins/Language.ascx" %>
<%@ Register TagPrefix="dnn" TagName="COPYRIGHT" Src="~/Admin/Skins/Copyright.ascx" %>
<% if (Request.isAuthenticated) %><link rel="stylesheet" type="text/css" href="/DesktopModules/NBright/NBrightGen/Skins/NWB_nbright/skinoverwrite.css" /><% End If %>
<div id="ControlPanel" runat="server" visible="false"></div>
<div class="NBright_pagemaster">
<div class="NBright_langpane"><dnn:LANGUAGE runat="server" id="dnnLANGUAGE" showMenu="False" showLinks="true" ItemTemplate="&lt;a href='[URL]' title='[CULTURE:NATIVENAME]'&gt;&lt;img src='/DesktopModules/NBright/NBrightGen/ui/images/flags/32/[CULTURE:NAME].png' alt='[CULTURE:NATIVENAME]' /&gt;&lt;/a&gt;" AlternateTemplate="&lt;a href='[URL]' title='[CULTURE:NATIVENAME]'&gt;&lt;img src='/DesktopModules/NBright/NBrightGen/ui/images/flags/32/[CULTURE:NAME].png' alt='[CULTURE:NATIVENAME]' /&gt;&lt;/a&gt;" SelectedItemTemplate="&lt;a href='[URL]' class='NBright_langsel' title='[CULTURE:NATIVENAME]'&gt;&lt;img src='/DesktopModules/NBright/NBrightGen/ui/images/flags/32/[CULTURE:NAME].png' alt='[CULTURE:NATIVENAME]' /&gt;&lt;/a&gt;" /></div>
<div class="NBright_content">
<div id="ContentPane" class="NBright_contentpane" runat="server" ContainerType="G" ContainerName="_default" ContainerSrc="No Container.ascx"></div>
</div>
<div class="NBright_copyright">Copyright &copy; Nevoweb Bright</div>
</div>