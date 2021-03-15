using System;
using System.Collections.Generic;
namespace TimeSharerApi.Models
{
    public class VolunteersResponseModel : BaseResponseModel
    {
        public List<Volunteer> Data { get; set; } = new List<Volunteer>();
    }
}
