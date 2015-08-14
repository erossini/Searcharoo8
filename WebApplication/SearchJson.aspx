<%@ Page Language="c#" 
Buffer="true"
autoeventwireup="true" 
Inherits="Searcharoo.WebApplication.SearchPageBase" %>
<%@ import Namespace="Searcharoo.Common" %>
%><script runat="server">
      protected override SortedList GetSearchResults(Searcharoo.Engine.Search se)
      {
          SortedList sl = se.GetResults(this.SearchQuery, _Catalog, false); // false = ALL results
          foreach (object k in sl.Keys)
          {   // need to escape the output for JSON - otherwise it might break
              // i'm removing all ' and " (just to be safe)
              Searcharoo.Common.ResultFile f = (Searcharoo.Common.ResultFile)sl[k];
              if (f.Description.Length > Preferences.SummaryCharacters)
              {
                f.Description = f.Description.Substring(0, Preferences.SummaryCharacters);
              }
              f.Description = f.Description.Replace("'", "").Replace(@"""", "").Replace(@"\", "");
              f.Title = f.Title.Replace("'", "").Replace(@"""", "").Replace(@"\", "");
              f.KeywordString = f.KeywordString.Replace("'", "").Replace(@"""", "").Replace(@"\", "");
              
              if (f.Title == "") f.Title = "[no title]";
          }
          return sl;
      }

      protected override int MaxResultsPerPage
      {
          get { return 200; }
      }
// The user controls are here (but invisible) so the base class doesn't break!!!
</script><%@ Register TagPrefix="roo" TagName="SearchPanel" Src="SearchControl.ascx" %><asp:Panel 
visible="false" runat="server">
<roo:SearchPanel id="ucSearchPanelHeader" runat="server"  visible="false" IsSearchResultsPage="false" />
<asp:Panel id="lblNoSearchResults" visible="false" runat="server"></asp:Panel>
</asp:Panel>[<asp:Repeater id="SearchResults" runat="server">
 <ItemTemplate>
{"name":"<%# DataBinder.Eval(Container.DataItem, "TitleText") %>"
,"description":"<%# DataBinder.Eval(Container.DataItem, "DescriptionText") %>"
,"url":"<%# DataBinder.Eval(Container.DataItem, "Url") %>"
,"tags":"<%# DataBinder.Eval(Container.DataItem, "KeywordString") %>"
,"size":"<%# DataBinder.Eval(Container.DataItem, "Size") %>"
,"date":"<%# DataBinder.Eval(Container.DataItem, "CrawledDate") %>"
,"rank":"<%# DataBinder.Eval(Container.DataItem, "Rank") %>"
,"gps":"<%# DataBinder.Eval(Container.DataItem, "GpsLocationText")%>"
,"extension":"<%# DataBinder.Eval(Container.DataItem, "Extension")%>"
}</ItemTemplate><SeparatorTemplate>,</SeparatorTemplate></asp:Repeater>]<asp:Panel runat="server" 
visible="false"><roo:SearchPanel id="ucSearchPanelFooter" runat="server" 
visible="false" IsSearchResultsPage="true" IsFooter="true"/></asp:Panel>