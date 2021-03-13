using System;
using System.Collections.Generic;
using TimeSharer.Models;

namespace TimeSharer.Interfaces
{
    public interface IVolunteerService
    {
        public List<Volunteer> Get();
        public Volunteer Create(Volunteer volunteer);
        public Volunteer Read(string id);
        public bool Update(string id, Details volunteerIn);
        public bool Delete(string id);
        public bool Delete(Volunteer volunteerIn);

    }
}
