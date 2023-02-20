using EPOLL.Website.DataAccess;
using EPOLL.Website.DataAccess.IdentityCustomModels;
using EPOLL.Website.Infrastructure.Interfaces;
using EPOLL.Website.Infrastructure.IOptions;
using EPOLL.Website.Infrastructure.Middlewares;
using EPOLL.Website.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Text;

namespace EPOLL.Website
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddMvc();
            services.AddControllersWithViews();
            // configured required services
            services.Configure<SentryOptions>(Configuration.GetSection("Sentry"));
            services.Configure<IEmailOptions>(Configuration.GetSection("EmailService"));

            services.AddScoped<IErrorReporter, ErrorReporter>();
            services.AddTransient<ISmtpService, SmtpService>();
            services.AddTransient<IQrCodeService, QrCodeService>();

            services.AddDbContext<EPollContext>(
                item => item.UseSqlite(Configuration.GetConnectionString("myconn"), x => x.MigrationsAssembly("EPOLL.Website")));

            services.AddIdentity<ApplicationUser, ApplicationRole>(opts =>
           {
               opts.Password.RequiredLength = 6;
               opts.Password.RequireNonAlphanumeric = false;
               opts.Password.RequireLowercase = false;
               opts.Password.RequireUppercase = false;
               opts.Password.RequireDigit = false;
           })
            .AddEntityFrameworkStores<EPollContext>()
            .AddDefaultTokenProviders();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(cfg => cfg.SlidingExpiration = false)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseMiddleware<SentryMiddleware>();
            ConfigureDatabaseDefaults(app);
            
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseMvc();// (routes =>
            //{
            //  routes.MapRoute(
            //     name: "default",
            //     template: "{controller=Home}/{action=Index}/{id?}");

            //    routes.MapRoute(

            //         name: "adminspace",
            //         template: "Administration/{controller}/{action=Index}/{id?}");
            //});
            // });

            //app.UseEndpoints(endpoints => {
            //    endpoints.MapControllers();
            // });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapAreaControllerRoute(
                //    "admin",
                //    "admin",
                //    "Admin/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    "default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static void ConfigureDatabaseDefaults(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<EPollContext>() as EPollContext;
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }
                context.SaveChanges();
            }
        }
    }
}
