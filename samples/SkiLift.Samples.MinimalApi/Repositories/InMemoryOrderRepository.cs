using SkiLift.Samples.MinimalApi.Models;

namespace SkiLift.Samples.MinimalApi.Repositories;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly Dictionary<string, Order> _orders = [];

    public Task<Order?> GetByIdAsync(string id)
    {
        _orders.TryGetValue(id, out var order);
        return Task.FromResult(order);
    }

    public Task<IEnumerable<Order>> GetAllAsync()
    {
        return Task.FromResult(_orders.Values.AsEnumerable());
    }

    public Task AddAsync(Order order)
    {
        if (string.IsNullOrEmpty(order.Id))
        {
            order.Id = Guid.NewGuid().ToString("N");
        }

        _orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _orders.Remove(id);
        return Task.CompletedTask;
    }
}