﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using NHS111.Models.Models.Web.Clock;

namespace NHS111.Models.Models.Web.FromExternalServices
{
    public class DosService
    {
        private readonly IClock _clock;

        public DosService()
            : this(new SystemClock())
        {
        }
        public DosService(IClock clock)
        {
            _clock = clock;
        }

        [JsonProperty(PropertyName = "idField")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "nameField")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "postcodeField")]
        public string PostCode { get; set; }

        [JsonProperty(PropertyName = "odsCodeField")]
        public string OdsCode { get; set; }

        [JsonProperty(PropertyName = "capacityField")]
        public DosCapacity Capacity { get; set; }

        [JsonProperty(PropertyName = "contactDetailsField")]
        public string ContactDetails { get; set; }

        [JsonProperty(PropertyName = "addressField")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "northingsField")]
        public int Northings { get; set; }

        [JsonProperty(PropertyName = "eastingsField")]
        public int Eastings { get; set; }

        [JsonProperty(PropertyName = "urlField")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "serviceTypeField")]
        public ServiceDetails ServiceType { get; set; }

        [JsonProperty(PropertyName = "nonPublicTelephoneNo")]
        public string NonPublicTelephoneNo { get; set; }

        [JsonProperty(PropertyName = "fax")]
        public string Fax { get; set; }

        [JsonProperty(PropertyName = "referralText")]
        public string ReferralText { get; set; }

        [JsonProperty(PropertyName = "distance")]
        public string Distance { get; set; }

        [JsonProperty(PropertyName = "notesField")]
        public string Notes { get; set; }

        [JsonProperty(PropertyName = "openAllHoursField")]
        public bool OpenAllHours { get; set; }

        private ServiceCareItemRotaSession[] _rotaSessions = new ServiceCareItemRotaSession[0];
        private ServiceCareItemRotaSession[] _rotaSessionsAndSpecifiedSessions = new ServiceCareItemRotaSession[0];

        [JsonProperty(PropertyName = "rotaSessionsField")]
        public ServiceCareItemRotaSession[] RotaSessions {
            get
            {
                if (_rotaSessionsAndSpecifiedSessions != null && _rotaSessionsAndSpecifiedSessions.Length != 0)
                    return _rotaSessionsAndSpecifiedSessions;

                return _rotaSessions;
            }
            set
            {
                _rotaSessionsAndSpecifiedSessions = CombineRotaSessionsAndSpecifiedSessions(value, _openTimeSpecifiedSessions);
                _rotaSessions = value;
            }
        }

        private string[] _openTimeSpecifiedSessions = new string[0];

        [JsonProperty(PropertyName = "openTimeSpecifiedSessionsField")]
        public string[] OpenTimeSpecifiedSessions
        {
            get { return _openTimeSpecifiedSessions; }
            set
            {
                _rotaSessionsAndSpecifiedSessions = CombineRotaSessionsAndSpecifiedSessions(_rotaSessions, value);
                _openTimeSpecifiedSessions = value;
            }
        }

        private List<DateTime> GetWeeksDates(DateTime startDate)
        {
            List<DateTime> list = new List<DateTime>();

            for (var i = 1; i < 8; i++)
            {
                list.Add(startDate);
                startDate = startDate.AddDays(1);
            }

            return list;
        }

        private bool IsDateInList(DateTime date, List<DateTime> nextWeekDates)
        {
            return nextWeekDates.Any(d => date.Date == d.Date);
        }

        private DateTime ConvertOpenTimeSpecifiedSessionToDate(string session)
        {
            try
            {
                var day = int.Parse(session.Substring(0, 2));
                var month = int.Parse(session.Substring(3, 2));
                var year = int.Parse(session.Substring(6, 4));

                return new DateTime(year, month, day);
            }
            catch (FormatException)
            {
                return new DateTime();
            }
        }

        private ServiceCareItemRotaSession[] CombineRotaSessionsAndSpecifiedSessions(ServiceCareItemRotaSession[] rotaSessions, string[] openTimeSpecifiedSessions)
        {
            if (openTimeSpecifiedSessions == null || openTimeSpecifiedSessions.Length == 0)
                return rotaSessions;

            var sessionsList = new List<ServiceCareItemRotaSession>();

            if (rotaSessions != null)
                sessionsList.AddRange(rotaSessions);

            var nextWeeksDates = GetWeeksDates(_clock.Now);

            foreach (var session in openTimeSpecifiedSessions)
            {
                if (session.Length != 22)
                    continue;

                var date = ConvertOpenTimeSpecifiedSessionToDate(session);
;                if (!IsDateInList(date, nextWeeksDates))
                    continue;
                
                var dayOfWeek = Mapper.Map<DayOfWeek>(date.DayOfWeek);

                short startTimeHours;
                short startTimeMinutes;

                short endTimeHours;
                short endTimeMinutes;

                try
                {
                    startTimeHours = short.Parse(session.Substring(11, 2));
                    startTimeMinutes = short.Parse(session.Substring(14, 2));

                    endTimeHours = short.Parse(session.Substring(17, 2));
                    endTimeMinutes = short.Parse(session.Substring(20, 2));
                }
                catch (FormatException)
                {
                    continue;
                }
                
                ServiceCareItemRotaSession rotaSession =
                    new ServiceCareItemRotaSession
                    {
                        StartDayOfWeek = dayOfWeek,
                        EndDayOfWeek = dayOfWeek,
                        StartTime = new TimeOfDay
                        {
                            Hours = startTimeHours,
                            Minutes = startTimeMinutes
                        },
                        EndTime = new TimeOfDay
                        {
                            Hours = endTimeHours,
                            Minutes = endTimeMinutes
                        },
                        Status = "Open"
                    };

                sessionsList.RemoveAll(s => s.StartDayOfWeek.Equals(dayOfWeek));
                sessionsList.Add(rotaSession);
            }

            return sessionsList.ToArray();
        }
    }

    public enum DosCapacity
    {
        /// <remarks/>
        High,
        /// <remarks/>
        Low,
        /// <remarks/>
        None,
    }

    public class ServiceCareItemRotaSession
    {
        [JsonProperty(PropertyName = "startDayOfWeekField")]
        public DayOfWeek StartDayOfWeek { get; set; }
        [JsonProperty(PropertyName = "startTimeField")]
        public TimeOfDay StartTime { get; set; }
        [JsonProperty(PropertyName = "endDayOfWeekField")]
        public DayOfWeek EndDayOfWeek { get; set; }
        [JsonProperty(PropertyName = "endTimeField")]
        public TimeOfDay EndTime { get; set; }
        [JsonProperty(PropertyName = "statusField")]
        public string Status { get; set; }
    }

    public enum DayOfWeek
    {
        /// <remarks/>
        Sunday,
        /// <remarks/>
        Monday,
        /// <remarks/>
        Tuesday,
        /// <remarks/>
        Wednesday,
        /// <remarks/>
        Thursday,
        /// <remarks/>
        Friday,
        /// <remarks/>
        Saturday,
    }

    public class TimeOfDay
    {
        [JsonProperty(PropertyName = "hoursField")]
        public short Hours { get; set; }
        [JsonProperty(PropertyName = "minutesField")]
        public short Minutes { get; set; }
    }
}
