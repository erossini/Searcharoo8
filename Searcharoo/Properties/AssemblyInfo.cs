using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Searcharoo.Common")]
[assembly: AssemblyDescription("Searcharoo4 core code")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ConceptDevelopment.net")]
[assembly: AssemblyProduct("Searcharoo.Common")]
[assembly: AssemblyCopyright("Copyright © 2007")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: System.CLSCompliant(true)]
[assembly: ComVisible(false)]
[assembly: Guid("1ef18d11-774f-4632-b009-0d6e2b0c1ada")]

[assembly: AssemblyVersion("4.0.0.0")]
[assembly: AssemblyFileVersion("4.0.0.0")]
/*
This project contains:

	Preferences		contains dereferences for the web.config settings

	CatalogBinder	allow the Catalog to be deserialized
	Catalog			contains Hashtable of Words 
	Word			instance of a Word, with a Hashtable of File references
	File			instance of a File, with URL and other attributes
	ResultFile		instance of a File with search ranking (inherits File) used for results only
	
	SpiderProgressEventHandler	used to send crawl progress messages
	ProgressEventArgs	used to send crawl progress messages
	Spider			contains all the intelligence to crawl links, download, parse and index pages
	
	IGoWord			Go words interface + implementation
	IStopper		Stop words interface + implementation
	IStemming		Stemming interface + implementation

*/
#region Url references
/*
http://www.dotnetbips.com/displayarticle.aspx?id=43f
http://www.microbion.co.uk/developers/csharp/dirlist.htm

Stripping HTML
http://www.4guysfromrolla.com/webtech/042501-1.shtml

Opening a file from ASP.NET
http://aspnet.4guysfromrolla.com/articles/051802-1.aspx

Practical parsing in Regular Expressions
http://weblogs.asp.net/rosherove/articles/6946.aspx
*/
#endregion