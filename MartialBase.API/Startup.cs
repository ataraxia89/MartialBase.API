// <copyright file="Startup.cs" company="Martialtech®">
// Solution: MartialBase.API
// Project: MartialBase.API
// Copyright © 2020 Martialtech®. All rights reserved.
// </copyright>

#pragma warning disable IDE0005 // Using directive is unnecessary
using System.Reflection;
using System.Text;

using MartialBase.API.AuthTools;
using MartialBase.API.AuthTools.Interfaces;
using MartialBase.API.Data;
using MartialBase.API.Data.Caching;
using MartialBase.API.Data.Caching.Interfaces;
using MartialBase.API.Data.Repositories;
using MartialBase.API.Data.Repositories.Interfaces;
using MartialBase.API.Middleware.BuilderExtensions;
using MartialBase.API.Utilities;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
#pragma warning restore IDE0005

namespace MartialBase.API
{
    /// <summary>
    /// A class used by ASP.NET Core to define automatically-invoked methods used to start the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance injected by the app runtime.</param>
        public Startup(IConfiguration configuration) =>
            Configuration = configuration ?? throw new Exception("Startup: Configuration is null.");

        /// <summary>
        /// The current <see cref="IConfiguration"/> instance.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// An ASP.NET Core method used to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
        /// <exception cref="InvalidOperationException">Thrown when no SQL connection string is specified in the app configuration.</exception>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();

            if (Configuration.GetConnectionString("SQLConnection") == null)
            {
                throw new InvalidOperationException("ConfigureServices: Connection string 'SQLConnection' returned null.");
            }

#if DEBUG
            services.AddDbContext<MartialBaseDbContext>(
                option => option.UseSqlite(Configuration.GetConnectionString("SQLConnection")));

            // Default Functionality Added by Swagger
            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MartialBase API", Version = "v1" });
                    c.AddSecurityDefinition(
                        "Bearer",
                        new OpenApiSecurityScheme()
                        {
                            Name = "Authorization",
                            Type = SecuritySchemeType.ApiKey,
                            Scheme = "Bearer",
                            BearerFormat = "JWT",
                            In = ParameterLocation.Header,
                            Description =
                                "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                        });
                    c.AddSecurityRequirement(
                        new OpenApiSecurityRequirement
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme, Id = "Bearer"
                                    }
                                },
                                new string[] { }
                            }
                        });
                });
#elif TESTING
            services.AddDbContext<MartialBaseDbContext>(
                option => option.UseSqlite(Configuration.GetConnectionString("SQLConnection")));
#else
            services.AddDbContext<MartialBaseDbContext>(
                option => option.UseSqlServer(Configuration.GetConnectionString("SQLConnection")));
#endif

            services.AddAuthentication(
                    options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })

                // Adding Jwt Bearer
                .AddJwtBearer(
                    options =>
                    {
                        options.SaveToken = true;
                        options.RequireHttpsMetadata = false;
                        options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidAudience = Configuration["JWT:ValidAudience"],
                            ValidIssuer = Configuration["JWT:ValidIssuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                        };
                    });

            services.AddControllers();

            services.AddScoped<IAddressesRepository, AddressesRepository>();
            services.AddScoped<IArtsRepository, ArtsRepository>();
            services.AddScoped<IArtGradesRepository, ArtGradesRepository>();
            services.AddScoped<IAzureUserHelper, AzureUserHelper>();
            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<IDocumentTypesRepository, DocumentTypesRepository>();
            services.AddScoped<IDocumentsRepository, DocumentsRepository>();
            services.AddScoped<IMartialBaseUsersRepository, MartialBaseUsersRepository>();
            services.AddScoped<IMartialBaseUserRolesRepository, MartialBaseUserRolesRepository>();
            services.AddScoped<IOrganisationsRepository, OrganisationsRepository>();
            services.AddScoped<ISchoolsRepository, SchoolsRepository>();
            services.AddScoped<IPeopleRepository, PeopleRepository>();
            services.AddScoped<IUserRolesRepository, UserRolesRepository>();

            services.AddScoped<IAzureUserHelper, AzureUserHelper>();
            services.AddScoped<IMartialBaseUserHelper, MartialBaseUserHelper>();

            services.AddScoped<IScopedCache, ScopedCache>();

#if DEBUG
            services.AddSwaggerGen(setupAction =>
            {
                setupAction.SwaggerDoc(
                    "MartialBaseAPIDocumentation",
                    new OpenApiInfo() { Title = "MartialBase API", Version = "1" });

                var xmlDocumentationFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlDocumentationFullPath = Path.Combine(AppContext.BaseDirectory, xmlDocumentationFile);
                setupAction.IncludeXmlComments(xmlDocumentationFullPath);

                setupAction.DocumentFilter<XmlDocumentFilter>();
            });
#endif
        }

        /// <summary>
        /// An ASP.NET Core method used to configure the HTTP request pipeline. This method gets called
        /// by the runtime.
        /// </summary>
        /// <remarks>This method is called after the ConfigureServices method.</remarks>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance provided by the built-in IoC container.</param>
        /// <param name="env">The <see cref="IWebHostEnvironment"/> instance.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandlingMiddleware();

            if (env.IsDevelopment())
            {
                app.UseStatusCodePages();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

#if DEBUG
            app.UseSwagger();

            app.UseSwaggerUI(setupAction =>
            {
                setupAction.SwaggerEndpoint(
                    "/swagger/MartialBaseAPIDocumentation/swagger.json",
                    "MartialBase API");
                setupAction.RoutePrefix = string.Empty;
            });
#endif
        }
    }
}
