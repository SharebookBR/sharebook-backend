using ShareBook.Domain.Common;

namespace ShareBook.Service.Recaptcha
{
    public class RecaptchaService : IRecaptchaService
    {
        // TODO: Fazer a validação real do "RecaptchaReactive" (https://developers.google.com/recaptcha/docs/verify)
        public Result SimpleValidationRecaptcha(string recaptcha)
        {
            Result result = new Result();
            if (string.IsNullOrWhiteSpace(recaptcha) || recaptcha.Length <= 100)
                result.Messages.Add("RecaptchaReactive está inválido!");

            return result;
        }
    }
}
