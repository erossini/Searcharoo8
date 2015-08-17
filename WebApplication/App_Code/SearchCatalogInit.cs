using Searcharoo.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace Searcharoo.WebApplication
{
    public class SearchCatalogInit
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>Get from Cache</summary>
        private Catalog _Catalog = null;

        /// <summary>Get from cache</summary>
        private Cache _Cache = null;

        public void GetCatalog(Page pg)
        {
            if ((string)pg.Application["CatalogLoad"] == "")
            {
                pg.Application.Lock();
                pg.Application["CatalogLoad"] = "Loading catalog in progress...";
                pg.Application.UnLock();

                log.Info("Loading catolog in progress...");

                // No catalog 'in memory', so let's look for one
                // First, for a serialized version on disk
                try
                {
                    _Catalog = Catalog.Load();  // returns null if not found
                }
                catch (Exception ex)
                {
                    log.Error("Loading catalog error", ex);
                }

                pg.Application.Lock();
                pg.Application["CatalogLoad"] = "Loading cache catalog in progress...";
                pg.Application.UnLock();

                log.Info("Loading cache catolog in progress...");

                try
                {
                    _Cache = Searcharoo.Common.Cache.Load();
                    _Catalog.FileCache = _Cache;
                }
                catch (Exception ex)
                {
                    log.Error("Loading cache catalog error", ex);
                }

                // Still no Catalog, so we have to start building a new one
                if (_Catalog == null)
                {
                    _Catalog = (Catalog)pg.Application["Searcharoo_Catalog"];
                    _Cache = (Searcharoo.Common.Cache)pg.Application["Searcharoo_Cache"];

                    if (_Catalog != null)
                    {
                        log.Info("Catalog retrieved from Cache[] " + _Catalog.Words);
                    }
                }
                else
                {
                    // Yep, there was a serialized catalog file
                    // Don't forget to add to cache for next time (the Spider does this too)
                    pg.Application.Lock();
                    pg.Application["Searcharoo_Catalog"] = _Catalog;
                    pg.Application["Searcharoo_Cache"] = _Cache;
                    pg.Application.UnLock();

                    if (_Catalog != null)
                    {
                        log.Info("Deserialized catalog and put in Cache[] " + _Catalog.Words);
                    }
                }

                pg.Application.Lock();
                pg.Application["CatalogLoad"] = "";
                pg.Application.UnLock();
            }
        }
    }
}