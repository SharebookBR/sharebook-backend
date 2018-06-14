using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Repository.Mapping
{
    public class BookUserMap
    {
        public BookUserMap(EntityTypeBuilder<BookUser> entityBuilder)
        {

            entityBuilder
             .HasKey(bu => new { bu.BookId, bu.UserId });

            entityBuilder
                 .HasOne(bu => bu.User)
                .WithMany(b => b.BookUsers)
                .HasForeignKey(bu => bu.BookId);

            entityBuilder
                 .HasOne(bu => bu.User)
                .WithMany(u => u.BookUsers)
                .HasForeignKey(bu => bu.UserId);
        }
    }
}
