using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Searcharoo.Common;

namespace Searcharoo.WebApplication
{
    /// <summary>
    /// Search.ascx User Control base class
    /// </summary>
    public class SearchControlBase : UserControl
    {
        protected Panel pnlResultsSearch;
        protected Panel pnlHomeSearch;
        protected HtmlGenericControl pHeading;
        protected HtmlTableRow rowSummary, rowFooter1, rowFooter2; 

        /// <summary>Size of the searchable catalog (number of unique words)</summary>
        public int WordCount = -1;

        /// <summary>Word/s displayed in search input box</summary>
        public string Word = "";

        /// <summary>
        /// Error message - on Home Page version ONLY
        /// ie. ONLY when IsSearchResultsPage = true
        /// </summary>
        public string _ErrorMessage;

        /// <summary>Whether the standalone home page version, or the on Search Results page</summary>
        private bool _IsSearchResultsPage;

        /// <summary>Whether the control is placed at the Header or Footer</summary>
        protected bool _IsFooter;

        /// <summary>
        /// Value is either
        ///   false: being displayed on the 'home page' - only thing on the page
        ///   true:  on the Results page (at the top _and_ bottom)
        /// <summary>
        public bool IsSearchResultsPage
        {
            get { return _IsSearchResultsPage; }
            set
            {
                _IsSearchResultsPage = value;
                if (_IsSearchResultsPage)
                {
                    pnlHomeSearch.Visible = false;
                    pnlResultsSearch.Visible = true;
                }
                else
                {
                    pnlHomeSearch.Visible = true;
                    pnlResultsSearch.Visible = false;
                }
            }
        }
        /// <summary>
        /// Footer control has more 'display items' than the one shown
        /// in the Header - setting this property shows/hides them
        /// </summary>
        public bool IsFooter
        {
            set
            {
                _IsFooter = value;
                //pHeading.Visible = !_IsFooter;
                //rowFooter1.Visible = _IsFooter;
                //rowFooter2.Visible = _IsFooter;
                //rowSummary.Visible = !_IsFooter;
            }
        }
        /// <summary>
        /// Error message to be displayed if search input box is empty
        /// </summary>
        public string ErrorMessage
        {
            set
            {
                _ErrorMessage = value;
            }
        }
        /// <summary>
        /// Nothing actually happens on the User Control Page_Load () 
        /// ... for now
        /// <summary>
        protected void Page_Load(object sender, EventArgs ea)
        {
        }

        /// <summary>
        /// Was originally used in Searcharoo3.aspx to generate the top and bottom 
        /// search boxes from a single User Control 'instance'. Decided not to use
        /// that approach - but left this in for reference.
        /// <summary>
        [Obsolete("Render control to string to embed mulitple times in a page; but no longer required.")]
        public override string ToString()
        {
            System.IO.StringWriter writer = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter buffer = new System.Web.UI.HtmlTextWriter(writer);
            this.Render(buffer);
            return writer.ToString();
        }
    }
}
