using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace TermoApp
{
    // Classe interna para guardar a estrutura dos dados no arquivo JSON
    internal class StatsData
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public List<int> Attempts { get; set; } = new();
        public List<bool> Results { get; set; } = new();
    }

    public static class StatsManager
    {
        private static readonly string FilePath;
        private static StatsData data;

        // Construtor estático: roda uma única vez quando a classe é usada pela primeira vez
        static StatsManager()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appFolder = Path.Combine(appData, "TermoApp");
                Directory.CreateDirectory(appFolder); // Garante que a pasta exista
                FilePath = Path.Combine(appFolder, "placar.json");
            }
            catch
            {
                // Fallback para a pasta local se não conseguir acessar AppData
                FilePath = "placar.json";
            }

            Load(); // Carrega os dados na inicialização
        }

        private static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    data = JsonSerializer.Deserialize<StatsData>(json) ?? new StatsData();
                }
                else
                {
                    data = new StatsData();
                }
            }
            catch
            {
                data = new StatsData(); // Se o arquivo estiver corrompido, começa do zero
            }

            // Garante que as listas nunca sejam nulas
            if (data.Attempts == null) data.Attempts = new List<int>();
            if (data.Results == null) data.Results = new List<bool>();
        }

        private static void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // Falha silenciosa se não conseguir salvar
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

        public static void Reset()
        {
            data = new StatsData();
            Save();
        }

        // Propriedades públicas para acessar os dados de forma segura
        public static int Wins => data?.Wins ?? 0;
        public static int Losses => data?.Losses ?? 0;
        public static IReadOnlyList<int> Attempts => data?.Attempts?.AsReadOnly() ?? new List<int>().AsReadOnly();

        public static int BestWinStreak
        {
            get
            {
                if (data?.Results == null || data.Results.Count == 0) return 0;
                int best = 0, current = 0;
                foreach (var wasWin in data.Results)
                {
                    if (wasWin)
                    {
                        current++;
                        if (current > best) best = current;
                    }
                    else
                    {
                        current = 0;
                    }
                }
                return best;
            }
        }
    }
}