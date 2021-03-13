using System;
using TimeSharerApi.Interfaces;

namespace TimeSharerApi.Models
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string VolunteersCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string OrganisationsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string OpportunitiesCollectionName { get; set; }
    }
}
