using Microsoft.Extensions.Configuration;
using Rollbar;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.AwsSqs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

public class MailSender : GenericJob, IJob
{
    private readonly IEmailService _emailService;
    private readonly MailSenderHighPriorityQueue _sqsHighPriority;
    private readonly MailSenderLowPriorityQueue _sqsLowPriority;
    private readonly  IConfiguration _configuration;
    private string _lastQueue;
    private IList<string> _log;

    public MailSender(
        IJobHistoryRepository jobHistoryRepo,
        IEmailService emailService,
        MailSenderLowPriorityQueue sqsLowPriority,
        MailSenderHighPriorityQueue sqsHighPriority,
        IConfiguration configuration) : base(jobHistoryRepo)
    {

        JobName = "MailSender";
        Description = @"Esse worker é o responsável por enviar emails. Por ter alta coesão e baixo acoplamento, 
                        a gente consegue implementar rate limit global da nossa aplicação.";
        Interval = Interval.Each5Minutes;
        Active = true;
        BestTimeToExecute = null;

        _emailService = emailService;
        _sqsLowPriority = sqsLowPriority;
        _sqsHighPriority = sqsHighPriority;
        _configuration = configuration;
        _log = new List<string>();       
    }

    public override async Task<JobHistory> WorkAsync()
    {
        AwsSqsEnabledValidation();
        
        var maxEmailsToSend = GetMaxEmailsToSend();
        var totalEmailsSent = 0;
        
        _log = new List<string>() { $"Iniciando o MailSender. maxEmailsToSend = {maxEmailsToSend}" };

        while(totalEmailsSent < maxEmailsToSend) {
            var sqsMessage = await GetSqsMessageAsync();

            // não tem mais emails pra enviar
            if(sqsMessage == null) break;

            totalEmailsSent += await SendEmailAsync(sqsMessage);
            
            await DeleteSqsMessageAsync(sqsMessage);
        }

        _log.Add($"Finalizando o MailSender. totalEmailsSent = {totalEmailsSent}");
        
        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = String.Join("\n", _log)
        };
    }

    private async Task<int> SendEmailAsync(SharebookMessage<MailSenderbody> sqsMessage)
    {
        var destinations = sqsMessage.Body.Destinations;
        var subject = sqsMessage.Body.Subject;
        var bodyHtml = sqsMessage.Body.BodyHTML;
        var copyAdmins = sqsMessage.Body.CopyAdmins;

        var emails = destinations.Select(x => x.Email).ToList();
        var bounces = await _emailService.GetBounces(emails);

        foreach (var destination in destinations)
        {
            try {
                if (_emailService.IsBounce(destination.Email, bounces))
                {
                    _log.Add($"Não enviei email para {destination.Email} porque está em estado de BOUNCE.");
                    continue;
                }

                string firstName = GetFirstName(destination.Name);
                var bodyHtml2 = bodyHtml.Replace("{name}", firstName, StringComparison.OrdinalIgnoreCase);
                await _emailService.SendSmtp(destination.Email, destination.Name, bodyHtml2, subject, copyAdmins);

                _log.Add($"Enviei um email com SUCESSO para {destination.Email}.");
            }
            catch(Exception ex) {
                RollbarLocator.RollbarInstance.Error(ex);
                _log.Add($"Ocorreu um ERRO ao enviar email para {destination.Email}. Erro: {ex.Message}");
            }

            // freio lógico
            Thread.Sleep(100);
        }
        
        return destinations.Count;
    }

    private static string GetFirstName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
            return string.Empty;

        string[] nameParts = fullName.Split(' ');
        return nameParts[0];
    }

    private async Task<SharebookMessage<MailSenderbody>> GetSqsMessageAsync()
    {
        var sqsMessage = await _sqsHighPriority.GetMessage();
        _lastQueue = "HighPriority";
    
        if(sqsMessage == null) {
            sqsMessage = await _sqsLowPriority.GetMessage();
            _lastQueue = "LowPriority";
        }

        if(sqsMessage != null)
            _log.Add($"Encontrei uma mensagem na fila. _lastQueue = {_lastQueue}");
        else
            _log.Add($"Não encontrei nenhuma mensagem nas filas de origem.");

        return sqsMessage;
    }

    private async Task DeleteSqsMessageAsync(SharebookMessage<MailSenderbody> sqsMessage)
    {
        _log.Add($"Removendo a mensagem da fila. _lastQueue = {_lastQueue}");
        
        if(_lastQueue == "HighPriority")
            await _sqsHighPriority.DeleteMessage(sqsMessage.ReceiptHandle);
        else
            await _sqsLowPriority.DeleteMessage(sqsMessage.ReceiptHandle);
    }

    private int GetMaxEmailsToSend()
    {
        var maxEmailsPerHour = int.Parse(_configuration["EmailSettings:MaxEmailsPerHour"]);
        return maxEmailsPerHour / 12;
    }

    private void AwsSqsEnabledValidation()
    {
        var awsSqsEnabled = bool.Parse(_configuration["AwsSqsSettings:IsActive"]);
        if(!awsSqsEnabled) throw new AwsSqsDisabledException("Serviço aws sqs está desabilitado no appsettings.");
    }
}
