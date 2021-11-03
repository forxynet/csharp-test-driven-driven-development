using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Moq;
using Xunit;

namespace DeskBooker.Web.Pages {
    public class BookDeskModelTests {
        //[Fact]
        public void ShouldCallBookDeskMethodOfProcess() {
            // Arrange
            var processorMock = new Mock<IDeskBookingRequestProcessor>();

            var bookDeskModel = new BookDeskModel(processorMock.Object) {
                DeskBookingRequest = new Core.Domain.DeskBookingRequest()
            };
            processorMock.Setup(x => x.BookDesk(bookDeskModel.DeskBookingRequest))
               .Returns(new DeskBookingResult {
                   Code = DeskBookingResultCode.Success
               });
            // Act
            bookDeskModel.OnPost();

            // Assert
            processorMock.Verify(x => x.BookDesk(bookDeskModel.DeskBookingRequest), Times.Once);
        }

        [Theory]
        [InlineData(1,true)]
        [InlineData(0,false)]
        public void ShouldCallBookDeskMethodOfProcessIfModelIsValid(int expectedBookDeskCalls,bool ismOdelValid) {
            // Arrange
            var processorMock = new Mock<IDeskBookingRequestProcessor>();

            var bookDeskModel = new BookDeskModel(processorMock.Object) {
                DeskBookingRequest = new Core.Domain.DeskBookingRequest()
            };

            processorMock.Setup(x => x.BookDesk(bookDeskModel.DeskBookingRequest))
                .Returns(new DeskBookingResult {
                    Code = DeskBookingResultCode.Success
                });

            if (!ismOdelValid) {
                bookDeskModel.ModelState.AddModelError("JUstAKey", "AnErrorMessage");
            }

            // Act
            bookDeskModel.OnPost();

            // Assert
            processorMock.Verify(x => x.BookDesk(bookDeskModel.DeskBookingRequest), 
                Times.Exactly(expectedBookDeskCalls));
        }

        [Fact]
        public void ShouldAddModelErrorIfNoDeskIsAvailable() {

            // Arrange
            var processorMock = new Mock<IDeskBookingRequestProcessor>();

            var bookDeskModel = new BookDeskModel(processorMock.Object) {
                DeskBookingRequest = new Core.Domain.DeskBookingRequest()
            };

            processorMock.Setup(x => x.BookDesk(bookDeskModel.DeskBookingRequest))
                .Returns(new DeskBookingResult {
                    Code = DeskBookingResultCode.Success
                });

            // Act
            bookDeskModel.OnPost();

            // Assert
            Assert.DoesNotContain("DeskBookingRequest.Date", bookDeskModel.ModelState);
        }
    }
}
