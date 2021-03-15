using System.Collections.Generic;

namespace TimeSharerApi.Models
{
    public class OpportunityResponseModel : BaseResponseModel
    {
        public List<Opportunity> Data { get; set; }
    }
}
