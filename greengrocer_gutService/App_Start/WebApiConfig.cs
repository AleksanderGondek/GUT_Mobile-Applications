using System.Data.Entity;
using System.Web.Http;
using greengrocer_gutService.Models;
using Microsoft.WindowsAzure.Mobile.Service;
using Newtonsoft.Json;

namespace greengrocer_gutService
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            // Use this class to set configuration options for your mobile service
            ConfigOptions options = new ConfigOptions();

            // Use this class to set WebAPI configuration options
            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));

            // To display errors in the browser during development, uncomment the following
            // line. Comment it out again when you deploy your service for production use.
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            
            // Set default and null value handling to "Include" for Json Serializer
            config.Formatters.JsonFormatter.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Include;
            
            Database.SetInitializer(new GreenGrocerGutInitializer());
        }
    }

    public class GreenGrocerGutInitializer : ClearDatabaseSchemaIfModelChanges<GreenGrocerGutContext>
    {
        protected override void Seed(GreenGrocerGutContext context)
        {
            // Added intems for starters

            base.Seed(context);
        }
    }
}

