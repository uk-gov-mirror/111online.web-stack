﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using NHS111.Models.Models.Domain;
using NHS111.Models.Models.Web.DosRequests;
using NHS111.Models.Models.Web.ITK;
using NHS111.Web.Presentation.Models;

namespace NHS111.Models.Mappers.WebMappings
{
    public class FromDosCaseToDosServicesByClinicalTermRequest : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<DosCase, DosServicesByClinicalTermRequest>()
                .ForMember(dest => dest.CaseId, opt => opt.MapFrom(src => src.CaseId))
                .ForMember(dest => dest.Postcode, opt => opt.MapFrom(src => src.PostCode))
                .ForMember(dest => dest.SearchDistance, opt => opt.MapFrom(src => src.SearchDistance))
                .ForMember(dest => dest.GpPracticeId, opt => opt.MapFrom(src => src.Surgery))
                .ForMember(dest => dest.Age,
                    opt => opt.ResolveUsing<AgeResolver>().FromMember(src => src.Age))
                .ForMember(dest => dest.Gender,
                    opt => opt.ResolveUsing<GenderResolver>().FromMember(src => src.Gender))
                .ForMember(dest => dest.Disposition, opt => opt.MapFrom(src => src.Disposition))
                .ForMember(dest => dest.SymptomGroupDiscriminatorCombos, opt => opt.MapFrom(src => string.Format("{0}={1}", src.SymptomGroup, src.SymptomDiscriminator)))
                .ForMember(dest => dest.NumberPerType, opt => opt.MapFrom(src => src.NumberPerType));
        }

        public class AgeResolver : ValueResolver<string, int>
        {
            protected override int ResolveCore(string source)
            {
                int age;
                if (!int.TryParse(source, out age)) return 1; //default to adult

                if (age >= 65) return 8;
                if (age >= 16 && age < 65) return 1;
                if (age >= 5 && age <= 15) return 2;
                if (age >= 1 && age <= 4) return 3;

                return 4;
            }
        }

        public class GenderResolver : ValueResolver<GenderEnum, string>
        {
            protected override string ResolveCore(GenderEnum source)
            {
                return source.ToString().ToCharArray().First().ToString();
            }
        }
    }
}
