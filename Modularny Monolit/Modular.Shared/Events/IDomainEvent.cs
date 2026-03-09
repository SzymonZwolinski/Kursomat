using System.Threading;
using System.Threading.Tasks;

namespace Modular.Shared.Events
{
    public interface IDomainEvent
    {
    }

    public interface IDomainEventHandler<TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
    }

    public interface IDomainEventDispatcher
    {
        Task DispatchAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default) where TEvent : IDomainEvent;
    }
}
