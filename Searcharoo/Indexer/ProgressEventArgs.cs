using System;

namespace Searcharoo.Indexer
{
    /// <summary>
    /// Declaring the Event Handler delegate
    /// </summary>
    public delegate void SpiderProgressEventHandler(object source, ProgressEventArgs ea);

    /// <summary>
    /// Progress Message logging level
    /// </summary>
    public enum Level
    { 
        /// <summary>Code *shouldn't* set ProgressEventHandlers to zero</summary>
        None = 0,
        Minimal = 1,
        Informational = 2,
        Detailed = 3,
        VeryDetailed = 4,
        /// <summary>This output should include ALL words in ALL files indexed - it is VERY VERBOSE !!</summary>
        Verbose = 5
    }
    /// <summary>
    /// Declare the Event arguments
    /// </summary>
    public class ProgressEventArgs : EventArgs
    {
        private int _Level = 0;
        private string _Message = null;
        private string _Description = null;

        public ProgressEventArgs(int level, string message)
        {
            this._Level = level;
            this._Message = message;
        }
        
        public ProgressEventArgs(int level, string message, string description)
        {
            this._Level = level;
            this._Message = message;
        }

        public int Level
        {
            get { return this._Level; }
        }

        public string Message
        {
            get { return this._Message; }
        }

        public string Description
        {
            get { return this._Description; }
        }

        public override string ToString()
        {
            return String.Format("{0}:{1}\r\n{2}", _Level, _Message, _Description);
        }
    }
}