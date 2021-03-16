using System.Collections.Generic;
using TimeSharerApi.Models;

namespace TimeSharerApi.Interfaces
{
    public interface IVolunteerService
    {
        public List<Volunteer> Get();
        public Volunteer Create(Volunteer volunteer);
        public Volunteer Get(string id);
        public bool Update(string id, VolunteerDetails volunteerIn);
        public bool Delete(string id);
    }
}
