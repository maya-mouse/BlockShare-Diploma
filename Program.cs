using BlockShare.Data;
using BlockShare.Services;
using BlockShare.SmartContracts;
using Microsoft.EntityFrameworkCore;

namespace BlockShare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSession();
            builder.Services.AddDistributedMemoryCache();
            // Add services to the container.
            builder.Services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
			

            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });
            builder.Services.AddSingleton<IpfsService>();
            builder.Services.AddSingleton<WalletService>();
			builder.Services.AddTransient<EmailService>();
            builder.Services.AddSingleton<EncryptionService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthorization();


			var serviceProvider = builder.Services.BuildServiceProvider();

			// ✴️ Отримуємо контекст та інші сервіси
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

			// (Якщо треба інші сервіси)
			// var walletService = scope.ServiceProvider.GetRequiredService<WalletService>();

			// ✴️ Створюємо інстанс свого класу з потрібними залежностями
			var contractDeployer = new SmartContractDeployer(db); // <-- передаємо
			var contractAddress =  contractDeployer.DeployContractAsync();
			Console.WriteLine("Контракт розгорнуто: " + contractAddress);


			var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
