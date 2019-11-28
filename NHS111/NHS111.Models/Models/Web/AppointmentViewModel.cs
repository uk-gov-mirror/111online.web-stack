﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHS111.Models.Models.Web
{
    public class AppointmentViewModel : OutcomeViewModel
    {
        public IEnumerable<SlotViewModel> Slots { get; set; }

        public AppointmentViewModel()
        {
            Slots = Enumerable.Empty<SlotViewModel>();
        }
    }
}