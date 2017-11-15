using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml.Linq;
using System.Threading;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }
        string path, link;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog okienko = new OpenFileDialog();
            okienko.Filter = "Pliki JPG |*.jpg|Pliki PNG |*.png";

            if (okienko.ShowDialog() == DialogResult.OK)
            {
                path = okienko.FileName;
                textBox1.Text = path;
            }
        }

        void UpLoad()
        {
            PosterUpload(new Bitmap(Bitmap.FromFile(path)));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            Thread thr = new Thread(UpLoad);
            thr.Start();
            
        }

        private void PosterUpload(Bitmap bitmap)
        {
            progressBar1.BeginInvoke(new Action(() => progressBar1.Increment(10)));
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            byte[] bitmapBytes = memoryStream.GetBuffer();
            string bitmapString = Convert.ToBase64String(bitmapBytes, Base64FormattingOptions.InsertLineBreaks);

            using (var w = new WebClient())
            {
                progressBar1.BeginInvoke(new Action(() => progressBar1.Increment(30)));
                var values = new NameValueCollection
                {
                    { "key", "433a1bf4743dd8d7845629b95b5ca1b4" },
                    { "image", bitmapString }
                };
                progressBar1.BeginInvoke(new Action(() => progressBar1.Increment(30)));

                byte[] response = w.UploadValues("http://imgur.com/api/upload.xml", values);

                progressBar1.BeginInvoke(new Action(() => progressBar1.Increment(10)));
                link = XDocument.Load(new MemoryStream(response)).ToString();

                int f = link.IndexOf("<original_image>");
                link = link.Remove(0, f);

                progressBar1.BeginInvoke(new Action(() => progressBar1.Increment(10)));
                f = link.IndexOf("</original_image>");
                link = link.Remove(f, link.Length - f).Replace("<original_image>", "");

                progressBar1.BeginInvoke(new Action(() => progressBar1.Increment(10)));
                textBox2.BeginInvoke(new Action(() => textBox2.Text = link));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
            button2.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;
        }

        private void Form1_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                foreach (string fileLoc in filePaths)
                {
                    path = fileLoc;
                    textBox1.Text = path;
                }
            }
        }
    }
}
