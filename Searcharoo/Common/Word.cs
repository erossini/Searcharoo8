using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using cd.net;
using Searcharoo.Common.Extensions;

namespace Searcharoo.Common
{
    /// <summary>Instance of a word</summary>
    [Serializable]
    public class Word
    {
        #region Private fields: _Text, _FileCollection
        /// <summary>Collection of files the word appears in</summary>
        /// <remarks>Key = File object, Value = Number of times word appears</remarks>
        private System.Collections.Hashtable _FileCollection = new System.Collections.Hashtable();

        private Dictionary<File, List<int>> _FilePositionCollection = new System.Collections.Generic.Dictionary<File, List<int>>();
        /// <summary>The word itself</summary>
        private string _Text;
        #endregion

        /// <summary>
        /// The catalogued word
        /// </summary>
        [XmlElement("t")]
        public string Text
        {
            get { return _Text; }
            set { _Text = value.UnicodeToCharacter(); }
        }
        /// <summary>
        /// Files that this Word appears in
        /// </summary>
        [XmlElement("i")]
        public File[] Files
        {
            get
            {
                File[] fileArray = new File[_FilePositionCollection.Count];
                //_FileCollection.Keys.CopyTo(fileArray, 0);
                _FilePositionCollection.Keys.CopyTo(fileArray, 0);
                return fileArray;
            }
            set
            {
                File[] fileArray = value;
                Hashtable index = new Hashtable();
            }
        }

        /// <summary>
        /// Empty constructor required for serialization
        /// </summary>
        public Word() { }

        /// <summary>Constructor with first file reference</summary>
        public Word(string text, File infile, int position)
        {
            _Text = text.UnicodeToCharacter();

            //WordInFile thefile = new WordInFile(filename, position);
            _FileCollection.Add(infile, 1);

            // [v7]
            List<int> l = new List<int>();
            l.Add(position);
            _FilePositionCollection.Add(infile, l);
        }

        /// <summary>Add a file referencing this word</summary>
        public void Add(File infile, int position)
        {
            //if (_FileCollection.ContainsKey(infile))
            //{
            //    int wordcount = (int)_FileCollection[infile];
            //    _FileCollection[infile] = wordcount + 1; //thefile.Add (position);
            //}
            //else
            //{
            //    //WordInFile thefile = new WordInFile(filename, position);
            //    _FileCollection.Add(infile, 1);
            //}

            // [v7]
            if (_FilePositionCollection.ContainsKey(infile))
            {
                _FilePositionCollection[infile].Add(position);
            }
            else
            {
                List<int> l = new List<int>();
                l.Add(position);
                _FilePositionCollection.Add(infile, l);
            }
        }

        /// <summary>Collection of files containing this Word (Value=WordCount)</summary>
        [Obsolete("Use InFilesWithPosition instead")]
        public Hashtable InFiles()
        {
            return _FileCollection;
        }

        /// <summary>Collection of files containing this Word (Value=List of position numbers)</summary>
        public Dictionary<File, List<int>> InFilesWithPosition()
        {
            return _FilePositionCollection; // [v7]
        }

        /// <summary>Debug string</summary>
        public override string ToString()
        {
            string temp = "";
            foreach (object tempFile in _FileCollection.Values) temp += ((File)tempFile).ToString();
            return "\tWORD :: " + _Text + "\n\t\t" + temp + "\n";
        }
    }  
}
