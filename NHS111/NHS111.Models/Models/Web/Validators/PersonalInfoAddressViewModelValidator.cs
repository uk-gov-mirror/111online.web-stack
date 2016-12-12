﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentValidation;

namespace NHS111.Models.Models.Web.Validators
{
    public class PersonalInfoAddressViewModelValidator : AbstractValidator<PersonalInfoAddressViewModel>
    {
        public PersonalInfoAddressViewModelValidator()
        {
            RuleFor(p => p.Postcode)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .SetValidator(new PostCodeFormatValidator<PersonalInfoAddressViewModel, string>(u => u.Postcode))
                .WithMessage("Please enter a valid UK postcode")
                .SetValidator(new PostCodeAllowedValidator<PersonalInfoAddressViewModel, string>(u => u.Postcode))
                .WithMessage("Sorry, this service is not currently available in your area.  Please call NHS 111 for advice now.");

            RuleFor(a => a.AddressLine1).NotEmpty();
            RuleFor(a => a.City).NotEmpty();
        }
    }
}
