using System.Web.Http;
using WebActivatorEx;
using EnergyFoecaster;
using Swashbuckle.Application;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace EnergyFoecaster
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

             GlobalConfiguration.Configuration
              .EnableSwagger(c =>
              {
                  c.SingleApiVersion("v1", "SwaggerDemoApi");
                  c.IncludeXmlComments(string.Format(@"{0}\bin\SwaggerDemoApi.XML",
                                       System.AppDomain.CurrentDomain.BaseDirectory));
              })
            .EnableSwaggerUi();
        }
    }

    private static string GetXmlCommentsPath()
    {
        return System.AppDomain.CurrentDomain.BaseDirectory + @"\bin\DemoWebAPIWithSwagger.XML";
    }
}
