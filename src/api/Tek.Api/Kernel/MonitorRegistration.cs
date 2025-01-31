﻿using Sentry.Infrastructure;

namespace Microsoft.Extensions.DependencyInjection;

public static class MonitorRegistration
{
    public static IServiceCollection AddMonitoring(this IServiceCollection services, MonitoringSettings monitoring, ReleaseSettings release)
    {
        services.AddSingleton<ILog, Tek.Service.Log>();
        services.AddSingleton<IMonitor, Tek.Service.Monitor>();

        SentrySdk.Init(options =>
        {
            options.AutoSessionTracking = true;
            options.DiagnosticLevel = SentryLevel.Info;
            options.Dsn = monitoring.Url;
            options.Environment = release.Environment.ToString().ToLower();
            options.Release = release.Version;
            options.SendDefaultPii = true;

            if (0.0 <= monitoring.Rate && monitoring.Rate <= 1.0)
            {
                options.TracesSampleRate = monitoring.Rate;
                options.ProfilesSampleRate = monitoring.Rate;
            }

            if (monitoring.Debug)
            {
                var folder = Path.GetDirectoryName(monitoring.Path);
                if (folder != null)
                {
                    Directory.CreateDirectory(folder);

                    options.Debug = true;
                    options.DiagnosticLogger = new FileDiagnosticLogger(monitoring.Path);
                }
            }

            services.AddSentryTunneling();

            // If Sentry's Debug setting is enabled with a FileDiagnosticLogger then the logger
            // sometimes throws an unhandled exception from here:
            // Sentry.Integrations.AppDomainProcessExitIntegration.HandleProcessExit.

            // The exception is a System.ObjectDisposedException with Message = "Cannot write to a
            // closed TextWriter". We need to disable Sentry's default automatic behaviour and
            // manually flush any queued events before application shutdown.

            options.DisableAppDomainProcessExitFlush();
        });

        return services;
    }
}
