using System;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using cd.net;
using Searcharoo.Common.Extensions;

namespace Searcharoo.Common
{
    /// <summary>
    /// Catalog of Words (and Files)
    /// <summary>
    /// <remarks>
    /// XmlInclude
    /// http://pluralsight.com/blogs/craig/archive/2004/07/08/1580.aspx
    /// 
    /// added v6 : fallback loading of catalog from the WebAppCatalogResource assembly
    /// to get around issues where the Trust level (eg. in shared hosting) is restricted
    /// to NOT ALLOW File.IO or WebClient requests. This requires the catalog to be built
    /// using the Indexer.EXE then compiled into a DLL then uploaded to /bin/
    /// </remarks>
    [Serializable]
    [System.Xml.Serialization.XmlInclude(typeof(Searcharoo.Common.Word))]
    [System.Xml.Serialization.XmlInclude(typeof(Searcharoo.Common.Location))]
    [System.Xml.Serialization.XmlInclude(typeof(Searcharoo.Common.CatalogWordFile))]
    public class Catalog
    {
        /// <summary>
        /// Internal datastore of Words referencing Files
        /// </summary>
        /// <remarks>
        /// Hashtable
        /// key    = STRING representation of the word, 
        /// value  = Word OBJECT (with File collection, etc)
        /// </remarks>
        private System.Collections.Hashtable _Index;	//TODO: implement collection with faster searching

        private System.Collections.Generic.Dictionary<File,int> _FileIndex;	//TODO: implement collection with faster searching

        private Cache _Cache;

        /// <summary>
        /// Words in the Catalog
        /// </summary>
        /// <remarks>
        /// Added property to allow Serialization to disk
        /// NOTE: the XmlInclude attribute on the Catalog class, which is what
        /// enables this array of 'non-standard' objects to be serialized correctly
        /// </remarks>
        [XmlElement("o")]
        [XmlIgnore()]
        [Obsolete("Use WordFiles and Files properties")]
        public Word[] Words
        {
            get
            {
                Word[] wordArray = new Word[_Index.Count];
                _Index.Values.CopyTo(wordArray, 0);
                return wordArray;
            }
            set
            {
                Word[] wordArray = value;
                _Index = new Hashtable();   //HACK: index doesn't get populated with wordArray
            }
        }

        [XmlElement("w")]
        public CatalogWordFile[] WordFiles
        {
            get 
            {
                PrepareForSerialization();
                return _WordfileArray;
            }
            set 
            {
                _WordfileArray = value;
                PostDeserialization();
            }
        }

        [XmlElement("f")]
        public File[] Files
        {
            get
            {
                PrepareForSerialization();
                return _FileList.ToArray();
            }
            set
            {
                File[] fa = value;
                _FileList = new List<File>(fa);
                PostDeserialization();
            }
        }

        #region Private fields and methods to manage XmlSerialization

        /// <summary>
        /// List of File objects that were referenced in the Catalog
        /// </summary>
        private System.Collections.Generic.List<File> _FileList;

        /// <summary>
        /// Array of CatalogWordFile objects, with 'ids' for each File 
        /// rather than a reference to a File object
        /// </summary>
        private CatalogWordFile[] _WordfileArray;
        private bool _SerializePreparationDone = false;

        /// <summary>
        /// Property helper for Files &amp; CatalogWordFiles, ensures the data retrieved
        /// from those two properties is 'in sync'
        /// </summary>
        private void PrepareForSerialization()
        {
            if (_SerializePreparationDone) return;
            // Create the list of 'Files' (ie. pages indexed) [v7]
            _FileList = new List<File>();
            foreach (File f in _FileIndex.Keys)
            {
                _FileList.Add(f);
                _Cache.SetIndexId(f.Url, f.IndexId);
            }

            _WordfileArray = new CatalogWordFile[_Index.Count];
            Word[] wordArray = new Word[_Index.Count];
            _Index.Values.CopyTo(wordArray, 0);
            
            // go through all the words
            for (int i = 0; i < wordArray.Length; i++)
            {
                CatalogWordFile wf = new CatalogWordFile();
                wf.Text = wordArray[i].Text;

                foreach (File f in wordArray[i].Files)
                {   // for each File, append the File.IndexId AND the List<int> of positions
                    wf.FileIdsWithPosition.Add(f.IndexId, wordArray[i].InFilesWithPosition()[f]);
                }

                _WordfileArray[i] = wf;
            }
            _SerializePreparationDone = true;
        }

