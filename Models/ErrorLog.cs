namespace Ecommerce.Models
{
    public class ErrorLog
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ExceptionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;      
        public string? StackTrace { get; set; }                  
        public string? RequestPath { get; set; } 
    }
}
