
using AMS.Helpers;
using AMS.Interfaces;
using AMS.Models;
using AMS.Repository;
using AMS.Services;
using AMS.Data;
using DinkToPdf;
using DinkToPdf.Contracts;

public static class ServiceConfiguration
{
    public static IServiceCollection AddServiceConfiguration(this IServiceCollection services, IConfiguration configuration)
    {

        // SERVICES
        //services.AddSession();

        services.AddHttpContextAccessor(); // Must be added before AuthService
        services.AddScoped<AuthService>();

        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        // DinkToPdf
        //services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
        services.AddSingleton<IConverter>(provider => new SynchronizedConverter(new PdfTools()));
        services.AddScoped<PdfService>();

        services.AddScoped<IViewRenderService, ViewRenderService>();



        // DATA
        services.AddSingleton<DapperContext>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));


        // REPOSITORIES
        services.AddScoped<IAdminRepository, AdminRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        

        return services;
    }
}