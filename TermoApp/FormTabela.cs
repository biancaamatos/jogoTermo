using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic; // Adicionado para IReadOnlyList (necessário para StatsManager)

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
        // private TableLayoutPanel barsTable; // REMOVIDO
        private Button btnReset, btnClose;
        private TableLayoutPanel mainLayout;

        public FormTabela()
        {
            Text = "Placar";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Bg;
            ForeColor = Color.White;
            // --- TAMANHO AJUSTADO PARA FICAR MAIS COMPACTO ---
            ClientSize = new Size(460, 240);
            MinimizeBox = false;
            MaximizeBox = false;

            InitializeComponents();

            // atualiza após carregar
            this.Load += (s, e) => { RefreshStats(); };
        }

        private void InitializeComponents()
        {
            // Layout: header / stats / buttons
            mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3, // --- ALTERADO de 6 para 3 ---
                Padding = new Padding(8),
                BackColor = Color.Transparent,
                AutoSize = false
            };

            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));   // header
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));   // stats
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // botões (ocupa o resto)
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

            // STATS
            var statsTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(6),
                BackColor = Color.Transparent
            };
            for (int c = 0; c < 4; c++) statsTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var cardWin = CreateStatCard("Vitórias", out lblWins, Success);
            var cardLoss = CreateStatCard("Derrotas", out lblLosses, Danger);
            var cardTotal = CreateStatCard("Partidas", out lblTotal, Color.LightSteelBlue);
            var cardBest = CreateStatCard("Sequência", out lblBestStreak, Highlight);

            statsTable.Controls.Add(cardWin, 0, 0);
            statsTable.Controls.Add(cardLoss, 1, 0);
            statsTable.Controls.Add(cardTotal, 2, 0);
            statsTable.Controls.Add(cardBest, 3, 0);

            mainLayout.Controls.Add(statsTable, 0, 1);

           
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

            // --- ALTERADO: Posição dos botões movida para a linha 2 ---
            mainLayout.Controls.Add(btnPanel, 0, 2);
        }

        // --- MÉTODO CreateBarRowUniform REMOVIDO ---

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
                StatsManager.Reset();
                RefreshStats();
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
        }
    }
}