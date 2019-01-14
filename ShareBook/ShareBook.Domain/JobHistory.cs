using System;
using System.Collections.Generic;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;

namespace ShareBook.Domain
{
    public class JobHistory : BaseEntity
    {
        public string JobName { get; set; }
        public bool IsSuccess { get; set; }

        // Alguns jobs precisam saber de onde terminou o ultimo ciclo pra continuar a partir daí.
        // permitindo dividir um GRANDE PROCESSAMENTO em vários PEQUENOS.
        public string LastResult { get; set; }
        public string Details { get; set; }

        // Precisamos monitorar a duração dos nossos jobs, levando em consideração
        // que estamos numa hospedagem compartilhada e nosso limite é de 300 segundos.
        public double TimeSpentSeconds { get; set; }
    }
}
