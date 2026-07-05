using Microsoft.Extensions.DependencyInjection;
using SchedulerJobCenter.Application.Services;
using System.Reflection;
namespace SchedulerJobCenter.Application.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IJobService, JobService>();
            services.AddMediatR(cfg =>
           cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            return services;
        }
    }
}
