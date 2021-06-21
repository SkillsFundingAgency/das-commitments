/*
CON-3410 - Get all the records which are going to be affected 
Usage instructions:
Execute the below againsts the Commitments db.
Copy the output, to keep a copy all the record which are going to be effected/
*/

Select * from Commitment
Where TransferSenderId is not null
and IsFullApprovalProcessed = 1
and Approvals = 3


/*
CON-3410 - Update Approval flag where the commitment is funded by transfer, is fully approved and Approvals value is 3 
Usage instructions:
Execute the below againsts the Commitments db.
Before running the below update statement make sure to run the above select statement to get a copy of the records which are going to be effected.
*/

Update Commitment
set Approvals  = 7
Where TransferSenderId is not null
and IsFullApprovalProcessed = 1
and Approvals = 3