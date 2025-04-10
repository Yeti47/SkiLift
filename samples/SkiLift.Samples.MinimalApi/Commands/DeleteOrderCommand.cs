using SkiLift;
using SkiLift.Samples.MinimalApi.Repositories;

namespace SkiLift.Samples.MinimalApi.Commands;

public record DeleteOrderCommand : ICommand
{
    public required string Id { get; init; }
}

public class DeleteOrderCommandHandler(IOrderRepository orderRepository) : ICommandHandler<DeleteOrderCommand>
{
    public async Task Handle(DeleteOrderCommand command, CancellationToken cancellationToken)
    {
        await orderRepository.DeleteAsync(command.Id);
    }
}