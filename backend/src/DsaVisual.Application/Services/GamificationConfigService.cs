using System.Text.Json;
using DsaVisual.Application.Dtos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// Triển khai GamificationConfigService nạp RAM (Singleton, thread-safe, 0ms read).
/// Lưu trữ cấu hình trong file JSON (gamification-settings.json) để tồn tại vĩnh viễn qua các lần restart.
/// </summary>
public sealed class GamificationConfigService : IGamificationConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _filePath;
    private readonly ILogger<GamificationConfigService>? _logger;
    private readonly object _syncLock = new();
    private GamificationSettingsDto _settings;

    public GamificationConfigService(ILogger<GamificationConfigService>? logger = null, IHostEnvironment? env = null)
    {
        _logger = logger;
        
        var baseDir = env?.ContentRootPath ?? AppContext.BaseDirectory;
        _filePath = Path.Combine(baseDir, "gamification-settings.json");

        _settings = LoadSettingsFromFile();
    }

    /// <summary>
    /// Constructor cho phép chỉ định đường dẫn file trực tiếp (hỗ trợ test).
    /// </summary>
    public GamificationConfigService(string filePath, ILogger<GamificationConfigService>? logger = null)
    {
        _logger = logger;
        _filePath = filePath;
        _settings = LoadSettingsFromFile();
    }

    private GamificationSettingsDto LoadSettingsFromFile()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                var loaded = JsonSerializer.Deserialize<GamificationSettingsDto>(json, JsonOptions);
                if (loaded is not null)
                {
                    _logger?.LogInformation("Đã tải cấu hình Gamification từ {FilePath}", _filePath);
                    return loaded;
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Không thể đọc file {FilePath}, sử dụng cấu hình mặc định.", _filePath);
        }

        var defaults = new GamificationSettingsDto();
        SaveSettingsToFile(defaults);
        return defaults;
    }

    private void SaveSettingsToFile(GamificationSettingsDto settings)
    {
        try
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_filePath, json);
            _logger?.LogInformation("Đã ghi cấu hình Gamification xuống {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Lỗi khi ghi cấu hình Gamification xuống {FilePath}", _filePath);
        }
    }

    public GamificationSettingsDto GetSettings()
    {
        lock (_syncLock)
        {
            return CloneSettings(_settings);
        }
    }

    public async Task<GamificationSettingsDto> UpdateSettingsAsync(GamificationSettingsDto newSettings, CancellationToken ct = default)
    {
        GamificationSettingsDto cloned;
        lock (_syncLock)
        {
            _settings = CloneSettings(newSettings);
            cloned = CloneSettings(_settings);
        }

        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(cloned, JsonOptions);
        await File.WriteAllTextAsync(_filePath, json, ct);
        _logger?.LogInformation("Cập nhật thành công cấu hình Gamification.");

        return cloned;
    }

    public async Task<GamificationSettingsDto> ResetToDefaultsAsync(CancellationToken ct = default)
    {
        var defaults = new GamificationSettingsDto();
        return await UpdateSettingsAsync(defaults, ct);
    }

    public int GetTheoryBaseXp()
    {
        lock (_syncLock)
        {
            return _settings.TheoryBaseXp;
        }
    }

    public int GetQuizBaseXp()
    {
        lock (_syncLock)
        {
            return _settings.QuizBaseXp;
        }
    }

    public int GetCodelabBaseXp()
    {
        lock (_syncLock)
        {
            return _settings.CodelabBaseXp;
        }
    }

    public int GetStreakBonusXp()
    {
        lock (_syncLock)
        {
            return _settings.StreakBonusXp;
        }
    }

    public int GetHeartsMaxFree()
    {
        lock (_syncLock)
        {
            return _settings.HeartsMaxFree;
        }
    }

    public int GetHeartsMaxPremium()
    {
        lock (_syncLock)
        {
            return _settings.HeartsMaxPremium;
        }
    }

    public int GetHeartRegenMinutes()
    {
        lock (_syncLock)
        {
            return _settings.HeartRegenMinutes;
        }
    }

    public int GetSessionHours()
    {
        lock (_syncLock)
        {
            return _settings.SessionHours;
        }
    }

    private static GamificationSettingsDto CloneSettings(GamificationSettingsDto src)
    {
        return new GamificationSettingsDto
        {
            TheoryBaseXp = src.TheoryBaseXp,
            QuizBaseXp = src.QuizBaseXp,
            CodelabBaseXp = src.CodelabBaseXp,
            StreakBonusXp = src.StreakBonusXp,
            HeartsMaxFree = src.HeartsMaxFree,
            HeartsMaxPremium = src.HeartsMaxPremium,
            HeartRegenMinutes = src.HeartRegenMinutes,
            SessionHours = src.SessionHours
        };
    }
}
