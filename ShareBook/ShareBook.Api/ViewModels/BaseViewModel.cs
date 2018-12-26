using Newtonsoft.Json;
using ShareBook.Domain.Common;
using System;

namespace ShareBook.Api.ViewModels
{
    public abstract class BaseViewModel : IIdProperty
    {
        [JsonIgnore]
        public Guid Id { get; set; }
    }
}
