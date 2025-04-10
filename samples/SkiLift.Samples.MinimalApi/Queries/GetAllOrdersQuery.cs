using SkiLift;
using SkiLift.Samples.MinimalApi.Models;
using SkiLift.Samples.MinimalApi.Repositories;

namespace SkiLift.Samples.MinimalApi.Queries;

public class GetAllOrdersQuery : IQuery<IEnumerable<Order>>
{
}

public class GetAllOrdersHandler(IOrderRepository orderRepository) : IQueryHandler<GetAllOrdersQuery, IEnumerable<Order>>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    public Task<IEnumerable<Order>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        return _orderRepository.GetAllAsync();
    }
}