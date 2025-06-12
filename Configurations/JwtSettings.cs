namespace DailyBrief.AI.Configurations;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiraEmHoras { get; set; }
}
