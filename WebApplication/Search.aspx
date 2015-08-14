<%@ Page Language="c#" autoeventwireup="true" Inherits="Searcharoo.WebApplication.SearchPageBase"%>
<%@ import Namespace="System" %>
<%@ import Namespace="System.Xml.Serialization" %>
<%@ import Namespace="System.Collections.Specialized" %>
<%@ import Namespace="Searcharoo.Common" %>
<%@ Register TagPrefix="roo" TagName="SearchPanel" Src="SearchControl.ascx" %>
<script runat="server">
    /// <summary>
    /// This method implements a 'rolling window' page-number index
    /// for the underlying PagedDataSource
    /// </summary>
    /// <remarks>
    /// http://www.sitepoint.com/article/asp-nets-pageddatasource
    /// http://www.uberasp.net/ArticlePrint.aspx?id=29
    /// 
    /// http://www.codeproject.com/KB/aspnet/Mastering_DataBinding.aspx
    /// </remarks>
    public string CreatePagerLinks(PagedDataSource objPds, string BaseUrl)
    {
        StringBuilder sbPager = new StringBuilder();
        StringBuilder sbPager1 = new StringBuilder();

        //sbPager1.Append("<td>Westhill Search</td>");

        if (objPds.IsFirstPage)
        {	// lower link is blank
            //sbPager.Append("<td></td>");
        }
        else
        {	// first+prev link
            //sbPager.Append("<div>");
            // first page link
            sbPager.Append("<a href=\"");
            sbPager.Append(CreatePageUrl(BaseUrl, 1));
            sbPager.Append("\" alt=\"First Page\" class=\"page gradient\" title=\"First Page\">First</a>&nbsp;");
            if (objPds.CurrentPageIndex != 1)
            {
                // previous page link
                sbPager.Append("<a href=\"");
                sbPager.Append(CreatePageUrl(BaseUrl, objPds.CurrentPageIndex));
                sbPager.Append("\" alt=\"Previous Page\" class=\"page gradient\" title=\"Previous Page\">Previous</a>&nbsp;");
            }
            //sbPager.Append("</div>");
        }

        // calc low and high limits for numeric links
        int intLow = objPds.CurrentPageIndex - 1;
        int intHigh = objPds.CurrentPageIndex + 3;
        if (intLow < 1) intLow = 1;
        if (intHigh > objPds.PageCount) intHigh = objPds.PageCount;
        if (intHigh - intLow < 5) while ((intHigh < intLow + 4) && intHigh < objPds.PageCount) intHigh++;
        if (intHigh - intLow < 5) while ((intLow > intHigh - 4) && intLow > 1) intLow--;

        for (int x = intLow; x < intHigh + 1; x++)
        {
            // numeric links
            if (x == objPds.CurrentPageIndex + 1)
            {
                //sbPager1.Append("<td width=10 align=center><font color=orange><b>o</b></td>");
                sbPager.Append("<span class=\"page active\">" + x.ToString() + "</span>");
            }
            else
            {
                //sbPager1.Append("<td width=10 align=center><font color=orange><b>o</b></td>");
                //sbPager.Append("<div>");
                sbPager.Append("<a href=\"");
                sbPager.Append(CreatePageUrl(BaseUrl, x));
                sbPager.Append("\" alt=\"Go to page\" class=\"page gradient\" title=\"Go to page\">");
                sbPager.Append(x.ToString());
                sbPager.Append("</a> ");
                //sbPager.Append("</div>");
            }
        }

        if (!objPds.IsLastPage)
        {
            //sbPager.Append("<div>");
            if ((objPds.CurrentPageIndex + 2) != objPds.PageCount)
            {
                // next page link
                sbPager.Append("&nbsp;<a href=\"");
                sbPager.Append(CreatePageUrl(BaseUrl, objPds.CurrentPageIndex + 2));
                sbPager.Append("\" alt=\"Next Page\" class=\"page gradient\" title=\"Next Page\">Next</a> ");
            }
            // last page link
            sbPager.Append("&nbsp;<a href=\"");
            sbPager.Append(CreatePageUrl(BaseUrl, objPds.PageCount));
            sbPager.Append("\" alt=\"Last Page\" class=\"page gradient\" title=\"Last Page\">Last</a>");
            //sbPager.Append("</div>");
        }
        else
        {
            if (objPds.PageCount == 1) sbPager.Append(""); // of 1
        }

        // convert the final links to a string and assign to labels
        return "<div>" + sbPager1.ToString() + "</div><div class='pagination'>" + sbPager.ToString() + "</div>";
    }
