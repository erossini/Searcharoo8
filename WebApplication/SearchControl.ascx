<%@ Control Language="c#" AutoEventWireup="true" Inherits="Searcharoo.WebApplication.SearchControlBase" %>
<%@ Import Namespace="Searcharoo.Common" %>
<%-- Panel that is visible when the search page is first visited --%>
<asp:Panel ID="pnlHomeSearch" runat="server">
    <form method="get" action="Search.aspx">
        <div class="heading">Westhill Search</div>
        <div>
            <input type="text" placeholder="Search for..." class="search" required name="<%=Preferences.QuerystringParameterName%>" id="<%=Preferences.QuerystringParameterName%>1" value="<%=Word%>">
            <span class="arrow">
                <input type="submit" value="Search" class="button">
            </span>
            <div id="errorResult">
                <%=_ErrorMessage%>
            </div>
        </div>
    </form>
</asp:Panel>

<%-- Panel that is visible when search results are being shown --%>
<asp:Panel ID="pnlResultsSearch" runat="server">
    <form method="get" id="bottom" action="Search.aspx" style="margin: 0px; padding: 0px;">
        <div>
            <div class="heading">Westhill Search</div>
            <div>
                <input type="text" placeholder="Search for..." class="search" required name="<%=Preferences.QuerystringParameterName%>" id="<%=Preferences.QuerystringParameterName%>1" value="<%=Word%>">
                <span class="arrow">
                    <input type="submit" value="Search" class="button">
                </span>
                <div id="errorResult">
                    <%=_ErrorMessage%>
                </div>
            </div>
        </div>
    </form>
</asp:Panel>