using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using ShareBook.Domain;
using ShareBook.Domain.Exceptions;

namespace ShareBook.Service.Muambator
{
  public class MuambatorService : IMuambatorService
  {
    public async Task<dynamic> AddPackageToTrackerAsync(Book book, User winner, string packageNumber)
    {
      var emailWinner = winner.Email;
      var emailDonor = book.User == null ? string.Empty : book.User.Email;
      var emailFacilitator = book.UserFacilitator == null ? string.Empty : book.UserFacilitator.Email;

      var url = $"https://www.muambator.com.br/api/clientes/v1/pacotes/{packageNumber}/?api-token={MuambatorConfigurator.Token}";

      dynamic result;

      try
      {
        var jsonRequest = new
        {
          nome = book.Title,
          categoria = "livro",
          emails = new[] { emailWinner, emailDonor, emailFacilitator }

        };

        string jsonResponse = url.WithHeader("Content-type", "application/json").PostJsonAsync(jsonRequest).ReceiveString().Result;
        result = JsonConvert.DeserializeObject(jsonResponse);

      }
      catch (FlurlHttpTimeoutException)
      {
        throw new ShareBookException("Request timed out.");
      }
      catch (FlurlHttpException ex)
      {
        var error = await ex.GetResponseJsonAsync<MuambatorDTO>();
        throw new ShareBookException(error == null ? ex.Message : error.Message);
      }

      return result;
    }

    public async Task<MuambatorDTO> RemovePackageToTrackerAsync(string packageNumber)
    {
      var url = $"https://www.muambator.com.br/api/clientes/v1/pacotes/{packageNumber}/?api-token={MuambatorConfigurator.Token}";

      MuambatorDTO result = new MuambatorDTO();

      try
      {
        result = await url.DeleteAsync().ReceiveJson();
      }
      catch (FlurlHttpTimeoutException)
      {
        throw new ShareBookException("Request timed out.");
      }
      catch (FlurlHttpException ex)
      {
        var error = await ex.GetResponseJsonAsync<MuambatorDTO>();
        throw new ShareBookException(error == null ? ex.Message : error.Message);
      }

      return result;
    }
  }
}