        /// <summary>
        /// Property helper for Files &amp; WordFiles, ensures when
        /// they are both 'set', the internal Catalog datastructure is
        /// setup correctly
        /// </summary>
        private void PostDeserialization()
        {
            if ((_WordfileArray != null) && (_FileList != null))
            { 
                foreach (CatalogWordFile wf in _WordfileArray)
                {
                    //foreach (int i in wf.FileIds)
                    //{
                    //    this.Add(wf.Text, _FileList[i],-1);
                    //}

                    foreach (int i in wf.FileIdsWithPosition.Keys)
                    { 
                        // get the file object
                        File f = null;
                        foreach (File h in _FileList)
                        {
                            if (h.IndexId == i)
                            { f = h; break; }
                        }
                        if (f == null)
                        {
                         //   throw new NullReferenceException("There should be a file object matching index " + i);
                            System.Diagnostics.Debug.WriteLine("There should be a file object matching index " + i);
                        }
                       else
                        foreach (int j in wf.FileIdsWithPosition[i])
                        {
                            this.Add(wf.Text, f, j);
                        }
                    }
                }
            }
        }
        #endregion
        

        /// <summary>
        /// String array representing the list of words. 
        /// Used mainly for debugging - ie in the Save() method - so you can 
        /// see what the Spider found.
        /// </summary>
        /// <remarks>
        /// Because there is no 'set' accessor, this property is not XmlSerialized;
        /// XmlIgnore attribute added anyway.
        /// </remarks>
        [XmlIgnore()]
        public string[] Dictionary
        {
            get
            {
                string[] wordArray = new string[_Index.Count];
                _Index.Keys.CopyTo(wordArray, 0);
                return wordArray;
            }
        }
        /// <summary>
        /// Number of Words in the Catalog
        /// </summary>
        /// <remarks>
        /// Because there is no 'set' accessor, this property is not XmlSerialized
        /// </remarks>
        public int Length
        {
            get { return _Index.Count; }
        }

        public Cache FileCache
        {
            get { return _Cache; }
            set { _Cache = value; }
        }

        /// <summary>
        /// Constructor - creates the Hashtable for internal data storage.
        /// </summary>
        public Catalog()
        {
            _Index = new System.Collections.Hashtable();
            _FileIndex = new System.Collections.Generic.Dictionary<File, int>();
            _Cache = new Cache();
        }

        /// <summary>
        /// Add a new Word/File pair to the Catalog
        /// </summary>
        public bool Add(string word, File infile, int position)
        {
            word = word.UnicodeToCharacter();
            infile.Url = infile.Url.UnicodeToCharacter();
            infile.Description = infile.Description.UnicodeToCharacter();
            infile.KeywordString = infile.KeywordString.UnicodeToCharacter();
            infile.Title = infile.Title.UnicodeToCharacter();

            // ### Keep a list of all the files we catalog [v7]
            if (!_FileIndex.ContainsKey(infile))
            {
                _FileIndex.Add(infile, _FileIndex.Count);
                if (infile.IndexId < 0)
                    infile.IndexId = _FileIndex.Count - 1;  // zero based
            }
            // ### Make sure the Word object is in the index ONCE only
            if (_Index.ContainsKey(word))
            {
                Word theword = (Word)_Index[word];	// get Word from Index, then add this file reference to the Word
                theword.Add(infile, position);
            }
            else
            {
                Word theword = new Word(word, infile, position);	// create a new Word object and add to Index
                _Index.Add(word, theword);
            }
            _SerializePreparationDone = false;  // adding to the catalog invalidates 'serialization preparation'
            return true;
        }

        /// <summary>
        /// Returns all the Files which contain the searchWord
        /// </summary>
        /// <returns>
        /// Hashtable prior to [v7]
        /// </returns>
        public Dictionary<File, List<int>> Search(string searchWord)
        {
            // apply the same 'trim' as when we're building the catalog
            //searchWord = searchWord.Trim(' ','?','\"',',','\'',';',':','.','(', ')','[',']','%','*','$','-').ToLower();
            Dictionary<File, List<int>> retval = null;
            if (_Index.ContainsKey(searchWord))
            {	// does all the work !!!
                Word thematch = (Word)_Index[searchWord];
                //retval = thematch.InFiles(); // return the collection of File objects
                retval = thematch.InFilesWithPosition();
            }
            return retval;
        }

        /// <summary>
        /// Debug string
        /// </summary>
        public override string ToString()
        {
            string wordlist = "";
            //foreach (object w in index.Keys) temp += ((Word)w).ToString();	// output ALL words, will take a long time
            return "CATALOG :: " + _Index.Values.Count.ToString() + " words.\n" + wordlist;
        }

