using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApplication4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            string[] inputStrings = textBox1.Text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            textBox3.Text = string.Empty;
            string output = string.Empty;
            foreach (var item in inputStrings)
	        {
                output += FuzzyString.FuzzyString.GetPrimaryKeyString(item) + " ------- " + item + "\r\n";
	        }
            textBox3.Text = output;
        }


        public class SimilirityAndString
        {
            public double simIndex { get; set; }
            public string inputString { get; set; }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            
            string[] inputStrings = textBox1.Text.Split(new char[] { '\n', ',', '.'}, StringSplitOptions.RemoveEmptyEntries);
            textBox3.Text = string.Empty;
            string output = string.Empty;
            List<SimilirityAndString> unSortedOutput = new List<SimilirityAndString>();
            string lookupString = textBox2.Text.ToLower();
            foreach (var item in inputStrings)
            {
                string candidate = item.Trim().ToLower();
                double sIndex = FuzzyString.FuzzyString.GetSimilarIndex(lookupString, candidate);
                if (sIndex > 30)
                unSortedOutput.Add(
                    new SimilirityAndString()
                    {
                        simIndex = sIndex,
                        inputString = item
                    });
            }

            var sortedOutput = unSortedOutput.OrderByDescending(item => item.simIndex);
            foreach (var item in sortedOutput)
            {
                output += string.Format("Similarity:{0:0.00} ---- {1}\r\n", item.simIndex, item.inputString);
            }
            textBox3.Text = output;
        }

        
    }
}
