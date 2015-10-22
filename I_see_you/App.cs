using ISY.Domain.Abstract;
using ISY.Domain.Entities;
using ISY.Domain.Concrete;
using Ninject;
using System;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I_see_you
{
    public partial class App
    {
        private IKernel kernel;

        protected override void OnStartup(System.Windows.StartupEventArgs e)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", System.IO.Directory.GetCurrentDirectory());
            base.OnStartup(e);
            ConfigureKernel();
            ComposeObject();
            Current.MainWindow.Show();
        }

        private void ComposeObject()
        {
            Current.MainWindow = kernel.Get<MainWindow>();
        }

        private void ConfigureKernel()
        {
            kernel = new StandardKernel();

            kernel.Bind<IRepositoryBase<Profile>>().To<RepositoryBase<Profile>>();


        }
    }
}
