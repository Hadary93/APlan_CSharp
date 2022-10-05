using Models.TopoModels.EULYNX.generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace aplan.eulynx
{
    /// <summary>
    /// Class used to produce utf-8 data.
    /// </summary>
    public class StringWriterUtf8 : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

    class EulynxSerializer
    {

        //instance
        private static EulynxSerializer eulynxSerializer;

        //singleton constructor
        private EulynxSerializer() { }

        //singleton method
        public static EulynxSerializer getInstance()
        {
            if (eulynxSerializer == null)
            {
                eulynxSerializer = new EulynxSerializer();
            }
            return eulynxSerializer;
        }


        /// <summary>
        /// References dictionary.
        /// </summary>
        private readonly Dictionary<string, string> xmlnsDict = new Dictionary<string, string>
        {
            {"xsi", @"http://www.w3.org/2001/XMLSchema-instance" },
            {"generic", @"http://dataprep.eulynx.eu/schema/Generic" },
            {"sig", @"http://dataprep.eulynx.eu/schema/Signalling"},
            {"db",@"http://dataprep.eulynx.eu/schema/DB" },
            {"sncf",@"http://dataprep.eulynx.eu/schema/SNCF" },
            {"nr",@"http://dataprep.eulynx.eu/schema/NR" },
            {"prorail",@"http://dataprep.eulynx.eu/schema/ProRail" },
            {"rfi",@"http://dataprep.eulynx.eu/schema/RFI" },
            {"trv",@"http://dataprep.eulynx.eu/schema/TRV" },

            //{"rsmCommon", @"http://www.railsystemmodel.org/schemas/RSM1.2beta/Common"},
            //{"rsmNE", @"http://www.railsystemmodel.org/schemas/RSM1.2beta/NetEntity"},
            //{"rsmTrack", @"http://www.railsystemmodel.org/schemas/RSM1.2beta/Track" },
            //{"rsmSig", @"http://www.railsystemmodel.org/schemas/RSM1.2beta/Signalling"}
            {"rsmCommon", @"http://www.railsystemmodel.org/schemas/Common/1.2"},
            {"rsmNE", @"http://www.railsystemmodel.org/schemas/NetEntity/1.2"},
            {"rsmTrack", @"http://www.railsystemmodel.org/schemas/Track/1.2" },
            {"rsmSig", @"http://www.railsystemmodel.org/schemas/Signalling/1.2"}
        };


        /// <summary>
        /// Serialize an object into an eulynx xml file.
        /// </summary>
        /// <param name="eulynx">Eulynx container</param>
        /// <param name="station">Name of the project station, for  file naming purpose</param>
        /// <param name="outputFolderPath">path to output folder</param>
        public void serialize(EulynxDataPrepInterface eulynx, string station, string outputFolderPath)
        {
            string outputPath = outputFolderPath + "\\" + "eulynx" + station + ".euxml";
            XmlWriterSettings writerSettings = new XmlWriterSettings { Indent = true};
            XmlSerializer serializer = new XmlSerializer(typeof(EulynxDataPrepInterface));
            XmlSerializerNamespaces xmlns = new XmlSerializerNamespaces();
            foreach (string prefix in xmlnsDict.Keys)
                xmlns.Add(prefix, xmlnsDict[prefix]);

            using (var stringWriter = new StringWriterUtf8())
            {
                using (XmlWriter writer = XmlWriter.Create(stringWriter, writerSettings))
                {
                    serializer.Serialize(writer, eulynx, xmlns);
                }
                var result = stringWriter.ToString();
                stringWriter.Flush();
                File.WriteAllText(path: outputPath, contents: result);
            }
            Console.WriteLine($"Output written to {Path.GetFullPath(outputPath)}");
        }

        /// <summary>
        /// Deserialize an XML file into an object of type T.
        /// </summary>
        /// <typeparam name="T">The data container class</typeparam>
        /// <param name="path">path to XML file, including filename and extension</param>
        /// <returns>Object of type T</returns>
        public T deserialize<T>(string path)
        {
            if (path == null) return default(T);

            try
            {
                var serialiser = new XmlSerializer(typeof(T));
                using (var streamreader = new StreamReader(path))
                {
                    return (T)serialiser.Deserialize(streamreader);
                }
            }
            catch (FileNotFoundException)
            {
                return default(T);
            }
            catch (ArgumentNullException)
            {
                return default(T);
            }
            catch (IOException)
            {
                return default(T);
            }
            catch (InvalidOperationException)
            {
                return default(T);
            }
        }

    }
}
