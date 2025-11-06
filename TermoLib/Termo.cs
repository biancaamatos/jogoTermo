using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;

namespace TermoLib
{
    public class Letra
    {
        public char Caracter;
        public char Cor;
        public Letra(char caracter, char cor) { Caracter = caracter; Cor = cor; }
    }

    public class Termo
    {
        private static Random rdn = new Random();

        public List<string> Dicionario;

        private List<string> Palavras;

        public string palavraSorteada;
        public List<List<Letra>> tabuleiro;
        public Dictionary<char, char> teclado;
        public int palavraAtual;

        public Termo()
        {
            Palavras = new List<string>();
            Dicionario = new List<string>();
            tabuleiro = new List<List<Letra>>();
            teclado = new Dictionary<char, char>();
            for (int i = 'A'; i <= 'Z'; i++) { teclado.Add((char)i, 'C'); }
            palavraAtual = 1;

            try
            {
                CarregaDicionario("Dicionario.txt");

                CarregaPalavrasSorteio("Palavras.txt");

                if (Palavras.Count == 0)
                {
                    Palavras = new List<string> { "TERMO", "JOGAR", "LETRA", "IDEIA", "LIVRO" };
                }

                if (Dicionario.Count == 0)
                {
                    Dicionario = new List<string>(Palavras);
                }
            }
            catch
            {
                if (Palavras.Count == 0) Palavras = new List<string> { "TERMO", "JOGAR", "LETRA", "IDEIA", "LIVRO" };
                if (Dicionario.Count == 0) Dicionario = new List<string>(Palavras);
            }
        }

        public async Task IniciarAsync()
        {
            await Task.Run(() =>
            {
                if (Palavras.Count == 0) CarregaDicionario("Dicionario.txt");
                if (Dicionario.Count == 0) CarregaPalavrasSorteio("Palavras.txt");

                if (Palavras.Count == 0) Palavras = new List<string> { "TERMO", "JOGAR", "LETRA", "IDEIA", "LIVRO" };
                if (Dicionario.Count == 0) Dicionario = new List<string>(Palavras);
            });

            SorteiaPalavra();
        }

        public void CarregaDicionario(string fileName)
        {
            try
            {
                string path = FindFilePath(fileName);
                if (path == null)
                {
                    Console.WriteLine($"Dicionario: arquivo '{fileName}' não encontrado nos caminhos pesquisados.");
                    System.Diagnostics.Debug.WriteLine($"Dicionario: arquivo '{fileName}' não encontrado nos caminhos pesquisados.");
                    return;
                }

                var lines = File.ReadAllLines(path, Encoding.UTF8);
                var list = lines
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(p => NormalizeWord(p))
                    .Where(p => p.Length == 5 && p.All(c => c >= 'A' && c <= 'Z'))
                    .Distinct()
                    .ToList();

                Palavras = list;

                Console.WriteLine($"Dicionario carregado de '{path}' — {Palavras.Count} palavra(s) válidas.");
                System.Diagnostics.Debug.WriteLine($"Dicionario carregado de '{path}' — {Palavras.Count} palavra(s) válidas.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar Dicionario: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar Dicionario: {ex}");
            }
        }

        private static string FindFilePath(string fileName)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
            var candidates = new List<string>
            {
                Path.Combine(basePath, fileName),
                Path.Combine(Directory.GetCurrentDirectory(), fileName),
                Path.Combine(basePath, "TermoLib", fileName)
            };

            string dir = basePath;
            for (int i = 0; i < 6 && !string.IsNullOrEmpty(dir); i++)
            {
                candidates.Add(Path.Combine(dir, fileName));
                try { dir = Directory.GetParent(dir)?.FullName; } catch { break; }
            }

            try
            {
                var asmDir = Path.GetDirectoryName(typeof(Termo).Assembly.Location);
                if (!string.IsNullOrEmpty(asmDir)) candidates.Add(Path.Combine(asmDir, fileName));
            }
            catch { }

            foreach (var p in candidates.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct())
            {
                try { if (File.Exists(p)) return p; } catch { }
            }
            return null;
        }

        public void CarregaPalavrasSorteio(string fileName)
        {
            try
            {
                var lines = ReadFileLines(fileName);
                var list = lines
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .Select(p => NormalizeWord(p))
                    .Where(p => p.Length == 5 && p.All(c => c >= 'A' && c <= 'Z'))
                    .Distinct()
                    .ToList();

                if (list.Count > 0) Dicionario = list;
            }
            catch
            {
            }
        }

