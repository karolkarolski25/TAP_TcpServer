using Microsoft.Extensions.DependencyInjection;
using Storage.DAL.Servies;
using StorageLibrary.Context;

namespace Storage.DAL
{
    public static class Startup
    {
        public static void RegisterDALDependencies(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IStorageService, StorageService>();
            serviceCollection.AddDbContext<IUserDataContext, UserDataContext>();
        }
    }
}
