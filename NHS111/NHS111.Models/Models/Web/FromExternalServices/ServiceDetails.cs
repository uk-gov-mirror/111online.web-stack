﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NHS111.Models.Models.Web.FromExternalServices
{
    public class ServiceDetails
    {
        [JsonProperty(PropertyName = "idField")]
        public long Id;
        [JsonProperty(PropertyName = "nameField")]
        public string Name;
    }
}
