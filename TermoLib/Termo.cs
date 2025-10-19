using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TermoLib;

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

        public List<string> palavras;
        public string palavraSorteada;
        public List<List<Letra>> tabuleiro;
        public Dictionary<char, char> teclado;
        public int palavraAtual;

        public Termo()
        {
            CarregaPalavras("Palavras.txt");
            SorteiaPalavra();
            palavraAtual = 1;
            tabuleiro = new List<List<Letra>>();
            teclado = new Dictionary<char, char>();
            for (int i = 'A'; i <= 'Z'; i++)
            {
                teclado.Add((char)i, 'C');
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
                palavras = new List<string> { "TERMO", "JOGAR", "LETRA", "IDEIA", "LIVRO" };
            }
        }

        public void SorteiaPalavra()
        {
            if (palavras == null || palavras.Count == 0) { palavraSorteada = "IDEIA"; return; }
            palavraSorteada = palavras[rdn.Next(0, palavras.Count)];
        }

        public void ChecaPalavra(string palavra)
        {
            if (string.IsNullOrEmpty(palavraSorteada)) return;
            if (palavra.Length != 5) return;

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
        }
    }
}
