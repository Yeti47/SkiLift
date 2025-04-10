using SkiLift;
using SkiLift.Samples.MinimalApi.Models;
using SkiLift.Samples.MinimalApi.Repositories;

namespace SkiLift.Samples.MinimalApi.Commands;

public record CreateOrderCommand : ICommand<string>
{
    public string CustomerName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
}

public class CreateOrderHandler(IOrderRepository orderRepository) : ICommandHandler<CreateOrderCommand, string>
{
    public async Task<string> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            CustomerName = request.CustomerName,
            TotalAmount = request.TotalAmount
        };

        await orderRepository.AddAsync(order);
        
        return order.Id;
    }
}