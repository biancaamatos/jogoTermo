using TermoLib;

namespace TermoTest
{
    [TestClass]
    public sealed class TestTermo
    {
        [TestMethod]
        public void TestReadFile()
        {
            Termo termo = new Termo();
            // A lista de palavras já é carregada no início.
            // Este teste agora serve para garantir que o carregamento (online ou local) funcionou.
            Console.WriteLine($"Total de palavras carregadas: {termo.palavras.Count}");
            // Podemos verificar se a lista tem palavras
            Assert.IsTrue(termo.palavras.Count > 0);
        }

        // SEU MÉTODO ORIGINAL, PRATICAMENTE IDÊNTICO
        [TestMethod]
        public void TestJogo()
        {
            Termo termo = new Termo();
            // Forçamos a palavra para o teste ser previsível
            termo.palavraSorteada = "IDEIA";
            ImprimirJogo(termo);

            // A chamada continua igual. O resultado (true/false) será ignorado, mas o código funciona.
            termo.ChecaPalavra("TERMO");

            Console.WriteLine("\n--- APÓS CHECAR A PALAVRA 'TERMO' ---");
            ImprimirJogo(termo);
        }

        // --- INCLUSÃO 1: NOVO TESTE PARA PALAVRA COM TAMANHO ERRADO ---
        [TestMethod]
        public void TestePalavraComTamanhoInvalido()
        {
            Termo termo = new Termo();
            bool resultado = termo.ChecaPalavra("SOL");
            // Este teste passa se a palavra for (corretamente) rejeitada.
            Assert.IsFalse(resultado);
            Console.WriteLine("Teste de palavra com 3 letras. Resultado (esperado False): " + resultado);
        }

        // --- INCLUSÃO 2: NOVO TESTE PARA PALAVRA QUE NÃO EXISTE ---
        [TestMethod]
        public void TestePalavraInexistente()
        {
            Termo termo = new Termo();
            // Assumindo que "XXXXX" não existe na sua lista de palavras
            bool resultado = termo.ChecaPalavra("XXXXX");
            // Este teste passa se a palavra for (corretamente) rejeitada.
            Assert.IsFalse(resultado);
            Console.WriteLine("Teste de palavra inexistente. Resultado (esperado False): " + resultado);
        }


        // Seu método de imprimir continua igual
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