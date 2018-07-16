//using SFA.DAS.Commitments.Api.Types.Commitment;
//using SFA.DAS.Commitments.Api.Types.Commitment.Types;
//using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
{
    public static class TestEntities
    {
        public static Commitment GetCommitmentForCreate()
        {
            return new Commitment
            {
                Reference = "COMREF",
                EmployerAccountId = 8315,
                LegalEntityId = "04985133",
                LegalEntityName = "COOL UN LTD",
                LegalEntityAddress = "Legal Entity Address",
                LegalEntityOrganisationType = Common.Domain.Types.OrganisationType.CompaniesHouse,
                ProviderId = 10005077,
                ProviderName = "PETERBOROUGH REGIONAL COLLEGE",
                //CommitmentStatus = CommitmentStatus.New,
                //EditStatus = EditStatus.Neither,
                //LastAction = LastAction.None,
                //LastUpdatedByEmployerName = "Juan Kerr",
                //LastUpdatedByEmployerEmail = "jk@example.com",
                EmployerLastUpdateInfo = new LastUpdateInfo { Name = "Anna-Leigh Probin", EmailAddress = "alp@example.com" },
                ProviderLastUpdateInfo = new LastUpdateInfo { Name = "Juan Kerr", EmailAddress = "jk@example.com" }
            };
        }
    }
}
/*Id	Reference	EmployerAccountId	LegalEntityId	LegalEntityName	LegalEntityAddress	LegalEntityOrganisationType	ProviderId	ProviderName	CommitmentStatus	EditStatus	CreatedOn	LastAction	LastUpdatedByEmployerName	LastUpdatedByEmployerEmail	LastUpdatedByProviderName	LastUpdatedByProviderEmail	TransferApprovalActionedByEmployerName	TransferApprovalActionedByEmployerEmail	TransferApprovalActionedOn
8	46MYXV	8315	04985133	COOL UN LTD	6 Hawkside, Wilnecote, Tamworth, B77 4HW	1	10005077	PETERBOROUGH REGIONAL COLLEGE	0	1	2018-06-18 12:44:34.527	0	Phil Davies	apprent@phildavies.co.uk	NULL	NULL	NULL	NULL	NULL*/
/*
 * 	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Reference] [nvarchar](100) NOT NULL,
	[EmployerAccountId] [bigint] NOT NULL,
	[LegalEntityId] [nvarchar](50) NOT NULL,
	[LegalEntityName] [nvarchar](100) NOT NULL,
	[LegalEntityAddress] [nvarchar](256) NOT NULL,
	[LegalEntityOrganisationType] [tinyint] NOT NULL,
	[ProviderId] [bigint] NULL,
	[ProviderName] [nvarchar](100) NULL,
	[CommitmentStatus] [smallint] NOT NULL,
	[EditStatus] [smallint] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[LastAction] [smallint] NOT NULL,
	[LastUpdatedByEmployerName] [nvarchar](255) NOT NULL,
	[LastUpdatedByEmployerEmail] [nvarchar](255) NOT NULL,
	[LastUpdatedByProviderName] [nvarchar](255) NULL,
	[LastUpdatedByProviderEmail] [nvarchar](255) NULL,
	[TransferSenderId] [bigint] SPARSE  NULL,
	[TransferSenderName] [nvarchar](100) SPARSE  NULL,
	[TransferApprovalStatus] [tinyint] SPARSE  NULL,
	[TransferApprovalActionedByEmployerName] [nvarchar](255) NULL,
	[TransferApprovalActionedByEmployerEmail] [nvarchar](255) NULL,
	[TransferApprovalActionedOn] [datetime2](7) NULL,
 * */
