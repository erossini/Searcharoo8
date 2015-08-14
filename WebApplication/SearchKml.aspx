<%@ Page Language="c#" 
Buffer="true"
autoeventwireup="true" 
Inherits="Searcharoo.WebApplication.SearchPageBase" 
ContentType="application/vnd.google-earth.kml+xml" 
%><script runat="server">
      protected override SortedList GetSearchResults(Searcharoo.Engine.Search se)
      {
          return se.GetResults(this.SearchQuery, _Catalog, true); // ONLY Geocoded results
      }
      protected override int MaxResultsPerPage
      {
          get
          {
              return 200; 
          }
      }</script><%@ Register TagPrefix="roo" TagName="SearchPanel" Src="SearchControl.ascx" %><?xml version="1.0" encoding="UTF-8"?>
<kml xmlns="http://www.opengis.net/kml/2.2">
   <Document><asp:Panel runat="server" visible="false">
		<roo:SearchPanel id="ucSearchPanelHeader" runat="server"  visible="false" IsSearchResultsPage="false" />
		<asp:Panel id="lblNoSearchResults" visible="false" runat="server"></asp:Panel>
		</asp:Panel>
		<name>Searcharoo Results for: <%=SearchQuery %></name>
		<description>The query took <%=_DisplayTime%> and produced <%=_Geocoded%> geocoded results.</description>
		<asp:Repeater id="SearchResults" runat="server">
	    <ItemTemplate>
        <Placemark>   
            <name><![CDATA[<%# DataBinder.Eval(Container.DataItem, "TitleText") %> ]]></name>
            <description><![CDATA[
                <%# DataBinder.Eval(Container.DataItem, "DescriptionText") %>
                <br />
                <a href="<%# DataBinder.Eval(Container.DataItem, "Url") %>"><img src="<%# DataBinder.Eval(Container.DataItem, "Url") %>" width="120" /></a>
                <br />
                Tags: <%# DataBinder.Eval(Container.DataItem, "KeywordString") %>
                <br />
                Size: <%# DataBinder.Eval(Container.DataItem, "Size") %>
                <br />
                Crawled Date: <%# DataBinder.Eval(Container.DataItem, "CrawledDate") %>
                <br />
                Rank: (<%# DataBinder.Eval(Container.DataItem, "Rank") %>)
            ]]></description>
            <Style><IconStyle><Icon><href><%# DataBinder.Eval(Container.DataItem, "Url") %></href></Icon></IconStyle></Style>
            <Point>      
                <extrude>1</extrude>
		        <altitudeMode>relativeToGround</altitudeMode>
                <coordinates><%# DataBinder.Eval(Container.DataItem, "GpsLocationText")%>,0</coordinates>    
            </Point>
        </Placemark>
	    </ItemTemplate>
		</asp:Repeater><asp:Panel runat="server" visible="false"><roo:SearchPanel id="ucSearchPanelFooter" runat="server" visible="false" IsSearchResultsPage="true" IsFooter="true"/></asp:Panel>
   </Document>
</kml>