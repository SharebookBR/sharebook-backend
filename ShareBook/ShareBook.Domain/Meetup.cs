using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;

namespace ShareBook.Domain;

public class Meetup : BaseEntity
{
    public int SymplaEventId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public string Cover { get; set; }
    public string YoutubeUrl { get; set; }
    public string SymplaEventUrl { get; set; }
    public ICollection<MeetupParticipant> MeetupParticipants { get; set; }
    public bool IsParticipantListSynced { get; set; } = false;
    public bool Active { get; set; } = true;
}