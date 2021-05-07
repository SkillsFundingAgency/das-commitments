using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Reservations.Api.Types
{
    //public class CreateAccountReservationResult
    //{
    //    public Reservation Reservation { get; set; }
    //    public GlobalRule Rule { get; set; }
    //    public bool AgreementSigned { get; set; }
    //}

    //public class GlobalRule
    //{
    //    public long Id { get; }
    //    public DateTime? ActiveFrom { get; }
    //    public GlobalRuleType RuleType { get; }
    //    public AccountRestriction Restriction { get; }
    //    public string RuleTypeText => Enum.GetName(typeof(GlobalRuleType), RuleType);
    //    public string RestrictionText => Enum.GetName(typeof(AccountRestriction), Restriction);
    //    public IEnumerable<UserRuleAcknowledgement> UserRuleAcknowledgements { get; }

    //    public GlobalRule(Entities.GlobalRule globalRule)
    //    {
    //        Id = globalRule.Id;
    //        ActiveFrom = globalRule.ActiveFrom;
    //        RuleType = (GlobalRuleType)globalRule.RuleType;
    //        Restriction = (AccountRestriction)globalRule.Restriction;
    //        UserRuleAcknowledgements = globalRule.UserRuleNotifications?.Select(notification => new UserRuleAcknowledgement(notification));
    //    }
    //}

    //public enum GlobalRuleType
    //{
    //    None = 0,
    //    FundingPaused = 1,
    //    ReservationLimit = 2
    //}
}
