using System;
using System.Drawing;
using System.Windows.Forms;

namespace First_Principles_Graphics
{
    public class SettingsForm : Form
    {
        private ViewerSettings settings;

        private Button okButton, cancelButton;
        private Button backgroundColorButton, lineColorButton;

        private NumericUpDown rotateSpeedNum;
        private NumericUpDown inertiaFrictionFactorNum;
        private NumericUpDown accelerationAmountNum;
        private NumericUpDown maxSpeedNum;
        private NumericUpDown inertiaFrictionEnlargementNum;
        private NumericUpDown accelerationAmountEnlargementNum;
        private NumericUpDown maxSpeedEnlargementNum;

        private Panel previewPanel;
        private float previewAngle = 0;
        private ToolTip toolTip;

        public SettingsForm(ViewerSettings settings)
        {
            this.settings = settings;
            toolTip = new ToolTip();
            InitializeComponent();
            StartPreviewTimer();
        }

        private void InitializeComponent()
        {
            this.Text = "Viewer Settings";
            this.Size = new Size(450, 650); // slightly smaller width
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            FlowLayoutPanel mainPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(10),
                Width = 420
            };
            this.Controls.Add(mainPanel);

            // Colors section
            mainPanel.Controls.Add(CreateSectionLabel("Colors"));

            backgroundColorButton = CreateColorButton("Background Color", settings.BackgroundColor, "Background color of viewer", OnBackgroundColorClick);
            lineColorButton = CreateColorButton("Line Color", settings.LineColor, "Line color of viewer", OnLineColorClick);
            mainPanel.Controls.Add(backgroundColorButton);
            mainPanel.Controls.Add(lineColorButton);

            // Preview panel (shorter and same width as buttons)
            // Preview panel (shorter and same width as buttons)
            Label previewLabel = new Label
            {
                Text = "Rotation Preview",
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 5)
            };
            mainPanel.Controls.Add(previewLabel);

