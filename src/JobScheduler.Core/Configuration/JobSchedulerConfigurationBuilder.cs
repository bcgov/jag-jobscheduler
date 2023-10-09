using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace JobScheduler.Core.Configuration
{
    /// <summary>
    /// Builder used to add and configure job scheduling services
    /// </summary>
    public class JobSchedulerConfigurationBuilder : IServiceCollection
    {
        private readonly IServiceCollection services;
        /// <inheritdoc/>

        public int Count => services.Count;
        /// <inheritdoc/>

        public bool IsReadOnly => services.IsReadOnly;
        /// <inheritdoc/>

        public ServiceDescriptor this[int index]
        {
            get => services[index];
            set => services[index] = value;
        }

        /// <summary>
        /// Factory method for JobSchedulerConfigurationBuilder
        /// </summary>
        /// <param name="services">The source DI services</param>
        /// <returns>A new builder instance</returns>
        public static JobSchedulerConfigurationBuilder CreateFrom(IServiceCollection services)
        {
            var builder = new JobSchedulerConfigurationBuilder(services);

            return builder;
        }

        private JobSchedulerConfigurationBuilder(IServiceCollection services)
        {
            this.services = services;
        }

        /// <inheritdoc/>

        public int IndexOf(ServiceDescriptor item) => services.IndexOf(item);

        /// <inheritdoc/>
        public void Insert(int index, ServiceDescriptor item) => services.Insert(index, item);

        /// <inheritdoc/>
        public void RemoveAt(int index) => services.RemoveAt(index);

        /// <inheritdoc/>
        public void Add(ServiceDescriptor item) => services.Add(item);

        /// <inheritdoc/>
        public void Clear() => services.Clear();

        /// <inheritdoc/>
        public bool Contains(ServiceDescriptor item) => services.Contains(item);

        /// <inheritdoc/>
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex) => services.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public bool Remove(ServiceDescriptor item) => services.Remove(item);

        /// <inheritdoc/>
        public IEnumerator<ServiceDescriptor> GetEnumerator() => services.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)services).GetEnumerator();
    }
}
