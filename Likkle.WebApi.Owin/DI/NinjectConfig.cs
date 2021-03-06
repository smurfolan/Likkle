﻿using System;
using System.Reflection;
using AutoMapper;
using Likkle.BusinessServices;
using Likkle.BusinessServices.Utils;
using Likkle.DataModel;
using Likkle.DataModel.UnitOfWork;
using Likkle.WebApi.Owin.Helpers;
using Ninject;

namespace Likkle.WebApi.Owin.DI
{
    public static class NinjectConfig
    {
        public static StandardKernel Kernel { get; set; }

        public static Lazy<IKernel> CreateKernel = new Lazy<IKernel>(() =>
        {
            StandardKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            RegisterServices(kernel);

            Kernel = kernel;

            return kernel;
        });

        private static void RegisterServices(KernelBase kernel)
        {
            // TODO - put in registrations here...

            
            kernel.Bind<ILikkleApiLogger>().To<LikkleApiLogger>().InSingletonScope();

            kernel.Bind<ILikkleDbContext>().To<LikkleDbContext>();
            kernel.Bind<ILikkleUoW>().To<LikkleUoW>();

            kernel.Bind<IAreaService>().To<AreaService>();
            kernel.Bind<IGroupService>().To<GroupService>();
            kernel.Bind<IUserService>().To<UserService>();
            kernel.Bind<ISubscriptionService>().To<SubscriptionService>();
            kernel.Bind<IAccelometerAlgorithmHelperService>().To<AccelometerAlgorithmHelperService>();
            kernel.Bind<ISubscriptionSettingsService>().To<SubscriptionSettingsService>();

            kernel.Bind<IConfigurationWrapper>().To<ConfigurationWrapper>();
            kernel.Bind<IGeoCodingManager>().To<GeoCodingManager>();
            kernel.Bind<IPhoneValidationManager>().To<PhoneValidationManager>();

            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.AddProfile<EntitiesMappingProfile>();
            });
            kernel.Bind<IConfigurationProvider>().ToConstant(mapperConfiguration);
        }
    }
}