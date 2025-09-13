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
            termo.CarregaPalavras("palavras.txt");
            Console.WriteLine(String.Join("\n", termo.palavras));
        }
        [TestMethod]
        public void TestJogo()
        {
            Termo termo = new Termo();
            ImprimirJogo(termo);
            termo.ChecaPalavra("APOIO");
            ImprimirJogo(termo);
        }

        public void ImprimirJogo(Termo termo)
        {
            Console.WriteLine("Palavra Sorteada: " + termo.palavraSorteada);
            foreach (var palavra in termo.tabuleiro)
            {
                foreach (var letra in palavra)
                {
                    Console.Write(letra.caracter + "(" + letra.cor + ") ");
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