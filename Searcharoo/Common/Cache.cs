using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using cd.net;
using Searcharoo.Common.Extensions;

namespace Searcharoo.Common
{
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(CachedFile))]
    public class CachedFile
    {
        private string url;
        private string[] words;

        private int indexId;
        public int IndexId
        {
            get { return indexId; }
            set { indexId = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value.UnicodeToCharacter(); }
        }

        public string[] Words
        {
            get { return words; }
            set { words = value; }
        }
    }

    /// <summary>
    /// Cache of each Document's text content (for search result 'preview')
    /// </summary>
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Cache))]
    public class Cache
    {
        /// <summary>
        /// Internal datastore of Files referencing cached content
        /// </summary>
        /// <remarks>
        /// Hashtable
        /// key    = STRING representation of the Url, 
        /// value  = STRING[] of all words at that Url
        /// </remarks>
        private System.Collections.Hashtable _Index = new System.Collections.Hashtable();	//TODO: implement collection with faster searching

        public bool Add(string[] words, File infile)
        {
            // ### Make sure the Word object is in the index ONCE only
            if (_Index.ContainsKey(infile.Url.UnicodeToCharacter()))
            {
                // already cached
                return false;
            }
            else
            {
                CachedFile cf = new CachedFile();
                cf.Url = infile.Url.UnicodeToCharacter();

                for(int i = 0; i < words.Length; i++)
                {
                    words[i] = words[i].UnicodeToCharacter();
                }
                cf.Words = words;

                _Index.Add(infile.Url, cf);
            }
            return true;
        }

        public bool Contains (string url)
        {
            return _Index.Contains(url.ToLower());
        }

        public void SetIndexId(string url, int index)
        {
            CachedFile cf = (CachedFile)_Index[url.ToLower()];
            if (cf != null)
            {
                cf.IndexId = index;
            }
        }

        public string[] GetDocumentCache(string url)
        {
            CachedFile cf = (CachedFile)_Index[url.ToLower()];
            if (cf != null)
            {
                return cf.Words;
            }
            return null;
        }

        [XmlElement("f")]
        public CachedFile[] Files
        {
            get
            {
                List<CachedFile> l = new List<CachedFile>();
                foreach (object o in _Index.Values)
                {
                    l.Add((CachedFile)o);
                }
                return l.ToArray();
            }
            set
            {
                foreach (object o in value)
                { 
                    CachedFile cf = (CachedFile)o;
                    _Index.Add(cf.Url, cf);
                }
            }
        }

        public bool Save()
        {
            string fileNameBase = System.IO.Path.GetDirectoryName(Preferences.CatalogFileName) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Preferences.CatalogFileName);
            // XML
            if (Preferences.InMediumTrust)
            {
                // TODO: Maybe use to save as ZIP - save space on disk? http://www.123aspx.com/redir.aspx?res=31602
                string xmlFileName = fileNameBase + "-cache.xml";
                Kelvin<Cache>.ToXmlFile(this, xmlFileName);

                return true;
            }

            // BINARY http://www.dotnetspider.com/technology/kbpages/454.aspx
            System.IO.Stream stream = new System.IO.FileStream(fileNameBase + "-cache.dat", System.IO.FileMode.Create);
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Close();

            return true;
        }
        public static Cache Load()
        {
            string fileNameBase = System.IO.Path.GetDirectoryName(Preferences.CatalogFileName) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(Preferences.CatalogFileName);
            if (Preferences.InMediumTrust)
            {
                try
                {
                    string xmlFileName = fileNameBase + "-cache.xml";
                    if (System.IO.File.Exists(xmlFileName))
                    {
                        Cache c1 = Kelvin<Cache>.FromXmlFile(xmlFileName);
                        return c1;
                    }
                    else
                    {
                        throw new Exception("Could not load cache from file " + xmlFileName + " - check that it exists.");
                    }
                }
                catch (Exception)
                {   // [v6] : if cannot load from .DAT or .XML, try to load from compiled resource
                    try
                    {   // http://www.devhood.com/tutorials/tutorial_details.aspx?tutorial_id=75
                        System.Reflection.Assembly a = System.Reflection.Assembly.Load("WebAppCatalogResource");
                        string[] resNames = a.GetManifestResourceNames();
                        if (resNames.Length > 1)
                        {
                            Cache c2 = Kelvin<Cache>.FromResource(a, resNames[1]);
                            return c2;
                        }
                        else 
                        {   // no 'cache' in the resources 
                            throw new Exception("Could not find '-cache' resource in " + a.FullName 
                                + " - check it was compiled in correctly (ie. the .XML file is marked as embedded resource).");
                        }
                    }
                    catch (Exception e1)
                    {
                        throw new Exception("Searcharoo Cache.Load() " + e1.Message, e1);
                    }
                }
            }
            else
            {   // hopefully in Full trust
                // using Binary serialization requires the Binder because of the embedded 'full name'
                // of the serializing assembly - all the above methods using Xml do not have this requirement
                if (System.IO.File.Exists(fileNameBase + "-cache.dat"))
                {
                    object deserializedCacheObject;
                    using (System.IO.Stream stream = new System.IO.FileStream(fileNameBase + "-cache.dat", System.IO.FileMode.Open))
                    {
                        System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        //object m = formatter.Deserialize (stream); // This doesn't work, SerializationException "Cannot find the assembly <random name>"
                        formatter.Binder = new CatalogBinder();	// This custom Binder is REQUIRED to find the classes in our current 'Temporary ASP.NET Files' assembly
                        deserializedCacheObject = formatter.Deserialize(stream);
                    } //stream.Close();
                    Cache catalog = deserializedCacheObject as Cache;
                    return catalog;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
