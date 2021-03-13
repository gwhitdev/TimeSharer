using System;
using System.Collections.Generic;
namespace TimeSharer.Models
{
    public class VolunteersResponseModel : BaseResponseModel
    {
        public List<Volunteer> Data {get; set;}
    }
}
