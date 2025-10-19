using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;      // Adicionado para carregar palavras online
using System.Threading.Tasks; // Adicionado para operações online

namespace TermoLib
{
    public class Letra
    {
        public char Caracter;
        public char Cor;
        public Letra(char caracter, char cor)
        {
            Caracter = caracter;
            Cor = cor;
        }
    }

    public class Termo
    {
        private static Random rdn = new Random();

        // --- CORREÇÃO DO ERRO 'var' APLICADA AQUI ---
        public List<string> palavras;
        public string palavraSorteada;
        public List<List<Letra>> tabuleiro;
        public Dictionary<char, char> teclado;
        public int palavraAtual;

        public Termo()
        {
            // Tenta carregar as palavras da URL. O .Wait() é usado para esperar a conclusão.
            CarregaPalavrasOnline("https://raw.githubusercontent.com/pythonprobr/palavras/master/palavras.txt").Wait();

            SorteiaPalavra();
            palavraAtual = 1;
            tabuleiro = new List<List<Letra>>();
            teclado = new Dictionary<char, char>();
            for (int i = 'A'; i <= 'Z'; i++)
            {
                teclado.Add((char)i, 'C');
            }
        }

        // --- FUNCIONALIDADE 9: Importar palavras online ---
        public async Task CarregaPalavrasOnline(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string palavrasDaWeb = await client.GetStringAsync(url);
                    palavras = palavrasDaWeb.Split('\n')
                                            .Where(p => p.Length == 5)
                                            .Select(p => p.ToUpper().Trim())
                                            .ToList();
                }
            }
            catch (Exception)
            {
                // Se falhar, carrega do arquivo local como um fallback.
                CarregaPalavras("Palavras.txt");
            }
        }

        public void CarregaPalavras(string fileName)
        {
            try
            {
                palavras = File.ReadAllLines(fileName).ToList();
            }
            catch (Exception)
            {
                // Se o arquivo local também falhar, usa uma lista padrão.
                palavras = new List<string> { "TERMO", "JOGAR", "LETRA", "IDEIA", "LIVRO" };
            }
        }

        public void SorteiaPalavra()
        {
            if (palavras == null || palavras.Count == 0) { palavraSorteada = "IDEIA"; return; }
            palavraSorteada = palavras[rdn.Next(0, palavras.Count)];
        }

        // --- FUNCIONALIDADES 7 e 8: Validação de palavras ---
        public bool ChecaPalavra(string palavra)
        {
            if (string.IsNullOrEmpty(palavraSorteada)) return false;

            if (palavra.Length != 5)
            {
                return false; // Valida se a palavra tem 5 letras
            }

            if (!palavras.Contains(palavra.ToUpper()))
            {
                return false; // Valida se a palavra existe na lista
            }

            var palavraTabuleiro = new List<Letra>();
            char[] letrasRestantes = palavraSorteada.ToCharArray();

            for (int i = 0; i < palavra.Length; i++)
            {
                char cor;
                if (palavra[i] == palavraSorteada[i])
                {
                    cor = 'V';
                    letrasRestantes[i] = '*';
                    teclado[palavra[i]] = 'V';
                }
                else
                {
                    cor = ' ';
                }
                palavraTabuleiro.Add(new Letra(palavra[i], cor));
            }

            for (int i = 0; i < palavra.Length; i++)
            {
                if (palavraTabuleiro[i].Cor == 'V') continue;
                char letra = palavra[i];
                int index = Array.IndexOf(letrasRestantes, letra);
                char cor;
                if (index != -1)
                {
                    cor = 'A';
                    letrasRestantes[index] = '*';
                }
                else
                {
                    cor = 'P';
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
    }
}