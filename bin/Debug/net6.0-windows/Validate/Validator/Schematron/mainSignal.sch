<?xml version="1.0" encoding="utf-8" ?>

<sch:schema xmlns:sch="http://purl.oclc.org/dsdl/schematron" queryBinding="xslt2">
	<sch:ns uri="http://www.w3.org/2001/XMLSchema-instance" prefix="xsi"/>
	<sch:ns uri="http://dataprep.eulynx.eu/schema/Generic" prefix="generic"/>
	<sch:ns uri="http://dataprep.eulynx.eu/schema/DB" prefix="db"/>
	<sch:ns uri="http://dataprep.eulynx.eu/schema/Signalling" prefix="sig"/>
	<sch:ns uri="http://www.railsystemmodel.org/schemas/RSM1.2beta/Common" prefix="rsmCommon"/>
	<sch:ns uri="http://www.railsystemmodel.org/schemas/RSM1.2beta/NetEntity" prefix="rsmNE"/>
	<sch:ns uri="http://www.railsystemmodel.org/schemas/RSM1.2beta/Signalling" prefix="rsmSig"/>
	<!-- add more namespace if needed (NR, ProRail, RFI etc.) -->

	<sch:title>Richtlinie 819.0202 Signale für Zug- und Rangierfahrten; Hauptsignale Version 4.0 - Gültig ab 01.10.2012</sch:title>
	
	<!-- all reference id to SignalAsset from SignalType 'main' -->
	<!-- // select all nodes of name ownsSignalType taht has a type 'main' wherever they are in the XML and selected their appliesToSignal reference-->
	<sch:let name="mainSignalAssetId" value="//generic:ownsSignalType[db:isOfSignalTypeType = 'main']/sig:appliesToSignal/@ref"/>
	<!-- all Eulynx Signal that associated with 'main' SignalType-->
	<sch:let name="mainSignalAsset" value="//generic:ownsTrackAsset[rsmCommon:id = $mainSignalAssetId]"/>
	<!-- all Rsm Signal that associated with Eulynx Signal that associated with 'main' SignalType -->
	<sch:let name="mainRsmSignal" value="//generic:ownsSignal[rsmCommon:id = $mainSignalAsset/sig:refersToRsmSignal/@ref]"/>
	<!-- all main signal(asset) main frame -->
	<sch:let name="mainSignalFrame" value="//generic:ownsSignalFrame[(rsmCommon:id = $mainSignalAsset/sig:hasSignalFrame/@ref) and sig:isOfSignalFrameType = 'main']"/>
	<!-- all Rsm main signal locations -->
	<sch:let name="mainRsmSignalLocation" value="//generic:usesLocation[rsmCommon:id = $mainRsmSignal/rsmNE:locations/@ref]"/>

	<sch:pattern name="4 Hauptsignale anordnen">
		<sch:rule context="$mainRsmSignalLocation">

			<sch:let name="currSignalLocation"
				value="."/>
			<sch:let name="currRsmSignal"
				value="$mainRsmSignal[rsmNE:locations/@ref = $currSignalLocation/rsmCommon:id]"/>

			<!-- 4.3 Regelanordnung-->
			<!-- Report if the signal is located to the right or centre but not applied in 'normal' direction-->
			<sch:report test="rsmCommon:associatedNetElements[rsmCommon:isLocatedToSide ='right' or rsmCommon:isLocatedToSide ='centre']/rsmCommon:appliesInDirection != 'normal'">
				The arrangement of the signal (with rsmSignal id: <sch:value-of select="$currRsmSignal/rsmCommon:id"/>) in the direction of '<sch:value-of select="rsmCommon:associatedNetElements/rsmCommon:appliesInDirection"/>' cannot be located to the side '<sch:value-of select="rsmCommon:associatedNetElements/rsmCommon:isLocatedToSide"/>'
			</sch:report>
			<!-- 4.3 Regelanordnung-->
			<!-- Report if the signal is located to the left or centre but not applied in 'reverse' direction-->
			<sch:report test="rsmCommon:associatedNetElements[rsmCommon:isLocatedToSide ='left' or rsmCommon:isLocatedToSide ='centre']/rsmCommon:appliesInDirection != 'reverse'">
				The arrangement of the signal (with rsmSignal id: <sch:value-of select="$currRsmSignal/rsmCommon:id"/>) in the direction of '<sch:value-of select="rsmCommon:associatedNetElements/rsmCommon:appliesInDirection"/>' cannot be located to the side '<sch:value-of select="rsmCommon:associatedNetElements/rsmCommon:isLocatedToSide"/>'
			</sch:report>
		</sch:rule>
	</sch:pattern>
	
	<sch:pattern name="4.8 Hauptsignale andordnen">
		<sch:rule context="$mainSignalFrame">

			<sch:let name="horizontalOffsetOfReferencePoint"
				value="./sig:hasPosition[@xsi:type='sig:HorizontalOffsetOfReferencePoint']"/>
			<sch:let name="currSignalFrame"
				value="."/>
			<sch:let name="currSignalAsset"
				value="$mainSignalAsset[sig:hasSignalFrame/@ref = $currSignalFrame//rsmCommon:id]"/>
			<sch:let name="currRsmSignal"
				value="$mainRsmSignal[rsmCommon:id = $currSignalAsset/sig:refersToRsmSignal/@ref]"/>

			<!-- 4.8  Abstände von Gleismitte -->
			<!-- Maximum distance of mainsignal from the center of the tracks (4.15m) -->
			<sch:report test="$horizontalOffsetOfReferencePoint/sig:value/rsmCommon:value >= 4.15">The distance between the center of the signal (with rsmSignal id: <sch:value-of select="$currRsmSignal/rsmCommon:id"/>) screen (main frame) and the center of the track '<sch:value-of select="$horizontalOffsetOfReferencePoint/sig:value/rsmCommon:value"/>m' should not exceed 4.15m</sch:report>

		</sch:rule>
	</sch:pattern>
	
	<!-- all Rsm main- and shunting-RouteBodies (Zugfahrstrasse & Rangierfahrstrasse) -->
	<sch:let name="RsmRouteBody" value="//rsmSig:ownsRouteBody[@xsi:type='db:MainRoute'] or //rsmSig:ownsRouteBody[@xsi:type='db:ShuntingRoute']"/>
	<!-- all Distance to danger point -->
	<sch:let name="distanceToDangerPoint" value="//generic:ownsSafetyDistance[@xsi:type='sig:DistanceToDangerPoint']"/>

	<sch:pattern name="11 Abstand der Hauptsignale vom maßgebenden Gefahrpunkt">
		<sch:rule context="$distanceToDangerPoint">

			<sch:let name="currDangerPoint" value="."/>
			<sch:let name="currSignalAsset" value="//generic:ownsTrackAsset[sig:refersToSafetyDistance/@ref = $currDangerPoint/rsmCommon:id]"/>
			<sch:let name="currRouteBody" value="//ownsRouteBody[sig:RouteEntry/sig:hasgroupMainSignal/@ref = currSignalAsset/rsmCommon:id]"/>
			<sch:let name="currRouteBodySpeed" value="//ownsRouteBodyProperty[xsi:type='sig:maxRouteBodySpeed' and appliesToRouteBody/@ref=$currRouteBody/rsmCommon:id]"/>
			<sch:let name="currRsmSignal"
				value="$mainRsmSignal[rsmCommon:id = $currSignalAsset/sig:refersToRsmSignal/@ref]"/>
			
			<!--  11.3 Mindestabstand zwischen Signal und Gefahrpunkt -->
			<sch:report test=".[sig:hasDesignSlope/rsmCommon:value = '0']/sig:hasMinimalLength/rsmCommon:value &lt; '200'">The distance of an entry or covering signal (with rsmSignal id: <sch:value-of select="$currRsmSignal/rsmCommon:id"/>) from the decisive danger point shall be at least 200 m for a decisive slope of 0 ‰.</sch:report>
			<!-- 11.4 Verkürzter Abstand -->
			<sch:report test=".[sig:hasDesignSlope/rsmCommon:value = '0']/sig:hasMinimalLength/rsmCommon:value &lt; '100' and currRouteBodyspeed = '100'">
				A shortening of the distance at decisive slope from 0 ‰ to 100 m is permissible for a decisive point of danger according to paragraph (2)a) for speeds up to 100 km/h is permissible. (with rsmSignal id: <sch:value-of select="$currRsmSignal/rsmCommon:id"/>)</sch:report>
		</sch:rule>
	</sch:pattern>

</sch:schema>