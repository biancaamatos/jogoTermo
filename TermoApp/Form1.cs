// TermoApp/Form1.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media; // NECESSÁRIO PARA SOUNDPLAYER
using System.Reflection; // NECESSÁRIO PARA CARREGAR RECURSOS
using System.Threading.Tasks;
using System.Windows.Forms;
using TermoLib; // Garanta que o using para TermoLib está presente

namespace TermoApp
{
    public partial class Form1 : Form
    {
        public Termo termo;
        int coluna = 1;

        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form1_Load);
            EfeitoSelecionado();
            for (int i = 1; i <= 6; i++) { for (int j = 1; j <= 5; j++) { Button botao = RetornaBotao($"btn{i}{j}"); if (botao != null) { botao.Click += new EventHandler(Tabuleiro_Click); } } }
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;

            // --- MODIFICAÇÃO APLICADA (Placar) ---
            // Vamos aplicar o estilo customizado em todos os botões que começam com "btn",
            // EXCETO o 'btnPlacar', que deve manter o visual padrão.
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Button btn && btn.Name.StartsWith("btn") && btn.Name != "btnPlacar")
                {
                    ConfiguraBotaoCustom(btn);
                }
            }
            // --- FIM DA MODIFICAÇÃO ---
        }

        // --- MÉTODO PARA TOCAR SONS EMBUTIDOS ---
        private void TocarSom(string nomeArquivoWav)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = $"TermoApp.Sons.{nomeArquivoWav}"; // Adapte "TermoApp" se seu namespace for diferente
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null) { using (var player = new SoundPlayer(stream)) { player.Play(); } }
                    else { System.Diagnostics.Debug.WriteLine($"Recurso de som não encontrado: {resourceName}"); }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Erro ao tocar som '{nomeArquivoWav}': {ex.Message}"); }
        }

        private async void Form1_Load(object sender, EventArgs e) { await ReiniciarJogoAsync(primeiraVez: true); }
        private async void btnReiniciar_Click(object sender, EventArgs e) { await ReiniciarJogoAsync(); }

        // --- MÉTODO ReiniciarJogoAsync COM NOVA ANIMAÇÃO SELETIVA ---
        private async Task ReiniciarJogoAsync(bool primeiraVez = false)
        {
            termo = new Termo();
            await termo.IniciarAsync();
            coluna = 1;

            var MensNaoAceito = this.Controls.Find("MensNaoAceito", true).FirstOrDefault() as Label;
            if (MensNaoAceito != null) MensNaoAceito.Visible = false;

            if (primeiraVez)
            {
                // Garante que a primeira linha tenha borda branca normal ao iniciar
                for (int i = 1; i <= 5; i++)
                {
                    var botao = RetornaBotao($"btn1{i}");
                    if (botao != null)
                    {
                        botao.FlatAppearance.BorderColor = Color.White;
                        botao.FlatAppearance.BorderSize = 3;
                    }
                }
                AtualizaSelecaoVisual(); // Chama aqui para a primeira vez
                return;
            }

            var botoesTabuleiro = new List<Button>();
            var tasksAnimacao = new List<Task>();

            for (int linha = 1; linha <= 6; linha++)
            {
                for (int col = 1; col <= 5; col++)
                {
                    var botao = RetornaBotao($"btn{linha}{col}");
                    if (botao != null)
                    {
                        botoesTabuleiro.Add(botao);
                        if (!string.IsNullOrEmpty(botao.Text))
                        {
                            tasksAnimacao.Add(AnimaLimparLetra(botao));
                        }
                    }
                }
            }

            await Task.WhenAll(tasksAnimacao);

            foreach (var botao in botoesTabuleiro)
            {
                botao.BackColor = Color.Brown;
                botao.ForeColor = SystemColors.ButtonHighlight;
                botao.FlatAppearance.BorderColor = Color.White;
                botao.FlatAppearance.BorderSize = 3;
                botao.Text = string.Empty;
                botao.Enabled = true;
            }

            var botoesTeclado = new List<Button>();
            for (char c = 'A'; c <= 'Z'; c++)
            {
                var btnTecla = RetornaBotao($"btn{c}");
                if (btnTecla != null) { botoesTeclado.Add(btnTecla); btnTecla.BackColor = Color.Brown; }
            }
            botoesTeclado.Add(RetornaBotao("btnEnter"));
            botoesTeclado.Add(RetornaBotao("bntBackSpace"));

            foreach (var btnTecla in botoesTeclado.Where(b => b != null))
            {
                btnTecla.BackColor = Color.Brown;
                btnTecla.Enabled = true;
            }

            AtualizaSelecaoVisual();
        }

        private void Tabuleiro_Click(object sender, EventArgs e)
        {
            var botao = (Button)sender;
            string nome = botao.Name;
            int linhaBotao = int.Parse(nome.Substring(3, 1));
            int colunaBotao = int.Parse(nome.Substring(4, 1));
            if (linhaBotao == termo.palavraAtual) { coluna = colunaBotao; AtualizaSelecaoVisual(); }
        }

        private void AtualizaSelecaoVisual()
        {
            if (termo == null || termo.palavraAtual > 6) return;
            for (int i = 1; i <= 5; i++)
            {
                var botao = RetornaBotao($"btn{termo.palavraAtual}{i}");
                if (botao != null) { if (i == coluna) { botao.FlatAppearance.BorderColor = Color.Orange; botao.FlatAppearance.BorderSize = 4; } else { botao.FlatAppearance.BorderColor = Color.White; botao.FlatAppearance.BorderSize = 3; } }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z) AdicionaLetraVirtual(e.KeyCode.ToString());
            else if (e.KeyCode == Keys.Back) bntBackSpace_Click(null, null);
            else if (e.KeyCode == Keys.Enter) btnEnter_Click(null, null);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Keys key = keyData & Keys.KeyCode;
            if (key >= Keys.A && key <= Keys.Z) { var botao = this.Controls.Find("btn" + key.ToString().ToUpper(), true).FirstOrDefault() as Button; if (botao != null && botao.Enabled) { botao.PerformClick(); return true; } }
            if (key == Keys.Enter) { var btnEnter = this.Controls.Find("btnEnter", true).FirstOrDefault() as Button; if (btnEnter != null && btnEnter.Enabled) { btnEnter.PerformClick(); return true; } }
            if (key == Keys.Left) { if (coluna > 1) coluna--; AtualizaSelecaoVisual(); return true; }
            if (key == Keys.Right) { if (coluna < 5) coluna++; else if (coluna == 5) coluna++; AtualizaSelecaoVisual(); return true; }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void AdicionaLetraVirtual(string letra)
        {
            // --- MODIFICAÇÃO APLICADA (Esconde mensagem) ---
            var MensNaoAceito = this.Controls.Find("MensNaoAceito", true).FirstOrDefault() as Label;
            if (MensNaoAceito != null && MensNaoAceito.Visible)
            {
                MensNaoAceito.Visible = false;
            }
            // --- FIM DA MODIFICAÇÃO ---

            if (termo.palavraAtual > 6 || coluna > 5) return;
            var nomeBotao = $"btn{termo.palavraAtual}{coluna}";
            var botaoTabuleiro = RetornaBotao(nomeBotao);
            if (botaoTabuleiro != null)
            {
                botaoTabuleiro.Text = letra.ToUpper();
                if (coluna <= 5) { coluna++; }
                AtualizaSelecaoVisual();
                TocarSom("click.wav");
            }
        }

        private void btnTeclado_Click(object sender, EventArgs e)
        {
            // --- MODIFICAÇÃO APLICADA (Esconde mensagem) ---
            var MensNaoAceito = this.Controls.Find("MensNaoAceito", true).FirstOrDefault() as Label;
            if (MensNaoAceito != null && MensNaoAceito.Visible)
            {
                MensNaoAceito.Visible = false;
            }
            // --- FIM DA MODIFICAÇÃO ---

            if (termo.palavraAtual > 6 || coluna > 5) return;
            var button = (Button)sender;
            var linha = termo.palavraAtual;
            var nomeButton = $"btn{linha}{coluna}";
            var buttonTabuleiro = RetornaBotao(nomeButton);
            if (buttonTabuleiro != null)
            {
                buttonTabuleiro.Text = button.Text;
                if (coluna <= 5) { coluna++; }
                AtualizaSelecaoVisual();
                TocarSom("click.wav");
            }
        }

        private async void btnEnter_Click(object sender, EventArgs e)
        {
            TocarSom("enter.wav"); // Som de Enter

            string palavra = "";
            for (int i = 1; i <= 5; i++) { var nomeBotao = $"btn{termo.palavraAtual}{i}"; var botao = RetornaBotao(nomeBotao); if (botao != null) palavra += botao.Text; }

            // --- MODIFICAÇÃO APLICADA (Mensagem persistente) ---
            var MensNaoAceito = this.Controls.Find("MensNaoAceito", true).FirstOrDefault() as Label;

            // 1. Checa se a palavra está incompleta (não tem 5 letras)
            if (palavra.Length != 5)
            {
                TocarSom("erro.wav");
                await AnimaShakeLinha(termo.palavraAtual);
                // Mensagem específica para palavra incompleta (agora é persistente)
                if (MensNaoAceito != null)
                {
                    MensNaoAceito.Text = "Digite uma palavra de 5 letras!";
                    MensNaoAceito.Visible = true;
                }
                return;
            }

            // 2. Se tem 5 letras, checa se é uma palavra válida (está no dicionário)
            if (!termo.EstaNoDicionario(palavra))
            {
                TocarSom("erro.wav");
                await AnimaShakeLinha(termo.palavraAtual);
                // Mensagem para palavra completa, mas inválida (agora é persistente)
                if (MensNaoAceito != null)
                {
                    MensNaoAceito.Text = "Palavra inválida!";
                    MensNaoAceito.Visible = true;
                }
                return;
            }
            // --- FIM DA MODIFICAÇÃO ---

            // Se passou nas checagens, o jogo continua...
            if (MensNaoAceito != null) MensNaoAceito.Visible = false;

            termo.ChecaPalavra(palavra);
            await AtualizaTabuleiro();
            AtualizaTeclado();

            if (palavra == termo.palavraSorteada)
            {
                TocarSom("vitoria.wav");
                StatsManager.RecordWin(termo.tabuleiro.Count);
                await MostrarMensagemTemporaria("VOCÊ VENCEU!", 2000); // Fim de jogo
                var formPlacarVitoria = new FormTabela();
                formPlacarVitoria.ShowDialog(this);
                await ReiniciarJogoAsync();
                return;
            }
            else if (termo.palavraAtual > 6)
            {
                TocarSom("derrota.wav");
                StatsManager.RecordLoss(7);
                await MostrarMensagemTemporaria($"A palavra era: {termo.palavraSorteada}", 3000); // Fim de jogo
                var formPlacarDerrota = new FormTabela();
                formPlacarDerrota.ShowDialog(this);
                await ReiniciarJogoAsync();
                return;
            }
            coluna = 1;
            if (termo.palavraAtual <= 6) { AtualizaSelecaoVisual(); }
        }

        private void bntBackSpace_Click(object sender, EventArgs e)
        {
            // --- MODIFICAÇÃO APLICADA (Esconde mensagem) ---
            var MensNaoAceito = this.Controls.Find("MensNaoAceito", true).FirstOrDefault() as Label;
            if (MensNaoAceito != null && MensNaoAceito.Visible)
            {
                MensNaoAceito.Visible = false;
            }
            // --- FIM DA MODIFICAÇÃO ---

            if (termo.palavraAtual > 6) return;
            if (coluna > 5) { coluna = 5; }
            else if (coluna > 1 && string.IsNullOrEmpty(RetornaBotao($"btn{termo.palavraAtual}{coluna}")?.Text)) { coluna--; }
            var botaoParaApagar = RetornaBotao($"btn{termo.palavraAtual}{coluna}");
            if (botaoParaApagar != null) { botaoParaApagar.Text = string.Empty; }

            // --- Som de deletar ---
            TocarSom("deletar.wav");

            AtualizaSelecaoVisual();
        }

        #region Código de Animações e Auxiliares

        private async Task AnimaLimparLetra(Button botao)
        {
            if (botao == null) return;
            Color startColor = botao.BackColor; Color endColor = Color.Brown;
            botao.Text = string.Empty; botao.ForeColor = SystemColors.ButtonHighlight;
            for (int i = 0; i <= 10; i++)
            {
                float ratio = i / 10.0f;
                int r = Math.Max(0, Math.Min(255, (int)(startColor.R + (endColor.R - startColor.R) * ratio)));
                int g = Math.Max(0, Math.Min(255, (int)(startColor.G + (endColor.G - startColor.G) * ratio)));
                int b = Math.Max(0, Math.Min(255, (int)(startColor.B + (endColor.B - startColor.B) * ratio)));
                botao.BackColor = Color.FromArgb(r, g, b);
                botao.FlatAppearance.BorderColor = Color.White; botao.FlatAppearance.BorderSize = 3;
                await Task.Delay(20);
            }
            botao.BackColor = Color.Brown; botao.FlatAppearance.BorderColor = Color.White;
        }

        private async Task AnimaReiniciarPadronizado(Button botao)
        {
            if (botao == null) return;
            int originalTop = botao.Top; int originalHeight = botao.Height;
            for (int i = 0; i <= 10; i++) { botao.Top = originalTop + i; botao.Height = originalHeight - i * 2; await Task.Delay(15); }
            botao.Text = string.Empty; botao.BackColor = Color.Brown;
            for (int i = 10; i >= 0; i--) { botao.Top = originalTop + i; botao.Height = originalHeight - i * 2; await Task.Delay(15); }
            botao.Top = originalTop; botao.Height = originalHeight;
        }

        private void ConfiguraBotaoCustom(Button botao)
        {
            botao.ForeColor = SystemColors.ButtonHighlight; botao.FlatAppearance.BorderColor = Color.White; botao.FlatAppearance.BorderSize = 3; botao.FlatStyle = FlatStyle.Flat;
            botao.Paint += (s, e) => { Button b = s as Button; if (b == null) return; e.Graphics.Clear(b.BackColor); using (Brush brush = new SolidBrush(SystemColors.ButtonHighlight)) using (StringFormat sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center }) { e.Graphics.DrawString(b.Text, b.Font, brush, b.ClientRectangle, sf); } using (Pen pen = new Pen(b.FlatAppearance.BorderColor, 3)) { e.Graphics.DrawRectangle(pen, 0, 0, b.Width - 1, b.Height - 1); } };
        }
        private void DesativarControles()
        {
            var excecoes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "btnPlacar", "btnReiniciar" };
            foreach (Control ctrl in this.Controls) { if (ctrl is Button btn && btn.Name.StartsWith("btn")) { if (!excecoes.Contains(btn.Name)) btn.Enabled = false; } }
        }
        private Button RetornaBotao(string name)
        {
            var controls = Controls.Find(name, true);
            return (controls.Length > 0) ? (Button)controls[0] : null;
        }

        private async Task AtualizaTabuleiro()
        {
            int linhaJogada = termo.tabuleiro.Count; var palavraJogada = termo.tabuleiro[linhaJogada - 1];
            for (int col = 0; col < 5; col++)
            {
                var botaoTab = RetornaBotao($"btn{linhaJogada}{col + 1}");
                if (botaoTab == null || string.IsNullOrEmpty(botaoTab.Text)) continue;
                Color corFinal = (palavraJogada[col].Cor == 'V') ? Color.Green : (palavraJogada[col].Cor == 'A') ? Color.Yellow : Color.Gray;

                botaoTab.FlatAppearance.BorderSize = 3;
                botaoTab.FlatAppearance.BorderColor = Color.White;

                await Task.Delay(50);
                await AnimaFlipEstiloTermo(botaoTab, corFinal);
                await Task.Delay(30);
            }
        }
        private async Task AnimaFlipEstiloTermo(Button botao, Color corFinal)
        {
            if (botao == null) return;
            const int delayFlip = 1;

            for (int i = 0; i < 10; i++)
            {
                botao.Top += 2; botao.Height -= 4;
                botao.FlatAppearance.BorderColor = Color.White;
                await Task.Delay(delayFlip);
            }

            botao.BackColor = corFinal;
            botao.FlatAppearance.BorderColor = Color.White;

            for (int i = 0; i < 10; i++)
            {
                botao.Top -= 2; botao.Height += 4;
                botao.FlatAppearance.BorderColor = Color.White;
                await Task.Delay(delayFlip);
            }

            botao.Top -= 4; await Task.Delay(10);
            botao.Top += 4;

            botao.FlatAppearance.BorderSize = 3;
            botao.FlatAppearance.BorderColor = Color.White;
        }

        private void AtualizaTeclado()
        {
            foreach (var letra in termo.teclado)
            {
                var botao = RetornaBotao($"btn{letra.Key}");
                if (botao == null) continue;
                Color cor = (letra.Value == 'V') ? Color.Green : (letra.Value == 'A') ? Color.Yellow : (letra.Value == 'P') ? Color.Gray : Color.Brown;
                botao.BackColor = cor;
                botao.FlatAppearance.BorderColor = (cor == Color.Brown) ? Color.White : cor;
                botao.FlatAppearance.BorderSize = 3;
            }
        }

        // Este método agora é usado apenas para mensagens de FIM DE JOGO
        private async Task MostrarMensagemTemporaria(string mensagem, int duracaoMs = 2000)
        {
            var MensNaoAceito = this.Controls.Find("MensNaoAceito", true).FirstOrDefault() as Label; if (MensNaoAceito == null) return;
            MensNaoAceito.Text = mensagem; MensNaoAceito.Visible = true; await Task.Delay(duracaoMs); MensNaoAceito.Visible = false;
        }

        private async Task AnimaShakeLinha(int linha)
        {
            var botoes = new List<Button>(); for (int i = 1; i <= 5; i++) botoes.Add(RetornaBotao($"btn{linha}{i}"));
            int shake = 5; for (int i = 0; i < 3; i++) { foreach (var btn in botoes) if (btn != null) btn.Left -= shake; await Task.Delay(40); foreach (var btn in botoes) if (btn != null) btn.Left += shake * 2; await Task.Delay(40); foreach (var btn in botoes) if (btn != null) btn.Left -= shake; await Task.Delay(40); }
        }
        private void EfeitoSelecionado()
        {
            foreach (Control ctrl in this.Controls) { if (ctrl is Button btn && btn.Name.StartsWith("btn") && btn.Name.Length == 6) { Color corOriginal = Color.Brown; Color bordaOriginal = Color.White; btn.MouseEnter += (s, e) => { btn.BackColor = ControlPaint.Light(corOriginal, 0.35f); btn.FlatAppearance.BorderColor = ControlPaint.Light(bordaOriginal, 0.5f); btn.FlatAppearance.BorderSize = 4; btn.Cursor = Cursors.Hand; }; btn.MouseLeave += (s, e) => { btn.BackColor = corOriginal; btn.FlatAppearance.BorderColor = bordaOriginal; btn.FlatAppearance.BorderSize = 3; btn.Cursor = Cursors.Default; }; } }
        }
        private async Task AnimaFadeOutCor(Button botao) // Mantido para o reset do teclado
        {
            if (botao == null) return;
            Color start = botao.BackColor; Color end = Color.Brown;
            for (int i = 0; i <= 3; i++) { double p = (double)i / 3; int r = Math.Max(0, Math.Min(255, (int)(start.R + (end.R - start.R) * p))); int g = Math.Max(0, Math.Min(255, (int)(start.G + (end.G - start.G) * p))); int b = Math.Max(0, Math.Min(255, (int)(start.B + (end.B - start.B) * p))); botao.BackColor = Color.FromArgb(r, g, b); botao.FlatAppearance.BorderSize = 3; botao.FlatAppearance.BorderColor = Color.White; botao.ForeColor = SystemColors.ButtonHighlight; await Task.Delay(2); }
            botao.BackColor = Color.Brown; botao.ForeColor = SystemColors.ButtonHighlight;
        }
        private void btnPlacar_Click(object sender, EventArgs e)
        {
            var formPlacar = new FormTabela();
            formPlacar.ShowDialog(this);
        }
        private void MostrarNomesBotoesDebug()
        {
            var nomes = this.Controls.OfType<Button>().Select(b => b.Name).OrderBy(n => n).ToArray();
            MessageBox.Show(string.Join(Environment.NewLine, nomes), "Botoes");
        }
        #endregion
    }
}