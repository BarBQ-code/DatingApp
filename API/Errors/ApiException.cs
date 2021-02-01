namespace API.Errors
{
    public class ApiException
    {
        public int StuatusCode { get; set; }
        public string Message { get; set; }
        public string Detail { get; set; }

        public ApiException(int stuatusCode, string message = null, string detail = null)
        {
            StuatusCode = stuatusCode;
            Message = message;
            Detail = detail;    
        }
    }
}