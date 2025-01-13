using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Moon_Asg7_Wordle
{
    public partial class MainForm : Form
    {
        private MoonAPIReader apiReader;

        private Dictionary<int, string> resultMessageDictionary = new Dictionary<int, string>();
        private Dictionary<string, Button> usedLetterDictionary = new Dictionary<string, Button>();
        private Dictionary<GroupBox, List<TextBox>> roundLetterDictionary = new Dictionary<GroupBox, List<TextBox>>();

        private List<GroupBox> roundGroupBoxes = new List<GroupBox>();

        private int roundCount = 0;
        private string answer = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            setup();
        }

        private void setup()
        {
            apiReader = new MoonAPIReader();

            roundGroupBoxes = new List<GroupBox>() {
                groupRound1, groupRound2, groupRound3,
                groupRound4, groupRound5, groupRound6 };

            loadDictionaries();

            // TEST:
            roundCount = 1;
        }

        private void loadDictionaries()
        {
            // (round count, result message) dictionary
            resultMessageDictionary.Add(1, "No way, you cheated!");
            resultMessageDictionary.Add(2, "Very Impressive!");
            resultMessageDictionary.Add(3, "Excellent!");
            resultMessageDictionary.Add(4, "Nicely done.");
            resultMessageDictionary.Add(5, "Cutting it close...");
            resultMessageDictionary.Add(6, "Barely squeaked by there, friend");
            resultMessageDictionary.Add(7, "Better luck next time. :-)");

            // (used letter, Button) dictionary
            foreach (char letter in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                Button button = Controls.Find($"button_{letter}", true).FirstOrDefault() as Button;
                usedLetterDictionary.Add(letter.ToString(), button);
            }
            
            // (GroupBox, round letters) dictionary
            foreach (GroupBox gb in roundGroupBoxes)
            {
                List<TextBox> roundLetters = new List<TextBox>();
                foreach (Control c in gb.Controls)
                {
                    if (c.GetType() == typeof(TextBox))
                        roundLetters.Add((TextBox)c);
                }
                // sort roundLetters by ascending Name property
                roundLetters.Sort((x, y) => string.Compare(x.Name, y.Name));
                roundLetterDictionary.Add(gb, roundLetters);
            }
        }

        private void resetGame(object sender, EventArgs e)
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
            // do a check to ensure that all letters are filled.
            //      or better yet, only enable 'set' button once all letters are filled.

            // use round to select current round groupbox
            //GroupBox currentRoundGB = (GroupBox)Controls.Find($"groupRound{roundCount}", true).FirstOrDefault();
            GroupBox currentRoundGB = roundGroupBoxes[roundCount - 1];

            // build a string from the characters in current round groupbox's textboxes
            string submission = string.Empty;
            foreach (TextBox tb in roundLetterDictionary[currentRoundGB])
            {
                submission += tb.Text;
            }
            Debug.Write($"checking value of string built from textboxes within {currentRoundGB}. Result: {submission}");
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text != null)
                textBox.Text = textBox.Text.ToUpper();
        }

        private void guessButton_Click(object sender, EventArgs e)
        {
            apiReader.testCheckGuess();
        }

        private void getWoTD_Click(object sender, EventArgs e)
        {
            apiReader.testGetWoTD();
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

        private void apiTest_Click(object sender, EventArgs e)
        {

        }

    }
}
