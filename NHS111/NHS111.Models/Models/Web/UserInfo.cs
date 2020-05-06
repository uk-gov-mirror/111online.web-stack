﻿using System;
using FluentValidation.Attributes;
using NHS111.Models.Models.Web.Validators;


namespace NHS111.Models.Models.Web
{
    [Validator(typeof(UserInfoValidator))]
    public class UserInfo
    {
        public AgeGenderViewModel Demography { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }

        private DateTime? _dob;
        public DateTime? DoB
        {
            get
            {
                if (Year != null && Month != null && Day != null)
                {
                    try
                    {
                        _dob = new DateTime(Year.Value, Month.Value, Day.Value);
                        return _dob;
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        return null;
                    }
                }
                return null;
            }
        }

        public string TelephoneNumber
        {
            get;
            set;
        }

        private String RemoveValidInternationalPrefix(string telephoneNumber)
        {
            if (telephoneNumber.StartsWith("00"))
                telephoneNumber = "+" + telephoneNumber.Substring(2);

            if (telephoneNumber.StartsWith("+44"))
                telephoneNumber = "0" + telephoneNumber.Substring(3);

            return telephoneNumber;
        }

        public string EmailAddress { get; set; }

        public AddressInfoViewModel HomeAddress { get; set; }
        public FindServicesAddressViewModel CurrentAddress { get; set; }

        public UserInfo()
        {
            HomeAddress = new AddressInfoViewModel();
            CurrentAddress = new FindServicesAddressViewModel();
        }
    }
}