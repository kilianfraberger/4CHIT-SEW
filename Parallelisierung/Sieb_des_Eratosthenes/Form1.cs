using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sieb_des_Eratosthenes
{
    public partial class Form1 : Form
    {
        private TextBox inputBox1;
        private TextBox liveMirrorBox;
        private TextBox inputBox2;
        private TextBox outputBox;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Sieb des Eratosthenes - Testprogramm";
            this.Width = 500;
            this.Height = 400;
            this.StartPosition = FormStartPosition.CenterScreen;

            int margin = 20;
            int controlWidth = 440;
            int buttonHeight = 60;
            int buttonWidth = 110;
            
            inputBox1 = new TextBox
            {
                Top = margin,
                Left = margin,
                Width = controlWidth
            };
            inputBox1.TextChanged += (s, e) => liveMirrorBox.Text = inputBox1.Text;
            
            liveMirrorBox = new TextBox
            {
                Top = inputBox1.Bottom + 10,
                Left = margin,
                Width = controlWidth,
                ReadOnly = true
            };
            
            inputBox2 = new TextBox
            {
                Top = liveMirrorBox.Bottom + 20,
                Left = margin,
                Width = controlWidth,
                Text = "400000000"
            };
            
            int buttonsTop = inputBox2.Bottom + 15;
            button1 = new Button { Text = "Sieb ohne Thread", Top = buttonsTop, Left = margin, Width = buttonWidth, Height = buttonHeight };
            button2 = new Button { Text = "Sieb mit Thread", Top = buttonsTop, Left = button1.Right + 10, Width = buttonWidth, Height = buttonHeight };
            button3 = new Button { Text = "Sieb mit BackgroundWorker", Top = buttonsTop, Left = button2.Right + 10, Width = buttonWidth + 40, Height = buttonHeight };
            button4 = new Button { Text = "Asynchrones Sieb", Top = buttonsTop, Left = button3.Right + 10, Width = buttonWidth + 20, Height = buttonHeight };

            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            button3.Click += Button3_Click;
            button4.Click += Button4_Click;
            
            outputBox = new TextBox
            {
                Top = button1.Bottom + 20,
                Left = margin,
                Width = controlWidth,
                ReadOnly = true,
                Multiline = true,
                Height = 30
            };
            
            Controls.Add(inputBox1);
            Controls.Add(liveMirrorBox);
            Controls.Add(inputBox2);
            Controls.Add(button1);
            Controls.Add(button2);
            Controls.Add(button3);
            Controls.Add(button4);
            Controls.Add(outputBox);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            outputBox.Text = " ... calculating ...";
            outputBox.Refresh();
            int upto = int.Parse(inputBox2.Text);
            int result;
            Sieve(upto, out result);
            outputBox.Text = result.ToString();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            outputBox.Text = " ... calculating ...";
            outputBox.Refresh();
            int upto = int.Parse(inputBox2.Text);
            int result = 0;
            Thread t = new Thread((_) => Sieve(upto, out result));
            t.Start();
            t.Join();
            outputBox.Text = result.ToString();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            outputBox.Text = " ... calculating ...";
            outputBox.Refresh();
            int upto = int.Parse(inputBox2.Text);
            int result = 0;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (s, e) =>
            {
                Sieve(upto, out result);
                e.Result = result;
            };
            worker.RunWorkerCompleted += (s, e) =>
            {
                outputBox.Text = e.Result.ToString();
            };
            worker.RunWorkerAsync();
        }

        private async void Button4_Click(object sender, EventArgs e)
        {
            outputBox.Text = " ... calculating ...";
            outputBox.Refresh();
            int upto = int.Parse(inputBox2.Text);
            int result = 0;
            await Task.Run(() => Sieve(upto, out result));
            outputBox.Text = result.ToString();
        }

        public void Sieve(int n, out int result)
        {
            bool[] isPrime = new bool[n + 1];
            for (int i = 2; i <= n; i++)
                isPrime[i] = true;

            for (int i = 2; i * i <= n; i++)
            {
                if (isPrime[i])
                {
                    for (int j = i * i; j <= n; j += i)
                        isPrime[j] = false;
                }
            }

            result = isPrime.Count(x => x);
        }
    }
}
