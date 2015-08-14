using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Collections.Specialized;
using System.Xml.Serialization;
using Searcharoo.Common;

namespace Searcharoo.WebApplication
{
    /// <summary>
    /// Search.aspx base class
    /// </summary>
    public class SearchPageBase : Page
    {
        protected SearchControlBase ucSearchPanelHeader;
        protected SearchControlBase ucSearchPanelFooter;
        protected Repeater SearchResults;
        protected Panel lblNoSearchResults;

        #region Private Fields: _WordCount, _ErrorMessage, _Catalog, _SearchTerm, _PagedResults, _DisplayTime, _Matches, _NumberOfMatches
        /// <summary>Displayed in HTML - count of words IN CATALOG (not results)</summary>
        protected int _WordCount;
        /// <summary>Displayed in HTML - error message IF an error occurred</summary>
        protected string _ErrorMessage = String.Empty;
        /// <summary>Get from Cache</summary>
        protected Catalog _Catalog = null;
        /// <summary>Get from Cache [v7]</summary>
        protected Cache _Cache = null;

        protected string _SearchTerm = String.Empty;

        /// <summary>Datasource to bind the results collection to, for paged display</summary>
        protected PagedDataSource _PagedResults = new PagedDataSource();
        /// <summary>Display string: time the search too</summary>
        protected string _DisplayTime;
        /// <summary>Display string: matches (links and number of)</summary>
        protected string _Matches = "";
        /// <summary>Display string: Number of pages that match the query</summary>
        protected string _NumberOfMatches;

        protected int _Geocoded;
        #endregion

        /// <summary>
        /// Available to override in the Kml page which filters out non-geocoded results
        /// </summary>
        protected virtual SortedList GetSearchResults (Searcharoo.Engine.Search se)
        {
            return se.GetResults(this.SearchQuery, _Catalog);
        }
        /// <summary>
        /// Available to override in the Kml page, which doesn't support 'paging'
        /// </summary>
        protected virtual int MaxResultsPerPage
        {
            get
            {
                return Preferences.ResultsPerPage;
            }
        }
        protected string SearchQuery
        {
            get
            {
                if (string.IsNullOrEmpty(Request.QueryString[Preferences.QuerystringParameterName]))
                {
                    return string.Empty;
                }
                else
                {
                    return Request.QueryString[Preferences.QuerystringParameterName].ToString().Trim(' ');
                }
            }
        }
        /// <summary>
        /// ALL processing happens here, since we are not using ASP.NET controls or events.
        /// Page_Load will:
        /// * check the Cache for a catalog to use 
        /// * if not, check the filesystem for a serialized cache
        /// * and if STILL not, Server.Transfer to the Spider to build a new cache
        /// * check the QueryString for search arguments (and if so, do a search)
        /// * otherwise just show the HTML of this page - a blank search form
        /// </summary>
        public void Page_Load()
        {
            // prevent Searcharoo from indexing itself (ie. it's own results page)
            if ((Request.UserAgent != null) && (Request.UserAgent.ToLower().IndexOf("searcharoo") > 0)) { Response.Clear(); Response.End(); return; }

            bool getCatalog = false;
            try
            {   // see if there is a catalog object in the cache
                _Catalog = (Catalog)Application["Searcharoo_Catalog"];
                _WordCount = _Catalog.Length; // if so, get the _WordCount
                _Cache = (Searcharoo.Common.Cache)Application["Searcharoo_Cache"];
            }
            catch (Exception ex)
            {
                // otherwise, we'll need to build the catalog
                Trace.Write("Catalog object unavailable : building a new one ! " + ex.ToString() );
                _Catalog = null; // in case
                _Cache = null;
            }

            ucSearchPanelHeader.WordCount = _WordCount;
            //ucSearchPanelFooter.WordCount = _WordCount;

            if (_Catalog == null)
            {
                getCatalog = true;
            }
            else if (_Catalog.Length == 0)
            {
                getCatalog = true;
            }

            if (getCatalog)
            {
                if ((string)Application["CatalogLoad"] == "")
                {
                    Application["CatalogLoad"] = "Loading catolog in progress...";

                    // No catalog 'in memory', so let's look for one
                    // First, for a serialized version on disk	
                    _Catalog = Catalog.Load();  // returns null if not found
                    _Cache = Searcharoo.Common.Cache.Load(); // [v7]
                    _Catalog.FileCache = _Cache;
                    // Still no Catalog, so we have to start building a new one
                    if (null == _Catalog)
                    {
                        //    Server.Transfer("SearchSpider.aspx");
                        _Catalog = (Catalog)Application["Searcharoo_Catalog"];
                        _Cache = (Searcharoo.Common.Cache)Application["Searcharoo_Cache"];
                        Trace.Write("Catalog retrieved from Cache[] " + _Catalog.Words);
                    }
                    else
                    {   // Yep, there was a serialized catalog file
                        // Don't forget to add to cache for next time (the Spider does this too)
                        Application["Searcharoo_Catalog"] = _Catalog;
                        Application["Searcharoo_Cache"] = _Cache;
                        Trace.Write("Deserialized catalog and put in Cache[] " + _Catalog.Words);
                    }

                    Application["CatalogLoad"] = "";
                }
            }

            if (this.SearchQuery == "")
            {
                //ucSearchPanelHeader.ErrorMessage = "Please type a word (or words) to search for<br>";
                //ucSearchPanelFooter.Visible = false;
                //ucSearchPanelFooter.IsFooter = true;
                ucSearchPanelHeader.IsSearchResultsPage = false;
            }
            else
            {
                //refactored into class - catalog can be build via a console application as well as the SearchSpider.aspx page
                Searcharoo.Engine.Search se = new Searcharoo.Engine.Search();
                SortedList output = this.GetSearchResults(se); // se.GetResults(this.SearchQuery, _Catalog);

                _NumberOfMatches = output.Count.ToString();
                if (output.Count > 0)
                {
                    _PagedResults.DataSource = output.GetValueList();
                    _PagedResults.AllowPaging = true;
                    _PagedResults.PageSize = MaxResultsPerPage; //;Preferences.ResultsPerPage; //10;
                    _PagedResults.CurrentPageIndex = Request.QueryString["page"] == null ? 0 : Convert.ToInt32(Request.QueryString["page"]) - 1;

                    _Matches = se.SearchQueryMatchHtml;
                    _DisplayTime = se.DisplayTime;
                    _Geocoded = se.GeocodedMatches;

                    SearchResults.DataSource = _PagedResults;
                    SearchResults.DataBind();
                }
                else
                {
                    lblNoSearchResults.Visible = true;
                }
                // Set the display info in the top & bottom user controls
                ucSearchPanelHeader.Word = this.SearchQuery;
                //ucSearchPanelFooter.Visible = true;
                //ucSearchPanelFooter.IsFooter = true;
                ucSearchPanelHeader.IsSearchResultsPage = true;
            }

        } // Page_Load


        public string CreatePageUrl(string searchFor, int pageNumber)
        {
            return "Search.aspx?" + Preferences.QuerystringParameterName + "=" + this.SearchQuery + "&page=" + pageNumber;
        }
    }
}