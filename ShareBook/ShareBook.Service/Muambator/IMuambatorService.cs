using System.Threading.Tasks;

namespace ShareBook.Service.Muambator
{
    public interface IMuambatorService
    {
        Task<MuambatorDTO> AddPackageToTrackerAsync(string emailReceiver, string emailFacilitator, string emailDonor, string packageNumber);

        Task<MuambatorDTO> RemovePackageToTrackerAsync(string packageNumber);
    }
}
