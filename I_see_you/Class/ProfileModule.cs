using ISY.Domain.Abstract;
using ISY.Domain.Concrete;
using ISY.Domain.Entities;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I_see_you.Class
{
    class ProfileModule:NinjectModule
    {
        public override void Load()
        {
            Bind<IRepositoryBase<Profile>>().To<RepositoryBase<Profile>>();
        }
    }
}
