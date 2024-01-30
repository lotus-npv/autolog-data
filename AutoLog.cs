using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using Serilog.Core;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using AutoLog.Job;

namespace AutoLog
{
    public class AutoWLog
    {
        public static Logger? Logger { get; set; }

        [Obsolete]
        public static void Load(WebApplicationBuilder builder)
        {
            Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

            // Add services to the container.
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Logger);
            builder.Services.AddHttpContextAccessor();

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddSerilog();

            builder.Services.AddQuartz(q =>
            {
                string CronSchedule = GetConfigString("Quartz:CronSchedule");
                q.UseMicrosoftDependencyInjectionJobFactory();
                // Just use the name of your job that you created in the Jobs folder.
                var jobKey = new JobKey("WriteLogToDbJob");
                q.AddJob<WriteLogToDbJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("WriteLogToDbJob-trigger")
                    //This Cron interval can be described as "run every minute" (when second is zero)
                    .WithCronSchedule(CronSchedule)
                );
            });
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        }

        public static string GetConfigString(string key)
        {
            var config = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            string values = config[key] ?? "null";

            return values;
        }

        private static string GetCallingMethodName()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame[] stackFrames = stackTrace.GetFrames();

            // Skip the first frame (GetCallingMethodName itself) and get the second frame
            // to get the method calling GetCallingMethodName.
            if (stackFrames.Length >= 2)
            {
                MethodBase callingMethod = stackFrames[2].GetMethod()!;
                return callingMethod.Name;
            }
            else
            {
                return "Unknown";
            }
        }

        public static void WLog<T>(string mes, Level logLevel)
        {
            string? MethodName = GetCallingMethodName();
            string? ClassName = typeof(T).FullName;

            switch (logLevel)
            {
                case Level.Debug:
                    Logger?.Debug($"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                case Level.Information:
                    Logger?.Information($"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                case Level.Warning:
                    Logger?.Warning($"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                case Level.Error:
                    Logger?.Error($"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                case Level.Fatal:
                    Logger?.Fatal($"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                default:
                    break;
            }
        }

        public static void WLog<T>(Exception ex, string mes, Level logLevel)
        {
            string? MethodName = GetCallingMethodName();
            string? ClassName = typeof(T).FullName;

            switch (logLevel)
            {
                case Level.Debug:
                    Logger?.Debug(ex, $"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                case Level.Information:
                    Logger?.Information(ex, $"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                case Level.Warning:
                    Logger?.Warning(ex, $"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                case Level.Error:
                    Logger?.Error(ex, $"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                case Level.Fatal:
                    Logger?.Fatal(ex, $"{logLevel} | {ClassName}.{MethodName} | {mes}");
                    break;
                default:
                    break;
            }
        }
    }
}

