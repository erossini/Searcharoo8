using System;
using Searcharoo.Common;
using Mono.GetOptions;

namespace Searcharoo.Indexer
{
    public class CommandLinePreferences : Mono.GetOptions.Options
    {
        #region private
        private int _VerbosityLevel = 3;
        private string _LogFileName = "";
        #endregion
        #region ctor
        public CommandLinePreferences()
        {
            this.ParsingMode = OptionsParsingMode.Both;
        }
        #endregion

        [Option("Verbosity level [1-5]", 'v', "verbosity")]
        public int Verbosity
        {
            get
            { return _VerbosityLevel; }
            set
            {
                _VerbosityLevel = value;
                Console.WriteLine("Verbosity was set to : " + _VerbosityLevel);
            }
        }

        [Option("Log file name", 'l', "log")]
        public string LogFileName
        {
            get
            { return _LogFileName; }
            set
            {
                _LogFileName = value;
                Console.WriteLine("LogFileName was set to : " + _LogFileName);
            }
        }

        [Option("Show usage syntax", 'u', "usage")]
        public override WhatToDoNext DoUsage()
        {
            base.DoUsage();
            return WhatToDoNext.AbandonProgram; //WhatToDoNext.GoAhead;
        }

        public override WhatToDoNext DoHelp() // uses parent's OptionAttribute as is
        {
            base.DoHelp();
            return WhatToDoNext.AbandonProgram; //WhatToDoNext.GoAhead;
        }
    }
}
