using AutoMapper;
using FluentValidation.Results;
using Moq;
using ShareBook.Api.Controllers;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Controllers
{
    public class BaseCrudControllerTests
    {
        [Fact]
        public async Task UpdateAsync_MapsUpdatedValueAndPreservesResultMetadata()
        {
            var id = Guid.NewGuid();
            var viewModel = new CategoryVM { Name = "Nova categoria" };
            var categoryToUpdate = new Category { Id = id, Name = viewModel.Name };
            var updatedCategory = new Category { Id = id, Name = viewModel.Name };
            var mappedCategory = new Category { Id = id, Name = viewModel.Name };
            var serviceResult = new Result<Category>(updatedCategory)
            {
                SuccessMessage = "Categoria atualizada."
            };

            var service = new Mock<IBaseService<Category>>();
            service.Setup(x => x.UpdateAsync(categoryToUpdate)).ReturnsAsync(serviceResult);

            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<Category>(viewModel)).Returns(categoryToUpdate);
            mapper.Setup(x => x.Map<Category>(updatedCategory)).Returns(mappedCategory);

            var controller = new BaseCrudController<Category, CategoryVM, Category>(service.Object, mapper.Object);

            var result = await controller.UpdateAsync(id, viewModel);

            Assert.True(result.Success);
            Assert.Same(mappedCategory, result.Value);
            Assert.Equal(serviceResult.SuccessMessage, result.SuccessMessage);
            Assert.Equal(id, viewModel.Id);
            mapper.Verify(x => x.Map<Category>(updatedCategory), Times.Once);
            mapper.Verify(x => x.Map<Category>(serviceResult), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_PreservesValidationMessagesWhenServiceRejectsUpdate()
        {
            var id = Guid.NewGuid();
            var viewModel = new CategoryVM { Name = string.Empty };
            var categoryToUpdate = new Category { Id = id, Name = viewModel.Name };
            var validationResult = new ValidationResult(new[]
            {
                new ValidationFailure(nameof(Category.Name), "Nome obrigatorio.")
            });
            var serviceResult = new Result<Category>(validationResult);

            var service = new Mock<IBaseService<Category>>();
            service.Setup(x => x.UpdateAsync(categoryToUpdate)).ReturnsAsync(serviceResult);

            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<Category>(viewModel)).Returns(categoryToUpdate);

            var controller = new BaseCrudController<Category, CategoryVM, Category>(service.Object, mapper.Object);

            var result = await controller.UpdateAsync(id, viewModel);

            Assert.False(result.Success);
            Assert.Null(result.Value);
            Assert.Equal(serviceResult.Messages, result.Messages);
            mapper.Verify(x => x.Map<Category>(It.IsAny<Category>()), Times.Never);
        }
    }
}
