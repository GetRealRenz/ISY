using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISY.Domain.Entities
{
  public class Profile:IDbEntity
    {

      public int Id { get; set; }
      public string Login { get; set; }
      public string Password { get; set; }
      public string Name { get; set; }
      public string Surname { get; set; }
      public string Email { get; set; }
      public string Gender { get; set; }
      public string Age { get; set; }
      public byte[] ImageData { get; set; }
    }
}
