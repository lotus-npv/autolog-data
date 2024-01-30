using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Reflection;

namespace AutoLog.Middleware
{
    public class ExceptionHandlingMiddlewareOfSelog
    {
        public RequestDelegate requestDelegate;
        private readonly ILogger<ExceptionHandlingMiddlewareOfSelog> logger;
        public ExceptionHandlingMiddlewareOfSelog
        (RequestDelegate requestDelegate, ILogger<ExceptionHandlingMiddlewareOfSelog> logger)
        {
            this.requestDelegate = requestDelegate;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await requestDelegate(context);

            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }
        private Task HandleException(HttpContext context, Exception ex)
        {
            StackTrace s = new(ex);
            StackFrame[] stackFrames = s.GetFrames();

            // Skip the first frame (GetCallingMethodName itself) and get the second frame
            // to get the method calling GetCallingMethodName.
            string MethodNamePath = s.GetFrame(0).GetMethod().DeclaringType.FullName;
            string? MethodName = "";
            if (stackFrames.Length >= 1)
            {
                MethodBase callingMethod = stackFrames[0].GetMethod();
                MethodName = callingMethod.Name;
            }
            
            
            //var MethodName = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == Assembly.GetExecutingAssembly()).Name;

            logger.LogError(ex, $"Error | {MethodNamePath}.{MethodName} | {ex.Message}");
            var errorMessageObject = new { ex.Message, Code = "system_error" };
            var errorMessage = JsonConvert.SerializeObject(errorMessageObject);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return context.Response.WriteAsync(errorMessage);
        }
    }
}
