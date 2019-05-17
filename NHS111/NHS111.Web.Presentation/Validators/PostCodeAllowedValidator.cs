﻿using System.Threading.Tasks;
using NHS111.Models.Models.Web.Validators;
using NHS111.Models.Models.Web.CCG;
using NHS111.Web.Presentation.Builders;
using System.Text.RegularExpressions;
using NHS111.Features;

namespace NHS111.Web.Presentation.Validators
{
    using System;

    public class PostCodeAllowedValidator : IPostCodeAllowedValidator
    {
        private readonly ICCGModelBuilder _ccgModelBuilder;
        private readonly IAllowedPostcodeFeature _allowedPostcodeFeature;
        
        public PostCodeAllowedValidator(IAllowedPostcodeFeature allowedPostcodeFeature, ICCGModelBuilder ccgModelBuilder)
        {
            _allowedPostcodeFeature = allowedPostcodeFeature;
            _ccgModelBuilder= ccgModelBuilder;
        }

        public PostcodeValidatorResponse IsAllowedPostcode(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
               return PostcodeValidatorResponse.InvalidSyntax;
            if (!_alphanumericRegex.IsMatch(postcode.Replace(" ", "")))
                return PostcodeValidatorResponse.InvalidSyntax;
            if (!_allowedPostcodeFeature.IsEnabled)
                return PostcodeValidatorResponse.InPathwaysArea;
            var ccgModelBuilderTask = Task.Run(async () => await _ccgModelBuilder.FillCCGDetailsModelAsync(postcode));
            CcgModel = ccgModelBuilderTask.Result;
            if (CcgModel.Postcode == null)
                return PostcodeValidatorResponse.PostcodeNotFound;
            if (CcgModel.PharmacyServicesAvailable)
                return PostcodeValidatorResponse.InAreaWithPharmacyServices;

            return PostcodeValidatorResponse.InPathwaysArea;
        }

        public async Task<PostcodeValidatorResponse> IsAllowedPostcodeAsync(string postcode)
        {
            if (string.IsNullOrWhiteSpace(postcode))
                return PostcodeValidatorResponse.InvalidSyntax;
            if (!_alphanumericRegex.IsMatch(postcode.Replace(" ", "")))
                return PostcodeValidatorResponse.InvalidSyntax;
            if (!_allowedPostcodeFeature.IsEnabled)
                return PostcodeValidatorResponse.InPathwaysArea;
            try {
                CcgModel = await _ccgModelBuilder.FillCCGDetailsModelAsync(postcode);
            } catch (ArgumentException) {
                return PostcodeValidatorResponse.InvalidSyntax;
            }

            if (CcgModel.Postcode == null)
                return PostcodeValidatorResponse.PostcodeNotFound;
            if (CcgModel.PharmacyServicesAvailable)
                return PostcodeValidatorResponse.InAreaWithPharmacyServices;

            return PostcodeValidatorResponse.InPathwaysArea;
        }

        public CCGDetailsModel CcgModel { get; private set; }
            
        private readonly Regex _alphanumericRegex = new Regex(@"^[a-zA-Z0-9]+$");
    }
}
