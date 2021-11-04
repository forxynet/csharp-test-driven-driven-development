using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace DeskBooker.Web.Pages {
    public class BookDeskModelTests {

        private Mock<IDeskBookingRequestProcessor> _processorMock;
        private BookDeskModel _bookDeskModel;
        private DeskBookingResult _deskBookingResult;

        public BookDeskModelTests() {
            _processorMock = new Mock<IDeskBookingRequestProcessor>();
            _bookDeskModel = new BookDeskModel(_processorMock.Object) {
                DeskBookingRequest = new DeskBookingRequest()
            };
            _deskBookingResult = new DeskBookingResult {
                Code = DeskBookingResultCode.Success
            };
            _processorMock.Setup(x => x.BookDesk(_bookDeskModel.DeskBookingRequest))
               .Returns(_deskBookingResult);
        }

        [Fact]
        public void ShouldCallBookDeskMethodOfProcess() {
            // Arrange
           
            // Act
            _bookDeskModel.OnPost();
            // Assert
            _processorMock.Verify(x => x.BookDesk(_bookDeskModel.DeskBookingRequest), Times.Once);
        }

        [Theory]
        [InlineData(1,true)]
        [InlineData(0,false)]
        public void ShouldCallBookDeskMethodOfProcessIfModelIsValid(
            int expectedBookDeskCalls,bool isModelValid) {
           
            if (!isModelValid) {
                _bookDeskModel.ModelState.AddModelError("JUstAKey", "AnErrorMessage");
            }

            // Act
            _bookDeskModel.OnPost();
            // Assert
            _processorMock.Verify(x => x.BookDesk(_bookDeskModel.DeskBookingRequest), 
                Times.Exactly(expectedBookDeskCalls));
        }

        [Fact]
        public void ShouldAddModelErrorIfNoDeskIsAvailable() {
            // Arrange
            _deskBookingResult.Code = DeskBookingResultCode.NoDeskAvailable;
            // Act
            _bookDeskModel.OnPost();
            // Assert
            Assert.Contains("DeskBookingRequest.Date", _bookDeskModel.ModelState);
        }

        [Fact]
        public void ShouldNotAddModelErrorIfDeskIsAvailable() {
            // Arrange
            _deskBookingResult.Code = DeskBookingResultCode.Success;
            // Act
            _bookDeskModel.OnPost();
            // Assert
            Assert.DoesNotContain("DeskBookingRequest.Date", _bookDeskModel.ModelState);
        }

        [Theory]
        [InlineData(typeof(PageResult),false,null)]
        [InlineData(typeof(PageResult),true,DeskBookingResultCode.NoDeskAvailable)]
        [InlineData(typeof(RedirectToPageResult),true,DeskBookingResultCode.Success)]
        public void ShouldReturnExpectedActionResult(Type expectedActionResultType,
            bool isModelValid, DeskBookingResultCode? deskBookingResultCode) {

            // Arrange
            if (!isModelValid) {
                _bookDeskModel.ModelState.AddModelError("JustAKey", "AnErrorMessage");
            }
            if (deskBookingResultCode.HasValue) {
                _deskBookingResult.Code = deskBookingResultCode.Value;
            }
            // Act
            IActionResult actionResult = _bookDeskModel.OnPost();
            // Assert
            Assert.IsType(expectedActionResultType, actionResult);
        }
  
        [Fact]
        public void ShouldRedirectToBookDeskConfirmationPage() {
            // Arrange
            _deskBookingResult.Code = DeskBookingResultCode.Success;
            _deskBookingResult.DeskBookingId = 7;
            _deskBookingResult.FirstName = "Hakan";
            _deskBookingResult.Date = new DateTime(2021, 11, 5);

            // Act
            IActionResult actionResult = _bookDeskModel.OnPost();

            // Assert
            var redirectToPageResult = Assert.IsType<RedirectToPageResult>(actionResult);
            Assert.Equal("BookDeskConfirmation", redirectToPageResult.PageName);

            IDictionary<string,object> routeValues = redirectToPageResult.RouteValues;
            Assert.Equal(3, routeValues.Count);

            var deskBookingId = Assert.Contains("DeskBookingId", routeValues);
            Assert.Equal(_deskBookingResult.DeskBookingId, deskBookingId);


            var firstName = Assert.Contains("FirstName", routeValues);
            Assert.Equal(_deskBookingResult.FirstName, firstName);

            var date = Assert.Contains("Date", routeValues);
            Assert.Equal(_deskBookingResult.Date, date);
        }
    }
}
