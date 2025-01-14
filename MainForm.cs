﻿using System;
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
        private MoonAPIReader moonApiReader;

        private Dictionary<int, string> resultMessageDictionary = new Dictionary<int, string>();
        private Dictionary<string, Button> usedLetterDictionary = new Dictionary<string, Button>();
        private Dictionary<GroupBox, List<TextBox>> roundLetterDictionary = new Dictionary<GroupBox, List<TextBox>>();

        private List<GroupBox> roundGroupBoxes = new List<GroupBox>();
        private List<TextBox> activeTextBoxes = new List<TextBox>();

        private int roundCount = -1;
        private string answer = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await setup();
        }

        private async Task setup()
        {
            moonApiReader = new MoonAPIReader();

            roundGroupBoxes = new List<GroupBox>() {
                groupRound0, groupRound1, groupRound2,
                groupRound3, groupRound4, groupRound5 };

            loadDictionaries();
            
            // TEST:
            roundCount = 0;

            bool isApiHealthy = await moonApiReader.isApiHealthy();
            if (!isApiHealthy)
                label_apiHealth.Visible = true;
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
            // Reset all round groupboxes and their text boxes
            foreach (GroupBox gb in roundGroupBoxes)
            {
                resetGroupedTextBoxes(gb);
                gb.Enabled = false;
            }

            // Reset hint colors of keyboard keys
            resetKeyboardButtons();

            // Enable first round's group of text boxes
            groupRound0.Enabled = true;

            // select (focus) the first round's text box
            roundLetterDictionary[groupRound0][0].Select();
        }

        /// <summary>
        /// Iterates through each 'round' of grouped text boxes and resets each of their text boxes' text and color.
        /// </summary>
        private void resetGroupedTextBoxes(GroupBox gb)
        {
            foreach (Control c in gb.Controls)
            {
                if (c.GetType() == typeof(TextBox))
                    resetTextBox((TextBox)c);
            }
        }

        /// <summary>
        /// Resets a text box's text and color.
        /// </summary>
        /// <param name="textBox">The text box to reset.</param>
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

        /*
         * Test event handlers:
         */

        private void guessButton_Click(object sender, EventArgs e)
        {
            moonApiReader.guess();
        }

        private void getWoTD_Click(object sender, EventArgs e)
        {
            moonApiReader.getWordForToday();
        }

        private void getWordForDate_Click(object sender, EventArgs e)
        {
            moonApiReader.getWordForDate(dateTimePicker.Value);
        }
        private void getWordForRandomDate_Click(object sender, EventArgs e)
        {
            moonApiReader.getWordForRandomDate();
        }

        /*
         * Event handlers for keyboard buttons:
         */

        private void keyboardLetterButton_Click(object sender, EventArgs e)
        {
            string letter = ((Button)sender).Text;
            int index = getFirstEmptyTextBoxIndex();

            if (index > -1) // -1 => every textbox is filled
                activeTextBoxes[index].Text = letter;
        }

        private int getFirstEmptyTextBoxIndex()
        {
            int index = -1;

            return index;
        }

        private void buttonBackspace_Click(object sender, EventArgs e)
        {

        }

        private void buttonClearWord_Click(object sender, EventArgs e)
        {

        }

        private async void apiHealthButton_Click(object sender, EventArgs e)
        {
            await moonApiReader.isApiHealthy();
        }
    }
}
