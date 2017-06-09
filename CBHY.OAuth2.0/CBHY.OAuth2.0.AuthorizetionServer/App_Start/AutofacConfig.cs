using Autofac;
using Autofac.Integration.Mvc;
using CBHY.OAuth2.AuthorizetionServer.Code;
using CHY.BaseFramework;
using CHY.BaseFramework.DAL;
using CHY.Framework.SqlServer;
using CHY.OAuth2.AuthorizationServer.OAuth2;
using OAuthAuthorizationServer.Code;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Mvc;

namespace CBHY.OAuth2.AuthorizetionServer.App_Start
{
    public class AutofacConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterFilterProvider();

            foreach (var conn in ConfigUtil.GetConnStrings())
            {
                builder.Register(c => new SqlConnection(conn.Value))
                    .Named<IDbConnection>(conn.Key)
                    .InstancePerRequest();
            }
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();

            builder.Register<Func<string, IDbConnection>>(c =>
            {
                var ic = c.Resolve<IComponentContext>();
                return named => ic.ResolveNamed<IDbConnection>(named);
            });


            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerRequest();
            builder.RegisterType<DatabaseKeyNonceStore>().As<DatabaseKeyNonceStore>().InstancePerRequest();
            builder.RegisterType<OAuth2AuthorizationServer>().As<IAuthorizationServerHost>().InstancePerRequest(); ;
            builder.RegisterType<AuthorizationServer>().As<AuthorizationServer>().InstancePerRequest();
         
            //var assemblies = BuildManager.GetReferencedAssemblies()
            //    .Cast<Assembly>()
            //    .Where(a => a.GetTypes().FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IService))) != null)
            //    .ToArray();

            //builder.RegisterAssemblyTypes(assemblies)
            //    .Where(t => t.GetInterfaces().Contains(typeof(IService)))
            //    .AsSelf()
            //    .InstancePerLifetimeScope();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}