using Google.Cloud.SecretManager.V1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Newtonsoft.Json.Linq;
using ZachPerini_6._3A_HA.Repositories;

namespace ZachPerini_6._3A_HA
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            string pathToKey = builder.Environment.ContentRootPath + "zp-homeassignment-d912c671a132.json";
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", pathToKey);

            //access the secret vaults
            string project = builder.Configuration["project"].ToString(); //reads the project id from appsettings.json


            string secretId = "myKeys";
            string secretVersionId = "1";
            // Create the client.
            SecretManagerServiceClient client = SecretManagerServiceClient.Create();

            // Build the resource name.
            SecretVersionName secretVersionName = new SecretVersionName(project, secretId, secretVersionId);

            // Call the API.
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

            // Convert the payload to a string. Payloads are bytes by default.
            String payload = result.Payload.Data.ToStringUtf8();

            dynamic jsonObject = JObject.Parse(payload);


            string clientId = jsonObject["Authentication:Google:ClientId"].ToString();
            string clientSecretId = jsonObject["Authentication:Google:ClientSecret"].ToString();
            string redisConnection = jsonObject["redis"].ToString();


            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecretId;
                });

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
                options.OnAppendCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            builder.Services.AddRazorPages();

            //string project = builder.Configuration["project"].ToString();
            string bucket = builder.Configuration["bucket"].ToString();
            string topic = builder.Configuration["topic"].ToString();

            //services is a collection holding all the initialised services so whens theres a controller 
            //asking for an instance of a particular class it exists and the injector class can give it to it.
            builder.Services.AddScoped<ArtefactsRepository>(x => new ArtefactsRepository(project));
            builder.Services.AddScoped<SharedUserRepository>(x => new SharedUserRepository(project));
            builder.Services.AddScoped<BucketsRepository>(x => new BucketsRepository(project, bucket));
            builder.Services.AddScoped<PubSubRepository>(x => new PubSubRepository(project, topic));
            builder.Services.AddScoped<RedisRepository>(x => new RedisRepository(redisConnection));



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Artefacts/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Artefacts}/{action=Index}/{id?}");

            app.Run();
        }

        private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                // TODO: Use your User Agent library of choice here.
                if (true)
                {
                    // For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }
    }
}