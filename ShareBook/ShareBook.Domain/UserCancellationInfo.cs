using System;

namespace ShareBook.Domain {
    public class UserCancellationInfo {
        protected UserCancellationInfo() {
            SetCancellationUserDate();
        }

        public UserCancellationInfo(Guid userId, string reason) {
            SetUserId(userId);
            SetReason(reason);
            SetCancellationUserDate();
        }

        public Guid UserId { get; private set; }
        public virtual User User { get; private set; }
        public DateTime CancellationUserDate { get; private set; }
        public string Reason { get; private set; }

        private void SetUserId(Guid userId) {
            UserId = userId;
        }

        private void SetReason(string reason) {
            if (string.IsNullOrEmpty(reason)) return;

            Reason = reason;
        }

        private void SetCancellationUserDate() {
            CancellationUserDate = DateTime.Today;
        }
    }
}