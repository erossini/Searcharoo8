using System;
using System.IO;
using System.Runtime.Serialization;

namespace Searcharoo.Common
{
    /// <summary>
    /// Required by Binary Deserializer
    /// </summary>
    /// <remarks>
    /// .NET XML and SOAP Serialization Samples, Tips (goxman)
    /// http://www.codeproject.com/soap/Serialization_Samples.asp
    /// 
    /// It's a long story, but basically if you DON'T provide this information
    /// the deserializer gets very confused if the code is recompiled and therefore
    /// has a different 'Type(version)'. See the Catalog.Save() method for it's use.
    /// </remarks>
    public class CatalogBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            // get the 'fully qualified (ie inc namespace) type name' into an array
            string[] typeInfo = typeName.Split('.');
            // because the last item is the class name, which we're going to 'look for'
            // in *this* namespace/assembly
            string className = typeInfo[typeInfo.Length - 1];
            if (className.Equals("Catalog"))
            {
                return typeof(Catalog);
            }
            else if (className.Equals("Word"))
            {
                return typeof(Word);
            }
            else if (className.Equals("CatalogWordFile"))
            {
                return typeof(CatalogWordFile);
            }
            else if (className.Equals("File"))
            {
                return typeof(File);
            }
            else if (className.Equals("Cache"))
            {
                return typeof(Cache);
            }
            else if (className.Equals("CacheFile"))
            {
                return typeof(Cache);
            }
            else
            {	// pass back exactly what was passed in!
                return Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));
            }
        }
    }
}
