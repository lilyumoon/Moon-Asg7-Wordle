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
using System.Windows.Forms.VisualStyles;

namespace Moon_Asg7_Wordle
{
    public partial class MainForm : Form
    {
        private MoonAPIReader moonApiReader;

        private Dictionary<int, string> resultMessageDictionary = new Dictionary<int, string>();
        private Dictionary<string, Button> usedLetterDictionary = new Dictionary<string, Button>();

        // this dictionary is built in order to reduce complexity of some of the in-class suggestions of text box processing
        private Dictionary<GroupBox, List<TextBox>> roundLetterDictionary = new Dictionary<GroupBox, List<TextBox>>();

        private List<GroupBox> roundGroupBoxes = new List<GroupBox>();

        private int roundCount = -1;
        private string answer = string.Empty;

        // Unused. Had over-enthusiastically planned to track which days were completed, but decided it was too far out of scope.
        Dictionary<string, bool> completedDays = new Dictionary<string, bool>();

        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            await setup();

            // ensure that the form processes all key events even if a control has focus
            // and assign a handler for KeyDown events
            this.KeyPreview = true;
            this.KeyDown += mainForm_KeyDown;
        }

        private async Task setup()
        {
            moonApiReader = new MoonAPIReader();

            roundGroupBoxes = new List<GroupBox>() {
                groupRound0, groupRound1, groupRound2,
                groupRound3, groupRound4, groupRound5 };

            buildDictionaries();
            
            // if API is down, display a warning
            bool isApiHealthy = await moonApiReader.isApiHealthy();
            if (!isApiHealthy)
                label_apiHealth.Visible = true;
        }