        private static string[] ReadFileLines(string fileName)
        {
            var tried = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string basePath = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;

            var candidates = new List<string>
            {
                Path.Combine(basePath, fileName),
                Path.Combine(Directory.GetCurrentDirectory(), fileName),
                Path.Combine(basePath, "TermoLib", fileName)
            };

            string dir = basePath;
            for (int i = 0; i < 6; i++)
            {
                try
                {
                    if (string.IsNullOrEmpty(dir)) break;
                    candidates.Add(Path.Combine(dir, fileName));
                    var parent = Directory.GetParent(dir);
                    dir = parent?.FullName;
                }
                catch
                {
                    break;
                }
            }

            try
            {
                var asmDir = Path.GetDirectoryName(typeof(Termo).Assembly.Location);
                if (!string.IsNullOrEmpty(asmDir)) candidates.Add(Path.Combine(asmDir, fileName));
            }
            catch { }

            foreach (var path in candidates)
            {
                if (string.IsNullOrWhiteSpace(path)) continue;
                if (tried.Contains(path)) continue;
                tried.Add(path);
                try
                {
                    if (File.Exists(path))
                    {
                        return File.ReadAllLines(path, Encoding.UTF8);
                    }
                }
                catch
                {
                }
            }

            return Array.Empty<string>();
        }

        private static string NormalizeWord(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return string.Empty;
            texto = texto.Trim().ToUpperInvariant();

            texto = texto.Where(c => !char.IsControl(c) && c != '\uFEFF').Aggregate(new StringBuilder(), (sb, c) => sb.Append(c)).ToString();

            var normalized = texto.Normalize(NormalizationForm.FormD);
            var sb2 = new StringBuilder();
            foreach (var ch in normalized)
            {
                var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (cat != UnicodeCategory.NonSpacingMark)
                    sb2.Append(ch);
            }
            var semAcentos = sb2.ToString().Normalize(NormalizationForm.FormC);

            var cleaned = new string(semAcentos.Where(c => c >= 'A' && c <= 'Z').ToArray());
            return cleaned;
        }

        public void SorteiaPalavra()
        {
            var source = (Dicionario != null && Dicionario.Count > 0) ? Dicionario : Palavras;
            if (source == null || source.Count == 0) { palavraSorteada = ""; return; }
            palavraSorteada = source[rdn.Next(0, source.Count)];
        }
        public bool ChecaPalavra(string palavra)
        {
            if (string.IsNullOrWhiteSpace(palavra)) return false;

            var palavraNorm = NormalizeWord(palavra);

            if (string.IsNullOrEmpty(palavraSorteada) || palavraNorm.Length != 5 || !Palavras.Contains(palavraNorm))
            {
                return false;
            }

            var palavraTabuleiro = new List<Letra>();
            char[] letrasRestantes = palavraSorteada.ToCharArray();

            for (int i = 0; i < palavraNorm.Length; i++)
            {
                char cor = ' ';
                if (palavraNorm[i] == palavraSorteada[i])
                {
                    cor = 'V';
                    letrasRestantes[i] = '*';
                    teclado[palavraNorm[i]] = 'V';
                }
                palavraTabuleiro.Add(new Letra(palavraNorm[i], cor));
            }

            for (int i = 0; i < palavraNorm.Length; i++)
            {
                if (palavraTabuleiro[i].Cor == 'V') continue;
                char letra = palavraNorm[i];
                int index = Array.IndexOf(letrasRestantes, letra);
                char cor = 'P';
                if (index != -1)
                {
                    cor = 'A';
                    letrasRestantes[index] = '*';
                }
                palavraTabuleiro[i].Cor = cor;
                var corAtualDaTecla = teclado[letra];
                if (corAtualDaTecla != 'V')
                {
                    if (cor == 'A' || corAtualDaTecla == 'C')
                    {
                        teclado[letra] = cor;
                    }
                }
            }
            tabuleiro.Add(palavraTabuleiro);
            palavraAtual++;
            return true;
        }

        public bool EstaNoDicionario(string palavra)
        {
            if (string.IsNullOrWhiteSpace(palavra)) return false;
            var p = NormalizeWord(palavra);
            return Palavras.Contains(p);
        }

        public int DicionarioCount() => Palavras?.Count ?? 0;
    }
}