<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
	<xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>


	<xsl:template match="/root">
	
		<xsl:variable name="adminskin"></xsl:variable>

		<ul id="NBright_menu">
			<xsl:for-each select="./ControlLinkInfo">
				<xsl:sort select="ViewOrder" data-type="number"/>
				<xsl:if test="./ControlTitle!=''">
					<xsl:if test="(normalize-space(./ControlKey)!='')">
						<xsl:if test="contains(./ControlTitle,'>')=false">
							<li>
								<a>
									<xsl:value-of select="./ControlTitle"/>
								</a>
								<div class="dropdown-column">
									<div class="col">
										<ul>
											<xsl:variable name="submenuoutput">
												<xsl:call-template name="submenu">
													<xsl:with-param name="parenttitle" select="./ControlTitle"/>
													<xsl:with-param name="adminskin" select="$adminskin"/>
												</xsl:call-template>
											</xsl:variable>

											<xsl:if test="$submenuoutput=''">
												<li>
													<xsl:attribute name="class">
														<xsl:value-of select="concat('menu_',./ControlKey)"/>
													</xsl:attribute>
													<a>
														<xsl:attribute name="href">
															<xsl:value-of select="./NavigateUrl"/>
															<xsl:value-of select="$adminskin"/>
														</xsl:attribute>
														<span>
															<xsl:value-of select="./ControlTitle"/>
														</span>
														<span class="hint">
															<xsl:value-of select="./Text"/>
														</span>
													</a>
												</li>
											</xsl:if>

											<xsl:if test="$submenuoutput!=''">
												<xsl:call-template name="submenu">
													<xsl:with-param name="parenttitle" select="./ControlTitle"/>
													<xsl:with-param name="adminskin" select="$adminskin"/>
												</xsl:call-template>
											</xsl:if>
										</ul>
									</div>
								</div>
							</li>
						</xsl:if>
					</xsl:if>
					<xsl:if test="(normalize-space(./ControlKey)='')">
						<xsl:if test="(normalize-space(./ControlTitle)!='')">
							<li>
								<a>
									<xsl:attribute name="href">
										<xsl:value-of select="./NavigateUrl"/>
									</xsl:attribute>
									<span>
										<xsl:value-of select="./ControlTitle"/>
									</span>
								</a>
							</li>
						</xsl:if>
					</xsl:if>
				</xsl:if>
			</xsl:for-each>
		</ul>

	</xsl:template>

	<xsl:template name="submenu">
		<xsl:param name="parenttitle"/>
		<xsl:param name="adminskin"/>

		<xsl:for-each select="/root/ControlLinkInfo">
			<xsl:sort select="ViewOrder"/>
			<xsl:if test="./ControlTitle!=''">
				<xsl:if test="substring-before(./ControlTitle,'>')=$parenttitle">
					<li>
						<xsl:attribute name="class">
							<xsl:value-of select="concat('menu_',./ControlKey)"/>
						</xsl:attribute>
						<a>
							<xsl:attribute name="href">
								<xsl:value-of select="./NavigateUrl"/>
								<xsl:value-of select="$adminskin"/>
							</xsl:attribute>
							<span>
								<xsl:value-of select="substring-after(./ControlTitle,'>')"/>
							</span>
							<span class="hint">
								<xsl:value-of select="./Text"/>
							</span>
						</a>
					</li>
				</xsl:if>
			</xsl:if>
		</xsl:for-each>

	</xsl:template>


</xsl:stylesheet>
