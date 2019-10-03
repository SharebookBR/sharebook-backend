using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using ShareBook.Domain.Exceptions;

namespace ShareBook.Service.Muambator
{
    public class MuambatorService : IMuambatorService
    {
        public async Task<MuambatorDTO> AddPackageToTrackerAsync(string emailReceiver, string packageNumber)
        {
            var url = $"https://www.muambator.com.br/api/clientes/v1/pacotes/{packageNumber}/?api-token={MuambatorConfigurator.Token}";

            MuambatorDTO result = new MuambatorDTO();

            try
            {
                result = await url.WithHeader("Content-type", "application/json").PostJsonAsync(new { emails = new[] { emailReceiver } })
                                  .ReceiveJson<MuambatorDTO>();
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
