using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Shared.Services
{
    public class ModelMapper : IModelMapper
    {
        private readonly IServiceProvider _serviceProvider;

        public ModelMapper(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task<T> Map<T>(object source) where T : class
        {
            var sourceType = source.GetType();
            var destinationType = typeof(T);

            Type[] typeArgs = { sourceType, destinationType };
            var mapperType = typeof(IMapper<,>).MakeGenericType(typeArgs);

            var mapper = _serviceProvider.GetService(mapperType);
            
            if(mapper == null)
            {
                throw new InvalidOperationException($"Unable to locate implementation of IMapper<{sourceType.Name},{destinationType.Name}>");
            }

            var mapMethod = mapper.GetType().GetMethod(nameof(IMapper<T, T>.Map));
            var result = mapMethod.Invoke(mapper, new[] { source });

            return (Task<T>)result;
        }
    }
}
