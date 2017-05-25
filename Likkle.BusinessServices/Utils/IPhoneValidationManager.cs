using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Likkle.BusinessServices.Utils
{
    public interface IPhoneValidationManager
    {
        bool PhoneNumberIsValid(string phoneNumber);
    }
}
