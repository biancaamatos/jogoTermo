using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TermoApp
{
    internal class StatsData
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public List<int> Attempts { get; set; } = new();
        public List<bool> Results { get; set; } = new(); // true = win, false = loss
    }

    public static class StatsManager
    {
        private static readonly string FilePath;
        private static StatsData data;

        static StatsManager()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            FilePath = Path.Combine(appData, "TermoApp", "placar.json");
            Load();
        }

        private static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    data = JsonSerializer.Deserialize<StatsData>(json) ?? new StatsData();
                    // garantir listas não-nulas caso arquivo antigo não tenha o campo
                    if (data.Attempts == null) data.Attempts = new List<int>();
                    if (data.Results == null) data.Results = new List<bool>();
                }
                else
                {
                    data = new StatsData();
                }
            }
            catch
            {
                data = new StatsData();
            }
        }

        private static void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(data);
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // Falha silenciosa na persistência; não impede execução.
            }
        }

        public static void RecordWin(int attempts)
        {
            data.Wins++;
            data.Attempts.Add(attempts);
            data.Results.Add(true);
            Save();
        }

        public static void RecordLoss(int attempts)
        {
            data.Losses++;
            data.Attempts.Add(attempts);
            data.Results.Add(false);
            Save();
        }

        public static int Wins => data.Wins;
        public static int Losses => data.Losses;
        public static IReadOnlyList<int> Attempts => data.Attempts.AsReadOnly();
        public static IReadOnlyList<bool> Results => data.Results.AsReadOnly();

        public static int BestWinStreak
        {
            get
            {
                if (data.Results == null || data.Results.Count == 0) return 0;
                int best = 0, cur = 0;
                foreach (var r in data.Results)
                {
                    if (r) { cur++; if (cur > best) best = cur; }
                    else cur = 0;
                }
                return best;
            }
        }

        public static void Reset()
        {
            data = new StatsData();
            Save();
        }
    }
}