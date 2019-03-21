using LightBDD.Core.Configuration;
using LightBDD.Framework.Configuration;
using LightBDD.Framework.Reporting.Formatters;
using LightBDD.XUnit2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Tests.BDD.Configurations
{
    class ConfiguredLightBddScopeAttribute : LightBddScopeAttribute
    {
        protected override void OnConfigure(LightBddConfiguration configuration)
        {
            // some example customization of report writers
            configuration
                .ReportWritersConfiguration()
                .AddFileWriter<PlainTextReportFormatter>("~\\Reports\\FeaturesReport.txt");
        }

        protected override void OnSetUp()
        {
            // additional code that has to be run before any LightBDD tests
        }

        protected override void OnTearDown()
        {
            // additional code that has to be run after all LightBDD tests
        }
    }
}
