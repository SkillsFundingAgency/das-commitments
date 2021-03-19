using AutoFixture;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Commands.ValidateApprenticeshipForEdit;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Testing.AutoFixture;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    public class ValidateApprenticeshipForEditCommandHandlerTests
    {
        [Test, MoqAutoData]
        public async Task EditValidationServiceIsCalled()
        {
            //Act
            var fixture = new ValidateApprenticeshipForEditCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.Verify_EditApprenticeshipValidationService_IsCalled_Once();
        }

        [Test, MoqAutoData]
        public async Task ModelMapperIsCalled()
        {
            //Act
            var fixture = new ValidateApprenticeshipForEditCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.Verify_ModelMapper_IsCalled_Once();
        }

        public class ValidateApprenticeshipForEditCommandHandlerTestsFixture
        {
            Mock<IEditApprenticeshipValidationService> _editValidationService;
            Mock<IModelMapper> _modelMapper;
            ValidateApprenticeshipForEditCommand _command;
            public IRequestHandler<ValidateApprenticeshipForEditCommand> Handler { get; set; }

            public ValidateApprenticeshipForEditCommandHandlerTestsFixture()
            {
                var fixture = new Fixture();
                _editValidationService = new Mock<IEditApprenticeshipValidationService>();
                _modelMapper = new Mock<IModelMapper>();
                _command = fixture.Create<ValidateApprenticeshipForEditCommand>();

                _editValidationService.Setup(x => x.Validate(It.IsAny<EditApprenticeshipValidationRequest>(), CancellationToken.None)).Returns(Task.FromResult(new EditApprenticeshipValidationResult()));
                Handler = new ValidateApprenticeshipForEditCommandHandler(_editValidationService.Object, _modelMapper.Object);
            }

            public async Task<ValidateApprenticeshipForEditCommandHandlerTestsFixture> Handle()
            {
                await Handler.Handle(_command, CancellationToken.None);
                return this;
            }

            public void Verify_EditApprenticeshipValidationService_IsCalled_Once()
            {
                _editValidationService.Verify(x => x.Validate(It.IsAny<EditApprenticeshipValidationRequest>(), CancellationToken.None), Times.Once);
            }

            public void Verify_ModelMapper_IsCalled_Once()
            {
                _modelMapper.Verify(x => x.Map<EditApprenticeshipValidationRequest>(_command), Times.Once);
            }
        }
    }
}
    

