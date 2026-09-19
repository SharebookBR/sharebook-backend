using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service.Muambator;

public interface IMuambatorService
{
    Task<dynamic> AddPackageToTrackerAsync(Book book, User winner, string packageNumber);

    Task<MuambatorDTO> RemovePackageToTrackerAsync(string packageNumber);
}
