using AutoFixture.Kernel;
using MediatR;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Linq;
using System.Reflection;

namespace SFA.DAS.CommitmentsV2.UnitTests
{
    public class ModelSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request,
            ISpecimenContext context)
        {
            var pi = request as Type;

            if (pi == null)
            {
                return new NoSpecimen();
            }

            if (pi == typeof(ApprenticeshipBase) || pi.Name == nameof(Apprenticeship))
            {
                return new Apprenticeship();
            }

            if (pi == typeof(ApprenticeshipUpdate))
            {
                return new ApprenticeshipUpdate();
            }

            return new NoSpecimen();
        }
    }

    /*public class UserInfoArgSpecimenBuilder : ISpecimenBuilder
    {
        private readonly UserInfo value;

        public UserInfoArgSpecimenBuilder(UserInfo value)
        {
            this.value = value;
        }

        public object Create(object request, ISpecimenContext context)
        {
            var pi = request as ParameterInfo;
            if (pi == null)
                return new NoSpecimen();

            if (!pi.Member.DeclaringType.GetInterfaces().ToList().Contains(typeof(IRequest)) ||
                pi.ParameterType != typeof(UserInfo) ||
                pi.Name != "userInfo")
                return new NoSpecimen();

            return value;
        }
    }*/
}
