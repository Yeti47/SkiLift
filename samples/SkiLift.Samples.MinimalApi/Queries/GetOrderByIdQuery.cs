using SkiLift;
using SkiLift.Samples.MinimalApi.Models;
using SkiLift.Samples.MinimalApi.Repositories;

namespace SkiLift.Samples.MinimalApi.Queries;

public record GetOrderByIdQuery : IQuery<Order?>
{
    public required string Id { get; set; }
}

public class GetOrderByIdHandler(IOrderRepository orderRepository) : IQueryHandler<GetOrderByIdQuery, Order?>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    public Task<Order?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return _orderRepository.GetByIdAsync(request.Id);
    }
}