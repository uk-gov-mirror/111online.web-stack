﻿using System;
using NHS111.Models.Models.Business;

namespace NHS111.Business.DOS
{
    public interface IProfileHoursOfOperation
    {
        ProfileServiceTimes GetServiceTime(DateTime date);

        bool ContainsInHoursPeriod(DateTime startDateTime, DateTime endDateTime);
    }
}