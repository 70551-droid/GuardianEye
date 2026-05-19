using GuardianEye.Data;
using GuardianEye.Models;

namespace GuardianEye.Filtering
{
    public interface IWebsiteFilter
    {
        Task<bool> IsUrlBlockedAsync(string url);
        Task<bool> AddBlockRuleAsync(string pattern, string category);
        Task<List<WebsiteRule>> GetAllRulesAsync();
    }

    public class WebsiteFilter : IWebsiteFilter
    {
        private readonly IDatabaseService _db;

        public WebsiteFilter(IDatabaseService db)
        {
            _db = db;
        }

        public async Task<bool> IsUrlBlockedAsync(string url)
        {
            try
            {
                var rules = await _db.QueryAsync<WebsiteRule>(
                    "SELECT * FROM WebsiteRules WHERE IsEnabled = 1 AND RuleType = 'Blacklist'");
                
                foreach (var rule in rules)
                {
                    if (url.Contains(rule.UrlPattern.Trim('*'), StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddBlockRuleAsync(string pattern, string category)
        {
            try
            {
                var result = await _db.ExecuteAsync(
                    @"INSERT INTO WebsiteRules (UrlPattern, Category, RuleType) VALUES (@Pattern, @Category, 'Blacklist')",
                    new { Pattern = pattern, Category = category });
                return result > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<WebsiteRule>> GetAllRulesAsync()
        {
            var rules = await _db.QueryAsync<WebsiteRule>(
                "SELECT * FROM WebsiteRules ORDER BY Category");
            return rules.ToList();
        }
    }
}