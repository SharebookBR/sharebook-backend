using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels.CustomValidators
{
    public class StringLengthRangeOptionalAttribute : ValidationAttribute
    {
        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public StringLengthRangeOptionalAttribute()
        {
            Minimum = 0;
            Maximum = int.MaxValue;
        }

        public override bool IsValid(object value)
        {
            string model = (string)value;

            if (value == null)
                return true;

            return model.Length > Minimum && model.Length <= Maximum;
        }

    }
}
