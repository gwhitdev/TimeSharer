using System;
namespace TimeSharerApi.Models
{
    public class Response<T>
    {
        public Response()
        {
        }

        public Response(T data)
        {
            Success = true;
            Message = string.Empty;
            Data = data;
        }

        public T Data { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; }
        public int NumberOfRecordsFound { get; set; }
    }
}
