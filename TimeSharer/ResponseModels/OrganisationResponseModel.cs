using System.Collections.Generic;

namespace TimeSharer.Models
{
    public class OrganisationResponseModel : BaseResponseModel
    {
        public Organisation[] Data { get; set; }
    }
}
