using System;
using System.Collections.Generic;
using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;

namespace ShareBook.Domain
{
    public class Job : BaseEntity
    {
        // O nome do job DEVE ser igual ao nome da classe em "6 - Jobs"
        public string Name { get; set; }
        public string Description { get; set; }
        public Interval Interval { get; set; }
        public bool Active { get; set; }
        public virtual ICollection<BookUser> JobHistory { get; set; }
    }

    public class JobHistory : BaseEntity
    {
        public virtual Job Job { get; set; }
        public bool IsSuccess { get; set; }

        // Alguns jobs precisam saber de onde terminou o ultimo ciclo pra continuar daí.
        public string LastResult { get; set; }
        public string Details { get; set; }

        // Precisamos monitorar a duração dos nossos jobs, levando em consideração
        // que estamos numa hospedagem compartilhada e nosso limite é de 300 segundos.
        public int TimeSpentSeconds { get; set; }
    }
}