        private void buildDictionaries()
        {
            // (round count, result message) dictionary
            resultMessageDictionary.Add(0, "No way, you cheated!");
            resultMessageDictionary.Add(1, "Very Impressive!");
            resultMessageDictionary.Add(2, "Excellent!");
            resultMessageDictionary.Add(3, "Nicely done.");
            resultMessageDictionary.Add(4, "Cutting it close...");
            resultMessageDictionary.Add(5, "Barely squeaked by there, friend");
            resultMessageDictionary.Add(6, "Better luck next time. :-)");

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

        private void resetGame()
        {
            // disable all controls that start a new game
            setWordPickerControlsEnabledState(false);

            // Reset all round groupboxes and their text boxes
            foreach (GroupBox gb in roundGroupBoxes)
            {
                foreach (TextBox textBox in roundLetterDictionary[gb])
                {
                    textBox.Text = string.Empty;
                    textBox.BackColor = Control.DefaultBackColor;
                    textBox.ForeColor = Control.DefaultForeColor;
                }
                gb.Enabled = false;
            }

            // Reset hint colors and enabled state of keyboard keys
            foreach (var button in usedLetterDictionary.Values)
            {
                button.BackColor = Control.DefaultBackColor;
            }
            setOnscreenKeyboardEnabledState(true);

            // reset round count
            roundCount = 0;

            // Enable first round's text boxes and set focus to the first
            roundGroupBoxes[roundCount].Enabled = true;
            roundLetterDictionary[groupRound0][0].Focus();
        }

        private void submitGuess()
        {
            GroupBox currentRoundGB = getActiveGroupBox();

            // build a string from the characters in current round groupbox's textboxes
            string submission = string.Empty;
            foreach (TextBox tb in roundLetterDictionary[currentRoundGB])
            {
                submission += tb.Text;
            }

            // validate submitted guess
            bool isValid = validateGuess(submission);

            // if guess is valid, check it. If not, give invalid-guess feedback.
            if (isValid)
            {
                feedbackLabel.Visible = false;
                checkGuess(submission);
            }
            else
            {
                // give feedback if guess is not valid
                feedbackLabel.Text = "Not in the word list. Please try again!";
                feedbackLabel.Visible = true;
            }
        }

        /// <summary>
        /// Checks if the submitted guess is a valid word.
        /// </summary>
        /// <param name="submission">The guess to validate.</param>
        /// <returns>true if valid</returns>
        private bool validateGuess(string submission)
        {
            bool isValid = false;

            Wordle.WordStatus wordStatus = Wordle.checkWordStatus(submission, answer);
            if (wordStatus == Wordle.WordStatus.ValidWord || wordStatus == Wordle.WordStatus.CorrectWord)
                isValid = true;

            return isValid;
        }

        private void checkGuess(string submission)
        {
            // build a list of letter statuses mapped to submission characters
            List<Wordle.LetterStatus> letterStatuses = new List<Wordle.LetterStatus>();

            for (int i = 1; i < 6; i++)
            {
                int letterIndex = i - 1;

                // get the letter status from Wordle.cs
                Wordle.LetterStatus letterStatus = Wordle.checkLetterStatus(i, submission[letterIndex].ToString(), answer);

                letterStatuses.Add(letterStatus);
            }

            /*
             * 
             * Special case for consideration: 
             *  ex. guess = crack; answer = knock (word of the day - 1/15/25)
             *  
             *  issue: with old logic, first c would be yellow and second c would be green
             * 
             * 
             */

            Color[] textBoxFeedbackColors = new Color[] { Color.Gray, Color.Gray, Color.Gray, Color.Gray, Color.Gray };
            bool[] consumedAnswerPositions = new bool[5];
            bool[] consumedGuessPositions = new bool[5];

            // identify and set feedback colors for any exact matches, and consume guess and answer positions
            for (int i = 0; i < 5; i++)
            {
                if (letterStatuses[i] == Wordle.LetterStatus.CorrectLetter)
                {
                    consumedAnswerPositions[i] = true;
                    consumedGuessPositions[i] = true;
                    textBoxFeedbackColors[i] = Color.Green;
                }
            }

            // second pass: handle potential false positives for ValidLetter
            for (int i = 0; i < 5; i++)
            {
                if (!consumedGuessPositions[i] && letterStatuses[i] == Wordle.LetterStatus.ValidLetter)
                {
                    char letterToCheck = submission[i];

                    // count occurrences of the letter in both the guess and the answer,
                    // considering only unconsumed answer positions
                    int submissionLetterCount = 0;
                    int answerLetterCount = 0;

                    for (int j = 0; j < 5; j++)
                    {
                        if (!consumedGuessPositions[j] && submission[j] == letterToCheck)
                            submissionLetterCount++;
                        if (!consumedAnswerPositions[j] && answer[j] == letterToCheck)
                            answerLetterCount++;
                    }

                    // if there are more occurrences in the guess than in the answer, adjust the statuses
                    if (submissionLetterCount > answerLetterCount)
                    {
                        int excessCount = submissionLetterCount - answerLetterCount;

                        // loop through the guess to correct the excess ValidLetter occurrences
                        for (int j = 0; j < 5 && excessCount > 0; j++)
                        {
                            if (!consumedGuessPositions[j] &&
                                submission[j] == letterToCheck &&
                                letterStatuses[j] == Wordle.LetterStatus.ValidLetter)
                            {
                                letterStatuses[j] = Wordle.LetterStatus.NotInWord;
                                excessCount--;
                            }
                        }
                    }
                }
            } // (end second pass)

            // iterate through each active textbox and apply the feedback color
            List<TextBox> activeTextBoxes = getActiveTextBoxes();
            for (int i = 0; i < 5; i++)
            {
                activeTextBoxes[i].BackColor = textBoxFeedbackColors[i];
            }

            // give feedback via on-screen keyboard keys
            for (int i = 0; i < 5; i++)
            {
                string letter = submission[i].ToString();

                // skip the current iteration if the key has already been marked 'green'
                if (usedLetterDictionary[letter].BackColor != Color.Green)
                {
                    Color keyboardFeedbackColor;

                    if (letterStatuses[i] == Wordle.LetterStatus.CorrectLetter)
                        keyboardFeedbackColor = Color.Green;
                    else if (letterStatuses[i] == Wordle.LetterStatus.ValidLetter)
                        keyboardFeedbackColor = Color.Yellow;
                    else
                    {
                        // only mark the key as gray if it's not already yellow
                        if (usedLetterDictionary[letter].BackColor != Color.Yellow)
                            keyboardFeedbackColor = Color.Gray;
                        else
                            keyboardFeedbackColor = Color.Yellow;
                    }

                    // apply feedback color to key
                    usedLetterDictionary[letter].BackColor = keyboardFeedbackColor;
                }
            }

            roundGroupBoxes[roundCount].Enabled = false;
            
            // if answer was guessed, end game and give feedback
            if (Wordle.checkWordStatus(submission, answer) == Wordle.WordStatus.CorrectWord)
            {
                setOnscreenKeyboardEnabledState(false);
                setWordPickerControlsEnabledState(true);
                feedbackLabel.Text = resultMessageDictionary[roundCount];
                feedbackLabel.Visible = true;
                roundCount = -1;
            }

            // if answer was not guessed...
            else
            {
                roundCount++;
                // if there are rounds left, start next round
                if (roundCount < 6)
                {
                    GroupBox activeGb = roundGroupBoxes[roundCount];
                    activeGb.Enabled = true;
                    var tb = getFirstEmptyActiveTextBox();
                    tb.Focus();
                }
                // if there are not rounds left, give end-game feedback
                else
                {
                    setOnscreenKeyboardEnabledState(false);
                    setWordPickerControlsEnabledState(true);
                    feedbackLabel.Text = resultMessageDictionary[roundCount];
                    feedbackLabel.Visible = true;
                    roundCount = -1;
                }
            }

        }

        /// <summary>
        /// Event handler for the form's KeyUp event. This is used so that keyboard 
        /// input is processed even if a control other than a textbox is focused.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && roundCount != -1)
                backspace();
            else if (e.KeyCode == Keys.Enter && roundCount != -1)
                submitGuess();
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

        /// <summary>
        /// Event handler for textbox TextChanged events. Corrects casing and handles focus progression.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            // Temporarily unsubscribe from the TextChanged event to avoid recursion
            textBox.TextChanged -= textBox_TextChanged;

            try
            {
                // If the Text is anything except for an empty string...
                if (textBox.Text != string.Empty)
                {
                    char input = textBox.Text[0];

                    // If the Text is a letter, check the casing.
                    if (char.IsLetter(input))
                    {
                        // If the text is lowercase, set it to uppercase.
                        if (char.IsLower(input))
                            textBox.Text = char.ToUpper(input).ToString();

                        // move focus forward
                        focusNextTextBox();
                    }
                    else
                    {
                        // If it is not a letter, clear it and maintain focus here.
                        textBox.Text = string.Empty;
                        textBox.Focus();
                    }
                }
                // If the Text is an empty string and the game is not over, focus the previous textbox.
                else if (roundCount != -1)
                    focusPreviousTextBox();
            }

            finally
            {
                // Resubscribe to the TextChanged event
                textBox.TextChanged += textBox_TextChanged;
            }
        }

