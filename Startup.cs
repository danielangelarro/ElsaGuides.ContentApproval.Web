using Elsa;
using Elsa.Persistence.EntityFramework.Core.Extensions;
using Elsa.Persistence.EntityFramework.Sqlite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ElsaGuides.ContentApproval.Web.Custom_Workflows.Registration;
using ElsaGuides.ContentApproval.Web.Entity;

namespace ElsaGuides.ContentApproval.Web
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        private IWebHostEnvironment Environment { get; }
        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var elsaSection = Configuration.GetSection("Elsa");

            services
                .AddDbContext<MyDbContext>(options =>
                    options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services
                .AddElsa(elsa => elsa
                    .UseEntityFrameworkPersistence<MyDbContext>(ef => ef.UseSqlite(Configuration.GetConnectionString("DefaultConnection")), autoRunMigrations: true)
                    .AddConsoleActivities()
                    .AddHttpActivities(elsaSection.GetSection("Server").Bind)
                    .AddEmailActivities(elsaSection.GetSection("Smtp").Bind)
                    .AddQuartzTemporalActivities()
                    .AddWorkflowsFrom<Startup>()

                    // Register Activities
                    .AddActivity<ValidateCompliance>()
                    .AddActivity<SaveDataActivity>()
                    .AddActivity<ApprovalActivity1>()
                    .AddActivity<ApprovalActivity2>()
                    .AddActivity<FinalizeActivity>()

                    // Register DBContext
                );

            services.AddElsaApiEndpoints();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app)
        {
            app
                .UseStaticFiles()
                .UseHttpActivities()
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapFallbackToPage("/_Host");
                });
        }
    }
}