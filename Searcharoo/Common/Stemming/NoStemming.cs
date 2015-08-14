using System;

namespace Searcharoo.Common
{
    public class NoStemming : IStemming
    {
        public string StemWord(string word)
        {
            return word;
        }
    }
}