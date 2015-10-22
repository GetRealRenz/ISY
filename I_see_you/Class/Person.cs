using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I_see_you.Class
{
   public class Person_marks
    {
       public string Login;
       public string FName;
       public string Lname;
       public string age;
       public string sex;
       public string Email;
       public string address;
       public string cordiantes;
    }

   public class Persone
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
