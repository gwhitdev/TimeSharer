using System;
namespace TimeSharerApi.Models
{
    public class BaseResponseModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NumberOfRecordsFound { get; set; }
    }
}
