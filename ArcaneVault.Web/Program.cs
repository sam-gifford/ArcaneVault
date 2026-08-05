// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

namespace ArcaneVault.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            // Add session support
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add authentication
            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/";
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                });

            // Add authorization
            builder.Services.AddAuthorization();

            // Add HttpClient for API communication
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7001";
            builder.Services.AddHttpClient("ApiClient", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Use session middleware
            app.UseSession();

            // Use authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRouting();

            app.MapStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();

            app.Run();
        }
    }
}