            previewPanel = new Panel
            {
                Width = 400,
                Height = 120, // slightly smaller height
                BackColor = settings.BackgroundColor,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 10)
            };
            previewPanel.Paint += PreviewPanel_Paint;
            mainPanel.Controls.Add(previewPanel);

            // Normal Rotation section
            mainPanel.Controls.Add(CreateSectionLabel("Normal Rotation"));
            rotateSpeedNum = CreateNumericWithDescription("Rotate Speed", settings.rotateSpeed, 1, 500,
                "Speed of rotation in degrees per tick", "How fast the wireframe rotates", 0, 1);
            mainPanel.Controls.Add(rotateSpeedNum.Parent);

            // Inertia Rotation section
            mainPanel.Controls.Add(CreateSectionLabel("Inertia Rotation"));
            inertiaFrictionFactorNum = CreateNumericWithDescription("Inertia Friction Factor", (decimal)settings.inertiaFrictionFactor, 0.01M, 1,
                "Friction slowing rotation", "How quickly rotation slows down", 2, 0.01M);
            accelerationAmountNum = CreateNumericWithDescription("Acceleration Amount", (decimal)settings.accelerationAmount, 0.01M, 10,
                "Acceleration applied when moving", "How quickly the wireframe speeds up", 2, 0.01M);
            maxSpeedNum = CreateNumericWithDescription("Max Speed", (decimal)settings.maxSpeed, 1, 1000,
                "Maximum rotation speed", "Top speed of the wireframe", 0, 1);
            mainPanel.Controls.Add(inertiaFrictionFactorNum.Parent);
            mainPanel.Controls.Add(accelerationAmountNum.Parent);
            mainPanel.Controls.Add(maxSpeedNum.Parent);

            // Inertia Scaling section
            mainPanel.Controls.Add(CreateSectionLabel("Inertia Scaling"));
            inertiaFrictionEnlargementNum = CreateNumericWithDescription("Scale Friction", (decimal)settings.inertiaFrictionEnlargement, 0.01M, 1,
                "Friction on scaling", "How quickly scaling slows down", 2, 0.01M);
            accelerationAmountEnlargementNum = CreateNumericWithDescription("Scale Acceleration", (decimal)settings.accelerationAmountEnlargement, 0.01M, 10,
                "Scaling acceleration", "How quickly scaling speeds up", 2, 0.01M);
            maxSpeedEnlargementNum = CreateNumericWithDescription("Scale Max Speed", (decimal)settings.maxSpeedEnlargement, 1, 200,
                "Max scaling speed", "Top speed of scaling", 0, 1);
            mainPanel.Controls.Add(inertiaFrictionEnlargementNum.Parent);
            mainPanel.Controls.Add(accelerationAmountEnlargementNum.Parent);
            mainPanel.Controls.Add(maxSpeedEnlargementNum.Parent);

            // OK/Cancel buttons
            FlowLayoutPanel buttonPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(10),
                AutoSize = true
            };
            okButton = new Button { Text = "OK", Width = 120, Height = 35 };
            okButton.Click += OkButton_Click;

            cancelButton = new Button { Text = "Cancel", Width = 120, Height = 35 };
            cancelButton.Click += (s, e) => DialogResult = DialogResult.Cancel;

            buttonPanel.Controls.Add(okButton);
            buttonPanel.Controls.Add(cancelButton);
            this.Controls.Add(buttonPanel);
        }

        private Label CreateSectionLabel(string text)
        {
            return new Label
            {
                Text = text,
                Font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(0, 10, 0, 5)
            };
        }

        private void StartPreviewTimer()
        {
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 50 };
            timer.Tick += (s, e) =>
            {
                previewAngle += (float)rotateSpeedNum.Value;
                if (previewAngle > 360) previewAngle -= 360;
                previewPanel.Invalidate();
            };
            timer.Start();
        }

        private void PreviewPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(settings.BackgroundColor);

            using (Pen pen = new Pen(settings.LineColor, 2))
            {
                Point center = new Point(previewPanel.Width / 2, previewPanel.Height / 2);
                // shorten line to fit panel width
                float length = 180; // leave margin, fits under buttons

                float angleRad = previewAngle * (float)Math.PI / 180f;
                Point end = new Point(
                    center.X + (int)(length * Math.Cos(angleRad)),
                    center.Y + (int)(length * Math.Sin(angleRad))
                );

                e.Graphics.DrawLine(pen, center, end);
            }
        }

        private Button CreateColorButton(string text, Color initialColor, string tooltipText, EventHandler onClick)
        {
            Button b = new Button
            {
                Text = text,
                BackColor = initialColor,
                ForeColor = GetReadableTextColor(initialColor),
                Width = 400,
                Height = 40,
                Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10)
            };
            b.Click += onClick;
            toolTip.SetToolTip(b, tooltipText);
            return b;
        }

        private Color GetReadableTextColor(Color bg)
        {
            double luminance = (0.299 * bg.R + 0.587 * bg.G + 0.114 * bg.B) / 255;
            return luminance > 0.6 ? Color.Black : Color.White;
        }

        private NumericUpDown CreateNumericWithDescription(string labelText, decimal value, decimal min, decimal max, string tooltipText, string description, int decimalPlaces = 2, decimal increment = 0.01M)
        {
            Label lbl = new Label { Text = labelText, AutoSize = true, Font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Bold) };
            toolTip.SetToolTip(lbl, tooltipText);

            NumericUpDown num = new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimalPlaces,
                Value = value,
                Width = 400,
                Height = 30,
                Increment = increment,
                Font = new Font(FontFamily.GenericSansSerif, 10),
                Margin = new Padding(0, 0, 0, 2)
            };
            toolTip.SetToolTip(num, tooltipText);

            Label descLabel = new Label
            {
                Text = description,
                AutoSize = true,
                ForeColor = Color.Gray,
                Font = new Font(FontFamily.GenericSansSerif, 8),
                Margin = new Padding(0, 0, 0, 5)
            };

            FlowLayoutPanel container = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Width = 420,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 5)
            };
            container.Controls.Add(lbl);
            container.Controls.Add(num);
            container.Controls.Add(descLabel);

            return num;
        }

        private void OnBackgroundColorClick(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog { Color = settings.BackgroundColor };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                settings.BackgroundColor = dlg.Color;
                backgroundColorButton.BackColor = dlg.Color;
                backgroundColorButton.ForeColor = GetReadableTextColor(dlg.Color);
                previewPanel.BackColor = dlg.Color;
            }
        }

        private void OnLineColorClick(object sender, EventArgs e)
        {
            ColorDialog dlg = new ColorDialog { Color = settings.LineColor };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                settings.LineColor = dlg.Color;
                lineColorButton.BackColor = dlg.Color;
                lineColorButton.ForeColor = GetReadableTextColor(dlg.Color);
                previewPanel.Invalidate();
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            settings.rotateSpeed = (int)rotateSpeedNum.Value;
            settings.inertiaFrictionFactor = (double)inertiaFrictionFactorNum.Value;
            settings.accelerationAmount = (double)accelerationAmountNum.Value;
            settings.maxSpeed = (double)maxSpeedNum.Value;

            settings.inertiaFrictionEnlargement = (double)inertiaFrictionEnlargementNum.Value;
            settings.accelerationAmountEnlargement = (double)accelerationAmountEnlargementNum.Value;
            settings.maxSpeedEnlargement = (double)maxSpeedEnlargementNum.Value;

            settings.BackgroundColor = backgroundColorButton.BackColor;
            settings.LineColor = lineColorButton.BackColor;

            this.DialogResult = DialogResult.OK;
        }
    }
}
