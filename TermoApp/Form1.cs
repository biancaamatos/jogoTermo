using TermoLib;

me de o codigo todo com essa parte em cima desseusing System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TermoLib;

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
            EfeitoSelecionado();

            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            foreach (Control ctrl in this.Controls)
                if (ctrl is Button btn && btn.Name.StartsWith("btn"))
                    ConfiguraBotaoCustom(btn);
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
                AdicionaLetraVirtual(e.KeyCode.ToString());
            else if (e.KeyCode == Keys.Back)
                bntBackSpace_Click(null, null);
            else if (e.KeyCode == Keys.Enter)
                btnEnter_Click(null, null);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;

            if (key >= Keys.A && key <= Keys.Z)
            {
                string letra = key.ToString().ToUpper();
                var botao = this.Controls.Find("btn" + letra, true).FirstOrDefault() as Button;
                if (botao != null)
                {
                    botao.PerformClick();
                    return true;
                }
            }

            if (key == Keys.Enter)
            {
                var btnEnter = this.Controls.Find("btnEnter", true).FirstOrDefault() as Button;
                if (btnEnter != null)
                {
                    btnEnter.PerformClick();
                    return true;
                }
            }

            if (key == Keys.Left)
            {
                if (coluna > 1) coluna--;
                return true;
            }

            if (key == Keys.Right)
            {
                if (coluna < 6) coluna++;
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AdicionaLetraVirtual(string letra)
        {
            if (termo.palavraAtual > 6 || coluna > 5) return;

            var nomeBotao = $"btn{termo.palavraAtual}{coluna}";
            var botaoTabuleiro = RetornaBotao(nomeBotao);
            if (botaoTabuleiro != null)
            {
                botaoTabuleiro.Text = letra.ToUpper();
                coluna++;
            }
        }

        private void btnTeclado_Click(object sender, EventArgs e)
        {
            if (termo.palavraAtual > 6 || coluna > 5) return;
            var button = (Button)sender;
            var linha = termo.palavraAtual;
            var nomeButton = $"btn{linha}{coluna}";
            var buttonTabuleiro = RetornaBotao(nomeButton);
            if (buttonTabuleiro != null)
            {
                buttonTabuleiro.Text = button.Text;
                coluna++;
            }
        }

        private async void btnEnter_Click(object sender, EventArgs e)
        {
            string palavra = "";
            for (int i = 1; i <= 5; i++)
            {
                var nomeBotao = $"btn{termo.palavraAtual}{i}";
                var botao = RetornaBotao(nomeBotao);
                if (botao != null) palavra += botao.Text;
            }

            if (palavra.Length != 5)
            {
                await AnimaShakeLinha(termo.palavraAtual);
                await MostrarMensagemTemporaria("Digite 5 letras");
                return;
            }

            termo.ChecaPalavra(palavra);
            await AtualizaTabuleiro();
            AtualizaTeclado();

            if (palavra == termo.palavraSorteada)
            {
                await MostrarMensagemTemporaria("VOCÊ VENCEU!", 5000);
                DesativarControles();
            }
            else if (termo.palavraAtual > 6)
            {
                await MostrarMensagemTemporaria($"A palavra era: {termo.palavraSorteada}", 5000);
                DesativarControles();
            }

            coluna = 1;
        }

        private void bntBackSpace_Click(object sender, EventArgs e)
        {
            if (coluna <= 1) return;
            coluna--;
            var linha = termo.palavraAtual;
            var nomeButton = $"btn{linha}{coluna}";
            var botao = RetornaBotao(nomeButton);
            if (botao != null) botao.Text = string.Empty;
        }

        private async void btnReiniciar_Click(object sender, EventArgs e)
        {
            {
                coluna = 1;

                // Pegamos apenas os botões preenchidos do tabuleiro
                var botoesParaLimpar = new List<Button>();
                for (int linha = 1; linha <= 6; linha++)
                {
                    for (int col = 1; col <= 5; col++)
                    {
                        var botao = RetornaBotao($"btn{linha}{col}");
                        if (botao != null && !string.IsNullOrEmpty(botao.Text))
                        {
                            botoesParaLimpar.Add(botao);
                            botao.Enabled = false;
                        }
                    }
                }

                // Animação de reset apenas nesses botões
                var tasks = botoesParaLimpar.Select(b => AnimaReiniciarPadronizado(b));
                await Task.WhenAll(tasks);

                // Restaura a aparência original de cada botão do tabuleiro
                foreach (var botao in botoesParaLimpar)
                {
                    botao.BackColor = Color.Brown;
                    botao.ForeColor = SystemColors.ButtonHighlight; // Letras brancas
                    botao.FlatAppearance.BorderColor = Color.White;
                    botao.FlatAppearance.BorderSize = 3;
                    botao.Text = string.Empty;
                    botao.Enabled = true;
                    botao.Refresh();
                }

                // Reset visual do teclado virtual com animação
                for (char c = 'A'; c <= 'Z'; c++)
                {
                    var btnTecla = RetornaBotao($"btn{c}");
                    if (btnTecla != null)
                    {
                        await AnimaFadeOutCor(btnTecla); // animação da cor de volta ao Brown
                        btnTecla.ForeColor = SystemColors.ButtonHighlight; // letras brancas
                    }
                }

                // Reaplica hover
                EfeitoSelecionado();
            }
        }

        private async Task AnimaReiniciarPadronizado(Button botao)
        {
            if (botao == null) return;

            int originalTop = botao.Top;
            int originalHeight = botao.Height;

            for (int i = 0; i <= 10; i++)
            {
                botao.Top = originalTop + i;
                botao.Height = originalHeight - i * 2;
                botao.FlatAppearance.BorderSize = 3;
                botao.FlatAppearance.BorderColor = Color.White;
                botao.ForeColor = SystemColors.ButtonHighlight;
                await Task.Delay(15);
            }

            botao.Text = string.Empty;
            botao.BackColor = Color.Brown;
            botao.FlatAppearance.BorderSize = 3;
            botao.FlatAppearance.BorderColor = Color.White;
            botao.ForeColor = SystemColors.ButtonHighlight;

            for (int i = 10; i >= 0; i--)
            {
                botao.Top = originalTop + i;
                botao.Height = originalHeight - i * 2;
                botao.FlatAppearance.BorderSize = 3;
                botao.FlatAppearance.BorderColor = Color.White;
                botao.ForeColor = SystemColors.ButtonHighlight;
                await Task.Delay(15);
            }

            botao.Top = originalTop;
            botao.Height = originalHeight;
        }

        private void ConfiguraBotaoCustom(Button botao)
        {
            botao.ForeColor = SystemColors.ButtonHighlight;
            botao.FlatAppearance.BorderColor = Color.White;
            botao.FlatAppearance.BorderSize = 3;
            botao.FlatStyle = FlatStyle.Flat;

            botao.Paint += (s, e) =>
            {
                Button b = s as Button;
                if (b == null) return;
                e.Graphics.Clear(b.BackColor);
                using (Brush brush = new SolidBrush(SystemColors.ButtonHighlight))
                using (StringFormat sf = new StringFormat()
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    e.Graphics.DrawString(b.Text, b.Font, brush, b.ClientRectangle, sf);
                }

                using (Pen pen = new Pen(b.FlatAppearance.BorderColor, 3))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, b.Width - 1, b.Height - 1);
                }
            };
        }

        private void DesativarControles()
        {
            foreach (Control ctrl in this.Controls)
                if (ctrl is Button btn && btn.Name.StartsWith("btn"))
                    btn.Enabled = false;
        }

        private Button RetornaBotao(string name)
        {
            var controls = Controls.Find(name, true);
            return (controls.Length > 0) ? (Button)controls[0] : null;
        }

        private async Task AtualizaTabuleiro()
        {
            int linhaJogada = termo.tabuleiro.Count;
            var palavraJogada = termo.tabuleiro[linhaJogada - 1];
            for (int col = 0; col < 5; col++)
            {
                var botaoTab = RetornaBotao($"btn{linhaJogada}{col + 1}");
                if (botaoTab == null) continue;
                if (string.IsNullOrEmpty(botaoTab.Text)) continue;

                Color corFinal = (palavraJogada[col].Cor == 'V') ? Color.Green :
                                 (palavraJogada[col].Cor == 'A') ? Color.Yellow : Color.Gray;

                botaoTab.FlatAppearance.BorderSize = 3;
                botaoTab.FlatAppearance.BorderColor = corFinal;
                await Task.Delay(150);

                await AnimaFlipEstiloTermo(botaoTab, corFinal);
                await Task.Delay(100);

                botaoTab.FlatAppearance.BorderSize = 3;
                botaoTab.FlatAppearance.BorderColor = Color.White;
            }
        }

        private void AtualizaTeclado()
        {
            foreach (var letra in termo.teclado)
            {
                var botao = RetornaBotao($"btn{letra.Key}");
                if (botao == null || string.IsNullOrEmpty(botao.Text)) continue;
                Color cor = (letra.Value == 'V') ? Color.Green :
                            (letra.Value == 'A') ? Color.Yellow :
                            (letra.Value == 'P') ? Color.Gray : SystemColors.Control;

                if (cor != SystemColors.Control)
                {
                    botao.BackColor = cor;
                    botao.FlatAppearance.BorderSize = 3;
                    botao.FlatAppearance.BorderColor = Color.White;
                }
            }
        }

        private async Task MostrarMensagemTemporaria(string mensagem, int duracaoMs = 2000)
        {
            if (MensNaoAceito == null) return;
            MensNaoAceito.Text = mensagem;
            MensNaoAceito.Visible = true;
            await Task.Delay(duracaoMs);
            MensNaoAceito.Visible = false;
        }

        private async Task AnimaShakeLinha(int linha)
        {
            var botoes = new List<Button>();
            for (int i = 1; i <= 5; i++) botoes.Add(RetornaBotao($"btn{linha}{i}"));
            int shake = 5;
            for (int i = 0; i < 3; i++)
            {
                foreach (var btn in botoes) if (btn != null) btn.Left -= shake; await Task.Delay(40);
                foreach (var btn in botoes) if (btn != null) btn.Left += shake * 2; await Task.Delay(40);
                foreach (var btn in botoes) if (btn != null) btn.Left -= shake; await Task.Delay(40);
            }
        }

        private async Task AnimaFlipEstiloTermo(Button botao, Color corFinal)
        {
            if (botao == null) return;
            for (int i = 0; i < 10; i++)
            {
                botao.Top += 2;
                botao.Height -= 4;
                botao.FlatAppearance.BorderSize = 3;
                botao.FlatAppearance.BorderColor = Color.White;
                await Task.Delay(8);
            }
            botao.BackColor = corFinal;
            for (int i = 0; i < 10; i++)
            {
                botao.Top -= 2;
                botao.Height += 4;
                botao.FlatAppearance.BorderSize = 3;
                botao.FlatAppearance.BorderColor = Color.White;
                await Task.Delay(8);
            }
            botao.Top -= 4;
            await Task.Delay(40);
            botao.Top += 4;

            botao.FlatAppearance.BorderSize = 3;
            botao.FlatAppearance.BorderColor = Color.White;
        }

        private void EfeitoSelecionado()
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn && btn.Name.StartsWith("btn") && btn.Name.Length == 6)
                {
                    Color corOriginal = Color.Brown;
                    Color bordaOriginal = Color.White;

                    btn.MouseEnter += (s, e) =>
                    {
                        btn.BackColor = ControlPaint.Light(corOriginal, 0.35f);
                        btn.FlatAppearance.BorderColor = ControlPaint.Light(bordaOriginal, 0.5f);
                        btn.FlatAppearance.BorderSize = 4;
                        btn.Cursor = Cursors.Hand;
                    };

                    btn.MouseLeave += (s, e) =>
                    {
                        btn.BackColor = corOriginal;
                        btn.FlatAppearance.BorderColor = bordaOriginal;
                        btn.FlatAppearance.BorderSize = 3;
                        btn.Cursor = Cursors.Default;
                    };
                }
            }
        }

        private async Task AnimaFadeOutCor(Button botao)
        {
            if (botao == null) return;

            Color start = botao.BackColor;
            Color end = Color.Brown;

            // Menos passos e delay menor = animação bem rápida
            for (int i = 0; i <= 3; i++)
            {
                double p = (double)i / 3;
                int r = (int)(start.R + (end.R - start.R) * p);
                int g = (int)(start.G + (end.G - start.G) * p);
                int b = (int)(start.B + (end.B - start.B) * p);

                botao.BackColor = Color.FromArgb(r, g, b);
                botao.FlatAppearance.BorderSize = 3;
                botao.FlatAppearance.BorderColor = Color.White;
                botao.ForeColor = SystemColors.ButtonHighlight; // letra branca sempre

                await Task.Delay(2); // delay mínimo
            }

            botao.BackColor = Color.Brown;
            botao.ForeColor = SystemColors.ButtonHighlight; // garante letra branca
        }
    }
}
