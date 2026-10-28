namespace Enterprise.BackgroundJobs.Domain.Enums
{
    public enum BackgroundJobStatus 
    { 
        Pending = 0, 
        Processing = 1, 
        Completed = 2, 
        Failed = 3 
    }
}