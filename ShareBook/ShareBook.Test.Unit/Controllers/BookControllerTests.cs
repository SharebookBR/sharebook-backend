using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ShareBook.Api.Controllers;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ShareBook.Test.Unit.Controllers
{
    public class BookControllerTests
    {
        Mock<IMapper> mockMapper;
        Mock<IBookService> bookServiceMock;
        Mock<IBookUserService> bookUserServiceMock;

        public BookControllerTests()
        {
            mockMapper = new Mock<IMapper>();
            bookServiceMock = new Mock<IBookService>();
            bookUserServiceMock = new Mock<IBookUserService>();

            mockMapper.Setup(x => x.Map<List<Top15NewBooksVM>>(It.IsAny<List<Book>>()))
                .Returns((List<Book> listSource) =>
                {

                    List<Top15NewBooksVM> books = new List<Top15NewBooksVM>();

                    foreach (var src in listSource)
                    {
                        books.Add(new Top15NewBooksVM()
                        {
                            Id = src.Id,
                            Title = src.Title,
                            Author = src.Author,
                            ImageSlug = src.ImageSlug,
                            ImageUrl = src.ImageUrl,
                            Approved = src.Approved
                        });
                    }

                    return books;
                    
                });

            mockMapper.Setup(x => x.Map<List<Random15BooksVM>>(It.IsAny<List<Book>>()))
               .Returns((List<Book> listSource) =>
               {

                   List<Random15BooksVM> books = new List<Random15BooksVM>();

                   foreach (var src in listSource)
                   {
                       books.Add(new Random15BooksVM()
                       {
                           Id = src.Id,
                           Title = src.Title,
                           Author = src.Author,
                           ImageSlug = src.ImageSlug,
                           ImageUrl = src.ImageUrl,
                           Approved = src.Approved
                       });
                   }

                   return books;

               });

            var book = new Book()
            {
                Author = "Robert Aley",
                Title = "Teoria discursiva do direito",
                ImageUrl = "www.sharebook.com.br\teoria-discursiva-do-direito.jpg",
                ImageSlug = "teoria-discursiva-do-direito.jpg",
                Slug = "teoria-discursiva-do-direito",
            };

            var list = new List<Book>
            {
                book
            };

            bookServiceMock.Setup(service => service.Top15NewBooks()).Returns(() =>
            {
                return new PagedList<Book>()
                {
                    Items = list
                };
            });

            bookServiceMock.Setup(service => service.Random15Books()).Returns(() =>
            {
                return new PagedList<Book>()
                {
                    Items = list
                };
            });
        }

        [Fact]
        public void ListTop15NewBooks()
        {
            var controller = new BookController( mockMapper.Object, bookServiceMock.Object, bookUserServiceMock.Object);
            IActionResult result = controller.Top15NewBooks();
            OkObjectResult okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public void ListRandom15Books()
        {
            var controller = new BookController(mockMapper.Object, bookServiceMock.Object, bookUserServiceMock.Object);
            IActionResult result = controller.Random15Books();

            OkObjectResult okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.NotNull(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);

            var random15Books = okResult.Value as List<Random15BooksVM>;
            Assert.NotNull(random15Books[0].ImageUrl);
            Assert.NotNull(random15Books[0].Title);
        }

    }
}
