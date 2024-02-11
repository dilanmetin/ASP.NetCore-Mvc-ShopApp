using Microsoft.EntityFrameworkCore;
using ShopApp.DataAccess.Concrete.EfCore;
using ShopApp.WebUI.Identity;

namespace ShopApp.WebUI.Extensions
{
    public static class MigrationManager
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using(var scope = host.Services.CreateScope())
            {
                using(var applicationContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>())
                {
                    try
                    {
                        applicationContext.Database.Migrate();
                    }
                    catch(System.Exception) {

                        //loglama
                        throw;
                    }
                }


                using (var shopContext = scope.ServiceProvider.GetRequiredService<ShopContext>())
                {
                    try
                    {
                        shopContext.Database.Migrate();
                    }
                    catch (System.Exception)
                    {

                        //loglama
                        throw;
                    }
                }
            }
             
            return host;
        }
    }
}