        private void getWoTD_Click(object sender, EventArgs e)
        {
            getWordOfTheDay();
        }

        /// <summary>
        /// Gets today's WoTD and starts a new game.
        /// </summary>
        private async void getWordOfTheDay()
        {
            setWordPickerControlsEnabledState(false);

            answer = await moonApiReader.getWordForToday();
            answer = answer.ToUpper();

            resetGame();
        }

        private void getWordForDate_Click(object sender, EventArgs e)
        {
            getWordForDate();
        }

        /// <summary>
        /// Gets the WoTD for a chosen date and starts a new game.
        /// </summary>
        private async void getWordForDate()
        {
            setWordPickerControlsEnabledState(false);

            answer = await moonApiReader.getWordForDate(dateTimePicker.Value);
            answer = answer.ToUpper();

            resetGame();
        }

        private void getWordForRandomDate_Click(object sender, EventArgs e)
        {
            getWordForRandomDate();
        }

        /// <summary>
        /// Gets a random day's WoTD and starts a new game.
        /// </summary>
        private async void getWordForRandomDate()
        {
            setWordPickerControlsEnabledState(false);

            answer = await moonApiReader.getWordForRandomDate();
            answer = answer.ToUpper();

            resetGame();
        }

        /*
         * Event handlers for on-screen keyboard buttons:
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
                int focusedTbIndex = getActiveTextBoxes().IndexOf(focusedTb);
                // if the focused textbox is not the last, move focus forward
                if (focusedTbIndex != 4)
                {
                    getActiveTextBoxes()[focusedTbIndex + 1].Focus();
                }
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
            {
                int focusedTbIndex = getActiveTextBoxes().IndexOf(focusedTb);

                // if the focused textbox is not the first, move focus backward
                if (focusedTbIndex != 0)
                    getActiveTextBoxes()[focusedTbIndex - 1].Focus();
            }
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

        private void setOnscreenKeyboardEnabledState(bool shouldBeEnabled)
        {
            foreach (var button in usedLetterDictionary.Values)
            {
                button.Enabled = shouldBeEnabled;
            }
            buttonClearWord.Enabled = shouldBeEnabled;
            buttonBackspace.Enabled = shouldBeEnabled;
        }

        private void setWordPickerControlsEnabledState(bool shouldBeEnabled)
        {
            todayGameButton.Enabled = shouldBeEnabled;
            randomGameButton.Enabled = shouldBeEnabled;
            pastGameButton.Enabled = shouldBeEnabled;
            dateTimePicker.Enabled = shouldBeEnabled;
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
            TextBox result = getActiveTextBoxes().Where(tb => tb.Text == string.Empty).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Gets the last filled active text box, or null if none.
        /// </summary>
        /// <returns>The last filled active text box control. Null if none.</returns>
        private TextBox getLastFilledActiveTextBox()
        {
            // use Linq to filter the active textBoxes by non-empty Text property and return the last 
            TextBox result = getActiveTextBoxes().Where(tb => tb.Text != string.Empty).LastOrDefault();
            return result;
        }

        /// <summary>
        /// Gets the current round's groupbox which contains the input textboxes.
        /// </summary>
        /// <returns>The current round's groupbox.</returns>
        private GroupBox getActiveGroupBox()
        {
            // use Linq to filter the roundLetterDictionary keys and return the first Enabled one
            GroupBox result = roundLetterDictionary.Keys.Where(gb => gb.Enabled == true).FirstOrDefault();
            return result;
        }

        /// <summary>
        /// Gets a list of the current round's textboxes.
        /// </summary>
        /// <returns>A list containing the current round's textbox controls.</returns>
        private List<TextBox> getActiveTextBoxes()
        {
            List<TextBox> result = roundLetterDictionary[getActiveGroupBox()];
            return result;
        }

    }
}
