using System;
using System.Collections.Generic;
using TimeSharerApi.Models;
namespace TimeSharerApi.Interfaces
{
    public interface IOpportunityService
    {
        public List<Opportunity> Get();
        public Opportunity Create(Opportunity opportunity);
        public Opportunity Get(string id);
        public bool Update(string id, OpportunityDetails opportunityDetailsIn);
        public bool Delete(string id);

    }
}
