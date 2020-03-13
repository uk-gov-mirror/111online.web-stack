﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.Ajax.Utilities;
using NHS111.Models.Models.Web;
using NHS111.Models.Models.Web.CCG;
using NHS111.Models.Models.Web.Validators;
using NHS111.Web.Presentation.Builders;
using IConfiguration = NHS111.Web.Presentation.Configuration.IConfiguration;

namespace NHS111.Web.Controllers
{
    public class LocationController : Controller
    {
        private readonly IPostCodeAllowedValidator _postCodeAllowedValidator;
        private readonly ILocationResultBuilder _locationResultBuilder;
        private readonly IConfiguration _configuration;

        public LocationController(IPostCodeAllowedValidator postCodeAllowedValidator, ILocationResultBuilder locationResultBuilder, IConfiguration configuration)
        {
            _postCodeAllowedValidator = postCodeAllowedValidator;
            _locationResultBuilder = locationResultBuilder;
            _configuration = configuration;
        }

        [HttpGet]
        public ActionResult Home(JourneyViewModel model)
        {
            return View(model);
        }
        

        [Route("Location")]
        [HttpPost]
        public ActionResult Location(LocationViewModel model)
        {
            return View(model);
        }
        
        [Route("Location/ChangePostcode")]
        [HttpGet]
        public ActionResult ChangePostcode(LocationViewModel model)
        {
            return View("Location", model);
        }

        [HttpPost]
        public async Task<ActionResult> Find(LocationViewModel model)
        {
            ModelState.Clear();
            var postcodeValidationRepsonse = await _postCodeAllowedValidator.IsAllowedPostcodeAsync(model.CurrentPostcode);
            if (postcodeValidationRepsonse == PostcodeValidatorResponse.InvalidSyntax) {
                ModelState.AddModelError("invalid-postcode", "Please enter a valid postcode");
                return View("location", model);
            }

            return DeriveApplicationView(model, postcodeValidationRepsonse, _postCodeAllowedValidator.CcgModel);
        }

        [HttpPost]
        public ActionResult FindByAddress(ConfirmLocationViewModel model)
        {
            ModelState.Clear();
            var postcodeValidationRepsonse = _postCodeAllowedValidator.IsAllowedPostcode(model.SelectedPostcode);

            return DeriveApplicationView(model, postcodeValidationRepsonse, _postCodeAllowedValidator.CcgModel);
        }

        [HttpPost]
        public async Task<ActionResult> ConfirmAddress(string longlat, ConfirmLocationViewModel model)
        {
            var results = await _locationResultBuilder.LocationResultByGeouilder(longlat);
            var locationResults = Mapper.Map<List<AddressInfoViewModel>>(results.DistinctBy(r => r.Thoroughfare));
            return View("ConfirmLocation", new ConfirmLocationViewModel { FoundLocations = locationResults, SessionId = model.SessionId, Campaign = model.Campaign, FilterServices = model.FilterServices, PathwayNo = model.PathwayNo, IsCustomJourney = model.IsCustomJourney});
        }

        private ActionResult DeriveApplicationView(JourneyViewModel model, PostcodeValidatorResponse postcodeValidationRepsonse, CCGDetailsModel ccg)
        {
            var moduleZeroViewName = "../Question/InitialQuestion";
            model.CurrentPostcode = ccg.Postcode;
            model.Campaign = string.IsNullOrEmpty(model.Campaign) ? ccg.StpName : model.Campaign;
            model.Source = string.IsNullOrEmpty(model.Source) ? ccg.CCG : model.Source;

            switch (postcodeValidationRepsonse) {
                case PostcodeValidatorResponse.InPathwaysAreaWithoutPharmacyServices: {
                    if (IsRequestingPharmacyPathway(model.PathwayNo))
                            return View("../Pathway/EmergencyPrescriptionsOutOfArea", model);

                    model.PathwayNo = _COVIDPathwayNo;
                    return View(moduleZeroViewName, model);
                }
                case PostcodeValidatorResponse.InPathwaysAreaWithPharmacyServices
                    : //postcode with pharmacy services but didn't request pharmacy pathway
                {
                    model.PathwayNo = _COVIDPathwayNo;
                    return View(moduleZeroViewName, model);
                }

                case PostcodeValidatorResponse.PostcodeNotFound:
                    return View("OutOfArea",
                        new OutOfAreaViewModel {
                            SessionId = model.SessionId, Campaign = ccg.StpName, Source = ccg.CCG,
                            FilterServices = model.FilterServices, IsCustomJourney = model.IsCustomJourney
                        });
            }

            return View("Location");
        }

        private bool IsRequestingPharmacyPathway(string pathwayNo) {
            return pathwayNo != null && pathwayNo.ToUpper() == EmergencyPrescriptionsPathwayNo;
        }

        private static string EmergencyPrescriptionsPathwayNo = "PW1827";
        private string _COVIDPathwayNo = "PW1851";
    }

}