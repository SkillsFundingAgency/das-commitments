CREATE TABLE [dbo].[HiringPayment] (
  ApprenticeshipId bigint NOT NULL,
  Created datetime2(0) NOT NULL,
  Updated datetime2(0) NULL,
  Status smallint NOT NULL,
  CONSTRAINT [PK_HiringPayment] PRIMARY KEY (ApprenticeshipId),
  CONSTRAINT [FK_HiringPayment_Apprenticeship]
    FOREIGN KEY (ApprenticeshipId) REFERENCES [dbo].[Apprenticeship] (Id)
);
