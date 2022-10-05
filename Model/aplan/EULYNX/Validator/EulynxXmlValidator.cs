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

        public void validate()
        {
            // Schema 
            validatorSchema = new ValidatorSchema(inputUri);
            validatorSchema.validate();
            validatorSchema.makeReport();

            // Schematron
           validatorSchematron = new ValidatorSchematron(inputUri);
           validatorSchematron.validate();
           validatorSchematron.makeReport();
        }

        public void reportConsole()
        {
            Console.WriteLine(validatorSchema.Status | validatorSchematron.Status);
        }
    }
}
