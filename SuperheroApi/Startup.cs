using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SuperheroApi.DbModels;

namespace SuperheroApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
         
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDbContext<ApiContext>(context => { context.UseInMemoryDatabase("SuperheroApi"); });
            //   services.AddDbContext<ApiContext>(context => { context.UseInMemoryDatabase("SuperheroApi"); });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            //Get the database context 
            var context = serviceProvider.GetService<ApiContext>();

            //populate the databse with some initial data when the progam is first run
            AddTestData(context);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }


        //Function to add some intial data to the in memory database.
        private static void AddTestData(ApiContext context)
        {
            //Initailize and add the three superheros to start with when the app fires up
            var testUser1 = new DbModels.Superhero
            {             
                Alias = "Batman", Origin = "Gotham City", ApprovalRate = "74%", Archenemy = "Superman"

            };
            var testUser2 = new DbModels.Superhero
            {
                Alias = "Superman",  Origin = "Metropolis", ApprovalRate = "41%",  Archenemy = "Lex Luthor"

            };

            var testUser3 = new DbModels.Superhero
            {
                Alias = "Wonder Woman",  Origin = "Themyscira",  ApprovalRate = "88%"

            };
            var testUser4 = new DbModels.Superhero
            {
                Alias = "Lex Luthor", Origin = "Metropolis", Archenemy = "Superman"

            };
            context.Users.Add(testUser1);
            context.Users.Add(testUser2);
            context.Users.Add(testUser3);
            context.Users.Add(testUser4);


            context.SaveChanges();

        }
    }
}
