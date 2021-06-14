using System;

namespace ShareBook.Api.ViewModels {
    public class AccessHistoryVM : BaseViewModel {
        public DateTime VisitingDay { get; set; }
        public string VisitorName { get; set; }
        public string Profile { get; set; }
    }
}