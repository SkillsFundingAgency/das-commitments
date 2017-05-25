using FluentAssertions;

using NUnit.Framework;

using SFA.DAS.Commitments.Application.Commands.CreateBulkUpload;

namespace SFA.DAS.Commitments.Application.UnitTests.Commands.CreateBulkUpload
{
    [TestFixture]
    public class WhenValidatngCreatingBulkUpload
    {
        private CreateBulkUploadValidator _validator;

        private CreateBulkUploadCommand _command;

        [SetUp]
        public void SetUp()
        {
            _validator = new CreateBulkUploadValidator();
            _command =
                new CreateBulkUploadCommand
                    {
                        CommitmentId = 1,
                        ProviderId = 666,
                        FileName = "bulk.csv",
                        BulkUploadFile = "<content>"
                    };
        }

        [Test]
        public void ShouldValidateCommand()
        {
            var result = _validator.Validate(_command);
            result.IsValid.Should().BeTrue();
        }

        [Test]
        public void ShouldAcceptMissingFileName()
        {
            _command.FileName = "";
            var result = _validator.Validate(_command);
            result.IsValid.Should().BeTrue();
        }

        [TestCase(-5)]
        [TestCase(0)]
        public void ShouldFailIfMissingCommitmentId(long commitmentId)
        {
            _command.CommitmentId = commitmentId;
            var result = _validator.Validate(_command);
            result.IsValid.Should().BeFalse();
        }

        [TestCase(-5)]
        [TestCase(0)]
        public void ShouldFailIfMissingProviderId(long providerId)
        {
            _command.ProviderId = 0;
            var result = _validator.Validate(_command);
            result.IsValid.Should().BeFalse();
        }

        [Test]
        public void ShouldFailIfMissingFileContent()
        {
            _command.BulkUploadFile = null;
            var result = _validator.Validate(_command);
            result.IsValid.Should().BeFalse();
        }
    }
}