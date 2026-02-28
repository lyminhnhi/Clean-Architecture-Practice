using CodeLeap.Application.Security;
using CodeLeap.Domain.Entities;
using CodeLeap.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CodeLeap.Infrastructure.Seeding
{
    public class DataSeederHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataSeederHostedService> _logger;
        private static readonly string _imgTest = "http://test.com";

        public DataSeederHostedService(
            IServiceProvider serviceProvider,
            ILogger<DataSeederHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            _logger.LogInformation("Starting database seeding...");

            await context.Database.MigrateAsync(cancellationToken);

            await SeedUsersAsync(context);
            await SeedProductsAsync(context);

            _logger.LogInformation("Database seeding completed.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        // Seed Users
        private static async Task SeedUsersAsync(AppDbContext context)
        {
            if (await context.Users.AnyAsync()) return;

            var users = new List<User>
            {
                new User(
                    email: "admin@codeleap.com",
                    passwordHash: SecurityHelper.HashPassword("Admin123!")
                ),
                new User(
                    email: "user@codeleap.com",
                    passwordHash: SecurityHelper.HashPassword("User123!")
                )
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();
        }

        // Seed Products
        private static async Task SeedProductsAsync(AppDbContext context)
        {
            if (await context.Products.AnyAsync()) return;

            var products = new List<Product>
            {
                new Product("Keyboard", "Desc", _imgTest),
                new Product("Mouse", "Desc", _imgTest),
                new Product("Monitor", "Desc", _imgTest),
                new Product("Laptop", "Desc", _imgTest),
                new Product("Headphone", "Desc", _imgTest)
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }
    }
}