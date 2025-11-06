using TermoLib;

namespace TermoTest
{
    [TestClass]
    public sealed class
    {
        [TestMethod]
        public void TestReadFile()
        {
            Termo termo = new Termo();
            Console.WriteLine($"Total de palavras carregadas: {termo.palavras.Count}");
            Assert.IsTrue(termo.palavras.Count > 0);
        }

        [TestMethod]
        public void TestJogo()
        {
            Termo termo = new Termo();
            termo.palavraSorteada = "IDEIA";
            ImprimirJogo(termo);

            termo.ChecaPalavra("TERMO");

            Console.WriteLine("\n--- APÓS CHECAR A PALAVRA 'TERMO' ---");
            ImprimirJogo(termo);
        }

        [TestMethod]
        public void TestePalavraComTamanhoInvalido()
        {
            Termo termo = new Termo();
            bool resultado = termo.ChecaPalavra("SOL");
            Assert.IsFalse(resultado);
            Console.WriteLine("Teste de palavra com 3 letras. Resultado (esperado False): " + resultado);
        }

        [TestMethod]
        public void TestePalavraInexistente()
        {
            Termo termo = new Termo();
            bool resultado = termo.ChecaPalavra("XXXXX");
            Assert.IsFalse(resultado);
            Console.WriteLine("Teste de palavra inexistente. Resultado (esperado False): " + resultado);
        }

        [TestMethod]
        public void DiagnosticoDicionario()
        {
            var termo = new TermoLib.Termo();
            Console.WriteLine($"BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            Console.WriteLine($"CurrentDirectory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"DicionarioCount = {termo.DicionarioCount()}");
            Console.WriteLine($"Esta 'CARGO' no dicionario? {termo.EstaNoDicionario("CARGO")}");
            Console.WriteLine("Primeiras 50 palavras carregadas:");
            foreach (var p in termo.palavras.Take(50))
            {
                Console.WriteLine(p);
            }
            Console.WriteLine($"ChecaPalavra('CARGO') = {termo.ChecaPalavra("CARGO")}");
        }

        public void ImprimirJogo(Termo termo)
        {
            Console.WriteLine("Palavra Sorteada: " + termo.palavraSorteada);
            foreach (var palavra in termo.tabuleiro)
            {
                foreach (var letra in palavra)
                {
                    Console.Write(letra.Caracter + "(" + letra.Cor + ") ");
                }
                Console.WriteLine();
            }
            foreach (var tecla in termo.teclado)
            {
                //Console.Write(tecla.Key + ": " + tecla.Value + " | ");
            }
        }
    }
}