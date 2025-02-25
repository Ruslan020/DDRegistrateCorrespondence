using Autofac;
using DocsVision.BackOffice.WebClient.Services;
using DocsVision.WebClient.Extensibility;
using DocsVision.WebClient.Helpers;
using DocsVision.WebClientLibrary.ObjectModel.Services.BindingConverters;
using DocsVision.WebClientLibrary.ObjectModel.Services.BindingResolvers;
using DocsVision.WebClientLibrary.ObjectModel.Services.LayoutModel;
using RegistrateCorrespondenceServerExtension.Controllers;
using RegistrateCorrespondenceServerExtension.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace RegistrateCorrespondenceServerExtension
{
    /// <summary>
    /// Задаёт описание расширения для WebClient, которое задано в текущей сборке
    /// </summary>
    public class RegistrateCorrespondenceServerExtension : WebClientExtension
    {
        /// <summary>
        /// Создаёт новый экземпляр <see cref="RegistrateCorrespondenceServerExtension" />
        /// </summary>
        /// <param name="serviceProvider">Сервис-провайдер</param>
        public RegistrateCorrespondenceServerExtension(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }

        /// <summary>
        /// Получить название расширения
        /// </summary>
        public override string ExtensionName
        {
            get { return Assembly.GetAssembly(typeof(RegistrateCorrespondenceServerExtension)).GetName().Name; }
        }

        /// <summary>
        /// Получить версию расширения
        /// </summary>
        public override Version ExtensionVersion
        {
            get { return new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion); }
        }

        #region WebClientExtension Overrides
        /// <summary>
        /// Gets registered service activators
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <returns>service type/activator mappings</returns>
        [Obsolete]
        protected override Dictionary<Type, Func<object>> GetServiceActivators(IServiceProvider serviceProvider)
        {
            return new Dictionary<Type, Func<object>>
            {
                {typeof(INotifyService), () => new NotifyService(serviceProvider) },
                {typeof(ISystemService), () => new SystemService(serviceProvider) },
                {typeof(IGetAdminsSecretaryIDService), () => new GetAdminsSecretaryIDService(serviceProvider) },
                {typeof(INewConstrolsMeth), () => new NewConstrolsMeth(serviceProvider) }
            };
        }

        /// <summary>
        /// Gets registered MVC controller activators
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <returns>WebApi controller type/activator mappings</returns>
        [Obsolete]
        protected override Dictionary<Type, Func<IController>> GetControllerActivators(IServiceProvider serviceProvider)
        {
            return new Dictionary<Type, Func<IController>>
            {
                {typeof(RegCorController), () => new RegCorController(serviceProvider) }
            };
        }

        [Obsolete]
        protected override WebClientNavigatorExtension GetNavigatorExtension()
        {
            var navigatorExtensionInitInfo = new WebClientNavigatorExtensionInitInfo
            {
                ExtensionName = ExtensionName,
                ExtensionVersion = ExtensionVersion
            };
            return new WebClientNavigatorExtension(navigatorExtensionInitInfo);
        }

        [Obsolete]
        public override string Namespace
        {
            get { return Constants.Namespace; }
        }

        /// <summary>
        /// Gets registered WebApi controller activators
        /// </summary>
        /// <param name="serviceProvider">service provider</param>
        /// <returns>WebApi controller type/activator mappings</returns>
        [Obsolete]
        protected override Dictionary<Type, Func<IHttpController>> GetApiControllerActivators(IServiceProvider serviceProvider)
        {
            return new Dictionary<Type, Func<IHttpController>>
            {
            };
        }

        /// <summary>
        /// Gets binding converters
        /// </summary>
        /// <returns>a list of binding converters</returns>
        [Obsolete]
        protected override List<IBindingConverter> GetBindingConverters()
        {
            return new List<IBindingConverter>
            {



            };
        }

        /// <summary>
        /// Gets binding resolvers
        /// </summary>
        /// <returns>a list of binding resolvers</returns>
        [Obsolete]
        protected override List<IBindingResolver> GetBindingResolvers()
        {
            return new List<IBindingResolver>
            {



            };
        }

        /// <summary>
        /// Gets control resolvers
        /// </summary>
        /// <returns>a list of control resolvers</returns>
        [Obsolete]
        protected override List<IControlResolver> GetControlResolvers()
        {
            return new List<IControlResolver>
            {



            };
        }

        /// <summary>
        /// Gets property resolvers
        /// </summary>
        /// <returns>a list of property resolvers</returns>
        [Obsolete]
        protected override List<IPropertyResolver> GetPropertyResolvers()
        {
            return new List<IPropertyResolver>
            {



            };
        }

        /// <summary>
        /// Gets card factories
        /// </summary>
        /// <returns>a dictionaty of card factories</returns>
        [Obsolete]
        protected override Dictionary<Guid, Func<ICardFactory>> GetCardFactories()
        {
            return new Dictionary<Guid, Func<ICardFactory>>
            {



            };
        }

        /// <summary>
        /// Регистрация типов в IoC контейнере
        /// </summary>
        /// <param name="containerBuilder"></param>
        public override void InitializeContainer(ContainerBuilder containerBuilder)
        {
            // Теперь регистрация сервисов и других объектов ВК осуществляется в едином методе - InitializeContainer, 
            // примеры регистрации различных типов ВК представлены ниже
            // containerBuilder.RegisterType<YourService>().As<IYourService>().SingleInstance();
            // containerBuilder.RegisterOrderedType<YourBindingConverterType, IBindingConverter>();
            // containerBuilder.RegisterOrderedType<YourBindingResolverType, IBindingResolver>();            
            // containerBuilder.RegisterOrderedType<YourControlResolverType, IControlResolver>();
            // containerBuilder.RegisterOrderedType<YourPropertyResolverType, IPropertyResolver>();  
            // containerBuilder.RegisterType<YourCardLifeCycle>().Keyed<ICardLifeCycle>(CardTypeID).SingleInstance();
            // containerBuilder.RegisterType<YourRowLifeCycle>().Keyed<IRowLifeCycle>(SectionID).SingleInstance(); 
        }

        /// <summary>
        /// Gets resource managers for layout extension
        /// </summary>
        /// <returns></returns>
        protected override List<ResourceManager> GetLayoutExtensionResourceManagers()
        {
            return new List<ResourceManager>
            {

            };
        }

        #endregion
    }
}