        /// <summary>
        /// Save the catalog to disk by BINARY serializing the object graph as a *.DAT file.
        /// </summary>
        /// <remarks>
        /// For 'reference', the method also saves XmlSerialized copies of the Catalog (which
        /// can get quite large) and just the list of Words that were found. In production, you
        /// would probably comment out/remove the Debugging code.
        /// 
        /// You may also wish to use a difficult-to-guess filename for the serialized data, 
        /// or else change the .DAT file extension to something that will be not be served by
        /// IIS (so that other people can't download your catalog).
        /// </remarks>
        public void Save()
        {
            // XML
            if (Preferences.InMediumTrust)
            {
                // TODO: Maybe use to save as ZIP - save space on disk? http://www.123aspx.com/redir.aspx?res=31602
                string xmlFileName = Path.GetDirectoryName(Preferences.CatalogFileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(Preferences.CatalogFileName) + ".xml";
                Kelvin<Catalog>.ToXmlFile(this, xmlFileName);

                //XmlSerializer serializerXmlCatalog = new XmlSerializer(typeof(Catalog));
                //System.IO.TextWriter writer = new System.IO.StreamWriter(xmlFileName);
                //serializerXmlCatalog.Serialize(writer, this);
                //writer.Close();
                
                _Cache.Save();

                return;
            }

            // BINARY http://www.dotnetspider.com/technology/kbpages/454.aspx
            System.IO.Stream stream = new System.IO.FileStream(Preferences.CatalogFileName, System.IO.FileMode.Create);
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            formatter.Serialize(stream, this);
            stream.Close();
            
            _Cache.Save();
            
            #region Debugging Serialization - these are only really useful for looking at; they're not re-loaded
            if (Preferences.DebugSerializeXml)
            {
                //Kelvin<Catalog>.ToBinaryFile(this, Path.GetFileNameWithoutExtension(Preferences.CatalogFileName) + "_Kelvin" + Path.GetExtension(Preferences.CatalogFileName));
                //Kelvin<Catalog>.ToXmlFile(this, Path.GetFileNameWithoutExtension(Preferences.CatalogFileName) + "_Kelvin.xml");
                Kelvin<string[]>.ToXmlFile(this.Dictionary, Path.GetFileNameWithoutExtension(Preferences.CatalogFileName) + "_debugwords.xml");

                // XML http://www.devhood.com/Tutorials/tutorial_details.aspx?tutorial_id=236
                //XmlSerializer serializerXmlWords = new XmlSerializer(typeof(string[]));
                //System.IO.TextWriter writerW = new System.IO.StreamWriter(Path.GetFileNameWithoutExtension(Preferences.CatalogFileName) + "_words.xml");
                //serializerXmlWords.Serialize(writerW, this.Dictionary);
                //writerW.Close();
            }
            #endregion
        }

        /// <summary>
        /// Use Kelvin too
        /// </summary>
        /// <returns>the catalog deserialized from disk, or NULL</returns>
        public static Catalog Load()
        {
            if (Preferences.InMediumTrust)
            {
                try
                {
                    string xmlFileName = Path.GetDirectoryName(Preferences.CatalogFileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(Preferences.CatalogFileName) + ".xml";
                    if (System.IO.File.Exists(xmlFileName))
                    {
                        Catalog c1 = Kelvin<Catalog>.FromXmlFile(xmlFileName);
                        return c1;
                    }
                }
                catch (Exception ex)
                {   // [v6] : if cannot load from .DAT or .XML, try to load from compiled resource
                    try
                    {   // http://www.devhood.com/tutorials/tutorial_details.aspx?tutorial_id=75
                        System.Reflection.Assembly a = System.Reflection.Assembly.Load("WebAppCatalogResource");
                        string[] resNames = a.GetManifestResourceNames();

                        Catalog c2 = Kelvin<Catalog>.FromResource(a, resNames[0]);
                        return c2;
                    }
                    catch (Exception e1)
                    {
                        throw new Exception("Searcharoo Catalog.Load() ", e1);
                    }
                }
                return null;
            }
            else
            {   // hopefully in Full trust
                // using Binary serialization requires the Binder because of the embedded 'full name'
                // of the serializing assembly - all the above methods using Xml do not have this requirement
                if (System.IO.File.Exists(Preferences.CatalogFileName))
                {
                    object deserializedCatalogObject;
                    using (System.IO.Stream stream = new System.IO.FileStream(Preferences.CatalogFileName, System.IO.FileMode.Open))
                    {
                        System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        //object m = formatter.Deserialize (stream); // This doesn't work, SerializationException "Cannot find the assembly <random name>"
                        formatter.Binder = new CatalogBinder();	// This custom Binder is REQUIRED to find the classes in our current 'Temporary ASP.NET Files' assembly
                        deserializedCatalogObject = formatter.Deserialize(stream);
                    } //stream.Close();
                    Catalog catalog = deserializedCatalogObject as Catalog;
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