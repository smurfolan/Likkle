using System;
using System.Reflection;
using Likkle.BusinessServices;
using Likkle.WebApi.Owin.Helpers;
using Ninject;

namespace Likkle.WebApi.Owin.DI
{
    public static class NinjectConfig
    {
        public static Lazy<IKernel> CreateKernel = new Lazy<IKernel>(() =>
        {
            StandardKernel kernel = new StandardKernel();
            kernel.Load(Assembly.GetExecutingAssembly());

            RegisterServices(kernel);

            return kernel;
        });

        private static void RegisterServices(KernelBase kernel)
        {
            // TODO - put in registrations here...

            kernel.Bind<IDataService>().To<DataService>();
            kernel.Bind<ILikkleApiLogger>().To<LikkleApiLogger>().InSingletonScope();
        }
    }
}