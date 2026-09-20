using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.Muambator;

public class MuambatorDTO
{
    public string Status { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public IList<dynamic> Results { get; set; } = new List<dynamic>();
}
