using eulynx_validator.Validator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eulynx_validator
{
    internal class EulynxValidator
    {
        private string _inputUri;
        private string _reportPath;
        private ValidatorSchema _validatorSchema;
        private ValidatorSchematron _validatorSchematron;

        public EulynxValidator(string inputUri, string reportPath)
        {
            _inputUri = inputUri;
            _reportPath = reportPath;
        }

        public void Validate()
        {
            // Schema   //validate according to EULYNX XSD.
            _validatorSchema = new ValidatorSchema(_inputUri, _reportPath);
           _validatorSchema.Validate();
            _validatorSchema.MakeReport();

            //Schematron // Validate according to custom validation schema made by us.
           _validatorSchematron = new ValidatorSchematron(_inputUri, _reportPath);
            _validatorSchematron.Validate();
            _validatorSchematron.MakeRerport();
        }

        public void ReportConsole()
        {
            Console.WriteLine(_validatorSchema.Status | _validatorSchematron.Status);
        }
    }
}
