using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareBook.Domain 
{
    public class AccessHistory : BaseEntity 
    {
        protected AccessHistory() { }

        public AccessHistory(string visitorName, VisitorProfile profile) {
            ChangeVisitorName(visitorName);
            ChangeProfile(profile);
        }

        public AccessHistory(Guid id, string visitorName, VisitorProfile profile) : this(visitorName, profile) {
            UserId = id;
        }

        public Guid? UserId { get; private set; }
        [ForeignKey("UserId")]
        public User User { get; private set; } //Visitante do perfil
        public string VisitorName { get; private set; }
        public VisitorProfile Profile { get; private set; }

        public void ChangeVisitorName(string name) 
        {
            if (string.IsNullOrEmpty(name)) return;

            VisitorName = name;
        }

        public void ChangeProfile(VisitorProfile newProfile) 
        {
            if (Profile.Equals(newProfile)) return;

            Profile = newProfile;
        }
    }
}