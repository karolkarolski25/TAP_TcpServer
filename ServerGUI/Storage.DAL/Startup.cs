using Microsoft.Extensions.DependencyInjection;
using StorageLibrary.Context;

namespace Storage.DAL
{
    public static class Startup
    {
        public static void RegisterDALDependiences(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IStorageService, StorageService>();
            serviceCollection.AddDbContext<IUserDataContext, UserDataContext>();
        }
    }
}
