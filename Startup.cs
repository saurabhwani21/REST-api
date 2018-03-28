using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Microsoft.EntityFrameworkCore;
using Library.API.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Diagnostics;
using NLog.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json.Serialization;

namespace Library.API
{
    public class Startup
    {
        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }		

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(setupAction =>
            {
                setupAction.ReturnHttpNotAcceptable = true;
                setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
                setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver();
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper, UrlHelper>(implementationFactory =>
            {
                var actionContext =
                implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();     
            
            services.AddTransient<ITypeHelperService, TypeHelperService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();

            //LogLevel.Trace: Helpful for a developer debugging an issue
            //LogLevel.Debug: Msgs that have short-term usefulness during development
            //LogLevel.Information: Used to track general flow of application
            // LogLevel.Warning: Does not cause the app to stop but needs future investigation.
            //LogLevel.Error: When current flow of app stops due to some failure/exception which cannot be handled.
            //LogLevel.Critical: something that needs immediate attention. /System crashes
            loggerFactory.AddDebug(LogLevel.Information);

            loggerFactory.AddNLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        //Used for logging errors
                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        if (exceptionHandlerFeature != null)
                        {
                            var logger = loggerFactory.CreateLogger("Global exception logger");
                            logger.LogError(500,
                                exceptionHandlerFeature.Error,
                                exceptionHandlerFeature.Error.Message);
                        }
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");

                    });                      
                });
            }

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.User, Models.UserDto>()
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                    $"{src.FirstName} {src.LastName}"));
                    //.ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                    //src.DateOfBirth.GetCurrentAge()));

                cfg.CreateMap<Entities.Score, Models.ScoreDto>();

                //Mapping for creation of new user
                cfg.CreateMap<Models.UserForCreationDto, Entities.User>();

                //Mapping for creation of new score 
                cfg.CreateMap<Models.ScoreForCreationDto, Entities.Score>();

                //Mapping for updation of score
                cfg.CreateMap<Models.ScoreForUpdateDto, Entities.Score>();

                //Mapping for partial updation (patch)
                cfg.CreateMap<Entities.Score, Models.ScoreForUpdateDto>();
            });


            libraryContext.EnsureSeedDataForContext();

            app.UseMvc(); 
        }
    }
}
