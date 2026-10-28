namespace Enterprise.OrderApi.Application.Exceptions;

public sealed class OrderValidationException(string message)
    : Exception(message);