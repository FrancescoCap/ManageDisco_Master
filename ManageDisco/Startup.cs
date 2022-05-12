using ManageDisco.Context;
using ManageDisco.Middleware;
using ManageDisco.Model;
using ManageDisco.Model.UserIdentity;
using ManageDisco.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
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
               
            });
            
            services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "ATF-F";
                options.Cookie.HttpOnly = false;
            });
            services.AddAuthentication(options => { 
            })                       
                .AddJwtBearer(opions => {
                    opions.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Jwt:ValidIssuer"],
                        ValidAudience = Configuration["Jwt:ValidAudience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateAudience = true,
                        RequireExpirationTime = true, 
                        ValidateIssuer = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:SecurityKey"]))
                    };
                });

            string ngRokClientRewrite = Configuration["NgRok:Client"];
            services.AddCors(options =>
            {
                options.AddPolicy("corsPolicy", policy => {
                    policy.AllowCredentials();
                    policy.AllowAnyMethod().AllowAnyHeader().WithOrigins().WithOrigins(new string[] { "http://localhost:4200", ngRokClientRewrite }); 
                    
                });
            });

            services.AddDistributedMemoryCache();
            services.AddSession(options => {

            });

            services.AddSingleton<Encryption>();
            services.AddSingleton<TwilioService>();
            services.AddScoped<TokenService>();

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

            //app.UseMiddleware<EncryptionMiddleware>();
            app.UseSession();
            //app.UseMiddleware<Security>();
            app.UseMiddleware<JwtCookieHandler>();
            app.UseMiddleware<UserPermissionMiddleware>();
            app.UseCors("corsPolicy");           
            app.UseRouting();
            
            app.UseAuthentication();            
            app.UseAuthorization();
                       
            CreateRoles(service).Wait();
            CreateProductShopType(service).Wait();
           

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default","api/{controller=Base}");                
            });
           
        }

        public async Task CreateAnonymusPath(DiscoContext db)
        {

            var paths = new AnonymusAllowed().GetAnonymusPaths();
            foreach (string k in paths.Keys)
            {
                var pathList = paths[k];
                foreach(string p in pathList)
                {
                    string path = pathList.FirstOrDefault(x => x == p);
                    var pathExist = db.AnonymusAllowed.Any(x => x.Controller == k && x.Path == path.Replace("/General", "") && (x.RedirectedPath == path || x.RedirectedPath == path + "/General"));
                    if (!pathExist)
                    {
                        await db.AnonymusAllowed.AddAsync(new AnonymusAllowed()
                        {
                            Controller = k,
                            Path = path.Replace("/General", ""),
                            RedirectedPath = path
                        });
                    }
                }
                
            }
            await db.SaveChangesAsync();

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

        public async Task CreatePermissionActionValues(DiscoContext db)
        {

            var permissionActionExist = await db.PermissionAction.ToListAsync();
            var permissionsToInsert = PermissionActionList.GetToInsertPermissions(permissionActionExist);
            await db.PermissionAction.AddRangeAsync(permissionsToInsert);
            await db.SaveChangesAsync();

        }

        private async Task CreateProductShopType(IServiceProvider serviceProvider)
        {
            string[] types = new string[] { ProductShopTypeCostants.PRODUCT_TYPE_ENTRANCE, ProductShopTypeCostants.PRODUCT_TYPE_TABLE, ProductShopTypeCostants.PRODUCT_TYPE_PRODUCT};
            using (var service = serviceProvider.GetRequiredService<DiscoContext>())
            {
               
                foreach (string type in types)
                {
                    if(!service.Set<ProductShopType>().Any(x => x.ProductShopTypeDescription == type))
                        service.Set<ProductShopType>().Add(new ProductShopType() { ProductShopTypeDescription = type});
                }
                await service.SaveChangesAsync();

                await CreatePermissionActionValues(service);
                await CreateAnonymusPath(service);
            }
               
        }

    }
}
