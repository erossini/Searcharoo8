using Searcharoo.WebApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace WebApplication
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        void Application_Start(object sender, EventArgs e)
        {
            log.Info("Application start");

            Application.Lock();
            Application["CatalogLoad"] = "";
            Application["DateStart"] = DateTime.Now;
            Application.UnLock();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {
            double diff = 0;

            try
            {
                TimeSpan tmp = (DateTime)Application["DateStart"] - DateTime.Now;
                diff = tmp.TotalSeconds;
            }
            catch (Exception ex)
            { }

            log.Info("Application End - " + diff.ToString());
        }
    }
}