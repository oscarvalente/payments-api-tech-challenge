using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaymentsAPI.EfStructures;

namespace integration_tests
{
    public class TestingWebAppFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {

                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType ==
                        typeof(DbContextOptions<PaymentsAPIDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);
                services.AddDbContext<PaymentsAPIDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryPaymentsAPIDbContext");
                });
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                using (var appContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>())
                {
                    try
                    {
                        appContext.Database.EnsureCreated();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Something went wrong while setting up integration tests - {ex.Message}");
                        throw;
                    }
                }
            });
            builder.UseEnvironment("Development");
        }
    }
}