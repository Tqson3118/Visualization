using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// SimulationCatalogService đọc <c>shared/simulation-catalog.json</c> (SDD §6.1 — nguồn chuẩn đồng bộ frontend).
/// Đường dẫn: cấu hình <c>DSA:CatalogPath</c>; mặc định tìm từ ContentRootPath lên 2 cấp (kèm dò ngược nhiều cấp
/// để chạy được từ bin/ khi publish). Category/level map trực tiếp từ JSON ('structure'|'algorithm', 'basic'|'advanced').
/// Schema: JSON tĩnh theo nhóm key (mảng/tìm kiếm/stack-queue/list/tree-heap/hash/graph).
/// </summary>
public sealed class SimulationCatalogService : ISimulationCatalogService
{
    private readonly IConfiguration _config;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<SimulationCatalogService> _logger;
    private readonly Lazy<Dictionary<string, SimulationMetaDto>> _catalog;

    public SimulationCatalogService(
        IConfiguration config,
        IHostEnvironment environment,
        ILogger<SimulationCatalogService> logger)
    {
        _config = config;
        _environment = environment;
        _logger = logger;
        _catalog = new Lazy<Dictionary<string, SimulationMetaDto>>(() => LoadCatalog());
    }

    public Task<Result<List<SimulationMetaDto>>> GetListAsync(CancellationToken ct)
    {
        var items = _catalog.Value.Values
            .OrderBy(s => s.Key)
            .ToList();
        return Task.FromResult(Result<List<SimulationMetaDto>>.Ok(items));
    }

    public Task<Result<SimulationMetaDto>> GetByKeyAsync(string key, CancellationToken ct)
    {
        return _catalog.Value.TryGetValue(key, out var meta)
            ? Task.FromResult(Result<SimulationMetaDto>.Ok(meta))
            : Task.FromResult(Result<SimulationMetaDto>.Fail(
                ErrorCodes.SIMULATION_KEY_INVALID, "Khóa mô phỏng không tồn tại trong danh mục", new()
                {
                    ["key"] = ["Khóa mô phỏng không tồn tại trong danh mục"]
                }));
    }

    public Task<Result<SimulationSchemaDto>> GetSchemaAsync(string key, CancellationToken ct)
    {
        if (!_catalog.Value.ContainsKey(key))
        {
            return Task.FromResult(Result<SimulationSchemaDto>.Fail(
                ErrorCodes.SIMULATION_KEY_INVALID, "Khóa mô phỏng không tồn tại trong danh mục"));
        }

        var schema = JsonDocument.Parse(SchemaFor(key)).RootElement.Clone();
        return Task.FromResult(Result<SimulationSchemaDto>.Ok(new SimulationSchemaDto { Key = key, Schema = schema }));
    }

    public Task<bool> ExistsAsync(string key, CancellationToken ct) =>
        Task.FromResult(_catalog.Value.ContainsKey(key));

    // ── Load catalog ──────────────────────────────────────────

    private Dictionary<string, SimulationMetaDto> LoadCatalog()
    {
        var path = ResolveCatalogPath();
        if (path is null)
        {
            _logger.LogError("Không tìm thấy shared/simulation-catalog.json — kiểm tra DSA:CatalogPath hoặc thư mục shared/");
            return [];
        }

        var json = File.ReadAllText(path);
        var items = JsonSerializer.Deserialize<List<SimulationMetaDto>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        _logger.LogInformation("Catalog loaded from {Path}: {Count} simulations", path, items.Count);
        return items.ToDictionary(i => i.Key, StringComparer.OrdinalIgnoreCase);
    }

    private string? ResolveCatalogPath()
    {
        var configured = _config["DSA:CatalogPath"];
        if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
        {
            return configured;
        }

        var fileName = "simulation-catalog.json";
        var candidates = new List<string>();

        if (!string.IsNullOrWhiteSpace(configured))
        {
            candidates.Add(configured);
        }

        // Từ ContentRootPath lên 2 cấp (SDD — mặc định), kèm dò ngược thêm cho bin/ publish
        var contentRoot = _environment.ContentRootPath;
        for (var level = 0; level <= 5; level++)
        {
            var baseDir = level == 0
                ? contentRoot
                : Path.GetFullPath(Path.Combine(contentRoot, string.Concat(Enumerable.Repeat($"..{Path.DirectorySeparatorChar}", level))));
            candidates.Add(Path.Combine(baseDir, "shared", fileName));
        }

        return candidates.FirstOrDefault(File.Exists);
    }

    // ── Schema tĩnh theo nhóm key (API_REFERENCE.md §4.5/§4.14) ──

    private static string SchemaFor(string key) => key switch
    {
        // Mảng + sắp xếp/tìm kiếm tuyến tính
        _ when key.StartsWith("sort.", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("search.", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("structure.array", StringComparison.OrdinalIgnoreCase) =>
            """{"type":"array","fields":[{"name":"values","type":"number[]","min":2,"max":100,"default":[5,3,8,1,9,2]},{"name":"target","type":"number","optional":true,"default":8}]}""",

        // Stack / Queue
        _ when key.StartsWith("stack.", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("queue.", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("structure.stack", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("structure.queue", StringComparison.OrdinalIgnoreCase) =>
            """{"type":"sequence","fields":[{"name":"operations","type":"string[]","default":["push 3","push 1","pop"]},{"name":"initial","type":"any[]","optional":true,"default":[]}]}""",

        // Danh sách liên kết
        _ when key.StartsWith("list.", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("structure.linkedlist", StringComparison.OrdinalIgnoreCase) =>
            """{"type":"list","fields":[{"name":"values","type":"number[]","min":1,"max":50,"default":[4,2,7,1]},{"name":"operation","type":"string","default":"insert"},{"name":"value","type":"number","default":5}]}""",

        // Cây + heap
        _ when key.StartsWith("tree.", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("heap.", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("structure.binarytree", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("structure.bst", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("structure.avl", StringComparison.OrdinalIgnoreCase) ||
               key.StartsWith("structure.heap", StringComparison.OrdinalIgnoreCase) =>
            """{"type":"tree","fields":[{"name":"values","type":"number[]","min":1,"max":50,"default":[50,30,70,20,40,60,80]},{"name":"value","type":"number","optional":true,"default":35}]}""",

        // Bảng băm
        _ when key.StartsWith("hash.", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("structure.hashtable", StringComparison.OrdinalIgnoreCase) =>
            """{"type":"hash","fields":[{"name":"keys","type":"string[]","default":["apple","banana","cherry"]},{"name":"operation","type":"string","default":"insert"},{"name":"key","type":"string","optional":true,"default":"apple"}]}""",

        // Đồ thị
        _ when key.StartsWith("graph.", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("structure.graph", StringComparison.OrdinalIgnoreCase) =>
            """{"type":"graph","fields":[{"name":"vertices","type":"number[]","min":1,"max":50,"default":[0,1,2,3,4]},{"name":"edges","type":"[number,number][]","max":200,"default":[[0,1],[0,2],[1,3],[2,3],[3,4]]},{"name":"start","type":"number","default":0},{"name":"target","type":"number","optional":true,"default":4}]}""",

        _ => """{"type":"generic","fields":[{"name":"values","type":"any[]","default":[]}]}"""
    };
}
