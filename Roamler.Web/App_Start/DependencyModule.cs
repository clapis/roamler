﻿using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Owin;
using Roamler.Data;
using Roamler.Data.EntityFramework;
using Roamler.Services;
using Roamler.SpatialSearch;
using Roamler.SpatialSearch.QuadTree;

namespace Roamler.Web
{
    public static class DependencyModule
    {
        public static void ConfigureDependencies(this IAppBuilder app, HttpConfiguration config)
        {
            var container = BuildContainer();

            config.DependencyResolver = new AutofacWebApiDependencyResolver(container); ;

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);

        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            // db
            builder.RegisterType<RoamlerDbContext>().InstancePerRequest();

            // repositories
            builder.RegisterType<LocationRepository>().As<ILocationRepository>();

            // services
            builder.RegisterType<LocationSearchService>().As<ILocationSearchService>();

            // controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // search engine
            builder.Register(c => new SpatialIndexBuilder(new RoamlerDbContext()))
                .As<ISpatialIndexBuilder>();
            builder.Register(c => c.Resolve<ISpatialIndexBuilder>().BuildIndex())
                .As<ISpatialIndex>()
                .SingleInstance();

            return builder.Build();
        }
    }
}