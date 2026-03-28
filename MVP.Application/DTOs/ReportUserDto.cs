namespace MVP.Application.DTOs;

public class ReportUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Modules { get; set; } = new();
}
