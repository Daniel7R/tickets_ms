namespace TicketsMS.Application.DTOs.Request
{
    public class ResponseDTO<T>
    {
        public T? Result { get; set; }
        //public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";
    }
}
