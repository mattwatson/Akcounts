using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using Akcounts.NewUI.Framework;
using Akcounts.NewUI.Utils;
using Caliburn.Micro;

namespace Akcounts.NewUI.MainWindow
{
    public class AkcountsBootstrapper : Bootstrapper<IMainWindow>
    {
        private CompositionContainer _container;

        static AkcountsBootstrapper()
        {
            LogManager.GetLog = type => new Log4netLogger(type);
        }

        protected override void Configure()
        {
            var assemblyCatalog = AssemblySource.Instance.Select(x => new AssemblyCatalog(x));
            _container = new CompositionContainer(new AggregateCatalog(assemblyCatalog));
            
            var batch = new CompositionBatch();
            batch.AddExportedValue<IWindowManager>(new WindowManager());
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            
            //TODO add setup of viewmodels in the following style
            //batch.AddExportedValue<Func<IMessageBox>>(() => container.GetExportedValue<IMessageBox>());
            //batch.AddExportedValue<Func<CustomerViewModel>>(() => container.GetExportedValue<CustomerViewModel>());
            //batch.AddExportedValue<Func<OrderViewModel>>(() => container.GetExportedValue<OrderViewModel>());
            
            batch.AddExportedValue(_container);

            _container.Compose(batch);
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var export = _container.GetExportedValues<object>(contract).FirstOrDefault();

            if (export == null)
            {
                throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
            }
            
            return export;
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            _container.SatisfyImportsOnce(instance);
        }

        //TODO could put close confirmation logic in here. See HellowScreens sample for an example.
    }
}