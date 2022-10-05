using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace eulynx_validator.Validator
{
    internal class ValidatorSchema
    {
        private string _inputUri; // eulynx XML File
        private string _reportPath;
        private string[] _xsdFilePath;
        private string[] _targetNamespace;
        private static List<string> _xmlSchemaReport;
        private int _status = 0; // success state -> 0: no error and warning, 1: error/s and warning/s
        public int Status
        { get { return (_xmlSchemaReport.Count) == 0 ? 0 : 1; } }

        public ValidatorSchema(string inputUri, string reportPath)
        {
            _inputUri = inputUri;
            _reportPath = reportPath;
            _xsdFilePath = new string[]
            {
                // new DP release

                //Properties.Resources.DB,
                //Properties.Resources.Generic,
                //Properties.Resources.NR,
                //Properties.Resources.ProRail,
                //Properties.Resources.RFI,
                //Properties.Resources.RsmCommon,
                //Properties.Resources.RsmNetEntity,
                //Properties.Resources.RsmSignalling,
                //Properties.Resources.RsmTrack,
                //Properties.Resources.Signalling,
                //Properties.Resources.SNCF,
                //Properties.Resources.TRV,

                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/DB.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/Generic.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/NR.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/ProRail.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RFI.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RsmCommon.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RsmNetEntity.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RsmSignalling.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RsmTrack.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/Signalling.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/SNCF.xsd",
                Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/TRV.xsd"

                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/DB.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/Generic.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/NR.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/ProRail.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RFI.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RsmCommon.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RsmNetEntity.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RsmSignalling.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/RsmTrack.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/Signalling.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/SNCF.xsd",
                //Environment.CurrentDirectory + "../../../Validator/EULYNX_DP_V1.0_Schema/TRV.xsd"

                // old DP
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\Eulynx_Schema\\Signalling.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\Eulynx_Schema\\Generic.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\Eulynx_Schema\\DB.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\Eulynx_Schema\\NR.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\Eulynx_Schema\\ProRail.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\Eulynx_Schema\\RFI.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\Eulynx_Schema\\SNCF.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\Eulynx_Schema\\TRV.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\RSM_Schema\\Common.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\RSM_Schema\\NetEntity.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\RSM_Schema\\Signalling.xsd",
                //Environment.CurrentDirectory + "..\\..\\..\\Validator\\EulynxSchemaOld\\RSM_Schema\\Track.xsd",
            };
            _targetNamespace = new string[]
            {
                 //new DP relase
                "http://dataprep.eulynx.eu/schema/DB/1.0",
                "http://dataprep.eulynx.eu/schema/Generic/1.0",
                "http://dataprep.eulynx.eu/schema/NR/1.0",
                "http://dataprep.eulynx.eu/schema/ProRail/1.0",
                "http://dataprep.eulynx.eu/schema/RFI/1.0",
                "http://www.railsystemmodel.org/schemas/Common/1.2",
                "http://www.railsystemmodel.org/schemas/NetEntity/1.2",
                "http://www.railsystemmodel.org/schemas/Signalling/1.2",
                "http://www.railsystemmodel.org/schemas/Track/1.2",
                "http://dataprep.eulynx.eu/schema/Signalling/1.0",
                "http://dataprep.eulynx.eu/schema/SNCF/1.0",
                "http://dataprep.eulynx.eu/schema/TRV/1.0"

                // old DP
                //"http://dataprep.eulynx.eu/schema/DB",
                //"http://dataprep.eulynx.eu/schema/Generic",
                //"http://www.railsystemmodel.org/schemas/RSM1.2beta/Common",

                //"http://dataprep.eulynx.eu/schema/Signalling",
                //"http://dataprep.eulynx.eu/schema/NR",
                //"http://dataprep.eulynx.eu/schema/ProRail",
                //"http://dataprep.eulynx.eu/schema/RFI",
                //"http://dataprep.eulynx.eu/schema/SNCF",
                //"http://dataprep.eulynx.eu/schema/TRV",

                //"http://www.railsystemmodel.org/schemas/RSM1.2beta/NetEntity",
                //"http://www.railsystemmodel.org/schemas/RSM1.2beta/Signalling",
                //"http://www.railsystemmodel.org/schemas/RSM1.2beta/Track",

            };
            _xmlSchemaReport = new List<string>();
        }

        public void Validate()
        {
            // Set the XMLReaderSettings
            // - Schema type (XSD)
            // - Generate XmlSchemaSet and add them to settings
            // - Add event handler (catch the error if validation is not match)
            XmlReaderSettings settings = new XmlReaderSettings();
            
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            //adding schemas that will be used for validation.
            settings.Schemas.Add(GenerateSchemaSet(_targetNamespace, _xsdFilePath));

            // the validation handling will result in output. this is handled by the callback function"Validation Callback".

            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallback);

            // Create the XmlReader object
            // this is a reader and we adjusted some settings for validation.
            XmlReader xmlReader = XmlReader.Create(new StringReader(File.ReadAllText(_inputUri)), settings);

            // Parse the file
            while (xmlReader.Read()) { }

            // Shows that validation is completed
            // Console.WriteLine("Schema Validation completed");

            // Close xmlReader object
            xmlReader.Close();   
        }

        public void MakeReport()
        {
            // Set a variable to the Documents path.
            string docPath = Environment.CurrentDirectory;

            // Write the string array to a new file named "logs.txt".
            //DirectoryInfo di = Directory.CreateDirectory(Environment.CurrentDirectory + "../../../Report");
            using (StreamWriter outputFile = new StreamWriter(_reportPath + "/XML Schema Report.txt"))  
                //../../../Report,        Path.Combine(docPath, Environment.CurrentDirectory + this._reportPath
            {
                foreach (string line in _xmlSchemaReport)
                    outputFile.WriteLine(line);
                if (_xmlSchemaReport.Count==0)
                {
                    outputFile.WriteLine("Validation is Successful");
                }
            }
        }
        /// <summary>
        /// generating a schema set for validation of XSD files.
        /// </summary>
        /// <param name="tNs"></param>
        /// <param name="schemaPaths"></param>
        /// <returns></returns>
        private XmlSchemaSet GenerateSchemaSet(string[] tNs, string[] schemaPaths)
        {
            // Create the XmlSchemaSet class
            XmlSchemaSet schemaSet = new XmlSchemaSet();

            // Add the schemas to the collection
            for (int i = 0; i < tNs.Length; i++)
            {
                schemaSet.Add(tNs[i], schemaPaths[i]);
            }

            return schemaSet;
        }
        /// <summary>
        /// function to concatinate the result of the validation to XSD files.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void ValidationCallback(object sender, ValidationEventArgs args)
        {
            // Console output
            string msg = String.Format(
                    args.Severity +
                    ": Line: {0}, Position {1}: \"{2}\" \n",
                    args.Exception.LineNumber,
                    args.Exception.LinePosition,
                    args.Exception.Message);
            // Console.WriteLine(msg);

            // Report File
            _xmlSchemaReport.Add(msg);
        }
    }
}
