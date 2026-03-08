namespace MVP.Infrastructure.Configuration;

public class SftpStorageOptions
{
    public const string SectionName = "SftpStorage";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 22;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BasePath { get; set; } = string.Empty;
}
