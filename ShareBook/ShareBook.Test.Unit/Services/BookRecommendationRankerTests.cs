using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class BookRecommendationRankerTests
    {
        private readonly Category _adventure = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Aventuras e Fantasia",
            ParentCategoryId = Guid.NewGuid(),
            ParentCategory = new Category { Name = "Infantil/Juvenil" }
        };

        [Fact]
        public void Rank_OldGoogleLandingPage_ShouldPutAvailableEquivalentWorkFirst()
        {
            var source = Book(
                "Percy Jackson e o Mar de Monstros",
                "Rick Riordan",
                "Percy e Annabeth enfrentam monstros da mitologia grega.",
                BookStatus.Sent,
                BookType.Printed);
            var availableCopy = Book(
                "O Mar de Monstros",
                "Rick Riordan",
                "Uma aventura mitológica com Percy, Annabeth e o Velocino de Ouro.",
                BookStatus.Available,
                BookType.Printed);
            var minotaur = Book(
                "O Minotauro",
                "Monteiro Lobato",
                "Mitologia grega, heróis, monstros, coragem e amizade.",
                BookStatus.Available,
                BookType.Eletronic);

            var result = BookRecommendationRanker.Rank(source, new[] { minotaur, availableCopy });

            Assert.Equal(availableCopy.Id, result[0].Id);
        }

        [Fact]
        public void Rank_PhysicalSeaOfMonsters_ShouldPutThematicEbookFirst()
        {
            var source = Book(
                "O Mar de Monstros",
                "Rick Riordan",
                "Percy e Annabeth partem em busca do Velocino de Ouro. Uma aventura com semideuses, criaturas lendárias, mitologia, monstros, coragem, amizade e lealdade.",
                BookStatus.Available,
                BookType.Printed);
            var minotaur = Book(
                "O Minotauro",
                "Monteiro Lobato",
                "Uma aventura pela mitologia grega, com heróis, monstros, deuses, coragem e amizade.",
                BookStatus.Available,
                BookType.Eletronic);
            var genericFantasy = Book(
                "O Reino Distante",
                "Outra Autora",
                "Uma fantasia sobre um reino encantado.",
                BookStatus.Available,
                BookType.Eletronic);
            var literalSeaMatch = Book(
                "Pedro, O Menino do Mar",
                "Rosa Morena",
                "Uma história sobre o oceano, golfinhos, natureza e preservação ambiental.",
                BookStatus.Available,
                BookType.Eletronic);
            var unrelated = Book(
                "Clean Architecture",
                "Robert Martin",
                "Princípios de arquitetura de software.",
                BookStatus.Available,
                BookType.Printed,
                new Category { Id = Guid.NewGuid(), Name = "Tecnologia" });

            var result = BookRecommendationRanker.Rank(source, new[] { genericFantasy, literalSeaMatch, unrelated, minotaur });

            Assert.Equal(minotaur.Id, result[0].Id);
        }

        [Fact]
        public void Rank_ShouldOnlyReturnAvailableBooksAndExcludeCurrentBook()
        {
            var source = Book("Origem", "Autor", "Aventura", BookStatus.Available, BookType.Printed);
            var available = Book("Disponível", "Outro", "Aventura", BookStatus.Available, BookType.Printed);
            var sent = Book("Já doado", "Outro", "Aventura", BookStatus.Sent, BookType.Printed);

            var result = BookRecommendationRanker.Rank(source, new[] { source, sent, available });

            var selected = Assert.Single(result);
            Assert.Equal(available.Id, selected.Id);
        }

        [Fact]
        public void Rank_ShouldDiversifyAfterSelectingTheBestMatch()
        {
            var source = Book("A Jornada", "Autor Original", "Aventura e amizade", BookStatus.Sent, BookType.Printed);
            var candidates = new List<Book>
            {
                Book("A Jornada", "Autor Original", "Aventura e amizade", BookStatus.Available, BookType.Printed),
                Book("A Jornada - edição especial", "Autor Original", "Aventura e amizade", BookStatus.Available, BookType.Printed),
                Book("A Jornada Ilustrada", "Autor Original", "Aventura e amizade", BookStatus.Available, BookType.Printed),
                Book("Outra Fantasia", "Autora Dois", "Aventura fantástica", BookStatus.Available, BookType.Eletronic),
                Book("Heróis do Reino", "Autor Três", "Heróis e amizade", BookStatus.Available, BookType.Eletronic),
                Book("Mitologia para Jovens", "Autor Quatro", "Mitologia e aventura", BookStatus.Available, BookType.Eletronic),
                Book("Uma Nova História", "Autor Cinco", "Uma história juvenil", BookStatus.Available, BookType.Printed),
                Book("Coragem no Mar", "Autor Seis", "Coragem, mar e aventura", BookStatus.Available, BookType.Printed)
            };

            var result = BookRecommendationRanker.Rank(source, candidates, 6);

            Assert.Equal(6, result.Count);
            Assert.Equal("A Jornada", result[0].Title);
            Assert.DoesNotContain(result, book => book.Title == "A Jornada - edição especial");
            Assert.DoesNotContain(result, book => book.Title == "A Jornada Ilustrada");
        }

        private Book Book(
            string title,
            string author,
            string synopsis,
            BookStatus status,
            BookType type,
            Category category = null)
        {
            var selectedCategory = category ?? _adventure;
            return new Book
            {
                Id = Guid.NewGuid(),
                Title = title,
                Author = author,
                Synopsis = synopsis,
                Status = status,
                Type = type,
                CategoryId = selectedCategory.Id,
                Category = selectedCategory,
                CreationDate = DateTime.UtcNow
            };
        }
    }
}
