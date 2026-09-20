using AutoMapper.Configuration.Conventions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.AwsSqs.Dto;

public class MailSenderbody
{
    public string Subject { get; set; } = string.Empty;
    public string BodyHTML { get; set; } = string.Empty;
    public IList<Destination> Destinations { get; set; } = new List<Destination>();
    public bool CopyAdmins { get; set; } = false;
}

public class Destination
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}