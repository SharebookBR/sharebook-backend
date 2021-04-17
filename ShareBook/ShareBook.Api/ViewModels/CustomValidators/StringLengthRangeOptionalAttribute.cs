using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels.CustomValidators
{
    public class StringLengthRangeOptionalAttribute : ValidationAttribute
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public override bool IsValid(object value)
        {
            string model = (string)value;

            if (value == null || model.Length == 0 || model == string.Empty)
                return true;

            return model.Length > Minimum && model.Length <= Maximum;
        }

    }
}
