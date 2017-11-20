using System.Threading.Tasks;

namespace Likkle.BusinessServices
{
    public interface IMailService
    {
        Task SendEmailForThrownException(string recipient, string exceptionBody);
    }
}
