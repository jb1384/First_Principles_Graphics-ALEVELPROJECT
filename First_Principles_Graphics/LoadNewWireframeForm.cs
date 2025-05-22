using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace First_Principles_Graphics
{
    public partial class LoadNewWireframeForm: Form
    {
        public string path;
        public LoadNewWireframeForm()
        {
            InitializeComponent();
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wireframefiles");
            string[] files = Directory.GetFiles(folderPath, "*.txt");

            //automatically create buttons
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.AutoScroll = true;
            this.Controls.Add(panel);

            foreach (string file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                Button fileButton = new Button();
                fileButton.Text = fileName;

                fileButton.Tag = file; 
                fileButton.AutoSize = true;
                fileButton.Margin = new Padding(5);
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
