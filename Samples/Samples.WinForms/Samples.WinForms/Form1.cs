using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xamarin.Essentials;

namespace Samples.WinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Task.Run(async () =>
            {
                await Task.Delay(5000);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    MessageBox.Show("Hello world");
                });
            });
        }
    }
}
