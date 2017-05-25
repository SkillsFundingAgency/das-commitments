using System;
using System.Threading.Tasks;

using FluentAssertions;

using FluentValidation;
using FluentValidation.Results;

using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Application.Commands.CreateBulkUpload;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateBulkUpload
{
    [TestFixture]
    public class WhenCreatingBulkUpload
    {
        private Mock<IBulkUploadRepository> _repository;
        private CreateBulkUploadHandler _sut;
        private Mock<CreateBulkUploadValidator> _validator;
        private CreateBulkUploadCommand _command;

        [SetUp]
        public void SetUp()
        {
            _validator = new Mock<CreateBulkUploadValidator>();
            _repository = new Mock<IBulkUploadRepository>();
            _sut = new CreateBulkUploadHandler(_validator.Object, _repository.Object, Mock.Of<ICommitmentsLogger>());

            _command =
                new CreateBulkUploadCommand
                {
                    CommitmentId = 1,
                    ProviderId = 666,
                    FileName = "bulk.csv",
                    BulkUploadFile = ""
                };
        }

        [Test]
        public void ShouldValidateAndSendToRepository()
        {
            _validator.Setup(m => m.Validate(_command)).Returns(new ValidationResult() );

            _sut.Handle(_command);

            _validator.Verify(m => m.Validate(_command), Times.Once);
            _repository.Verify(m => m.InsertBulkUploadFile(_command.BulkUploadFile, _command.FileName, _command.CommitmentId), Times.Once);
        }

        [Test]
        public void ShouldThrowxceptionIfValidationFail()
        {
            _validator.Setup(m => m.Validate(_command)).Returns(
                new ValidationResult { Errors = { new ValidationFailure("MockProperty", "This is an error message") }});
            
            Func<Task> act = async () => await _sut.Handle(_command);
            act.ShouldThrow<ValidationException>();

            _validator.Verify(m => m.Validate(_command), Times.Once);
            _repository.Verify(m => m.InsertBulkUploadFile(_command.BulkUploadFile, _command.FileName, _command.CommitmentId), Times.Never);
        }
    }
}
