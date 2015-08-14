<%@ Page Language="C#" AutoEventWireup="true"  Buffer="false" %>
<script runat="server">
    /// <summary>
    /// Handles 404 (File Not Found) to emit KML for Google Earth to display [v6]
    /// </summary>
    /// <remarks>
    /// Whether the original (not found) url is in the aspxerrorpath OR x-rewrite-url 
    /// depends on how the custom error handling is setup: in IIS5/6 via the Custom Errors
    /// Tab, or in Cassini/IIS7 via web.config (at least, I think that's how they differ)
    /// </remarks>
    public void Page_Load()
    {
        string qs = Request.QueryString["aspxerrorpath"]; // if 404 set in web.config
        if (qs == null)
        {
            qs = Request.Headers["X-REWRITE-URL"]; // if 404 set to URL /404.aspx in IIS5
        }
        if (qs.ToLower().StartsWith("/searchkml/"))
        {
            if (qs.ToLower().EndsWith(".kml"))
            {   // extract the actual search query
                string q = qs.Substring("/SearchKml/".Length, qs.Length - "/SearchKml/".Length - ".kml".Length);
                Response.BufferOutput = false;
                Response.StatusCode = 200;
                Response.StatusDescription = "OK";
                Response.ContentType = "application/vnd.google-earth.kml+xml";
                Response.AddHeader("content-disposition", "attachment; filename=" + q + ".kml");
                // execute the query using the SearchKml.aspx page
                Server.Execute("/SearchKml.aspx?searchfor=" + q);
                Response.End();
            }
        }
        if (qs.ToLower().StartsWith("/searchjson/"))
        {
            if (qs.ToLower().EndsWith(".js"))
            {   // extract the actual search query
                string q = qs.Substring("/SearchJson/".Length, qs.Length - "/SearchJson/".Length - ".js".Length);
                Response.BufferOutput = false;
                Response.StatusCode = 200;
                Response.StatusDescription = "OK";
                Response.ContentType = "text/javascript";
                //Response.AddHeader("content-disposition", "attachment; filename=" + q + ".kml");
                // execute the query using the SearchKml.aspx page
                Server.Execute("/SearchJson.aspx?searchfor=" + q);
                Response.End();
            }
        }
        Response.StatusCode = 404;
        // otherwise the basic '404' message below gets written to the browser
    }
</script>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>404</title>
    <meta name="description" http-equiv="description" content="The page could not be found." />
    <meta name="robots" http-equiv="robots" content="noindex,nofollow" />
</head>
<body>
    <div>
        404 - Page not found
    </div>
</body>
</html>
