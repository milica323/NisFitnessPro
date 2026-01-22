using StackExchange.Redis;
using NisFitnessPro.Models;
using System.Text.Json;

namespace NisFitnessPro.Services;

public class RedisService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _db = _redis.GetDatabase();
    }

    [Obsolete]
    public async Task<bool> SaveActivityAsync(RunnerActivity activity)
{
    // 1. DISTRIBUTED LOCK (Sprečavanje kolizije kod ažuriranja rekorda)
    var lockKey = "lock:global_record";
    var token = Guid.NewGuid().ToString();
    // Pokušavamo da dobijemo lock na 5 sekundi
    bool lockAcquired = await _db.LockTakeAsync(lockKey, token, TimeSpan.FromSeconds(5));

    var tran = _db.CreateTransaction();

    // 2. ZSET: Rang lista
    _ = tran.SortedSetIncrementAsync("leaderboard", activity.Username, activity.Distance);

    // 3. GEO: Lokacija sa TTL-om (da trkač nestane posle 30 min neaktivnosti)
    _ = tran.GeoAddAsync("runner_locations", new GeoEntry(activity.Longitude, activity.Latitude, activity.Username));
    _ = tran.KeyExpireAsync("runner_locations", TimeSpan.FromMinutes(30));

    // 4. LIST: Activity Feed (Zadržavanje samo poslednjih 10)
    string feedMessage = $"{activity.Username} je pretrčao {activity.Distance}km u {DateTime.Now:HH:mm}";
    _ = tran.ListLeftPushAsync("activity_feed", feedMessage);
    _ = tran.ListTrimAsync("activity_feed", 0, 9);

    // Izvršavanje atomične transakcije
    bool committed = await tran.ExecuteAsync();

    if (committed && lockAcquired)
    {
        try
        {
            // 5. COMPLEX CACHING LOGIC (Provera i ažuriranje rekorda pod lock-om)
            var currentMax = await _db.StringGetAsync("global_record");
            if (!currentMax.HasValue || activity.Distance > (double)currentMax)
            {
                await _db.StringSetAsync("global_record", activity.Distance);
                
                // 6. PUB/SUB (Real-time obaveštenje o novom rekordu)
                await _db.PublishAsync("fitness_alerts", $"NOVI REKORD: {activity.Username} je postavio {activity.Distance}km!");
            }
        }
        finally
        {
            // OBAVEZNO oslobađanje lock-a
            await _db.LockReleaseAsync(lockKey, token);
        }
    }

    return committed;
}

    public async Task<IEnumerable<LeaderboardEntry>> GetTopRunnersAsync()
    {
        var top = await _db.SortedSetRangeByRankWithScoresAsync("leaderboard", 0, 4, Order.Descending);
        return top.Select(x => new LeaderboardEntry(x.Element!, x.Score));
    }

    public async Task<IEnumerable<string>> GetActivityFeedAsync()
    {
        var feed = await _db.ListRangeAsync("activity_feed", 0, 9);
        return feed.Select(x => x.ToString());
    }

    public async Task<GeoRadiusResult[]> GetNearbyRunnersAsync(double lat, double lon)
    {
        return await _db.GeoRadiusAsync("runner_locations", lon, lat, 5, GeoUnit.Kilometers);
    }

    public async Task<double> GetGlobalRecordAsync()
    {
        var record = await _db.StringGetAsync("global_record");
        return record.HasValue ? (double)record : 0;
    }
}