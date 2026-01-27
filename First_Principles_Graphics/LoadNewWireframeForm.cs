using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO; // Required for directory operations
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace First_Principles_Graphics
{
    public partial class LoadNewWireframeForm : Form
    {
        public string path;

        public LoadNewWireframeForm()
        {
            InitializeComponent();

            this.Text = "Select Model";
            this.Size = new Size(600, 500); 

            // Create the main scrolling panel
            FlowLayoutPanel mainPanel = new FlowLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.AutoScroll = true;
            mainPanel.FlowDirection = FlowDirection.LeftToRight;
            mainPanel.WrapContents = true;
            mainPanel.Padding = new Padding(10);
            this.Controls.Add(mainPanel);

            string rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wireframefiles");

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            // 1. Load files in the root folder (Uncategorized)
            string[] rootFiles = Directory.GetFiles(rootPath, "*.txt");
            if (rootFiles.Length > 0)
            {
                AddCategoryHeader(mainPanel, "Uncategorized");
                AddButtonsForFiles(mainPanel, rootFiles);
            }

            // 2. Load files from subdirectories (Categorized)
            string[] subDirectories = Directory.GetDirectories(rootPath);
            foreach (string dir in subDirectories)
            {
                string categoryName = new DirectoryInfo(dir).Name;
                string[] files = Directory.GetFiles(dir, "*.txt");

                if (files.Length > 0)
                {
                    AddCategoryHeader(mainPanel, categoryName);
                    AddButtonsForFiles(mainPanel, files);
                }
            }
        }

        private void AddCategoryHeader(FlowLayoutPanel panel, string text)
        {
            Label header = new Label();
            header.Text = text;
            header.Font = new Font("Arial", 12, FontStyle.Bold | FontStyle.Underline);
            header.ForeColor = Color.DarkSlateBlue;
            header.AutoSize = true;
            header.Margin = new Padding(5, 20, 5, 5); 

            if (panel.Controls.Count > 0)
            {
                panel.SetFlowBreak(panel.Controls[panel.Controls.Count - 1], true);
            }

            panel.Controls.Add(header);

            panel.SetFlowBreak(header, true);
        }

        private void AddButtonsForFiles(FlowLayoutPanel panel, string[] files)
        {
            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                // Clean up underscores for display
                string displayName = fileName.Replace("_", " ");

                Button fileButton = new Button();
                fileButton.Text = displayName;
                fileButton.Tag = file; // Store full path in tag
                fileButton.AutoSize = true;
                fileButton.Padding = new Padding(5);
                fileButton.Margin = new Padding(5);
                fileButton.BackColor = Color.WhiteSmoke;
                fileButton.FlatStyle = FlatStyle.Flat;
                fileButton.Cursor = Cursors.Hand;

                fileButton.Click += FileButton_Click;

                panel.Controls.Add(fileButton);
            }
        }

        private void FileButton_Click(object sender, EventArgs e)
        {
            Button clicked = sender as Button;
            path = clicked.Tag as string;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}