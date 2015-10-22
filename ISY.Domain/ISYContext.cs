using ISY.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISY.Domain
{
    public class ISYContext:DbContext
    {
        public ISYContext():base("IsyBb")
        {

        }
        public DbSet<Profile> Profile { get; set; }
    }
}
