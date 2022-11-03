using ShareBook.Domain.Common;

namespace ShareBook.Service.Recaptcha
{
    public class RecaptchaService : IRecaptchaService
    {
        // TODO: Fazer a validação real do "RecaptchaReactive" (https://developers.google.com/recaptcha/docs/verify)
        public Result SimpleValidationRecaptcha(string recaptchaReactive)
        {
            Result result = new Result();
            if (string.IsNullOrWhiteSpace(recaptchaReactive) || recaptchaReactive.Length <= 100)
                result.Messages.Add("RecaptchaReactive está inválido!");

            return result;
        }
    }
}
