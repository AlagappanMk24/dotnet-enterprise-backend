namespace Enterprise.OrderApi.Domain.Enums;

public enum OrderStatus
{
    PendingBackgroundProcessing = 1,
    BackgroundProcessingCompleted = 2
}