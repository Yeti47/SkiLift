using SkiLift.Samples.MinimalApi.Models;

namespace SkiLift.Samples.MinimalApi.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(string id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task AddAsync(Order order);
    Task DeleteAsync(string id);
}