using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CMS.UI.Common.Validation
{
    public sealed class PercentageOfAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly string sourcePropertyName;
        private readonly int minimumPercentage;

        public PercentageOfAttribute(string sourcePropertyName, int minimumPercentage)
        {
            this.sourcePropertyName = sourcePropertyName;
            this.minimumPercentage = minimumPercentage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var propertyName = validationContext.ObjectType.GetProperty(this.sourcePropertyName);
            if (propertyName == null)
                return new ValidationResult(String.Format("Uknown property {0}", this.sourcePropertyName));

            int propertyValue = (int)propertyName.GetValue(validationContext.ObjectInstance, null);

            var minValue = (propertyValue / 100) * this.minimumPercentage;
            if ((int)value > minValue)
                return ValidationResult.Success;

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));

        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessageString,
                ValidationType = "percentageof"
            };
            rule.ValidationParameters["propertyname"] = this.sourcePropertyName;
            rule.ValidationParameters["minimumpercentage"] = this.minimumPercentage;
            yield return rule;
        }
    }

}