using ISY.Domain.Entities;
using ISY.Domain.Abstract;
using ISY.Domain.Concrete;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISY_SERV
{
    class ProfileModule:NinjectModule
    {
        public override void Load()
        {
            Bind<IRepositoryBase<Profile>>().To<RepositoryBase<Profile>>();
        }
    }
}
