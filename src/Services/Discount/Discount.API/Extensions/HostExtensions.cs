using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            int retryForAvailability = retry.GetValueOrDefault();

            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var configuration = services.GetRequiredService<IConfiguration>();
            var logger = services.GetRequiredService<ILogger<TContext>>();

            try
            {
                logger.LogInformation("Migrating Postgresql database...");

                using var connection = new NpgsqlConnection(configuration["DatabaseSettings:ConnectionString"]);

                connection.Open();

                using var command = new NpgsqlCommand
                {
                    Connection = connection
                };

                command.CommandText = @"CREATE TABLE IF NOT EXISTS Coupon(Id SERIAL PRIMARY KEY, 
                                                            ProductName VARCHAR(24) NOT NULL,
                                                            Description TEXT,
                                                            Amount INT)";
                command.ExecuteNonQuery();

                command.CommandText = @"SELECT COUNT(*) FROM Coupon;";

                int count = Convert.ToInt32(command.ExecuteScalar() ?? "0");

                if (count == 0)
                {
                    command.CommandText = @"INSERT INTO Coupon(ProductName, Description, Amount) VALUES 
                                        ('IPhone X', 'IPhone Discount', 150),
                                        ('Samsung 10', 'Samsung Discount', 100);";

                    command.ExecuteNonQuery();

                    logger.LogInformation("Postgresql database migrated.");
                }
                else
                {
                    logger.LogInformation("Postgresql database already migrated. Nothing to do.");
                }
            }
            catch (NpgsqlException ex)
            {
                logger.LogError(ex, "An error occurred while migrating the postgresql database");

                if (retryForAvailability < 50)
                {
                    retryForAvailability++;
                    Thread.Sleep(2000);
                    MigrateDatabase<TContext>(host, retryForAvailability);
                }
            }

            return host;
        }
    }
}