using ManageDisco.Context;
using ManageDisco.Model.UserIdentity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManageDisco
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
            //non funzionano le migrazioni. Probabilmente il problema è qui
            services.AddDbContext<DiscoContext>(options => { 
                options.UseSqlServer(Configuration.GetConnectionString("connString"));              
               
            }, ServiceLifetime.Transient);
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "ATF-F";
                options.Cookie.HttpOnly = false;
            });

            services.AddAuthentication()
                .AddJwtBearer(opions => {
                    opions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Jwt:ValidIssuer"],
                        ValidAudience = Configuration["Jwt:ValidAudience"],
                        ValidateLifetime = true,
                        ValidateAudience = true,
                        RequireExpirationTime = true, 
                        ValidateIssuer = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecurityKey"]))
                    };
                });
            services.AddCors(options =>
            {
                options.AddPolicy("corsPolicy", policy => {
                    policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                });
            });
            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<DiscoContext>();

            services.AddControllers();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider service)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
               
            }
            app.UseCors("corsPolicy");
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            CreateRoles(service).Wait();           
          
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default","api/{controller=Base}");                
            });
        }

        public async Task CreateRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var rolesArray = Enum.GetValues(typeof(RolesEnum));
            for (int i = 0; i < rolesArray.Length; i++)
            {

                if (!await roleManager.RoleExistsAsync(rolesArray.GetValue(i).ToString()))
                {
                    await roleManager.CreateAsync(new IdentityRole() { Name = rolesArray.GetValue(i).ToString() });
                }
            }
        }

    }
}
