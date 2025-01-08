using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moon_Asg7_Wordle
{
    public partial class MainForm : Form
    {

        private Dictionary<int, string> resultMessageDictionary = new Dictionary<int, string>();
        private Dictionary<int, string> usedLetterDictionary = new Dictionary<int, string>();

        private List<GroupBox> groupBoxes = new List<GroupBox>();

        private int roundCount = 0;
        private string answer = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }






        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text != null)
                textBox.Text = textBox.Text.ToUpper();
        }

        private void keyboardLetterButton_Click(object sender, EventArgs e)
        {

        }

        private void buttonBackspace_Click(object sender, EventArgs e)
        {

        }

        private void buttonClearWord_Click(object sender, EventArgs e)
        {

        }
    }
}
