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
        const int WM_KEYUP = 0x0101;

        private MoonAPIReader moonApiReader;

        private Dictionary<int, string> resultMessageDictionary = new Dictionary<int, string>();
        private Dictionary<string, Button> usedLetterDictionary = new Dictionary<string, Button>();

        // this dictionary is built in order to reduce complexity of some of the in-class suggestions of text box processing
        private Dictionary<GroupBox, List<TextBox>> roundLetterDictionary = new Dictionary<GroupBox, List<TextBox>>();

        private List<GroupBox> roundGroupBoxes = new List<GroupBox>();

        private int roundCount = -1;
        private string answer = string.Empty;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await setup();

            // ensure that the form processes all key events even if a control has focus
            // and assign a handler for KeyUp events
            this.KeyPreview = true;
            this.KeyUp += mainForm_KeyUp;
        }

        private async Task setup()
        {
            moonApiReader = new MoonAPIReader();

            roundGroupBoxes = new List<GroupBox>() {
                groupRound0, groupRound1, groupRound2,
                groupRound3, groupRound4, groupRound5 };

            loadDictionaries();
            
            bool isApiHealthy = await moonApiReader.isApiHealthy();
            if (!isApiHealthy)
                label_apiHealth.Visible = true;

            resetGame();
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
                button.Click += keyboardLetterButton_Click;
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
            resetGame();
        }

        private void resetGame()
        {
            // Reset all round groupboxes and their text boxes
            foreach (GroupBox gb in roundGroupBoxes)
            {
                resetGroupedTextBoxes(gb);
                gb.Enabled = false;
            }

            // Reset hint colors of keyboard keys
            resetKeyboardButtons();

            // Enable first round's group of text boxes and set focus to the first
            groupRound0.Enabled = true;
            roundLetterDictionary[groupRound0][0].Select();
        }

        /// <summary>
        /// Iterates through each of a groupbox's child text boxes and resets text and color.
        /// </summary>
        private void resetGroupedTextBoxes(GroupBox gb)
        {
            foreach (TextBox textBox in roundLetterDictionary[gb])
            {
                textBox.Text = string.Empty;
                textBox.BackColor = Control.DefaultBackColor;
                textBox.ForeColor = Control.DefaultForeColor;
            }
        }

        private void resetKeyboardButtons()
        {
            foreach (var kvp in usedLetterDictionary)
            {
                kvp.Value.BackColor = Control.DefaultBackColor;
            }
        }

        private void submitGuess()
        {
            // do a check to ensure that all letters are filled.
            //      or better yet, only enable 'set' button once all letters are filled.

            // use round to select current round groupbox
            //GroupBox currentRoundGB = (GroupBox)Controls.Find($"groupRound{roundCount}", true).FirstOrDefault();
            GroupBox currentRoundGB = roundGroupBoxes[roundCount];

            // build a string from the characters in current round groupbox's textboxes
            string submission = string.Empty;
            foreach (TextBox tb in roundLetterDictionary[currentRoundGB])
            {
                submission += tb.Text;
            }
            Debug.Write($"checking value of string built from textboxes within {currentRoundGB}. Result: {submission}");
        }

        /// <summary>
        /// Event handler for the form's KeyUp event. This is used so that keyboard 
        /// input is processed even if a control other than a textbox is focused.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.Z)
                fillLetter(e.KeyCode.ToString());
            else if (e.KeyCode == Keys.Back)
                backspace();
        }

        /// <summary>
        /// Attempts to fill the first empty text box with a letter. Does nothing if there are no empty text boxes.
        /// </summary>
        /// <param name="letter">The letter to attempt to add to the text box.</param>
        private void fillLetter(string letter)
        {
            TextBox firstEmpty = getFirstEmptyActiveTextBox();
            if (firstEmpty != null)
            {
                firstEmpty.Text = letter.ToUpper();
                focusNextTextBox();
            }
        }

        /*
         * This handler is no longer necessary or relevant as casing is handled within the fillLetter method.
         */
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text != null)
                textBox.Text = textBox.Text.ToUpper();
        }

        /*
         * API testing event handlers:
         */

        private void getWoTD_Click(object sender, EventArgs e)
        {
            getWordOfTheDay();
        }

        /// <summary>
        /// Gets and stores the current word of the day.
        /// </summary>
        private async void getWordOfTheDay()
        {
            answer = await moonApiReader.getWordForToday();

            // test:
            answerLabel.Text = answer;
        }

        private void getWordForDate_Click(object sender, EventArgs e)
        {
            getWordForDate();
        }

        /// <summary>
        /// Gets and stores the word of the day for a particular date.
        /// </summary>
        private async void getWordForDate()
        {
            answer = await moonApiReader.getWordForDate(dateTimePicker.Value);
            
            // test:
            answerLabel.Text = answer;
        }

        private void getWordForRandomDate_Click(object sender, EventArgs e)
        {
            getWordForRandomDate();
        }

        /// <summary>
        /// Gets and stores the word of the day for a random date.
        /// </summary>
        private async void getWordForRandomDate()
        {
            answer = await moonApiReader.getWordForRandomDate();

            // test:
            answerLabel.Text = answer;
        }

        /*
         * Event handlers for keyboard buttons:
         */

        /// <summary>
        /// Event handler for the on-screen keyboard's letter button 'Click' events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void keyboardLetterButton_Click(object sender, EventArgs e)
        {
            string letter = ((Button)sender).Text;
            fillLetter(letter);
        }

        /// <summary>
        /// Event handler for the on-screen keyboard's backspace button 'Click' event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonBackspace_Click(object sender, EventArgs e)
        {
            backspace();
        }

        /// <summary>
        /// Event handler for the on-screen keyboard's clear button 'Click' event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClearWord_Click(object sender, EventArgs e)
        {
            // find all active textboxes with text and clear the text
            foreach (TextBox tb in getActiveTextBoxes().Where(target => target.Text != string.Empty))
            {
                tb.Text = string.Empty;
            }

            // set focus to the first textbox
            getActiveTextBoxes().First().Focus();
        }

        /// <summary>
        /// Attempts to clear the last filled text box. Moves focus to previous textbox if successful.
        /// </summary>
        private void backspace()
        {
            // should not be able to submit an answer, because as soon as backspace happens, there will not be 5 filled textboxes.
            setCheckButtonEnabledState(false);

            var last = getLastFilledActiveTextBox();
            if (last != null)
                last.Text = string.Empty;

            // move focus to preceding textbox, if any
            focusPreviousTextBox();
        }

        /// <summary>
        /// Attempts to move focus to the next textbox. If the last is already focused, does nothing.
        /// </summary>
        private void focusNextTextBox()
        {
            // find the focused text box, if any
            TextBox focusedTb = getActiveTextBoxes().Find(tb => tb.Focused == true);

            if (focusedTb != null)
            {
                // if the focused textbox is not the last, move focus forward
                if (getActiveTextBoxes().IndexOf(focusedTb) != -1)
                    this.SelectNextControl(focusedTb, true, true, false, false);
                // if it is, then all textboxes have been filled and the 'Check' button should be enabled
                else
                {
                    setCheckButtonEnabledState(true);
                }
            }
        }

        /// <summary>
        /// Attempts to move focus to the previous textbox. If the first is already focused, does nothing.
        /// </summary>
        private void focusPreviousTextBox()
        {
            // find the focused text box, if any
            TextBox focusedTb = getActiveTextBoxes().Find(tb => tb.Focused == true);

            if (focusedTb != null)
                // if the focused textbox is not the first, move focus backward
                if (getActiveTextBoxes().IndexOf(focusedTb) != 0)
                    this.SelectNextControl(focusedTb, false, true, false, false);
        }

        /// <summary>
        /// Enables or disables the current round's check button based on given parameter.
        /// </summary>
        /// <param name="shouldBeEnabled">true if should be enabled</param>
        private void setCheckButtonEnabledState(bool shouldBeEnabled)
        {
            foreach (Control c in getActiveGroupBox().Controls)
            {
                if (c.GetType() == typeof(Button))
                    c.Enabled = shouldBeEnabled;
            }
        }

        // test:
        private async void apiHealthButton_Click(object sender, EventArgs e)
        {
            await moonApiReader.isApiHealthy();
        }

        /// <summary>
        /// Event handler for the game's check buttons' 'Click' event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkButton_Click(object sender, EventArgs e)
        {
            submitGuess();
        }

        /// <summary>
        /// Gets the first empty active text box, or null if none.
        /// </summary>
        /// <returns>The first empty active text box control. Null if none.</returns>
        private TextBox getFirstEmptyActiveTextBox()
        {
            // use Linq to filter the active textBoxes by empty Text property and return the first
            var result = getActiveTextBoxes().Where(tb => tb.Text == string.Empty).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Gets the last filled active text box, or null if none.
        /// </summary>
        /// <returns>The last filled active text box control. Null if none.</returns>
        private TextBox getLastFilledActiveTextBox()
        {
            // use Linq to filter the active textBoxes by non-empty Text property and return the last 
            var result = getActiveTextBoxes().Where(tb => tb.Text != string.Empty).LastOrDefault();
            return result;
        }

        /// <summary>
        /// Gets the current round's groupbox which contains the input textboxes.
        /// </summary>
        /// <returns>The current round's groupbox.</returns>
        private GroupBox getActiveGroupBox()
        {
            // use Linq to filter the roundLetterDictionary keys and return the first Enabled one
            var result = roundLetterDictionary.Keys.Where(gb => gb.Enabled == true).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Gets a list of the current round's textboxes.
        /// </summary>
        /// <returns>A list containing the current round's textbox controls.</returns>
        private List<TextBox> getActiveTextBoxes()
        {
            var result = roundLetterDictionary[getActiveGroupBox()];
            return result;
        }

    }
}
