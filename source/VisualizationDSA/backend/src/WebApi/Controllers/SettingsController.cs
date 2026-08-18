using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.Infrastructure.Data;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>
    /// Cấu hình hệ thống (Admin) — GET/PUT /api/v1/settings.
    /// Trả MẢNG settings kiểu key/value theo hình FE SystemSettingsDto (admin.ts fetchSettings()).
    /// </summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/settings")]
    [RequireJwtRole("Admin")]
    public class SettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public SettingsController(ApplicationDbContext dbContext) { _dbContext = dbContext; }

        private async Task<System.Collections.Generic.Dictionary<string, string>> LoadAsync()
        {
            var dict = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);
            var rows = await _dbContext.Set<SystemSetting>().AsNoTracking().ToListAsync();
            foreach (var s in rows) dict[s.Key] = s.Value;
            var result = new System.Collections.Generic.Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var kv in dict) result[kv.Key] = kv.Value;
            return result;
        }

        private static T Pick<T>(System.Collections.Generic.Dictionary<string, string> d, string key, T def)
            => d.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? (T)Convert.ChangeType(v, typeof(T)) : def;

        [HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            var d = await LoadAsync();
            var allowedDomains = (d.TryGetValue("allowedDomains", out var dom) && !string.IsNullOrWhiteSpace(dom))
                ? dom.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray()
                : new[] { "fpt.edu.vn" };
            return Ok(new
            {
                siteName = Pick(d, "siteName", "VisualizationDSA"),
                allowedDomains = allowedDomains,
                passwordPolicy = new
                {
                    minLength = Pick(d, "passwordMinLength", 8),
                    requireUppercase = Pick(d, "passwordRequireUppercase", true),
                    requireDigit = Pick(d, "passwordRequireDigit", true),
                    requireSpecial = Pick(d, "passwordRequireSpecial", false),
                },
                uploadMaxMb = Pick(d, "uploadMaxMb", 10),
                sandboxSeconds = Pick(d, "sandboxSeconds", 30),
                sandboxMemoryMb = Pick(d, "sandboxMemoryMb", 256),
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsStorage? body)
        {
            if (body == null) return Ok(await GetSettings());
            var updatedBy = TryGetCurrentUserId();

            void Set(string key, string? value)
            {
                if (value == null) return;
                var existing = _dbContext.Set<SystemSetting>().AsNoTracking().FirstOrDefault(x => x.Key == key);
                if (existing == null) _dbContext.Set<SystemSetting>().Add(new SystemSetting(key, value, updatedBy: updatedBy));
                else existing.Update(value, null, updatedBy);
            }

            if (body.SiteName != null) Set("siteName", body.SiteName);
            if (body.AllowedDomains != null) Set("allowedDomains", string.Join(",", body.AllowedDomains));
            if (body.PasswordPolicy != null)
            {
                if (body.PasswordPolicy.MinLength.HasValue) Set("passwordMinLength", body.PasswordPolicy.MinLength.Value.ToString());
                if (body.PasswordPolicy.RequireUppercase.HasValue) Set("passwordRequireUppercase", body.PasswordPolicy.RequireUppercase.Value ? "true" : "false");
                if (body.PasswordPolicy.RequireDigit.HasValue) Set("passwordRequireDigit", body.PasswordPolicy.RequireDigit.Value ? "true" : "false");
                if (body.PasswordPolicy.RequireSpecial.HasValue) Set("passwordRequireSpecial", body.PasswordPolicy.RequireSpecial.Value ? "true" : "false");
            }
            if (body.UploadMaxMb.HasValue) Set("uploadMaxMb", body.UploadMaxMb.Value.ToString());
            if (body.SandboxSeconds.HasValue) Set("sandboxSeconds", body.SandboxSeconds.Value.ToString());
            if (body.SandboxMemoryMb.HasValue) Set("sandboxMemoryMb", body.SandboxMemoryMb.Value.ToString());

            await _dbContext.SaveChangesAsync(CancellationToken.None);
            return Ok(await GetSettings());
        }

        private Guid? TryGetCurrentUserId()
        {
            var s = JwtHelper.ExtractSubFromToken(Request);
            return s != null && Guid.TryParse(s, out var id) ? id : null;
        }
    }

    public class UpdateSettingsStorage
    {
        public string? SiteName { get; set; }
        public string[]? AllowedDomains { get; set; }
        public PasswordPolicyStorage? PasswordPolicy { get; set; }
        public int? UploadMaxMb { get; set; }
        public int? SandboxSeconds { get; set; }
        public int? SandboxMemoryMb { get; set; }
    }
    public class PasswordPolicyStorage
    {
        public int? MinLength { get; set; }
        public bool? RequireUppercase { get; set; }
        public bool? RequireDigit { get; set; }
        public bool? RequireSpecial { get; set; }
    }
}