</script>
<html>
<head>
    <title>Westhill search</title>
    <meta http-equiv="robots" content="none">
    <link href="css/Searcharoo.css" rel="stylesheet" />
    <link rel="stylesheet" type="text/css" href="css/SearcharooMax960.css" media="all and (max-width: 1024px)" />
</head>
<body>
    <roo:SearchPanel ID="ucSearchPanelHeader" runat="server" IsSearchResultsPage="false" />

    <asp:Panel ID="lblNoSearchResults" Visible="false" runat="server">
        Your search - <b><%=_SearchTerm%></b> - did not match any documents. 
	    <br>
        <br>
        It took <%=_DisplayTime%>.
	    <p>Suggestions:</p>
        <ul>
            <li>Check your spelling</li>
            <li>Try similar meaning words (synonyms)</li>
            <li>Try fewer keywords: <%=_Matches%></li>
        </ul>
    </asp:Panel>

    <asp:Repeater ID="SearchResults" runat="server">
        <HeaderTemplate>
            <div id="resultText">
                <%=_NumberOfMatches%> results for <%=_Matches%> took <%=_DisplayTime%> (<%=_Geocoded%>geocoded<% if (_Geocoded > 0)
                                                                                                                  {%><a href="/SearchKml/<%=Request["searchfor"]%>.kml">view in Google Earth</a><%} %>)
            </div>
            <div id="resultSpace">&nbsp;</div>
            <div id="result">
        </HeaderTemplate>
        <ItemTemplate>
            <div id="resultBox">
                <div id="resultTitle">
                    <asp:Literal runat="server" Visible='<%# (string)DataBinder.Eval(Container.DataItem, "Extension") != "html" %>' Text='<%# DataBinder.Eval(Container.DataItem, "Extension") %>' />
                    <a href="<%# DataBinder.Eval(Container.DataItem, "Url") %>"><b><%# DataBinder.Eval(Container.DataItem, "Title") %></b></a>
                    <a href="<%# DataBinder.Eval(Container.DataItem, "Url") %>" target="_blank" title="Open this link in new window">&uarr;</a>
                    <font color="gray">(<%# DataBinder.Eval(Container.DataItem, "Rank") %>)</font>
                    <%# DataBinder.Eval(Container.DataItem, "GpsLocationHtml")%>0
                </div>
                <div id="resultDescription">
                    <%# DataBinder.Eval(Container.DataItem, "Description") %>...
                </div>
                <div id="resultKeywords">
                    <asp:literal ID="Literal1" runat="server" Visible='<%# (string)DataBinder.Eval(Container.DataItem, "KeywordString") != "" %>' Text='' />
                    <asp:literal runat="server" Visible='<%# (string)DataBinder.Eval(Container.DataItem, "KeywordString") != "" %>' Text='<%# DataBinder.Eval(Container.DataItem, "KeywordString") %>' />
                </div>
                <div id="resultLink">
                    <font color="green"><%# DataBinder.Eval(Container.DataItem, "Url") %> - <%# DataBinder.Eval(Container.DataItem, "Size") %> bytes</font>
                    <font color="gray">- <%# DataBinder.Eval(Container.DataItem, "Extension") %> - <%# DataBinder.Eval(Container.DataItem, "CrawledDate") %></font>
                </div>
            </div>
            <div id="resultSpace">&nbsp;</div>
        </ItemTemplate>
        <FooterTemplate>
            </div>
            <div id="pager"><%=CreatePagerLinks(_PagedResults, Request.Url.ToString() )%></div>
        </FooterTemplate>
    </asp:Repeater>
</body>
</html>