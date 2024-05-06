<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="xml" indent="yes"/>
  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="PackageId">
    <PackageId>StyleCheckerBeta</PackageId>
  </xsl:template>

  <xsl:template match="Project">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <ItemGroup>
        <PackageReference Include="StyleChecker" Version="1.0.27" PrivateAssets="all" />
      </ItemGroup>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
