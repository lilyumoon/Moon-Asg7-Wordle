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
        private Dictionary<string, Button> usedLetterDictionary = new Dictionary<string, Button>();
        private Dictionary<GroupBox, List<TextBox>> roundLetterDictionary = new Dictionary<GroupBox, List<TextBox>>();

        private List<GroupBox> roundGroupBoxes = new List<GroupBox>();

        private int roundCount = 0;
        private string answer = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            setup();
        }

        private void setup()
        {
            roundGroupBoxes = new List<GroupBox>() { 
                groupRound1, groupRound2, groupRound3, 
                groupRound4, groupRound5, groupRound6 };
        }

        private void loadDictionaries()
        {
            // result messages
            resultMessageDictionary.Add(1, "No way, you cheated!");
            resultMessageDictionary.Add(2, "Very Impressive!");
            resultMessageDictionary.Add(3, "Excellent!");
            resultMessageDictionary.Add(4, "Nicely done.");
            resultMessageDictionary.Add(5, "Cutting it close...");
            resultMessageDictionary.Add(6, "Barely squeaked by there, friend");
            resultMessageDictionary.Add(7, "Better luck next time. :-)");

            // used letters
            foreach (char letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                usedLetterDictionary.Add(letter.ToString(), Controls.Find($"button_{letter}", true).FirstOrDefault() as Button);
            }
            
            // round letters
            foreach (GroupBox gb in roundGroupBoxes)
            {
                List<TextBox> roundLetters = new List<TextBox>();
                foreach (Control c in gb.Controls)
                {
                    // TODO: check that this builds the lists of textboxes in ascending order

                    if (c.GetType() == typeof(TextBox))
                        roundLetters.Add((TextBox)c);
                }
                roundLetterDictionary.Add(gb, roundLetters);
            }
        }

        private void resetGame()
        {
            // Reset all round groupboxes
            foreach (GroupBox gb in roundGroupBoxes)
            {
                gb.Enabled = false;
            }

            // Reset hint colors of keyboard keys
            resetKeyboardButtons();

            // Enable first round
            groupRound1.Enabled = true;

            // select (focus) the first guess text box of first round
            roundLetterDictionary[groupRound1][0].Select();
        }

        private void resetGuessTextBoxes()
        {
            foreach (GroupBox gb in roundGroupBoxes)
            {
                foreach (Control c in gb.Controls)
                {
                    if (c.GetType() == typeof(TextBox))
                        resetTextBox((TextBox)c);
                }
            }
        }

        private void resetTextBox(TextBox textBox)
        {
            textBox.Text = string.Empty;
            textBox.BackColor = Control.DefaultBackColor;
            textBox.ForeColor = Control.DefaultForeColor;
        }

        private void resetKeyboardButtons()
        {
            foreach (var kvp in usedLetterDictionary)
            {
                kvp.Value.BackColor = Control.DefaultBackColor;
            }
        }

        private void setButton_Click(object sender,  EventArgs e)
        {
            submitGuess();
        }

        private void submitGuess()
        {
            // use round to select current round's GroupBox
            GroupBox roundGB = (GroupBox)Controls.Find($"groupRound{roundCount}", true).FirstOrDefault();

            // get current round GroupBox's textBoxes
            foreach (Control c in roundGB.Controls)
            {

            }
            // ensure they are ordered by ascending

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
