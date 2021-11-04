using DeskBooker.Core.Domain;
using DeskBooker.Core.Processor;
using Moq;
using Xunit;

namespace DeskBooker.Web.Pages {
    public class BookDeskModelTests {
        private Mock<IDeskBookingRequestProcessor> _processorMock;
        private BookDeskModel _bookDeskModel;
        private DeskBookingResult _deskBookingResult;

        public BookDeskModelTests() {
            _processorMock = new Mock<IDeskBookingRequestProcessor>();

            _bookDeskModel = new BookDeskModel(_processorMock.Object) {
                DeskBookingRequest = new Core.Domain.DeskBookingRequest()
            };

            _deskBookingResult = new DeskBookingResult {
                Code = DeskBookingResultCode.Success
            };

            _processorMock.Setup(x => x.BookDesk(_bookDeskModel.DeskBookingRequest))
               .Returns(_deskBookingResult);
        }
        //[Fact]
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
        public void ShouldCallBookDeskMethodOfProcessIfModelIsValid(int expectedBookDeskCalls,bool ismOdelValid) {
           
            if (!ismOdelValid) {
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
    }
}
