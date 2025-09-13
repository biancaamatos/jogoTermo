using TermoLib;
using static System.Windows.Forms.LinkLabel;
namespace TermoApp
{
    public partial class Form1 : Form
    {
        public Termo termo;
        int coluna = 1;

        public Form1()
        {
            InitializeComponent();
            termo = new Termo();
        }

        private void btnTeclado_Click(object sender, EventArgs e)
        {
            if (coluna > 5) return;
            // Botão do teclao que foi clicado
            var button = (Button)sender;
            var linha = termo.palavraAtual;
            var nomeButton = $"btn{linha}{coluna}";
            //  Botão do tabuleiro da linha e coluna atual
            var buttonTabuleiro = RetornaBotao(nomeButton);
            buttonTabuleiro.Text = button.Text;
            coluna++;
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            var palavra = string.Empty;
            for (int i = 1; i <= 5; i++)
            {
                string nomeBotao = $"btn{termo.palavraAtual}{i}";
                var botao = RetornaBotao(nomeBotao);
                palavra += botao.Text;
            }
            termo.ChecaPalavra(palavra);
            AtualizaTabuleiro();
            AtualizaTeclado();
            coluna = 1;
        }

        private Button RetornaBotao(string name)
        {
            return (Button)Controls.Find(name, true)[0];
        }
        private void AtualizaTabuleiro()
        {
            for (int col = 1; col <= 5; col++)
            {
                var letra = termo.tabuleiro[termo.palavraAtual - 2][col - 1];
                var nomeBotaoTab = $"btn{termo.palavraAtual - 1}{col}";
                var botaoTab = RetornaBotao(nomeBotaoTab);
                var nomeBotaoKey = $"btn{letra.Caracter}";
                var botaoKey = RetornaBotao(nomeBotaoKey);


                if (letra.Cor == 'A')
                {
                    botaoTab.BackColor = Color.Yellow;
                    botaoKey.BackColor = Color.Yellow;
                }
                else if (letra.Cor == 'V')
                {
                    botaoTab.BackColor = Color.Green;
                    botaoKey.BackColor = Color.Green;
                }

                else
                {
                    botaoTab.BackColor = Color.Gray;
                    botaoKey.BackColor = Color.Gray;
                }
            }
        }

        private void AtualizaTeclado()
        {

        }
    }
}