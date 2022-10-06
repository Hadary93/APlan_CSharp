using System;

namespace aplan.eulynx.validator
{
    internal class EulynxXmlValidator
    {
        //instance
        private static EulynxXmlValidator eulynxXmlValidator;
        private ValidatorSchema validatorSchema;
        private ValidatorSchematron validatorSchematron;
        public string inputUri { get; set; }

        //singleton constructor
        private EulynxXmlValidator() { }

        //singleton method
        public static EulynxXmlValidator getInstance()
        {
            if (eulynxXmlValidator == null)
            {
                eulynxXmlValidator = new EulynxXmlValidator();
            }
            return eulynxXmlValidator;
        }

        public string validate(string _inputUri)
        {
            string report=null;
            // Schema 
            validatorSchema = new ValidatorSchema(_inputUri);
            validatorSchema.validate();
            report=validatorSchema.makeReport();

            // Schematron
            //validatorSchematron = new ValidatorSchematron(inputUri);
            //validatorSchematron.validate();
            //validatorSchematron.makeReport();
            return report;
        }

        public void reportConsole()
        {
            Console.WriteLine(validatorSchema.Status | validatorSchematron.Status);
        }
    }
}
