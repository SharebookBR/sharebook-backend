using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Service.Muambator
{
    public interface IMuambatorService
    {
        Task<dynamic> AddPackageToTrackerAsync(Book book, User winner, string packageNumber);

        Task<MuambatorDTO> RemovePackageToTrackerAsync(string packageNumber);
    }
}
