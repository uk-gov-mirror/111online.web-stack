﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using FluentValidation.Validators;

namespace NHS111.Models.Models.Web.Validators
{
    class EmailAddressValidator<TModel, TProperty> : PropertyValidator, IClientValidatable
    {
        private string _dependencyElement;
        public EmailAddressValidator(Expression<Func<TModel, TProperty>> expression) 
        :base("Enter valid email")
        {
            _dependencyElement = (expression.Body as MemberExpression).Member.Name;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var emailAddressModel = ((EmailAddressViewModel) context.Instance);

            if (EmailSkippedAndEmpty(emailAddressModel))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(emailAddressModel.EmailAddress))
            {
                return false;
            }

            return GetMatch(emailAddressModel.EmailAddress).Success;
        }

        private static bool EmailSkippedAndEmpty(EmailAddressViewModel emailAddressModel)
        {
            return emailAddressModel.Skipped && string.IsNullOrWhiteSpace(emailAddressModel.EmailAddress);
        }

        private Match GetMatch(string email)
        {
            return Regex.Match(email.Trim().ToLower(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var ruleEmail = new ModelClientValidationRule()
            {
                ErrorMessage = this.ErrorMessageSource.GetString(),
                ValidationType = "emailaddress"
            };

            ruleEmail.ValidationParameters["prefixelement"] = _dependencyElement;

            yield return ruleEmail;
        }
    }
}
