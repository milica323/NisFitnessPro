namespace NisFitnessPro.Models;

public record RunnerActivity(string Username, double Distance, double Latitude, double Longitude);

public record LeaderboardEntry(string Username, double Distance);

public record GlobalStats(double MaxDistance);