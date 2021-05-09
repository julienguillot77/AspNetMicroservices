using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            if (!orderContext.Orders.Any())
            {
                orderContext.Orders.AddRange(GetPreconfiguredOrders());
                await orderContext.SaveChangesAsync();
                logger.LogInformation($"Seed database associated with context {nameof(OrderContext)}.");
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders() =>
            new List<Order>
            {
                new Order() {UserName = "julien", FirstName = "Julien", LastName = "Guillot", EmailAddress = "julienguillot77@gmail.com", AddressLine = "Akita", Country = "Japan", TotalPrice = 350 }
            };
    }
}