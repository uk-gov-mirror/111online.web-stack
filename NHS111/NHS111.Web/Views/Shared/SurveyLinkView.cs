﻿using System.Web.Mvc;
using NHS111.Features;

namespace NHS111.Web.Views.Shared
{
    public class SurveyLinkView : WebViewPage
    {
        public ISurveyLinkFeature SurveyLinkFeature { get; set; }

        public SurveyLinkView()
        {
            SurveyLinkFeature = new SurveyLinkFeature();
        }

        public override void Execute() { }
    }
}