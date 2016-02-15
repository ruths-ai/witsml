﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using PDS.Framework;
using PDS.Witsml.Studio.ViewModels;

namespace PDS.Witsml.Studio
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        public IContainer Container { get; private set; }

        protected override void Configure()
        {
            var catalog = new AggregateCatalog
            (
                AssemblySource.Instance
                    .Select(x => new AssemblyCatalog(x))
            );

            Container = ContainerFactory.Create(catalog);

            Container.Register<IWindowManager>(new WindowManager());
            Container.Register<IEventAggregator>(new EventAggregator());
        }

        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key)
                ? Container.Resolve(service)
                : Container.Resolve(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return Container.ResolveAll(service);
        }

        protected override void BuildUp(object instance)
        {
            Container.BuildUp(instance);
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var path = Path.Combine(Environment.CurrentDirectory, "Plugins");

            return new[] { GetType().Assembly }
                .Union(Directory.GetFiles(path, "*.dll")
                .Select(x => Assembly.LoadFrom(x)));
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShellViewModel>();
        }
    }
}