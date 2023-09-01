using System.Threading.Tasks;

namespace JobScheduler.Core
{
    internal interface IEventBus
    {
        Task Publish<T>(T message);

        Task Subscribe<T, THandler>()
            where THandler : IEventHandler<T>;
    }

    internal interface IEventHandler<in T>
    {
        Task Handle(T evt);
    }
}
