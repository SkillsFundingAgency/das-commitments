// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using MediatR;
using SFA.DAS.Commitments.Api.Client;
using SFA.DAS.Tasks.Api.Client;
using StructureMap;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.DependencyResolution
{
    using StructureMap.Graph;

    public class DefaultRegistry : Registry {
        private const string ServiceName = "SFA.DAS.ProviderApprenticeshipsService";
        private const string ServiceNamespace = "SFA.DAS";

        public DefaultRegistry() {
            Scan(
                scan => {
                    scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(ServiceNamespace));
                    scan.RegisterConcreteTypesAgainstTheFirstInterface();
                    scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    scan.ConnectImplementationsToTypesClosing(typeof(IAsyncRequestHandler<,>));
                    scan.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                    scan.ConnectImplementationsToTypesClosing(typeof(IAsyncNotificationHandler<>));
                });
            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
            For<IMediator>().Use<Mediator>();
            For<ICommitmentsApi>().Use<CommitmentsApi>();
            For<ITasksApi>().Use<TasksApi>();

            //For<IUserRepository>().Use<FileSystemUserRepository>();
            //For<IStandardsRepository>().Use<FileSystemStandardsRepository>();
        }
    }
}