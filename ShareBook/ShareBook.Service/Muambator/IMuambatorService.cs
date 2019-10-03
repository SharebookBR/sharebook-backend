using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Service.Muambator
{
    public interface IMuambatorService
    {
        Task<MuambatorDTO> AddPackageToTrackerAsync(string emailReceiver, string packageNumber);

        Task<MuambatorDTO> RemovePackageToTrackerAsync(string packageNumber);
    }
}
