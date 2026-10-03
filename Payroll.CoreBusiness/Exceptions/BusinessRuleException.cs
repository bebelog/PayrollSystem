namespace Payroll.CoreBusiness.Exceptions;

public class BusinessRuleException : Exception
{
    public string? ErrorCode { get; }

    public BusinessRuleException(string message, string? errorCode = null) 
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessRuleException(string message, Exception innerException, string? errorCode = null) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
