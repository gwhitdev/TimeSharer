using System;
namespace TimeSharerApi.Interfaces
{
    public interface IDatabaseSettings
    {
        string VolunteersCollectionName { get; set; }
        string UsersCollectionName { get; set; }
        string OrganisationsCollectionName { get; set; }
        string OpportunitiesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
