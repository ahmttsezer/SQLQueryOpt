using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace QueryDebugger.Core.Services
{
    public class HistoryService
    {
        private const string FileName = "history.json";
        
        public async Task SaveHistoryAsync(HistoryData data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                await File.WriteAllTextAsync(FileName, json);
            }
            catch { /* Ignore file errors */ }
        }

        public async Task<HistoryData> LoadHistoryAsync()
        {
            try
            {
                if (File.Exists(FileName))
                {
                    var json = await File.ReadAllTextAsync(FileName);
                    return JsonSerializer.Deserialize<HistoryData>(json) ?? new HistoryData();
                }
            }
            catch { /* Ignore */ }
            
            return new HistoryData();
        }
    }

    public class HistoryData
    {
        public List<string> RecentConnectionStrings { get; set; } = new List<string>();
        public string LastQuery { get; set; }
    }
}

