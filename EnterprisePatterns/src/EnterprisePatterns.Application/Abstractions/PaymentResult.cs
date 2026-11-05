namespace EnterprisePatterns.Application.Abstractions
{
    /// <summary>Outcome of a charge attempt.</summary>
    /// <remarks>
    /// <b>Pattern / Principle Used:</b> Result object + Factory Methods (Creational).<br/>
    /// <b>Purpose:</b> Models success and decline explicitly, without exceptions for expected outcomes.<br/>
    /// <b>Why It Is Used:</b> Declines are normal business flow; exceptions are reserved for infrastructure failure.
    /// </remarks>
    public sealed record PaymentResult(bool Succeeded, string? TransactionId, string? FailureReason)
    {
        public static PaymentResult Success(string transactionId) => new(true, transactionId, null);

        public static PaymentResult Declined(string reason) => new(false, null, reason);
    }
}