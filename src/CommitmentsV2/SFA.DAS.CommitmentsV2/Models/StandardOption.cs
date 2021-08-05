using System;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class StandardOption
    {
        //[Key]
        //public Guid Id { get; set; }
        public string StandardUId { get; set; }
        public string Option { get; set; }
    }
}
