﻿
namespace NHS111.Web.Presentation.Builders
{
    using System;
    using NHS111.Models.Models.Domain;
    using NHS111.Models.Models.Web;
    using NHS111.Models.Models.Web.FromExternalServices;
    using NHS111.Models.Models.Web.Validators;

    public interface IReferralResultBuilder {
        ReferralResultViewModel Build(ITKConfirmationViewModel itkConfirmationViewModel);
        ReferralResultViewModel BuildFailureResult(ITKConfirmationViewModel itkConfirmationViewModel);
        ReferralResultViewModel BuildDuplicateResult(ITKConfirmationViewModel itkConfirmationViewModel);
        ReferralResultViewModel BuildConfirmationResult(ITKConfirmationViewModel itkConfirmationViewModel);
        ServiceUnavailableReferralResultViewModel BuildServiceUnavailableResult(OutcomeViewModel outcomeViewModel,
            DosCheckCapacitySummaryResult dosResult);
    }

    public class ReferralResultBuilder
        : IReferralResultBuilder {

        public ReferralResultBuilder(IPostCodeAllowedValidator postCodeAllowedValidator) {
            if (postCodeAllowedValidator == null)
                throw new ArgumentNullException("postCodeAllowedValidator");
            _postCodeAllowedValidator = postCodeAllowedValidator;
        }

        public ReferralResultViewModel Build(ITKConfirmationViewModel itkConfirmationViewModel) {
            if (itkConfirmationViewModel == null)
                throw new ArgumentNullException("outcomeModel");

            if (itkConfirmationViewModel.HasAcceptedCallbackOffer.HasValue && itkConfirmationViewModel.HasAcceptedCallbackOffer.Value)
                itkConfirmationViewModel.WaitTimeText = _dx334WaitTimeText; //todo data drive the 334 outcome

            if (itkConfirmationViewModel.ItkSendSuccess.HasValue && itkConfirmationViewModel.ItkSendSuccess.Value) {
                return BuildConfirmationResult(itkConfirmationViewModel);
            }

            if (itkConfirmationViewModel.ItkDuplicate.HasValue && itkConfirmationViewModel.ItkDuplicate.Value) {
                return BuildDuplicateResult(itkConfirmationViewModel);
            }

            return BuildFailureResult(itkConfirmationViewModel);
        }

        public ReferralResultViewModel BuildFailureResult(ITKConfirmationViewModel itkConfirmationViewModel) {
            if (itkConfirmationViewModel == null)
                throw new ArgumentNullException("itkConfirmationViewModel");

            if (itkConfirmationViewModel.OutcomeGroup != null) {
                if (itkConfirmationViewModel.OutcomeGroup.Is999NonUrgent)
                    return new Call999ReferralFailureResultViewModel(itkConfirmationViewModel);

                if (itkConfirmationViewModel.OutcomeGroup.IsEDCallback)
                    return new AccidentAndEmergencyReferralFailureResultViewModel(itkConfirmationViewModel);
            }

            return new ReferralFailureResultViewModel(itkConfirmationViewModel);
        }

        public ReferralResultViewModel BuildDuplicateResult(ITKConfirmationViewModel itkConfirmationViewModel)
        {
            if (itkConfirmationViewModel == null)
                throw new ArgumentNullException("itkConfirmationViewModel");

            if (itkConfirmationViewModel.OutcomeGroup != null) {
                if (itkConfirmationViewModel.OutcomeGroup.Is999NonUrgent)
                    return new Call999DuplicateReferralResultViewModel(itkConfirmationViewModel);

                if (itkConfirmationViewModel.OutcomeGroup.IsEDCallback)
                    return new AccidentAndEmergencyDuplicateReferralResultViewModel(itkConfirmationViewModel);

                if (itkConfirmationViewModel.OutcomeGroup.IsPharmacyGroup)
                    //Temporarily removed until status of Dupe bug is known https://trello.com/c/5hqJVLDv
                    return new TemporaryEmergencyPrescriptionDuplicateReferralResultViewModel(itkConfirmationViewModel); 
            }
            //Temporarily removed until status of Dupe bug is known https://trello.com/c/5hqJVLDv
            // return new DuplicateReferralResultViewModel(itkConfirmationViewModel); Temporarily removed until status of Dupe bug is known
            return new TemporaryReferralDuplicateReferralResultViewModel(itkConfirmationViewModel);
        }

        public ReferralResultViewModel BuildConfirmationResult(ITKConfirmationViewModel itkConfirmationViewModel) {
            if (itkConfirmationViewModel == null)
                throw new ArgumentNullException("itkConfirmationViewModel");

            if (itkConfirmationViewModel.OutcomeGroup != null) {
                if (itkConfirmationViewModel.OutcomeGroup.Is999NonUrgent)
                    return new Call999ReferralConfirmationResultViewModel(itkConfirmationViewModel);

                if (itkConfirmationViewModel.OutcomeGroup.IsEDCallback)
                    return new AccidentAndEmergencyReferralConfirmationResultViewModel(itkConfirmationViewModel);

                if (itkConfirmationViewModel.OutcomeGroup.IsPharmacyGroup)
                    return new EmergencyPrescriptionReferralConfirmationResultsViewModel(itkConfirmationViewModel);
            }

            return new ReferralConfirmationResultViewModel(itkConfirmationViewModel);
        }

        public ServiceUnavailableReferralResultViewModel BuildServiceUnavailableResult(OutcomeViewModel outcomeViewModel, DosCheckCapacitySummaryResult dosResult) {
            if (outcomeViewModel == null)
                throw new ArgumentNullException("outcomeViewModel");

            var result = new ServiceUnavailableReferralResultViewModel(outcomeViewModel);
            if (outcomeViewModel.OutcomeGroup != null) {
                if (outcomeViewModel.OutcomeGroup.Is999NonUrgent)
                    result = new Call999ServiceUnavailableReferralResultViewModel(outcomeViewModel);
                if (outcomeViewModel.OutcomeGroup.IsEDCallback)
                    result = new AccidentAndEmergencyServiceUnavailableReferralResultViewModel(outcomeViewModel);
                if (outcomeViewModel.OutcomeGroup.IsPharmacyGroup)
                    result = new EmergencyPrescriptionServiceUnavailableReferralResultViewModel(outcomeViewModel);
            }

            result.OutcomeModel = outcomeViewModel;
            outcomeViewModel.UnavailableSelectedService = outcomeViewModel.SelectedService;
            outcomeViewModel.DosCheckCapacitySummaryResult = dosResult;
            outcomeViewModel.DosCheckCapacitySummaryResult.ServicesUnavailable = dosResult.ResultListEmpty;

            var postcodeValidatorRepsonse =_postCodeAllowedValidator.IsAllowedPostcode(outcomeViewModel.CurrentPostcode);
            outcomeViewModel.UserInfo.CurrentAddress.IsInPilotArea = postcodeValidatorRepsonse.IsInPilotAreaForOutcome(outcomeViewModel.OutcomeGroup);
            return result;
        }

        private readonly IPostCodeAllowedValidator _postCodeAllowedValidator;
        private readonly string _dx334WaitTimeText = "30 minutes";
    }
}
