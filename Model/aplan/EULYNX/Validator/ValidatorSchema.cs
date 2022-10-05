using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace aplan.eulynx.validator
{
    internal class ValidatorSchema
    {
        private string inputUri; // eulynx XML File
        private string[] xsdFilePath;
        private string[] targetNamespace;
        private static List<string> xmlSchemaReport;
        private int status = 0; // success state -> 0: no error and warning, 1: error/s and warning/s
        public int Status
        { get { return (xmlSchemaReport.Count) == 0 ? 0 : 1; } }

        public ValidatorSchema(string inputUri)
        {
            this.inputUri = inputUri;
            xsdFilePath = new string[]
            {
                @"..\..\..\resources\EulynxSchema\DB.xsd",
                @"..\..\..\resources\EulynxSchema\Generic.xsd",
                @"..\..\..\resources\EulynxSchema\NR.xsd",
                @"..\..\..\resources\EulynxSchema\ProRail.xsd",
                @"..\..\..\resources\EulynxSchema\RFI.xsd",
                @"..\..\..\resources\EulynxSchema\RsmCommon.xsd",
                @"..\..\..\resources\EulynxSchema\RsmNetEntity.xsd",
                @"..\..\..\resources\EulynxSchema\RsmSignalling.xsd",
                @"..\..\..\resources\EulynxSchema\RsmTrack.xsd",
                @"..\..\..\resources\EulynxSchema\Signalling.xsd",
                @"..\..\..\resources\EulynxSchema\SNCF.xsd",
                @"..\..\..\resources\EulynxSchema\TRV.xsd"
            };
            targetNamespace = new string[]
            {
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
            };

            xmlSchemaReport = new List<string>();
        }

        public void validate()
        {
            // Set the XMLReaderSettings
            // - Schema type (XSD)
            // - Generate XmlSchemaSet and add them to settings
            // - Add event handler (catch the error if validation is not match)
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.Schemas.Add(generateSchemaSet(targetNamespace, xsdFilePath));
            settings.ValidationEventHandler += new ValidationEventHandler(validationCallback);

            // Create the XmlReader object
            XmlReader xmlReader = XmlReader.Create(new StringReader(File.ReadAllText(inputUri)), settings);

            // Parse the file
            while (xmlReader.Read()) { }

            // Shows that validation is completed
            // Console.WriteLine("Schema Validation completed");

            // Close xmlReader object
            xmlReader.Close();   
        }

        public void makeReport()
        {
            // Set a variable to the Documents path.
            string docPath = Environment.CurrentDirectory;

            // Write the string array to a new file named "logs.txt".
            DirectoryInfo di = Directory.CreateDirectory(@"..\..\..\report");
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, @"..\..\..\report\XML Schema Report.txt")))
            {
                foreach (string line in xmlSchemaReport)
                    outputFile.WriteLine(line);
            }

        }

        private XmlSchemaSet generateSchemaSet(string[] tNs, string[] schemaPaths)
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

        private static void validationCallback(object sender, ValidationEventArgs args)
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
            xmlSchemaReport.Add(msg);
        }
    }
}
