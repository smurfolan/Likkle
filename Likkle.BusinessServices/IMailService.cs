namespace Likkle.BusinessServices
{
    public interface IMailService
    {
        void ReportExceptionOnEmail(string recipient, string exceptionBody);
    }
}
