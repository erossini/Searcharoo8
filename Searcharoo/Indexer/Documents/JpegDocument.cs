using System;
using System.Collections; // DictionaryEntry
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Searcharoo.Common
{
    /// <summary>
    /// Extract a handful of Exif/Xmp fields for indexing... 
    /// </summary>
    /// <remarks>
    /// 
    /// EXIFextractor by Asim Goheer
    /// http://www.codeproject.com/KB/graphics/exifextractor.aspx
    /// 
    /// XMP Metadata (2.0)
    /// http://www.shahine.com/omar/ReadingXMPMetadataFromAJPEGUsingC.aspx
    /// 
    /// Gallery Server Pro
    /// http://www.codeproject.com/KB/web-image/Gallery_Server_Pro.aspx
    /// 
    /// Another library
    /// http://www.codeproject.com/KB/GDI-plus/ImageInfo.aspx
    /// 
    /// Yet another (VB.NET)
    /// http://www.codeproject.com/KB/vb/exif_reader.aspx
    /// </remarks>
    public class JpegDocument : DownloadDocument
    {
        private string _All;
        private string _WordsOnly;

        public JpegDocument(Uri location):base(location)
        {
            Extension = "jpg";
        }

        /// <summary>
        /// Set 'all' and 'words only' to the same value (no parsing)
        /// </summary>
        public override string All
        {
            get { return _All; }
            set { 
                _All = value;
                _WordsOnly = _All;
            }
        }
        
        public override string WordsOnly
        {
            get { return _WordsOnly; }
        }

        public override string[] WordsArray
        {
            get { return this.WordsStringToArray(WordsOnly); }
        }
       
        /// <summary>
        /// no-op
        /// </summary>
        public override void Parse()
        {
            // no parsing 
        }

        /// <summary>
        /// Extract Title, Description, Keywords (tags), Make, Model from image
        /// </summary>
        /// <remarks>
        /// TODO: GPS coords to be searchable/retrievable
        /// Gps LatitudeRef : N
        /// Gps Latitude : 14.1136388888889
        /// Gps LongitudeRef : E
        /// Gps Longitude : 99.2318611111111
        /// Gps GpsTime : 0/0
        /// Gps GpsStatus : A
        /// Gps MapDatum : WGS-84
        /// </remarks>
        public override bool GetResponse(System.Net.HttpWebResponse webresponse)
        {
            string filename = System.IO.Path.Combine(Preferences.DownloadedTempFilePath, (System.IO.Path.GetFileName(this.Uri.LocalPath)));
            this.Title = System.IO.Path.GetFileNameWithoutExtension(filename);
            GpsData gl = new GpsData();
            SaveDownloadedFile(webresponse, filename);
            try
            {
                Goheer.EXIF.EXIFextractor er2 = new Goheer.EXIF.EXIFextractor(filename, "","");
                foreach (DictionaryEntry de in er2)
                {
                    //Console.WriteLine(de.Value); //s.First + " : " + s.Second);
                    switch (de.Key.ToString())
                    {
                        case "Equip Make"://: SONY
                            _WordsOnly += " " + de.Value.ToString().Trim(new char[] { '\0' });
                            break;
                        case "Equip Model": // : DSC-H9
                            _WordsOnly += " " + de.Value.ToString().Trim(new char[] { '\0' });
                            break;
                        case "Gps LatitudeRef": // : N
                            gl.LatitudeRef = de.Value.ToString().Trim(new char[]{'\0'});
                            break;
                        case "Gps Latitude": // : 14.1136388888889
                            gl.Latitude = de.Value.ToString().Trim(new char[] { '\0' });
                            break;
                        case "Gps LongitudeRef": // : E
                            gl.LongitudeRef = de.Value.ToString().Trim(new char[] { '\0' });
                            break;
                        case "Gps Longitude": // :
                            gl.Longitude = de.Value.ToString().Trim(new char[] { '\0' });
                            break;
                        default:
                            //_WordsOnly = _WordsOnly + de.Key + " : " + de.Value + Environment.NewLine;
                            break;
                    }
                }


                string xmp = GetXmpXmlDocFromImage(filename);
                LoadDoc(xmp);
                _WordsOnly += " " + Title;
                _WordsOnly += " " + Description;
                foreach (string k in Keywords)
                {
                    _WordsOnly += " " + k; // so they're indexed!
                }
                this.GpsLocation = gl.ToLocation();

                this.All = _WordsOnly;
                System.IO.File.Delete(filename);    // clean up
            }
            catch (Exception)
            {
//                ProgressEvent(this, new ProgressEventArgs(2, "IFilter failed on " + this.Uri + " " + e.Message + ""));
            }
            if (this.All != string.Empty)
            {
                if (this.Description == string.Empty)
                {   // if no description was set, use the rest of the text
                    this.Description = base.GetDescriptionFromWordsOnly(WordsOnly);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gps LatitudeRef : N
        /// Gps Latitude : 14.1136388888889
        /// Gps LongitudeRef : E
        /// Gps Longitude : 99.2318611111111
        /// </summary>
        private class GpsData
        {
            public string LatitudeRef = string.Empty;
            public string Latitude = string.Empty;
            public string LongitudeRef = string.Empty;
            public string Longitude = string.Empty;
            public override string ToString()
            {
                string location = string.Empty;
                if ((Latitude != string.Empty) && (Longitude != string.Empty))
                {   // lat & lon supplied
                    if (LatitudeRef.ToUpper() == "S") location += "-";
                    location += Latitude;
                    location += ',';
                    if (LongitudeRef.ToUpper() == "W") location += "-";
                    location += Longitude;
                }
                return location;
            }
            public Location ToLocation()
            {
                return Location.FromString(this.ToString());
            }
        }

#region XMP
        //private List<string> Keywords = new List<string>();
        private Int32 Rating;

        // http://www.shahine.com/omar/ReadingXMPMetadataFromAJPEGUsingC.aspx
        public static string GetXmpXmlDocFromImage(string filename)
        {
            string contents;
            string xmlPart;
            string beginCapture = "<rdf:RDF";
            string endCapture = "</rdf:RDF>";
            int beginPos;
            int endPos;

            using (System.IO.StreamReader sr = new System.IO.StreamReader(filename))
            {
                contents = sr.ReadToEnd();
                System.Diagnostics.Debug.Write(contents.Length + " chars" + Environment.NewLine);
                sr.Close();
            }

            beginPos = contents.IndexOf(beginCapture, 0);
            endPos = contents.IndexOf(endCapture, 0);

            System.Diagnostics.Debug.Write("xml found at pos: " + beginPos.ToString() + " - " + endPos.ToString());

            xmlPart = contents.Substring(beginPos, (endPos - beginPos) + endCapture.Length);

            System.Diagnostics.Debug.Write("Xml len: " + xmlPart.Length.ToString());

            return xmlPart;
        }

        // http://www.shahine.com/omar/ReadingXMPMetadataFromAJPEGUsingC.aspx
        private void LoadDoc(string xmpXmlDoc)
        {
            XmlDocument doc = new XmlDocument();
            XmlNamespaceManager NamespaceManager;
            try
            {
                doc.LoadXml(xmpXmlDoc);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occured while loading XML metadata from image. The error was: " + ex.Message);
            }

            try
            {
                doc.LoadXml(xmpXmlDoc);

                NamespaceManager = new XmlNamespaceManager(doc.NameTable);
                NamespaceManager.AddNamespace("rdf", "http://www.w3.org/1999/02/22-rdf-syntax-ns#");
                NamespaceManager.AddNamespace("exif", "http://ns.adobe.com/exif/1.0/");
                NamespaceManager.AddNamespace("x", "adobe:ns:meta/");
                NamespaceManager.AddNamespace("xap", "http://ns.adobe.com/xap/1.0/");
                NamespaceManager.AddNamespace("tiff", "http://ns.adobe.com/tiff/1.0/");
                NamespaceManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");

                // get ratings
                XmlNode xmlNode = doc.SelectSingleNode("/rdf:RDF/rdf:Description/xap:Rating", NamespaceManager);

                // Alternatively, there is a common form of RDF shorthand that writes simple properties as
                // attributes of the rdf:Description element.
                if (xmlNode == null)
                {
                    xmlNode = doc.SelectSingleNode("/rdf:RDF/rdf:Description", NamespaceManager);
                    xmlNode = xmlNode.Attributes["xap:Rating"];
                }

                if (xmlNode != null)
                {
                    this.Rating = Convert.ToInt32(xmlNode.InnerText);
                }

                // get keywords
                xmlNode = doc.SelectSingleNode("/rdf:RDF/rdf:Description/dc:subject/rdf:Bag", NamespaceManager);
                if (xmlNode != null)
                {

                    foreach (XmlNode li in xmlNode)
                    {
                        Keywords.Add(li.InnerText);
                    }
                }

                // get description
                xmlNode = doc.SelectSingleNode("/rdf:RDF/rdf:Description/dc:description/rdf:Alt", NamespaceManager);

                if (xmlNode != null)
                {
                    this.Description = xmlNode.ChildNodes[0].InnerText;
                }

                // get title
                xmlNode = doc.SelectSingleNode("/rdf:RDF/rdf:Description/dc:title/rdf:Alt", NamespaceManager);

                if (xmlNode != null)
                {
                    this.Title = xmlNode.ChildNodes[0].InnerText;
                }

            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error occured while readning meta-data from image. The error was: " + ex.Message);
            }
            finally
            {
                doc = null;
            }
        }
#endregion
    }
}


/* EXIFextract code from
 * http://www.codeproject.com/KB/graphics/exifextractor.aspx
 * 
 * includes updated GPS coordinate extraction by blackmanticore
 * http://www.codeproject.com/KB/graphics/exifextractor.aspx?fid=207371&select=2516453#xx2516453xx
 * http://www.codeproject.com/script/Membership/Profiles.aspx?mid=2045089
 */
namespace Goheer
{
    namespace EXIF
    {

        /// <summary>
        /// EXIFextractor Class
        /// </summary>
        /// <remarks>
        /// Code source:
        /// http://www.codeproject.com/KB/graphics/exifextractor.aspx
        /// </remarks>
        public class EXIFextractor : IEnumerable
        {
            /// <summary>
            /// Get the individual property value by supplying property name
            /// These are the valid property names :
            /// </summary>
            /// <remarks>
            /// "Exif IFD"
            /// "Gps IFD"
            /// "New Subfile Type"
            /// "Subfile Type"
            /// "Image Width"
            /// "Image Height"
            /// "Bits Per Sample"
            /// "Compression"
            /// "Photometric Interp"
            /// "Thresh Holding"
            /// "Cell Width"
            /// "Cell Height"
            /// "Fill Order"
            /// "Document Name"
            /// "Image Description"
            /// "Equip Make"
            /// "Equip Model"
            /// "Strip Offsets"
            /// "Orientation"
            /// "Samples PerPixel"
            /// "Rows Per Strip"
            /// "Strip Bytes Count"
            /// "Min Sample Value"
            /// "Max Sample Value"
            /// "X Resolution"
            /// "Y Resolution"
            /// "Planar Config"
            /// "Page Name"
            /// "X Position"
            /// "Y Position"
            /// "Free Offset"
            /// "Free Byte Counts"
            /// "Gray Response Unit"
            /// "Gray Response Curve"
            /// "T4 Option"
            /// "T6 Option"
            /// "Resolution Unit"
            /// "Page Number"
            /// "Transfer Funcition"
            /// "Software Used"
            /// "Date Time"
            /// "Artist"
            /// "Host Computer"
            /// "Predictor"
            /// "White Point"
            /// "Primary Chromaticities"
            /// "ColorMap"
            /// "Halftone Hints"
            /// "Tile Width"
            /// "Tile Length"
            /// "Tile Offset"
            /// "Tile ByteCounts"
            /// "InkSet"
            /// "Ink Names"
            /// "Number Of Inks"
            /// "Dot Range"
            /// "Target Printer"
            /// "Extra Samples"
            /// "Sample Format"
            /// "S Min Sample Value"
            /// "S Max Sample Value"
            /// "Transfer Range"
            /// "JPEG Proc"
            /// "JPEG InterFormat"
            /// "JPEG InterLength"
            /// "JPEG RestartInterval"
            /// "JPEG LosslessPredictors"
            /// "JPEG PointTransforms"
            /// "JPEG QTables"
            /// "JPEG DCTables"
            /// "JPEG ACTables"
            /// "YCbCr Coefficients"
            /// "YCbCr Subsampling"
            /// "YCbCr Positioning"
            /// "REF Black White"
            /// "ICC Profile"
            /// "Gamma"
            /// "ICC Profile Descriptor"
            /// "SRGB RenderingIntent"
            /// "Image Title"
            /// "Copyright"
            /// "Resolution X Unit"
            /// "Resolution Y Unit"
            /// "Resolution X LengthUnit"
            /// "Resolution Y LengthUnit"
            /// "Print Flags"
            /// "Print Flags Version"
            /// "Print Flags Crop"
            /// "Print Flags Bleed Width"
            /// "Print Flags Bleed Width Scale"
            /// "Halftone LPI"
            /// "Halftone LPIUnit"
            /// "Halftone Degree"
            /// "Halftone Shape"
            /// "Halftone Misc"
            /// "Halftone Screen"
            /// "JPEG Quality"
            /// "Grid Size"
            /// "Thumbnail Format"
            /// "Thumbnail Width"
            /// "Thumbnail Height"
            /// "Thumbnail ColorDepth"
            /// "Thumbnail Planes"
            /// "Thumbnail RawBytes"
            /// "Thumbnail Size"
            /// "Thumbnail CompressedSize"
            /// "Color Transfer Function"
            /// "Thumbnail Data"
            /// "Thumbnail ImageWidth"
            /// "Thumbnail ImageHeight"
            /// "Thumbnail BitsPerSample"
            /// "Thumbnail Compression"
            /// "Thumbnail PhotometricInterp"
            /// "Thumbnail ImageDescription"
            /// "Thumbnail EquipMake"
            /// "Thumbnail EquipModel"
            /// "Thumbnail StripOffsets"
            /// "Thumbnail Orientation"
            /// "Thumbnail SamplesPerPixel"
            /// "Thumbnail RowsPerStrip"
            /// "Thumbnail StripBytesCount"
            /// "Thumbnail ResolutionX"
            /// "Thumbnail ResolutionY"
            /// "Thumbnail PlanarConfig"
            /// "Thumbnail ResolutionUnit"
            /// "Thumbnail TransferFunction"
            /// "Thumbnail SoftwareUsed"
            /// "Thumbnail DateTime"
            /// "Thumbnail Artist"
            /// "Thumbnail WhitePoint"
            /// "Thumbnail PrimaryChromaticities"
            /// "Thumbnail YCbCrCoefficients"
            /// "Thumbnail YCbCrSubsampling"
            /// "Thumbnail YCbCrPositioning"
            /// "Thumbnail RefBlackWhite"
            /// "Thumbnail CopyRight"
            /// "Luminance Table"
            /// "Chrominance Table"
            /// "Frame Delay"
            /// "Loop Count"
            /// "Pixel Unit"
            /// "Pixel PerUnit X"
            /// "Pixel PerUnit Y"
            /// "Palette Histogram"
            /// "Exposure Time"
            /// "F-Number"
            /// "Exposure Prog"
            /// "Spectral Sense"
            /// "ISO Speed"
            /// "OECF"
            /// "Ver"
            /// "DTOrig"
            /// "DTDigitized"
            /// "CompConfig"
            /// "CompBPP"
            /// "Shutter Speed"
            /// "Aperture"
            /// "Brightness"
            /// "Exposure Bias"
            /// "MaxAperture"
            /// "SubjectDist"
            /// "Metering Mode"
            /// "LightSource"
            /// "Flash"
            /// "FocalLength"
            /// "Maker Note"
            /// "User Comment"
            /// "DTSubsec"
            /// "DTOrigSS"
            /// "DTDigSS"
            /// "FPXVer"
            /// "ColorSpace"
            /// "PixXDim"
            /// "PixYDim"
            /// "RelatedWav"
            /// "Interop"
            /// "FlashEnergy"
            /// "SpatialFR"
            /// "FocalXRes"
            /// "FocalYRes"
            /// "FocalResUnit"
            /// "Subject Loc"
            /// "Exposure Index"
            /// "Sensing Method"
            /// "FileSource"
            /// "SceneType"
            /// "CfaPattern"
            /// "Gps Ver"
            /// "Gps LatitudeRef"
            /// "Gps Latitude"
            /// "Gps LongitudeRef"
            /// "Gps Longitude"
            /// "Gps AltitudeRef"
            /// "Gps Altitude"
            /// "Gps GpsTime"
            /// "Gps GpsSatellites"
            /// "Gps GpsStatus"
            /// "Gps GpsMeasureMode"
            /// "Gps GpsDop"
            /// "Gps SpeedRef"
            /// "Gps Speed"
            /// "Gps TrackRef"
            /// "Gps Track"
            /// "Gps ImgDirRef"
            /// "Gps ImgDir"
            /// "Gps MapDatum"
            /// "Gps DestLatRef"
            /// "Gps DestLat"
            /// "Gps DestLongRef"
            /// "Gps DestLong"
            /// "Gps DestBearRef"
            /// "Gps DestBear"
            /// "Gps DestDistRef"
            /// "Gps DestDist"
            /// </remarks>
            public object this[string index]
            {
                get
                {
                    return properties[index];
                }
            }
            //
            private System.Drawing.Bitmap bmp;
            //
            private string data;
            //
            private translation myHash;
            //
            //private Hashtable properties;
            private Dictionary<string, string> properties;
            //
            internal int Count
            {
                get
                {
                    return this.properties.Count;
                }
            }
            //
            string sp;
            
            //[Obsolete()]
            //public void setTag(int id, string data)
            //{
            //    Encoding ascii = Encoding.ASCII;
            //    this.setTag(id, data.Length, 0x2, ascii.GetBytes(data));
            //}
            
            //[Obsolete()]
            //public void setTag(int id, int len, short type, byte[] data)
            //{
            //    PropertyItem p = CreatePropertyItem(type, id, len, data);
            //    this.bmp.SetPropertyItem(p);
            //    buildDB(this.bmp.PropertyItems);
            //}
            
            //[Obsolete()]
            //private static PropertyItem CreatePropertyItem(short type, int tag, int len, byte[] value)
            //{
            //    PropertyItem item;

            //    // Loads a PropertyItem from a Jpeg image stored in the assembly as a resource.
            //    Assembly assembly = Assembly.GetExecutingAssembly();
            //    Stream emptyBitmapStream = assembly.GetManifestResourceStream("EXIFextractor.decoy.jpg");
            //    System.Drawing.Image empty = System.Drawing.Image.FromStream(emptyBitmapStream);

            //    item = empty.PropertyItems[0];

            //    // Copies the data to the property item.
            //    item.Type = type;
            //    item.Len = len;
            //    item.Id = tag;
            //    item.Value = new byte[value.Length];
            //    value.CopyTo(item.Value, 0);

            //    return item;
            //}

            /// <summary>
            /// 
            /// </summary>
            /// <param name="bmp"></param>
            /// <param name="sp"></param>
            public EXIFextractor(ref System.Drawing.Bitmap bmp, string sp)
            {
                properties = new Dictionary<string,string>();
                //
                this.bmp = bmp;
                this.sp = sp;
                //
                myHash = new translation();
                buildDB(this.bmp.PropertyItems);
            }
            string msp = "";
            public EXIFextractor(ref System.Drawing.Bitmap bmp, string sp, string msp)
            {
                properties = new Dictionary<string, string>();
                this.sp = sp;
                this.msp = msp;
                this.bmp = bmp;
                //				
                myHash = new translation();
                this.buildDB(bmp.PropertyItems);

            }
            /// <summary>
            /// Fixes locked image
            /// </summary>            
            public static PropertyItem[] GetExifProperties(string fileName)
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream,
                        /* useEmbeddedColorManagement = */ true,
                        /* validateImageData = */ false))
                    {
                        return image.PropertyItems;
                    }
                }
            }
            /// <summary>
            /// Fixes locked image
            /// </summary>
            public static PropertyItem[] GetExifProperties(string fileName, out long size)
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    size = stream.Length;

                    using (System.Drawing.Image image = System.Drawing.Image.FromStream(stream,
                        /* useEmbeddedColorManagement = */ true,
                        /* validateImageData = */ false))
                    {
                        return image.PropertyItems;
                    }
                }
                return null;
            }
            public EXIFextractor(string file, string sp, string msp)
            {
                properties = new Dictionary<string,string>();
                this.sp = sp;
                this.msp = msp;

                myHash = new translation();
                //				
                this.buildDB(GetExifProperties(file));

            }

            /// <summary>
            /// 
            /// </summary>
            private void buildDB(System.Drawing.Imaging.PropertyItem[] parr)
            {
                properties.Clear();
                //
                data = "";
                //
                Encoding ascii = Encoding.ASCII;
                //
                foreach (System.Drawing.Imaging.PropertyItem p in parr)
                {
                    string v = "";
                    string name = (string)myHash[p.Id];
                    // tag not found. skip it
                    if (name == null) continue;
                    //
                    data += name + ": ";
                    //
                    //1 = BYTE An 8-bit unsigned integer.,
                    if (p.Type == 0x1)
                    {
                        v = p.Value[0].ToString();
                    }
                    //2 = ASCII An 8-bit byte containing one 7-bit ASCII code. The final byte is terminated with NULL.,
                    else if (p.Type == 0x2)
                    {
                        // string					
                        v = ascii.GetString(p.Value);
                    }
                    //3 = SHORT A 16-bit (2 -byte) unsigned integer,
                    else if (p.Type == 0x3)
                    {
                        // orientation // lookup table					
                        switch (p.Id)
                        {
                            case 0x8827: // ISO
                                v = "ISO-" + convertToInt16U(p.Value).ToString();
                                break;
                            case 0xA217: // sensing method
                                {
                                    switch (convertToInt16U(p.Value))
                                    {
                                        case 1: v = "Not defined"; break;
                                        case 2: v = "One-chip color area sensor"; break;
                                        case 3: v = "Two-chip color area sensor"; break;
                                        case 4: v = "Three-chip color area sensor"; break;
                                        case 5: v = "Color sequential area sensor"; break;
                                        case 7: v = "Trilinear sensor"; break;
                                        case 8: v = "Color sequential linear sensor"; break;
                                        default: v = " reserved"; break;
                                    }
                                }
                                break;
                            case 0x8822: // aperture 
                                switch (convertToInt16U(p.Value))
                                {
                                    case 0: v = "Not defined"; break;
                                    case 1: v = "Manual"; break;
                                    case 2: v = "Normal program"; break;
                                    case 3: v = "Aperture priority"; break;
                                    case 4: v = "Shutter priority"; break;
                                    case 5: v = "Creative program (biased toward depth of field)"; break;
                                    case 6: v = "Action program (biased toward fast shutter speed)"; break;
                                    case 7: v = "Portrait mode (for closeup photos with the background out of focus)"; break;
                                    case 8: v = "Landscape mode (for landscape photos with the background in focus)"; break;
                                    default: v = "reserved"; break;
                                }
                                break;
                            case 0x9207: // metering mode
                                switch (convertToInt16U(p.Value))
                                {
                                    case 0: v = "unknown"; break;
                                    case 1: v = "Average"; break;
                                    case 2: v = "CenterWeightedAverage"; break;
                                    case 3: v = "Spot"; break;
                                    case 4: v = "MultiSpot"; break;
                                    case 5: v = "Pattern"; break;
                                    case 6: v = "Partial"; break;
                                    case 255: v = "Other"; break;
                                    default: v = "reserved"; break;
                                }
                                break;
                            case 0x9208: // light source
                                {
                                    switch (convertToInt16U(p.Value))
                                    {
                                        case 0: v = "unknown"; break;
                                        case 1: v = "Daylight"; break;
                                        case 2: v = "Fluorescent"; break;
                                        case 3: v = "Tungsten"; break;
                                        case 17: v = "Standard light A"; break;
                                        case 18: v = "Standard light B"; break;
                                        case 19: v = "Standard light C"; break;
                                        case 20: v = "D55"; break;
                                        case 21: v = "D65"; break;
                                        case 22: v = "D75"; break;
                                        case 255: v = "other"; break;
                                        default: v = "reserved"; break;
                                    }
                                }
                                break;
                            case 0x9209:
                                {
                                    switch (convertToInt16U(p.Value))
                                    {
                                        case 0: v = "Flash did not fire"; break;
                                        case 1: v = "Flash fired"; break;
                                        case 5: v = "Strobe return light not detected"; break;
                                        case 7: v = "Strobe return light detected"; break;
                                        default: v = "reserved"; break;
                                    }
                                }
                                break;
                            default:
                                v = convertToInt16U(p.Value).ToString();
                                break;
                        }
                    }
                    //4 = LONG A 32-bit (4 -byte) unsigned integer,
                    else if (p.Type == 0x4)
                    {
                        // orientation // lookup table					
                        v = convertToInt32U(p.Value).ToString();
                    }
                    //5 = RATIONAL Two LONGs. The first LONG is the numerator and the second LONG expresses the//denominator.,
                     else if( p.Type == 0x5 )
                    { // modifications by http://www.codeproject.com/script/Membership/Profiles.aspx?mid=2045089
                    //My code starts here
                    if (p.Id == 0x0004)
                    {
                    //lat
                    // rational
                    byte[] deg = new byte[4];
                    byte[] degdiv = new byte[4];
                    byte[] min = new byte[4];
                    byte[] mindiv = new byte[4];
                    byte[] sec = new byte[4];
                    byte[] secdiv = new byte[4];
                    Array.Copy(p.Value, 0, deg, 0, 4);
                    Array.Copy(p.Value, 4, degdiv, 0, 4);
                    Array.Copy(p.Value, 8, min, 0, 4);
                    Array.Copy(p.Value, 12, mindiv, 0, 4);
                    Array.Copy(p.Value, 16, sec, 0, 4);
                    Array.Copy(p.Value, 20, secdiv, 0, 4);
                    double a = convertToInt32U(deg);
                    double b = convertToInt32U(degdiv);
                    double c = convertToInt32U(min);
                    double d = convertToInt32U(mindiv);
                    double e = convertToInt32U(sec);
                    double f = convertToInt32U(secdiv);
                    double o =((a/b) + ((c/b)/60) + ((e/f)/3600));
                    v = o.ToString();
                    // Rational r = new Rational(a, b);
                    }
                    else if (p.Id == 0x0002)
                    {
                        //long
                        byte[] deg = new byte[4];
                        byte[] degdiv = new byte[4];
                        byte[] min = new byte[4];
                        byte[] mindiv = new byte[4];
                        byte[] sec = new byte[4];
                        byte[] secdiv = new byte[4];
                        Array.Copy(p.Value, 0, deg, 0, 4);
                        Array.Copy(p.Value, 4, degdiv, 0, 4);
                        Array.Copy(p.Value, 8, min, 0, 4);
                        Array.Copy(p.Value, 12, mindiv, 0, 4);
                        Array.Copy(p.Value, 16, sec, 0, 4);
                        Array.Copy(p.Value, 20, secdiv, 0, 4);
                        double a = convertToInt32U(deg);
                        double b = convertToInt32U(degdiv);
                        double c = convertToInt32U(min);
                        double d = convertToInt32U(mindiv);
                        double e = convertToInt32U(sec);
                        double f = convertToInt32U(secdiv);
                        double o = ((a / b) + ((c / b) / 60) + ((e / f) / 3600));
                        v = o.ToString();
                    }
                    //My Code ends here
                    else
                    {
                        // rational
                        byte[] n = new byte[p.Len / 2];
                        byte[] d = new byte[p.Len / 2];
                        Array.Copy(p.Value, 0, n, 0, p.Len / 2);
                        Array.Copy(p.Value, p.Len / 2, d, 0, p.Len / 2);
                        uint a = convertToInt32U(n);
                        uint b = convertToInt32U(d);
                        Rational r = new Rational(a, b);
                        //
                        //convert here
                        //
                        switch (p.Id)
                        {
                            case 0x9202: // aperture
                                v = "F/" + Math.Round(Math.Pow(Math.Sqrt(2), r.ToDouble()), 2).ToString();
                                break;
                            case 0x920A:
                                v = r.ToDouble().ToString();
                                break;
                            case 0x829A:
                                v = r.ToDouble().ToString();
                                break;
                            case 0x829D: // F-number
                                v = "F/" + r.ToDouble().ToString();
                                break;
                            default:
                                v = r.ToString("/");
                                break;
                        }
                    }
                    }
                    //7 = UNDEFINED An 8-bit byte that can take any value depending on the field definition,
                    else if (p.Type == 0x7)
                    {
                        switch (p.Id)
                        {
                            case 0xA300:
                                {
                                    if (p.Value[0] == 3)
                                    {
                                        v = "DSC";
                                    }
                                    else
                                    {
                                        v = "reserved";
                                    }
                                    break;
                                }
                            case 0xA301:
                                if (p.Value[0] == 1)
                                    v = "A directly photographed image";
                                else
                                    v = "Not a directly photographed image";
                                break;
                            default:
                                v = "-";
                                break;
                        }
                    }
                    //9 = SLONG A 32-bit (4 -byte) signed integer (2's complement notation),
                    else if (p.Type == 0x9)
                    {
                        v = convertToInt32(p.Value).ToString();
                    }
                    //10 = SRATIONAL Two SLONGs. The first SLONG is the numerator and the second SLONG is the
                    //denominator.
                    else if (p.Type == 0xA)
                    {

                        // rational
                        byte[] n = new byte[p.Len / 2];
                        byte[] d = new byte[p.Len / 2];
                        Array.Copy(p.Value, 0, n, 0, p.Len / 2);
                        Array.Copy(p.Value, p.Len / 2, d, 0, p.Len / 2);
                        int a = convertToInt32(n);
                        int b = convertToInt32(d);
                        Rational r = new Rational(a, b);
                        //
                        // convert here
                        //
                        switch (p.Id)
                        {
                            case 0x9201: // shutter speed
                                v = "1/" + Math.Round(Math.Pow(2, r.ToDouble()), 2).ToString();
                                break;
                            case 0x9203:
                                v = Math.Round(r.ToDouble(), 4).ToString();
                                break;
                            default:
                                v = r.ToString("/");
                                break;
                        }
                    }
                    // add it to the list
                    if (!properties.ContainsKey(name))
                        properties.Add(name, v);
                    // cat it too
                    data += v;
                    data += this.sp;
                }

            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return data;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="arr"></param>
            /// <returns></returns>
            int convertToInt32(byte[] arr)
            {
                if (arr.Length != 4)
                    return 0;
                else
                    return arr[3] << 24 | arr[2] << 16 | arr[1] << 8 | arr[0];
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="arr"></param>
            /// <returns></returns>
            int convertToInt16(byte[] arr)
            {
                if (arr.Length != 2)
                    return 0;
                else
                    return arr[1] << 8 | arr[0];
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="arr"></param>
            /// <returns></returns>
            uint convertToInt32U(byte[] arr)
            {
                if (arr.Length != 4)
                    return 0;
                else
                    return Convert.ToUInt32(arr[3] << 24 | arr[2] << 16 | arr[1] << 8 | arr[0]);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="arr"></param>
            /// <returns></returns>
            uint convertToInt16U(byte[] arr)
            {
                if (arr.Length != 2)
                    return 0;
                else
                    return Convert.ToUInt16(arr[1] << 8 | arr[0]);
            }
            #region IEnumerable Members

            public IEnumerator GetEnumerator()
            {
                // TODO:  Add EXIFextractor.GetEnumerator implementation
                return (new EXIFextractorEnumerator(this.properties));
            }

            #endregion
        }

        //
        // dont touch this class. its for IEnumerator
        // 
        //
        class EXIFextractorEnumerator : IEnumerator
        {
            Dictionary<string,string> exifTable;
            IDictionaryEnumerator index;

            internal EXIFextractorEnumerator(Dictionary<string,string> exif)
            {
                this.exifTable = exif;
                this.Reset();
                index = exif.GetEnumerator();
            }

            #region IEnumerator Members

            public void Reset()
            {
                this.index = null;
            }

            public object Current
            {
                get
                {
                    return (new DictionaryEntry(this.index.Key, this.index.Value));
                }
            }

            public bool MoveNext()
            {
                if (index != null && index.MoveNext())
                    return true;
                else
                    return false;
            }

            #endregion

        }

        /// <summary>
        /// Summary description for translation.
        /// </summary>
        public class translation : Hashtable
        {
            /// <summary>
            /// 
            /// </summary>
            public translation()
            {
                this.Add(0x8769, "Exif IFD");
                this.Add(0x8825, "Gps IFD");
                this.Add(0xFE, "New Subfile Type");
                this.Add(0xFF, "Subfile Type");
                this.Add(0x100, "Image Width");
                this.Add(0x101, "Image Height");
                this.Add(0x102, "Bits Per Sample");
                this.Add(0x103, "Compression");
                this.Add(0x106, "Photometric Interp");
                this.Add(0x107, "Thresh Holding");
                this.Add(0x108, "Cell Width");
                this.Add(0x109, "Cell Height");
                this.Add(0x10A, "Fill Order");
                this.Add(0x10D, "Document Name");
                this.Add(0x10E, "Image Description");
                this.Add(0x10F, "Equip Make");
                this.Add(0x110, "Equip Model");
                this.Add(0x111, "Strip Offsets");
                this.Add(0x112, "Orientation");
                this.Add(0x115, "Samples PerPixel");
                this.Add(0x116, "Rows Per Strip");
                this.Add(0x117, "Strip Bytes Count");
                this.Add(0x118, "Min Sample Value");
                this.Add(0x119, "Max Sample Value");
                this.Add(0x11A, "X Resolution");
                this.Add(0x11B, "Y Resolution");
                this.Add(0x11C, "Planar Config");
                this.Add(0x11D, "Page Name");
                this.Add(0x11E, "X Position");
                this.Add(0x11F, "Y Position");
                this.Add(0x120, "Free Offset");
                this.Add(0x121, "Free Byte Counts");
                this.Add(0x122, "Gray Response Unit");
                this.Add(0x123, "Gray Response Curve");
                this.Add(0x124, "T4 Option");
                this.Add(0x125, "T6 Option");
                this.Add(0x128, "Resolution Unit");
                this.Add(0x129, "Page Number");
                this.Add(0x12D, "Transfer Funcition");
                this.Add(0x131, "Software Used");
                this.Add(0x132, "Date Time");
                this.Add(0x13B, "Artist");
                this.Add(0x13C, "Host Computer");
                this.Add(0x13D, "Predictor");
                this.Add(0x13E, "White Point");
                this.Add(0x13F, "Primary Chromaticities");
                this.Add(0x140, "ColorMap");
                this.Add(0x141, "Halftone Hints");
                this.Add(0x142, "Tile Width");
                this.Add(0x143, "Tile Length");
                this.Add(0x144, "Tile Offset");
                this.Add(0x145, "Tile ByteCounts");
                this.Add(0x14C, "InkSet");
                this.Add(0x14D, "Ink Names");
                this.Add(0x14E, "Number Of Inks");
                this.Add(0x150, "Dot Range");
                this.Add(0x151, "Target Printer");
                this.Add(0x152, "Extra Samples");
                this.Add(0x153, "Sample Format");
                this.Add(0x154, "S Min Sample Value");
                this.Add(0x155, "S Max Sample Value");
                this.Add(0x156, "Transfer Range");
                this.Add(0x200, "JPEG Proc");
                this.Add(0x201, "JPEG InterFormat");
                this.Add(0x202, "JPEG InterLength");
                this.Add(0x203, "JPEG RestartInterval");
                this.Add(0x205, "JPEG LosslessPredictors");
                this.Add(0x206, "JPEG PointTransforms");
                this.Add(0x207, "JPEG QTables");
                this.Add(0x208, "JPEG DCTables");
                this.Add(0x209, "JPEG ACTables");
                this.Add(0x211, "YCbCr Coefficients");
                this.Add(0x212, "YCbCr Subsampling");
                this.Add(0x213, "YCbCr Positioning");
                this.Add(0x214, "REF Black White");
                this.Add(0x8773, "ICC Profile");
                this.Add(0x301, "Gamma");
                this.Add(0x302, "ICC Profile Descriptor");
                this.Add(0x303, "SRGB RenderingIntent");
                this.Add(0x320, "Image Title");
                this.Add(0x8298, "Copyright");
                this.Add(0x5001, "Resolution X Unit");
                this.Add(0x5002, "Resolution Y Unit");
                this.Add(0x5003, "Resolution X LengthUnit");
                this.Add(0x5004, "Resolution Y LengthUnit");
                this.Add(0x5005, "Print Flags");
                this.Add(0x5006, "Print Flags Version");
                this.Add(0x5007, "Print Flags Crop");
                this.Add(0x5008, "Print Flags Bleed Width");
                this.Add(0x5009, "Print Flags Bleed Width Scale");
                this.Add(0x500A, "Halftone LPI");
                this.Add(0x500B, "Halftone LPIUnit");
                this.Add(0x500C, "Halftone Degree");
                this.Add(0x500D, "Halftone Shape");
                this.Add(0x500E, "Halftone Misc");
                this.Add(0x500F, "Halftone Screen");
                this.Add(0x5010, "JPEG Quality");
                this.Add(0x5011, "Grid Size");
                this.Add(0x5012, "Thumbnail Format");
                this.Add(0x5013, "Thumbnail Width");
                this.Add(0x5014, "Thumbnail Height");
                this.Add(0x5015, "Thumbnail ColorDepth");
                this.Add(0x5016, "Thumbnail Planes");
                this.Add(0x5017, "Thumbnail RawBytes");
                this.Add(0x5018, "Thumbnail Size");
                this.Add(0x5019, "Thumbnail CompressedSize");
                this.Add(0x501A, "Color Transfer Function");
                this.Add(0x501B, "Thumbnail Data");
                this.Add(0x5020, "Thumbnail ImageWidth");
                this.Add(0x502, "Thumbnail ImageHeight");
                this.Add(0x5022, "Thumbnail BitsPerSample");
                this.Add(0x5023, "Thumbnail Compression");
                this.Add(0x5024, "Thumbnail PhotometricInterp");
                this.Add(0x5025, "Thumbnail ImageDescription");
                this.Add(0x5026, "Thumbnail EquipMake");
                this.Add(0x5027, "Thumbnail EquipModel");
                this.Add(0x5028, "Thumbnail StripOffsets");
                this.Add(0x5029, "Thumbnail Orientation");
                this.Add(0x502A, "Thumbnail SamplesPerPixel");
                this.Add(0x502B, "Thumbnail RowsPerStrip");
                this.Add(0x502C, "Thumbnail StripBytesCount");
                this.Add(0x502D, "Thumbnail ResolutionX");
                this.Add(0x502E, "Thumbnail ResolutionY");
                this.Add(0x502F, "Thumbnail PlanarConfig");
                this.Add(0x5030, "Thumbnail ResolutionUnit");
                this.Add(0x5031, "Thumbnail TransferFunction");
                this.Add(0x5032, "Thumbnail SoftwareUsed");
                this.Add(0x5033, "Thumbnail DateTime");
                this.Add(0x5034, "Thumbnail Artist");
                this.Add(0x5035, "Thumbnail WhitePoint");
                this.Add(0x5036, "Thumbnail PrimaryChromaticities");
                this.Add(0x5037, "Thumbnail YCbCrCoefficients");
                this.Add(0x5038, "Thumbnail YCbCrSubsampling");
                this.Add(0x5039, "Thumbnail YCbCrPositioning");
                this.Add(0x503A, "Thumbnail RefBlackWhite");
                this.Add(0x503B, "Thumbnail CopyRight");
                this.Add(0x5090, "Luminance Table");
                this.Add(0x5091, "Chrominance Table");
                this.Add(0x5100, "Frame Delay");
                this.Add(0x5101, "Loop Count");
                this.Add(0x5110, "Pixel Unit");
                this.Add(0x5111, "Pixel PerUnit X");
                this.Add(0x5112, "Pixel PerUnit Y");
                this.Add(0x5113, "Palette Histogram");
                this.Add(0x829A, "Exposure Time");
                this.Add(0x829D, "F-Number");
                this.Add(0x8822, "Exposure Prog");
                this.Add(0x8824, "Spectral Sense");
                this.Add(0x8827, "ISO Speed");
                this.Add(0x8828, "OECF");
                this.Add(0x9000, "Ver");
                this.Add(0x9003, "DTOrig");
                this.Add(0x9004, "DTDigitized");
                this.Add(0x9101, "CompConfig");
                this.Add(0x9102, "CompBPP");
                this.Add(0x9201, "Shutter Speed");
                this.Add(0x9202, "Aperture");
                this.Add(0x9203, "Brightness");
                this.Add(0x9204, "Exposure Bias");
                this.Add(0x9205, "MaxAperture");
                this.Add(0x9206, "SubjectDist");
                this.Add(0x9207, "Metering Mode");
                this.Add(0x9208, "LightSource");
                this.Add(0x9209, "Flash");
                this.Add(0x920A, "FocalLength");
                this.Add(0x927C, "Maker Note");
                this.Add(0x9286, "User Comment");
                this.Add(0x9290, "DTSubsec");
                this.Add(0x9291, "DTOrigSS");
                this.Add(0x9292, "DTDigSS");
                this.Add(0xA000, "FPXVer");
                this.Add(0xA001, "ColorSpace");
                this.Add(0xA002, "PixXDim");
                this.Add(0xA003, "PixYDim");
                this.Add(0xA004, "RelatedWav");
                this.Add(0xA005, "Interop");
                this.Add(0xA20B, "FlashEnergy");
                this.Add(0xA20C, "SpatialFR");
                this.Add(0xA20E, "FocalXRes");
                this.Add(0xA20F, "FocalYRes");
                this.Add(0xA210, "FocalResUnit");
                this.Add(0xA214, "Subject Loc");
                this.Add(0xA215, "Exposure Index");
                this.Add(0xA217, "Sensing Method");
                this.Add(0xA300, "FileSource");
                this.Add(0xA301, "SceneType");
                this.Add(0xA302, "CfaPattern");
                this.Add(0x0, "Gps Ver");
                this.Add(0x1, "Gps LatitudeRef");
                this.Add(0x2, "Gps Latitude");
                this.Add(0x3, "Gps LongitudeRef");
                this.Add(0x4, "Gps Longitude");
                this.Add(0x5, "Gps AltitudeRef");
                this.Add(0x6, "Gps Altitude");
                this.Add(0x7, "Gps GpsTime");
                this.Add(0x8, "Gps GpsSatellites");
                this.Add(0x9, "Gps GpsStatus");
                this.Add(0xA, "Gps GpsMeasureMode");
                this.Add(0xB, "Gps GpsDop");
                this.Add(0xC, "Gps SpeedRef");
                this.Add(0xD, "Gps Speed");
                this.Add(0xE, "Gps TrackRef");
                this.Add(0xF, "Gps Track");
                this.Add(0x10, "Gps ImgDirRef");
                this.Add(0x11, "Gps ImgDir");
                this.Add(0x12, "Gps MapDatum");
                this.Add(0x13, "Gps DestLatRef");
                this.Add(0x14, "Gps DestLat");
                this.Add(0x15, "Gps DestLongRef");
                this.Add(0x16, "Gps DestLong");
                this.Add(0x17, "Gps DestBearRef");
                this.Add(0x18, "Gps DestBear");
                this.Add(0x19, "Gps DestDistRef");
                this.Add(0x1A, "Gps DestDist");
            }
        }
        /// <summary>
        /// private class
        /// </summary>
        internal class Rational
        {
            private int n;
            private int d;
            public Rational(int n, int d)
            {
                this.n = n;
                this.d = d;
                simplify(ref this.n, ref this.d);
            }
            public Rational(uint n, uint d)
            {
                this.n = Convert.ToInt32(n);
                this.d = Convert.ToInt32(d);

                simplify(ref this.n, ref this.d);
            }
            public Rational()
            {
                this.n = this.d = 0;
            }
            public string ToString(string sp)
            {
                if (sp == null) sp = "/";
                return n.ToString() + sp + d.ToString();
            }
            public double ToDouble()
            {
                if (d == 0)
                    return 0.0;

                return Math.Round(Convert.ToDouble(n) / Convert.ToDouble(d), 2);
            }
            private void simplify(ref int a, ref int b)
            {
                if (a == 0 || b == 0)
                    return;

                int gcd = euclid(a, b);
                a /= gcd;
                b /= gcd;
            }
            private int euclid(int a, int b)
            {
                if (b == 0)
                    return a;
                else
                    return euclid(b, a % b);
            }
        }

    }
}