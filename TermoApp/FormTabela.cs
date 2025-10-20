using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TermoApp
{
    public class FormTabela : Form
    {
        // cores alinhadas ao jogo
        private readonly Color Bg = Color.RosyBrown;
        private readonly Color CardBrown = Color.SaddleBrown;
        private readonly Color CardLight = Color.FromArgb(250, 245, 242);
        private readonly Color Highlight = Color.Orange;
        private readonly Color Success = Color.FromArgb(46, 204, 113);
        private readonly Color Danger = Color.FromArgb(231, 76, 60);
        private readonly Color CardText = Color.FromArgb(30, 30, 30);

        // controles
        private Label lblTitle;
        private Label lblWins, lblLosses, lblTotal, lblBestStreak;
        private TableLayoutPanel barsTable;
        private Button btnReset, btnClose;
        private TableLayoutPanel mainLayout;
                
        public FormTabela()
        {
            Text = "Placar";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Bg;
            ForeColor = Color.White;
            ClientSize = new Size(460, 560);
            MinimizeBox = false;
            MaximizeBox = false;

            InitializeComponents();

            // atualiza após carregar e também ao redimensionar
            this.Load += (s, e) => { RefreshStats(); };
            this.Resize += (s, e) => { RefreshStats(); };
        }

        private void InitializeComponents()
        {
            // Layout: header / stats / spacer / bars title / bars panel / buttons
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(8),
                BackColor = Color.Transparent,
                AutoSize = false
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));   // header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));   // stats
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 12F));   // spacer
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));   // bars title
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));   // bars panel (usa todo espaço disponível)
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));   // buttons
            Controls.Add(mainLayout);

            // header
            var header = new Panel { Dock = DockStyle.Fill, BackColor = CardBrown, Padding = new Padding(6) };
            lblTitle = new Label
            {
                Text = "PLACAR",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };
            header.Controls.Add(lblTitle);
            mainLayout.Controls.Add(header, 0, 0);

            // STATS: 3 cartões principais (Vitórias, Derrotas, Partidas) + Melhor sequência
            var statsTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(6),
                BackColor = Color.Transparent
            };
            for (int c = 0; c < 4; c++) statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

            var cardWin = CreateStatCard("Vitórias", out lblWins, Success);
            var cardLoss = CreateStatCard("Derrotas", out lblLosses, Danger);
            var cardTotal = CreateStatCard("Partidas", out lblTotal, Color.LightSteelBlue);
            var cardBest = CreateStatCard("Melhor sequência", out lblBestStreak, Highlight);

            statsTable.Controls.Add(cardWin, 0, 0);
            statsTable.Controls.Add(cardLoss, 1, 0);
            statsTable.Controls.Add(cardTotal, 2, 0);
            statsTable.Controls.Add(cardBest, 3, 0);

            mainLayout.Controls.Add(statsTable, 0, 1);

            // spacer
            var spacer = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            mainLayout.Controls.Add(spacer, 0, 2);

            // bars title
            var barsTitle = new Label
            {
                Text = "Distribuição de tentativas",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = CardText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 6, 0, 0)
            };
            mainLayout.Controls.Add(barsTitle, 0, 3);

            // PAINEL DE BARRAS: TableLayoutPanel com 6 linhas uniformes
            barsTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.Transparent,
                Padding = new Padding(8),
                Margin = new Padding(0)
            };
            const float rowHeight = 48F; // altura uniforme por linha
            for (int r = 0; r < 6; r++) barsTable.RowStyles.Add(new RowStyle(SizeType.Absolute, rowHeight));
            for (int i = 1; i <= 6; i++)
            {
                var row = CreateBarRowUniform(i);
                row.Dock = DockStyle.Fill;
                barsTable.Controls.Add(row, 0, i - 1);
            }
            mainLayout.Controls.Add(barsTable, 0, 4);

            // botões
            var btnPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(6),
                Margin = new Padding(0)
            };
            btnClose = MakeButton("Fechar", CardBrown);
            btnClose.Click += (s, e) => Close();
            btnReset = MakeButton("Limpar", Color.LightCoral);
            btnReset.Click += BtnReset_Click;
            btnPanel.Controls.Add(btnClose);
            btnPanel.Controls.Add(btnReset);
            mainLayout.Controls.Add(btnPanel, 0, 5);
        }

        // Cria uma linha de barra como TableLayoutPanel (col0=index, col1=bg)
        // OverlayLabel dentro do bg mostra o count (1..N) e índice à esquerda continua visível
        private TableLayoutPanel CreateBarRowUniform(int index)
        {
            var rowTable = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 1,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            rowTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F)); // índice
            rowTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); // barra (preenchimento)

            var lblIndex = new Label
            {
                Text = index.ToString(),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = CardText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0)
            };

            var bg = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(235, 233, 232), Margin = new Padding(6, 8, 6, 8) };
            bg.Paint += (s, e) => DrawRounded(e.Graphics, bg.ClientRectangle, 8, bg.BackColor, Color.Transparent);

            // overlay no bg (vai mostrar a quantidade de vezes — ex.: 1,2,3...)
            var lblOverlay = new Label
            {
                Text = "0",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(90, 90, 90),
                BackColor = Color.Transparent,
                Dock = DockStyle.Right,
                Width = 56,
                TextAlign = ContentAlignment.MiddleRight,
                Padding = new Padding(0, 0, 8, 0)
            };
            bg.Controls.Add(lblOverlay);

            // barra interna (filho do bg) — adicionada depois para ficar na frente
            var bar = new Panel { BackColor = Highlight, Height = 28, Width = 16, Dock = DockStyle.Left };
            bar.Margin = new Padding(0);
            bar.Paint += (s, e) =>
            {
                var r = bar.ClientRectangle;
                using (var brush = new LinearGradientBrush(r, ControlPaint.Light(Highlight, 0.12f), ControlPaint.Dark(Highlight, 0.05f), LinearGradientMode.Horizontal))
                    e.Graphics.FillRectangle(brush, r);
            };
            bg.Controls.Add(bar);

            // garantir overlay visível inicialmente; RefreshStats ajusta cor quando coberto
            lblOverlay.BringToFront();

            rowTable.Controls.Add(lblIndex, 0, 0);
            rowTable.Controls.Add(bg, 1, 0);

            // Tag guarda referências: (barPanel, bgPanel, overlayLabel)
            rowTable.Tag = Tuple.Create(bar, bg, lblOverlay);
            return rowTable;
        }

        private Button MakeButton(string text, Color actionColor)
        {
            var b = new Button
            {
                Text = text,
                Width = 96,
                Height = 32,
                BackColor = actionColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(6)
            };
            b.FlatAppearance.BorderSize = 0;
            var orig = b.BackColor;
            b.MouseEnter += (s, e) => b.BackColor = ControlPaint.Light(orig, 0.12f);
            b.MouseLeave += (s, e) => b.BackColor = orig;
            return b;
        }

        private Control CreateStatCard(string title, out Label valueLabel, Color accent)
        {
            var card = new Panel { Margin = new Padding(6), BackColor = Color.Transparent };
            var inner = new Panel { Dock = DockStyle.Fill, BackColor = CardLight, Padding = new Padding(8) };
            inner.Paint += (s, e) => DrawCard(e.Graphics, inner.ClientRectangle, CardLight, CardBrown);

            var lblTitle = new Label
            {
                Text = title,
                Dock = DockStyle.Top,
                Height = 18,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(100, 100, 100)
            };
            valueLabel = new Label
            {
                Text = "0",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleLeft
            };

            inner.Controls.Add(lblTitle);
            inner.Controls.Add(valueLabel);
            card.Controls.Add(inner);
            card.MinimumSize = new Size(96, 80);
            return card;
        }

        private void DrawCard(Graphics g, Rectangle rect, Color fill, Color border)
        {
            var shadow = rect; shadow.Offset(0, 2);
            using (var path = RoundedRect(shadow, 8))
            using (var brush = new SolidBrush(Color.FromArgb(18, 0, 0, 0)))
                g.FillPath(brush, path);

            using (var path2 = RoundedRect(rect, 8))
            using (var brush2 = new SolidBrush(fill))
                g.FillPath(brush2, path2);
        }

        private void DrawRounded(Graphics g, Rectangle rect, int radius, Color fill, Color border)
        {
            using (var path = RoundedRect(rect, radius))
            {
                using (var brush = new SolidBrush(fill)) g.FillPath(brush, path);
                if (border != Color.Transparent)
                {
                    using (var pen = new Pen(border, 1)) g.DrawPath(pen, path);
                }
            }
        }

        private GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(r.Left, r.Top, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Top, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.Left, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Deseja realmente limpar o placar?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                StatsManager.Reset();
                RefreshStats();
            }
        }

        // Atualiza UI com dados atuais
        private void RefreshStats()
        {
            var wins = StatsManager.Wins;
            var losses = StatsManager.Losses;
            var attempts = StatsManager.Attempts.ToList();
            var total = wins + losses;
            int best = StatsManager.BestWinStreak;

            lblWins.Text = wins.ToString();
            lblLosses.Text = losses.ToString();
            lblTotal.Text = total.ToString();
            lblBestStreak.Text = best.ToString();

            // distribuição
            int[] distro = new int[6];
            foreach (var a in attempts) if (a >= 1 && a <= 6) distro[a - 1]++;

            int max = Math.Max(1, distro.Max());

            // recalcula barras; cada linha é um TableLayoutPanel com Tag = (bar, bg, overlayLabel)
            for (int i = 0; i < barsTable.Controls.Count; i++)
            {
                if (barsTable.Controls[i] is TableLayoutPanel row && row.Tag is Tuple<Panel, Panel, Label> tup)
                {
                    var bar = tup.Item1;
                    var bg = tup.Item2;
                    var lblOverlay = tup.Item3;
                    int count = distro[i];
                    lblOverlay.Text = count.ToString();

                    // ajusta largura da barra
                    int bgWidth = Math.Max(80, bg.ClientSize.Width - 24);
                    int target = (int)((double)count / max * bgWidth);
                    bar.Width = Math.Max(12, target);

                    // se a barra cobrir o overlay, deixa o overlay claro (contraste), caso contrário padrão escuro
                    int overlayRightX = bg.ClientSize.Width - lblOverlay.Width - 8;
                    int barRightX = bar.Width;
                    if (barRightX >= overlayRightX)
                        lblOverlay.ForeColor = Color.White;
                    else
                        lblOverlay.ForeColor = CardText;

                    // garante overlay na frente para legibilidade
                    lblOverlay.BringToFront();
                }
            }
        }
    }
}