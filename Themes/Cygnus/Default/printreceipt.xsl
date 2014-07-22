<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="xml" indent="yes" encoding="UTF-8"/>

  <xsl:decimal-format name="decimalformat" decimal-separator='.' grouping-separator=',' />

  <xsl:template match="/genxml">

    <xsl:variable name="numformat" select="'#,###,##0.00'" />

        <table border="0">
          <tr>
            <td width="400"  border="0">
              <font size="20">
                <br/>
                <br/>
                <xsl:text>RECEIPT</xsl:text>
              </font>
            </td>
            <td width="400"  border="0">
              <br/>
              <br/>
              <font size="12">
                <xsl:value-of select="billaddress/genxml/textbox/company" />
                <br/>
                <xsl:value-of select="billaddress/genxml/textbox/firstname" />
                <br/>
                <xsl:value-of select="billaddress/genxml/textbox/lastname" />
                <br/>
                <xsl:value-of select="billaddress/genxml/textbox/unit" />
                <br/>
                <xsl:value-of select="billaddress/genxml/textbox/street" />
                <br/>
                <xsl:value-of select="billaddress/genxml/textbox/city" />
                <br/>
                <xsl:value-of select="billaddress/genxml/textbox/region" />
                <br/>
                <xsl:value-of select="billaddress/genxml/textbox/postalcode" />
                <br/>
              </font>
            </td>
          </tr>
          <tr>
            <td border="0">
              <font size="9">
                <xsl:value-of select="settings/genxml/textbox/txtcompanyaddress"/>
              </font>
            </td>
            <td border="0">
              <font size="10">
                <br/>
                <br/>
                <br/>
                <br/>
                <br/>
                Client Ref : <xsl:value-of select="details/genxml/hidden/clientref" />
                Invoice Ref : <xsl:value-of select="details/genxml/hidden/invoiceref" />
                Invoice Date  : <xsl:value-of select="substring(details/genxml/textbox/dateinvoice,1,10)" />
              </font>
            </td>
          </tr>
          <tr>
            <td colspan="2"  border="0">
              <font size="8">
                <xsl:value-of select="details/genxml/textbox/txtnotes" />
              </font>
            </td>
          </tr>
        </table>
        <table cellpadding="2">
          <tr>
            <td width="650" backgroundcolor="1" align="center">
              <font size="8">
                <b>Description</b>
              </font>
            </td>
            <td width="120" backgroundcolor="1" align="center">
              <font size="8">
                <b>
                  (<xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>)
                </b>
              </font>
            </td>
          </tr>
          <xsl:for-each select="items/genxml">

            <tr>
              <td width="650">
                <font size="8">
                  <xsl:value-of select="productname" />
                </font>
              </td>
              <td width="120" align="right">
                <font size="8">
                  <xsl:value-of select="format-number(unitcost, $numformat,'decimalformat')" />
                </font>
              </td>
            </tr>
          </xsl:for-each>
          <tr>
            <td colspan="2" backgroundcolor="1">
              <font size="2">
              </font>
            </td>
          </tr>
          <tr>
            <td width="650" align="right">
              <font size="8">
                Sub-Total (<xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>) :
              </font>
            </td>
            <td width="120" align="right">
              <font size="8">
                <xsl:value-of select="format-number(details/genxml/hidden/dblsubtotal, $numformat,'decimalformat')" />
              </font>
            </td>
          </tr>
          <tr>
            <td width="650" align="right">
              <font size="8">
                Tax (<xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>) :
              </font>
            </td>
            <td width="120" align="right">
              <font size="8">
                <xsl:value-of select="format-number(details/genxml/hidden/dbltaxtotal, $numformat,'decimalformat')" />
              </font>
            </td>
          </tr>
          <tr>
            <td width="650" align="right">
              <b>
                Total (<xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>) :
              </b>
            </td>
            <td width="120" align="right">
              <xsl:value-of select="format-number(details/genxml/hidden/dblgrandtotal, $numformat,'decimalformat')" />
            </td>
          </tr>
          <xsl:if test="details/genxml/textbox/txtalreadypaid!='' and details/genxml/textbox/txtalreadypaid!='0.00'">
            <tr>
              <td width="650" align="right">
                <font size="8">Already Paid :</font>
              </td>
              <td width="120" align="right">
                <font size="8">
                  <xsl:value-of select="format-number(details/genxml/textbox/txtalreadypaid, $numformat,'decimalformat')" />
                </font>
              </td>
            </tr>
            <tr>
              <td width="650" align="right">
                <b>
                  Total Due (<xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>) :
                </b>
              </td>
              <td width="120" align="right">
                <xsl:value-of select="format-number(details/genxml/hidden/dbltotaldue, $numformat,'decimalformat')" />
              </td>
            </tr>
          </xsl:if>
        </table>
        <xsl:if test="details/genxml/textbox/txtnumpayments > '1'">
          <br/>
          <font size="5">
            <xsl:value-of select="settings/genxml/textbox/txtpagefooter"/>
          </font>
          <pagebreak/>
          <br/>
          <br/>
          <br/>
          <br/>
          <br/>
          <font size="8">
            <i>Keep for your information :</i>
            <br />
            Monthly Payments.
          </font>
          <table cellpadding="1">
            <tr  backgroundcolor="1">
              <td width="40">
                <font size="6">No.</font>
              </td>
              <td width="120">
                <font size="6">Date</font>
              </td>
              <td width="90" align="right">
                <font size="6">
                  Amount <xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>
                </font>
              </td>
              <td width="90" align="right">
                <font size="6">
                  Tax <xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>
                </font>
              </td>
              <td width="90" align="right">
                <font size="6">
                  Paid <xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>
                </font>
              </td>
              <td width="90" align="right">
                <font size="6">
                  Amount <xsl:value-of select="settings/genxml/textbox/txtcurrencysign"/>
                </font>
              </td>
            </tr>
            <xsl:for-each select="/root/payments/r">
              <tr>
                <td>
                  <font size="6">
                    <xsl:value-of select="num" />
                  </font>
                </td>
                <td>
                  <font size="6">
                    <xsl:value-of select="substring(date,1,10)" />
                  </font>
                </td>
                <td align="right">
                  <font size="6">
                    <xsl:value-of select="ht" />
                  </font>
                </td>
                <td align="right">
                  <font size="6">
                    <xsl:value-of select="tax" />
                  </font>
                </td>
                <td align="right">
                  <font size="6">
                    <xsl:value-of select="paid" />
                  </font>
                </td>
                <td align="right">
                  <font size="6">
                    <xsl:value-of select="due" />
                  </font>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </xsl:if>
        <br/>
        <font size="8">
          <b></b>
          <br/>
        </font>
        <font size="5">
        </font>

        <xsl:if test="details/genxml/textbox/txtnumpayments > '1'">
          <br/>
          - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
          <br/>
          <font size="8">

          </font>
          <br/>
          <font size="5">
          </font>
        </xsl:if>

  </xsl:template>

  <xsl:template name="DisplayInfosValue">
    <!--Display Value abd BR  only if not blank-->
    <xsl:param name="DLabel" select="''" />
    <xsl:param name="DValue" select="''" />
    <xsl:if test="$DValue!=''">
      <xsl:value-of select="$DLabel" />
      <xsl:value-of select="$DValue" />
      <br />
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>
