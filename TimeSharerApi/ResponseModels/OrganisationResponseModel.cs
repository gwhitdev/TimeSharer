using System.Collections.Generic;

namespace TimeSharerApi.Models
{
    public class OrganisationResponseModel : BaseResponseModel
    {
        public Organisation[] Data { get; set; }
    }
}
