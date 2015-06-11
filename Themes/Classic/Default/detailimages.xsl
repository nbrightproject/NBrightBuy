<?xml version="1.0"?>

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

 <xsl:output method="html" indent="yes" encoding="utf-8" omit-xml-declaration="yes"/>
  <xsl:param name="classicgallerywidth"></xsl:param>
  <xsl:param name="classicgalleryheight"></xsl:param>

  <xsl:template match="/">

  <xsl:if test="genxml/imgs/genxml[1]/hidden/imageurl != ''">
<ul  class="gallery"> 
      <xsl:for-each select="genxml/imgs/genxml">

        <xsl:if test="position() &gt; 1">
          
          <xsl:variable name="mypos">
          <xsl:value-of select="position()"/>
        </xsl:variable>
        
        <li>
          <xsl:attribute name="class">
            <xsl:text>image</xsl:text>
            <xsl:value-of select="position()" />
          </xsl:attribute>
          <a>
            <xsl:attribute name="href">
              <xsl:value-of select="./hidden/imageurl" />
            </xsl:attribute>
            <xsl:attribute name="data-imagelightbox">
              <xsl:text disable-output-escaping="yes">nbb</xsl:text>
            </xsl:attribute>
            <img>
              <xsl:attribute name="src">
                <xsl:text disable-output-escaping="yes">/DesktopModules/NBright/NBrightBuy/NBrightThumb.ashx?src=</xsl:text>
                <xsl:value-of select="./hidden/imageurl" />
                <xsl:text>&amp;w=</xsl:text>
                <xsl:value-of select="$classicgallerywidth" />
                <xsl:text>&amp;h=</xsl:text>
                <xsl:value-of select="$classicgalleryheight" />
              </xsl:attribute>
              <xsl:attribute name="alt">
                <xsl:value-of select="position()" />
                <xsl:value-of select="../../lang/genxml/imgs/genxml[position() = $mypos]/textbox/txtimagedesc" />                
              </xsl:attribute>
              <xsl:attribute name="title">
                <xsl:value-of select="../../lang/genxml/imgs/genxml[position() = $mypos]/textbox/txtimagedesc" />
              </xsl:attribute>
            </img>
          </a>
          
        </li>
        </xsl:if>

      </xsl:for-each>
</ul> 
  </xsl:if>
  
</xsl:template>

</xsl:stylesheet>