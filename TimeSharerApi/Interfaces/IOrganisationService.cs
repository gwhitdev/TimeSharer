using System;
using System.Collections.Generic;
using TimeSharerApi.Models;

namespace TimeSharerApi.Interfaces
{
    public interface IOrganisationService
    {
        public List<Organisation> Get();
        public Organisation Create(Organisation organisation);
        public Organisation Get(string id);
        public bool Update(string id, OrganisationDetails organisationIn);
        public bool Delete(string id);
    }
}
