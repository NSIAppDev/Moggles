using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Moggles
{
    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        //private readonly IHostingEnvironment _hostingEnvironment;
        //private readonly IModelMetadataProvider _modelMetadataProvider;

        //public CustomExceptionFilterAttribute(
        //    IHostingEnvironment hostingEnvironment,
        //    IModelMetadataProvider modelMetadataProvider)
        //{
        //    _hostingEnvironment = hostingEnvironment;
        //    _modelMetadataProvider = modelMetadataProvider;
        //}

        public override void OnException(ExceptionContext context)
        {
            //if (!_hostingEnvironment.IsDevelopment())
            //{
            //    // do nothing
            //    return;
            //}
            var result = new ViewResult { ViewName = "Error" };
            //result.ViewData = new ViewDataDictionary(_modelMetadataProvider, context.ModelState);
            //result.ViewData.AddAsync("Exception", context.Exception);
            //// TODO: Pass additional detailed data via ViewData
            context.Result = result;
        }
    }
}
