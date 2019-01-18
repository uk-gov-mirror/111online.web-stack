﻿
namespace NHS111.Web.Presentation.Test.Builders {
    using Moq;
    using NHS111.Models.Models.Domain;
    using NHS111.Models.Models.Web;
    using NHS111.Models.Models.Web.Validators;
    using NUnit.Framework;
    using Presentation.Builders;

    [TestFixture]
    public class ReferralResultBuilderTests {
        private readonly Mock<IPostCodeAllowedValidator> _mockPostcodeValidator = new Mock<IPostCodeAllowedValidator>();
        private OutcomeViewModel _referralRequestResult;

        [SetUp]
        public void Setup() {
            _referralRequestResult = new OutcomeViewModel {
                OutcomeGroup = new OutcomeGroup()
            };
        }

        [Test]
        public void Build_WithSuccessfulReferral_ReturnsReferralConfirmationModel() {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkSendSuccess = true;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<ReferralConfirmationResultViewModel>(result);
        }

        [Test]
        public void Build_WithSuccessful999Referral_ReturnsReferralConfirmationModel() {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkSendSuccess = true;
            _referralRequestResult.OutcomeGroup = OutcomeGroup.Call999Cat3;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<Call999ReferralConfirmationResultViewModel>(result);
        }

        [Test]
        public void Build_WithSuccessfulEDReferral_ReturnsReferralConfirmationModel() {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkSendSuccess = true;
            _referralRequestResult.OutcomeGroup = OutcomeGroup.AccidentAndEmergency;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<AccidentAndEmergencyReferralConfirmationResultViewModel>(result);
        }

        [Test]
        public void Build_WithUnsuccessfulReferral_ReturnsReferralFailureModel() {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkSendSuccess = false;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<ReferralFailureResultViewModel>(result);
        }

        [Test]
        public void Build_WithUnsuccessful999Referral_ReturnsReferralFailureModel() {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkSendSuccess = false;
            _referralRequestResult.OutcomeGroup = OutcomeGroup.Call999Cat3;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<ReferralFailureResultViewModel>(result);
        }

        [Test]
        public void Build_WithUnsuccessfulEDReferral_ReturnsReferralFailureModel() {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkSendSuccess = false;
            _referralRequestResult.OutcomeGroup = OutcomeGroup.AccidentAndEmergency;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<ReferralFailureResultViewModel>(result);
        }

        [Test]
        public void Build_WithDuplicateReferral_ReturnsDuplicateReferralModel()
        {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkDuplicate = true;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<DuplicateReferralResultViewModel>(result);
        }

        [Test]
        public void Build_WithDuplicate999Referral_ReturnsDuplicateReferralModel()
        {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkDuplicate = true;
            _referralRequestResult.OutcomeGroup = OutcomeGroup.Call999Cat3;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<DuplicateReferralResultViewModel>(result);
        }

        [Test]
        public void Build_WithDuplicateEDReferral_ReturnsDuplicateReferralModel()
        {
            var builder = new ReferralResultBuilder(_mockPostcodeValidator.Object);
            _referralRequestResult.ItkDuplicate = true;
            _referralRequestResult.OutcomeGroup = OutcomeGroup.AccidentAndEmergency;
            var result = builder.Build(_referralRequestResult);
            Assert.IsInstanceOf<DuplicateReferralResultViewModel>(result);
        }

    }
}