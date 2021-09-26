using System;
using System.Collections.Generic;
using System.Linq;

namespace ShareBook.Domain.DTOs {
    public class UserCancellationDto {
        private readonly IList<string> _cancellationDonationsRequestsList;
        private readonly IList<string> _cancellationsBooksForDonationsList;
        private readonly IList<string> _canceledBooksList;
        public UserCancellationDto() {
            _cancellationDonationsRequestsList = new List<string>();
            _cancellationsBooksForDonationsList = new List<string>();
            _canceledBooksList = new List<string>();
        }
        public Guid UserId { get; private set; }
        public DateTime Date { get; private set; }
        public string Reason { get; private set; }
        public string CancellationDonationsRequests => GetCancellationDonationsRequests();
        public string CancellationsBooksForDonations => GetCancellationsBooksForDonations();
        public string CanceledBooks => GetCanceledBooks();

        public void SetUserId(Guid id) {
            UserId = id;
        }

        public void SetDate(DateTime date) {
            Date = date;
        }

        public void SetReason(string reason) {
            Reason = reason;
        }

        public void AddCanceledDonationRequest(string cancellation) {
            _cancellationDonationsRequestsList.Add($"<li>{cancellation}</li>");
        }

        public void AddCancellationsBooksForDonations(string cancellation) {
            _cancellationsBooksForDonationsList.Add($"<li>{cancellation}</li>");
        }

        public void AddCanceledBooks(string cancellation) {
            _canceledBooksList.Add($"<li>{cancellation}</li>");
        }

        private string GetCancellationDonationsRequests() {
            var s = _cancellationDonationsRequestsList.Aggregate((a, b) => a + Environment.NewLine + b);
            return s;
        }

        private string GetCancellationsBooksForDonations() {
            var s = _cancellationsBooksForDonationsList.Aggregate((a, b) => a + Environment.NewLine + b);
            return s;
        }

        private string GetCanceledBooks() {
            var s = _canceledBooksList.Aggregate((a, b) => a + Environment.NewLine + b);
            return s;
        }
    }
}