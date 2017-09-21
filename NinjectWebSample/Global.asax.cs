using System.Data.SqlClient;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Ninject;
using NServiceBus;
//using NServiceBus.Persistence.Sql;

namespace NinjectWebSample
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            IKernel kernel = new StandardKernel();

            kernel.Bind<IEndpointInstance>().ToMethod(x => EndpointBuilder.Build(kernel)).InSingletonScope();

            var endpoint = kernel.Get<IEndpointInstance>();

            endpoint.Stop().Wait();
        }
    }

    class EndpointBuilder
    {
        public static IEndpointInstance Build(IKernel kernel)
        {
            var connectionString = @"Server=.\SQLEXPRESS;Database=Test;User=ServiceControl;Password=P@ssw0rd";

            // Set up Logging

            var endpointConfiguration = new EndpointConfiguration("EndpointName");

            // Set up Serialization

            endpointConfiguration.SendFailedMessagesTo("error");

            // Set up Recoverability

            //var transport = endpointConfiguration.UseTransport<LearningTransport>();
            //transport.StorageDirectory(@"C:\Temp\FooBarNabajar");

            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(connectionString);

            // Configure Routing


            //var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            //persistence.SqlVariant(SqlVariant.MsSqlServer);
            //persistence.ConnectionBuilder(connectionBuilder: () => new SqlConnection(connectionString));
            //persistence.SubscriptionSettings().DisableCache();

            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            SetupIoC(endpointConfiguration, kernel);

            // Set up Unit of Work Provider Adapter

            return Endpoint.Start(endpointConfiguration).Result;
        }

        private static void SetupIoC(EndpointConfiguration endpointConfiguration, IKernel kernel)
        {
            if (kernel != null)
            {
                endpointConfiguration.UseContainer<NinjectBuilder>(
                    customizations: cust => { cust.ExistingKernel(kernel); });
            }
        }
    }
}
