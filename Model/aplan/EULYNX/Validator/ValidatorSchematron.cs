using Saxon.Api;
using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace aplan.eulynx.validator
{
    internal class ValidatorSchematron
    {
        private string inputUri; // eulynx XML File
        private string schematronFilePath;
        private string schxsltIncludeStylesheet;
        private string schxsltCompileStylesheet;
        private string schematronReport;
        private int status = 0; // success state -> 0: no error and warning, 1: error/s and warning/s
        public int Status
        { get { return status; } }

        private Processor processor;
        private XsltCompiler xsltCompiler;
        private XsltExecutable xsltExecutable;
        private XsltTransformer xsltTransformer;
        private XdmDestination xdmDestination;

        public ValidatorSchematron(string inputUri) 
        { 
            this.inputUri = inputUri;
            schematronFilePath = @"..\..\..\resources\Schematron\mainSignal.sch";
            schxsltIncludeStylesheet = @"..\..\..\resources\Schematron\schxslt-1.8.6\2.0\include.xsl";
            schxsltCompileStylesheet = @"..\..\..\resources\Schematron\schxslt-1.8.6\2.0\compile-for-svrl.xsl";
            processor = new Processor(false);
            xsltCompiler = processor.NewXsltCompiler();
        }

        public void validate()
        {
            /* there is 2 other compilation if the schematron using include and abstract function */

            //  xslt -stylesheet iso_dsdl_include.xsl  theSchema.sch > theSchema1.sch
            XdmDestination schemaXdm = saxonXsltTransform(schxsltCompileStylesheet, schematronFilePath);

            //  xslt -stylesheet theSchema.xsl  myDocument.xml > myResult.xml
            schemaXdm = saxonXsltTransform(schemaXdm.XdmNode, inputUri);

            // This XdmNode can be trim first to create cleaner code. But schematron report is svrl by default
            schematronReport = schemaXdm.XdmNode.ToString();

            // set _status
            XdmNode rootnode = schemaXdm.XdmNode.Children().First();

            foreach (XdmNode node in rootnode.Children())
            {
                if (node.NodeName.LocalName == "successful-report" || node.NodeName.LocalName == "failed-assert" && status != 0)
                {
                    // show all report and assert schematron (console)
                    // Console.WriteLine(node.NodeName.LocalName + ": " +node.Children().First().StringValue);

                    status = 1;
                }
            }
        }

        public void makeReport()
        {
            // Set a variable to the Documents path.
            string docPath = Environment.CurrentDirectory;

            // Write the string array to a new file named "logs.txt".
            DirectoryInfo di = Directory.CreateDirectory(@"..\..\..\report");
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, @"..\..\..\report\SchematronReport.svrl")))
            {
                outputFile.WriteLine(schematronReport);
            }
        }


        private XdmDestination saxonXsltTransform(XdmNode xdmStyleSheet, string inputPath)
        {
            FileStream fileStream = new FileStream(inputPath, FileMode.Open);

            xsltExecutable = xsltCompiler.Compile(xdmStyleSheet);
            xsltTransformer = xsltExecutable.Load();

            xsltTransformer.SetInputStream(fileStream, new Uri(inputPath));
            xdmDestination = new XdmDestination();
            xsltTransformer.Run(xdmDestination);

            fileStream.Close();

            return xdmDestination;
        }

        private XdmDestination saxonXsltTransform(string xdmStyleSheet, string inputPath)
        {
            XmlReader xmlReader = XmlReader.Create(xdmStyleSheet);
            FileStream fileStream = new FileStream(inputPath, FileMode.Open);
            
            xsltExecutable = xsltCompiler.Compile(xmlReader);
            xsltTransformer = xsltExecutable.Load();

            xsltTransformer.SetInputStream(fileStream, new Uri(inputPath));
            xdmDestination = new XdmDestination();
            xsltTransformer.Run(xdmDestination);

            xmlReader.Close();
            fileStream.Close();

            return xdmDestination;
        }
    }
}
