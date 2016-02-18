<?xml version="1.0" encoding="utf-8"?>
<!-- Created with Liquid XML Studio 2012 Developer Edition (Trial) 10.0.5.3999 (http://www.liquid-technologies.com) -->
<xsl:stylesheet version="2.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    xmlns="http://www.w3.org/1999/xhtml">
    <xsl:output method="html" omit-xml-declaration="yes" encoding="utf-8" indent="no" />
    <xsl:preserve-space elements="*" />
    <xsl:template match="/">
        <html>
            <head>
                <style type="text/css">
                    body {
                        font-family: Trebuchet MS,Verdana,Arial;
                        font-size: 0.8em;
                    }
                    table {
                    	font-size: 1em;
                    	border-spacing: 0;
                    }
                    table td {
                    	padding: 0;
                    	border-left: solid 1px #888;
                    	border-right: solid 1px #888;
                    	border-bottom: solid 1px #888;
                    }
                    table th {
                    	padding: 2px;
                    }
                    table tr {
                    	 height: 20px;
                    }
                    th.file {
                    	width: 400px;
                    }
                    th.mutants, th.sequence_points {
                    	width: 118px;
                    }
                    h1 {
                        font-size: 1.5em;
                    }
                    pre {
                        font-family: Consolas, Lucida Console, Courier New, Courier;
                    }
                    .red {
                        background-color: #fcc;
                    }
                    .amber {
                        background-color: #fff8e8;
                    }
                    .green {
                        background-color: #cfc;
                    }
                    .title, th {
                        background-color: #000;
                        color: #fff;
                        font-weight: bold;
                        text-align: left;
                    }
                    .bar-wrap {
                    	position: relative;
                    }
                    .bar-wrap, .bar-value, .bar-text {
                    	width: 120px;
                    	height: 20px;
                    }
                    .bar-wrap, .bar-value {
                    	background-color: #fdd;
                    }
                    .bar-text {
                    	position: absolute;
                    	top: 0; left: 0;
                    	padding-top: 2px;
                    	color: #000;
                    	text-align: center;
                    	width: 100%;
                    }
                </style>
            </head>
            <body>
                <xsl:apply-templates select="/" mode="summary" />
                <xsl:apply-templates select="//SourceFile" mode="detail">
                    <xsl:sort select="FileName" />
                </xsl:apply-templates>
            </body>
        </html>
    </xsl:template>
    
    <xsl:template match="/" mode="summary">
    	<table>
    		<thead>
    			<tr>
    				<th class="file">File</th>
    				<th class="mutants">Mutants</th>
    				<th class="sequence_points">Sequence points</th>
    			</tr>
    		</thead>
    		<tbody>
	             	<xsl:apply-templates select="//SourceFile" mode="summary">
                        <xsl:sort select="FileName" />
                    </xsl:apply-templates>
    		</tbody>
    	</table>
    </xsl:template>

    <xsl:template match="SourceFile" mode="summary">
    	<tr>
    		<td>
                <xsl:value-of select="FileName" />
    		</td>
    		<td>
    			<div class="bar-wrap">
					<xsl:variable name="width">
						<xsl:choose>
    						<xsl:when test=".//AppliedMutant">
								<xsl:value-of select="100 * count(.//AppliedMutant[@Killed='true']) div count(.//AppliedMutant)" />
							</xsl:when>
							<xsl:otherwise>100</xsl:otherwise>
						</xsl:choose>
					</xsl:variable>
  					<div class="bar-value" style="background: #dfd; width: {$width}%">
    					<div class="bar-text" >
    						<xsl:value-of select="count(.//AppliedMutant[@Killed='true'])" /> /
    						<xsl:value-of select="count(.//AppliedMutant)" />
    					</div>
    				</div>
    			</div>
    		</td>
    		<td>
    			<div class="bar-wrap">
    			<xsl:if test=".//SequencePoint">
    				<div class="bar-value" style="background: #dfd; width: {100 * count(.//SequencePoint[.//AppliedMutant]) div count(.//SequencePoint)}%">
    				<div class="bar-text" >
    			<xsl:value-of select="count(.//SequencePoint[.//AppliedMutant])" /> /
    			<xsl:value-of select="count(.//SequencePoint)" />
    				</div>
    				</div>
    			</xsl:if>
    			</div>
    		</td>
    	</tr>
    </xsl:template>
    
    <xsl:template match="SourceFile" mode="detail">
        <h1>
            <xsl:value-of select="FileName" />
        </h1>
        <pre class="brush:csharp">
            <xsl:text disable-output-escaping="yes">&lt;span class="title"&gt;Line: Mutants: Source code                                                                       &lt;/span&gt;
</xsl:text>
            <xsl:apply-templates select="Lines/Line" />
        </pre>
    </xsl:template>
    
    <xsl:template match="Line">
        <xsl:variable name="lineNumber" select="@Number" />
        <xsl:variable name="sequencePoints" select="count(../..//SequencePoint[number(@StartLine) &lt;= number($lineNumber) and number(@EndLine) &gt;= number($lineNumber)])" />
        <xsl:variable name="totalMutants" select="count(../..//SequencePoint[number(@StartLine) &lt;= number($lineNumber) and number(@EndLine) &gt;= number($lineNumber)]//AppliedMutant)" />
        <xsl:variable name="killedMutants" select="count(../..//SequencePoint[number(@StartLine) &lt;= number($lineNumber) and number(@EndLine) &gt;= number($lineNumber)]//AppliedMutant[@Killed='true'])" />
        <xsl:variable name="class">
            <xsl:choose>
                <xsl:when test="$sequencePoints = 0">white</xsl:when>
                <xsl:when test="$totalMutants = 0">amber</xsl:when>
                <xsl:when test="$totalMutants &gt; $killedMutants">red</xsl:when>
                <xsl:otherwise>green</xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
        <xsl:variable name="mutantFraction">
            <xsl:if test="$totalMutants &gt; 0">
                <xsl:value-of select="concat($killedMutants, '/', $totalMutants)" />
            </xsl:if>
        </xsl:variable>
        <xsl:value-of select="substring(concat('    ', $lineNumber), string-length($lineNumber) + 1)" />: <xsl:value-of select="substring(concat('       ', $mutantFraction), string-length($mutantFraction) + 1)" />: <span class="{$class}"><xsl:value-of select="text()" /><xsl:text disable-output-escaping="yes">&amp;#x200b;</xsl:text></span><xsl:text disable-output-escaping="yes">
</xsl:text>
    </xsl:template>
</xsl:stylesheet>
