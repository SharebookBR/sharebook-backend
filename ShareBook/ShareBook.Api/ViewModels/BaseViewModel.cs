using Newtonsoft.Json;
using System;

namespace ShareBook.Api.ViewModels
{
    public abstract class BaseViewModel
    {
        [JsonIgnore]
        public Guid Id { get; set; }
    }
}
