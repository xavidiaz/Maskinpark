namespace Maskinpark.Client.Models;

public class Machine
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsOnline { get; set; }
    public string LastData { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
