﻿using System;

namespace ShareBook.Domain.Common
{
    public abstract class BaseEntity : IIdProperty
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime? CreationDate { get; set; } = DateTime.UtcNow;
    }
}
