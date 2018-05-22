using System.Collections.Generic;

namespace ShareBook.VM.Common
{
    public class ResultServiceVM
    {
        public List<string> Messages { get; set; } = new List<string>();

        public bool Success => Messages.Count == 0;

        public string SuccessMessage { get; set; }
    }
}