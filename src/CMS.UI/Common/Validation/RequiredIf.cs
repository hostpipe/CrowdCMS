using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CMS.UI.Common.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RequiredIfAttribute : ValidationAttribute, IClientValidatable
    {
        public string DependentProperty;
        public object TargetValue;

        public override bool IsValid(object value)
        {
            return value != null && !String.IsNullOrEmpty(value.ToString()) && !String.IsNullOrWhiteSpace(value.ToString());
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = this.ErrorMessage,
                ValidationType = "requireif"
            };
            rule.ValidationParameters.Add("dependatnproperty", this.DependentProperty);
            rule.ValidationParameters.Add("targetvalue", this.TargetValue);
            yield return rule;
        }
    }

    public class RequiredIfValidator : DataAnnotationsModelValidator<RequiredIfAttribute>
    {
        public RequiredIfValidator(ModelMetadata metadata, ControllerContext context, RequiredIfAttribute attribute)
            : base(metadata, context, attribute) {
        }

        public override IEnumerable<ModelValidationResult> Validate(object container)
        {
            // get a reference to the property this validation depends upon
            var field = Metadata.ContainerType.GetProperty(Attribute.DependentProperty);
            if (field != null)
            {
                // get the value of the dependent property
                var value = field.GetValue(container, null);
                // compare the value against the target value
                if ((value == null && Attribute.TargetValue == null) ||
                   (value != null && value.Equals(Attribute.TargetValue)))
                {
                    // match => means we should try validating this field
                    if (!Attribute.IsValid(Metadata.Model))
                        // validation failed - return an error
                        yield return new ModelValidationResult { Message = Attribute.ErrorMessage };
                }
            }
        }
    }
}