namespace InLap.Infrastructure.Configuration
{
    public class InfrastructureOptions
    {
        public string FilesBasePath { get; set; } = "file-store";
        public long MaxUploadBytes { get; set; } = 1_000_000;
    }
}
