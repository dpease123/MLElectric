using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Description;

namespace PowerUsageApi.SwaggerFilters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SwaggerImplementationNotesAttribute : Attribute
    {
        public string ImplementationNotes { get; private set; }

        public SwaggerImplementationNotesAttribute(string implementationNotes)
        {
            this.ImplementationNotes = implementationNotes;
        }
    }

    public class ApplySwaggerImplementationNotesFilterAttributes : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var attr = apiDescription.GetControllerAndActionAttributes<SwaggerImplementationNotesAttribute>().FirstOrDefault();
            if (attr != null)
            {
                operation.description = attr.ImplementationNotes;
            }
        }
    }


   

}