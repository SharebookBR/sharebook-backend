using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShareBook.Service;

/// <summary>
/// Reúne o texto legível e os campos estruturados de uma mensagem de bounce.
/// DSNs RFC 3464 guardam o destinatário e o status em message/delivery-status,
/// que não aparece em <see cref="MimeMessage.TextBody"/>.
/// </summary>
public static class BounceMessageContentExtractor
{
    public static string Extract(MimeMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        var sections = new List<string>();
        AppendMessage(message, sections);

        return string.Join(Environment.NewLine, sections.Where(section => !string.IsNullOrWhiteSpace(section)));
    }

    private static void AppendMessage(MimeMessage message, ICollection<string> sections)
    {
        foreach (var part in message.BodyParts)
        {
            switch (part)
            {
                case TextPart textPart:
                    sections.Add(textPart.Text);
                    break;
                case MessageDeliveryStatus deliveryStatus:
                    foreach (var group in deliveryStatus.StatusGroups)
                    foreach (var header in group)
                        sections.Add($"{header.Field}: {header.Value}");
                    break;
                case MessagePart { Message: not null } messagePart:
                    AppendMessage(messagePart.Message, sections);
                    break;
            }
        }
    }
}
