﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace WinForms_Test
{
    public partial class Form1 : Form
    {
        static readonly HttpClient client = new HttpClient();
        Card[] cards = { };
        string[] lines = { };
        string[] cardKeywords = { };
        int libraryCount = 60;
        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
        }

        //Just opens up file window to select deck file
        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            testDialog.ShowDialog();
        }

        //Gets file-path for deck loading
        private void testDialog_FileOk(object sender, CancelEventArgs e)
        {
            fileTextBox.Text = testDialog.FileName;
        }

        //Loads deck file from file-path
        private async void btnLoadDeck_Click(object sender, EventArgs e)
        {
            lblStatus.Visible = true;
            lblSort.Visible = false;
            btnOpenFile.Visible = false;
            btnLoadDeck.Visible = false;
            fileTextBox.Visible = false;
            lblLoadDeck.Visible = false;
            sortTypeListBox.Visible = false;
            cardImagesGroupBox.Visible = false;
            lblStatus.Text = "Just a moment . . .";
            progBarLoadDeck.Value = 0;
            progBarLoadDeck.Visible = true;
            try
            {
                lines = File.ReadAllLines(fileTextBox.Text);
                if (Path.GetExtension(fileTextBox.Text) != ".txt")
                {
                    lblStatus.Text = "Invalid file type. Try again.";
                    return;
                }
                else if (lines.Length < 2)
                {
                    lblStatus.Text = "Invalid file. Try again.";
                    return;
                }
            }
            catch (ArgumentException)
            {
                lblStatus.Text = "File not found. Try again.";
                return;
            }
            catch (FileNotFoundException)
            {
                lblStatus.Text = "File not found. Try again.";
                return;
            }

            try
            {
                cards = await GetCardsAsync();
            }
            catch (IndexOutOfRangeException)
            {
                lblStatus.Text = "Invalid file. Try again.";
                return;
            }
            await DisplayCardsAsync();
            progBarLoadDeck.Visible = false;
            lblStatus.Visible = false;
            lblSort.Visible = true;
            btnOpenFile.Visible = true;
            btnLoadDeck.Visible = true;
            fileTextBox.Visible = true;
            lblLoadDeck.Visible = true;
            sortTypeListBox.Visible = true;
            cardImagesGroupBox.Visible = true;
        }

        //==========These three are literally just text box aesthetics==========
        private void fileTextBox_Enter(object sender, EventArgs e)
        {
            if (fileTextBox.Text == "File path goes here . . .")
            {
                fileTextBox.Text = string.Empty;
                fileTextBox.ForeColor = Color.Black;
            }
        }
        private void fileTextBox_Leave(object sender, EventArgs e)
        {
            if (fileTextBox.Text == string.Empty)
            {
                fileTextBox.ForeColor = Color.Gray;
                fileTextBox.Text = "File path goes here . . .";
            }
        }
        private void fileTextBox_TextChanged(object sender, EventArgs e)
        {
            if (fileTextBox.Text == "File path goes here . . .")
            {
                fileTextBox.ForeColor = Color.Gray;
            }
            else
            {
                fileTextBox.ForeColor = Color.Black;
            }
        }

        //Displays cards & percent chance of drawing any card type
        private void sortListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblUpdateValues.Visible = false;
            IEnumerable<Card> query = Enumerable.Empty<Card>();
            decimal totalPercent = 0m;
            sortTextBox.Text = string.Empty;
            switch (sortListBox.Tag)
            {
                case "Card Type":
                    query = 
                        from card in cards
                        where card.type_line.Contains($"{sortListBox.Text}") && card.amount > 0
                        select card;
                    break;
                case "Rarity":
                    query =
                        from card in cards
                        where card.rarity.Equals($"{sortListBox.Text}") && card.amount > 0
                        select card;
                    break;
            }

            foreach (Card card in query)
            {
                totalPercent += card.percent;
            }
            sortTextBox.Text += totalPercent + "% chance of drawing\r\n";
            foreach (Card card in query)
            {
                sortTextBox.Text += card.name + "\r\n";
            }
        }

        //Tells sortListBox what to do basically
        private void sortTypeListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            sortListBox.Tag = sortTypeListBox.Text;
            sortListBox.Visible = true;
            sortTextBox.Visible = true;
            keywordTextBox.Visible = false;
            lblFilter.Visible = true;
            wCheckBox.Visible = false;
            uCheckBox.Visible = false;
            bCheckBox.Visible = false;
            rCheckBox.Visible = false;
            gCheckBox.Visible = false;
            sortListBox.Items.Clear();
            keywordTextBox.Text = string.Empty;

            switch (sortTypeListBox.Text)
            {
                case "Card Type":
                    string[] cardTypes = { "Land", "Instant", "Sorcery", "Creature", "Artifact", "Enchantment", "Planeswalker" };
                    foreach (string str in cardTypes)
                    {
                        sortListBox.Items.Add(str);
                    }
                    break;
                case "Keywords":
                    sortListBox.Visible = false;
                    keywordTextBox.Visible = true;
                    keywordTextBox.Tag = "Normal";
                    keywordTextBox.Multiline = true;
                    break;
                case "Custom Keywords":
                    sortListBox.Visible = false;
                    keywordTextBox.Visible = true;
                    keywordTextBox.Tag = "Custom";
                    keywordTextBox.Multiline = false;
                    break;
                case "Rarity":
                    string[] rarities = { "common", "uncommon", "rare", "mythic" };
                    foreach (string str in rarities)
                    {
                        sortListBox.Items.Add(str);
                    }
                    break;
                case "Color":
                    wCheckBox.Visible = true;
                    uCheckBox.Visible = true;
                    bCheckBox.Visible = true;
                    rCheckBox.Visible = true;
                    gCheckBox.Visible = true;
                    sortListBox.Visible = false;
                    break;
            }
        }

        //Gets card names & percentage for offical game keywords
        private void keywordTextBox_TextChanged(object sender, EventArgs e)
        {
            lblUpdateValues.Visible = false;
            sortTextBox.Text = string.Empty;
            decimal totalPercent = 0m;
            if ((string)keywordTextBox.Tag == "Normal")
            {
                string[] keywordsInput = keywordTextBox.Lines;
                string[] keywords = new string[keywordsInput.Length];
                for (int i = 0; i < keywords.Length; i++)
                {
                    keywords[i] = UppercaseFirst(keywordsInput[i].ToLower());
                }
                List<string> selectedCards = new List<string>();
                foreach (Card card in cards)
                {
                    if (card.keywords.Any(x => keywords.Contains(x)))
                    {
                        if (card.amount > 0)
                        {
                            selectedCards.Add(card.name);
                            totalPercent += card.percent;
                        }
                    }
                }

                sortTextBox.Text += totalPercent + " % chance of drawing\r\n";
                foreach (string str in selectedCards)
                {
                    sortTextBox.Text += str + "\r\n";
                }
            }
            //TODO: This portion still adds the percentages of each keyword, instead of further filtering them
            else if ((string)keywordTextBox.Tag == "Custom")
            {
                sortTextBox.Text = string.Empty;
                string[] searchInput = keywordTextBox.Text.Split(' ');
                string[] search = new string[searchInput.Length];
                for (int i = 0; i < search.Length; i++)
                {
                    search[i] = searchInput[i].ToLower();
                }
                List<string> selectedCards = new List<string>();
                foreach (Card card in cards)
                {
                    string[] oracleWords = card.oracle_text?.Split(' ');
                    if (oracleWords == null)
                    {
                        continue;
                    }
                    for (int i = 0; i < oracleWords.Length; i++)
                    {
                        oracleWords[i] = oracleWords[i].ToLower();
                    }
                    if (IsSubArray(oracleWords, search, oracleWords.Length, search.Length))
                    {
                        if (card.amount > 0)
                        {
                            selectedCards.Add(card.name);
                            totalPercent += card.percent;
                        }
                    }

                }

                sortTextBox.Text += totalPercent + " % chance of drawing\r\n";
                foreach (string str in selectedCards)
                {
                    sortTextBox.Text += str + "\r\n";
                }
            }
        }

        //For debug purposes, will remove for production
        private void debugTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        //These two just let you drop a file into fileTextBox
        private void fileTextBox_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            foreach (string str in fileList)
            {
                fileTextBox.Text = str;
            }
        }
        private void fileTextBox_DragEnter(object sender, DragEventArgs e)
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

        //These update the checkbox-ed cards everytime a checkbox is changed
        private void UpdateCheckBoxes(object sender, EventArgs e)
        {
            lblUpdateValues.Visible = false;
            sortTextBox.Text = string.Empty;
            decimal totalPercent = 0m;
            List<string> checkedColors = new List<string>();
            if (wCheckBox.Checked)
            {
                checkedColors.Add("W");
            }
            if (uCheckBox.Checked)
            {
                checkedColors.Add("U");
            }
            if (bCheckBox.Checked)
            {
                checkedColors.Add("B");
            }
            if (rCheckBox.Checked)
            {
                checkedColors.Add("R");
            }
            if (gCheckBox.Checked)
            {
                checkedColors.Add("G");
            }
            checkedColors.Sort();
            List<string> selectedCards = new List<string>();
            foreach (Card card in cards)
            {
                List<string> both = card.color_identity.Intersect(checkedColors).ToList();
                both.Sort();
                if (checkedColors.Count == both.Count)
                {
                    if (both.SequenceEqual(checkedColors))
                    {
                        if (card.amount > 0)
                        {
                            selectedCards.Add(card.name);
                            totalPercent += card.percent;
                        }
                    }
                }
            }
            if (!wCheckBox.Checked && !uCheckBox.Checked && !bCheckBox.Checked && !rCheckBox.Checked && !gCheckBox.Checked)
            {
                sortTextBox.Text = "0% chance of drawing\r\n";
            }
            else
            {
                sortTextBox.Text += totalPercent + "% chance of drawing\r\n";
                foreach (string str in selectedCards)
                {
                    sortTextBox.Text += str + "\r\n";
                }
            }
        }

        //Draws/Undraws a card, then updates card labels
        private void OnPictureBoxClick(object sender, MouseEventArgs e)
        {
            PictureBox pB = sender as PictureBox;
            int index = int.Parse((string)pB.Tag);

            if (e.Button == MouseButtons.Left)
            {
                if (cards[index].amount > 0)
                {
                    cards[index].amount--;
                    UpdateCardPercent(index);
                    UpdateCardLabels();
                    NotifyUpdateValues();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (cards[index].amount < cards[index].maxAmount)
                {
                    cards[index].amount++;
                    UpdateCardPercent(index);
                    UpdateCardLabels();
                    NotifyUpdateValues();
                }
            }
            else
            {
                return;
            }
        }

        //
        //
        // All methods past this point are not EventHandlers - They are simply for added function & readability
        //
        //

        //Reads deck file and converts it to list of card objects
        //Just don't even try to understand how this works. I don't even know anymore.
        private async Task<Card[]> GetCardsAsync()
        {

            //Creates new array of only lines of cards from file
            int newLinesLength = 0;
            for (int i = 1; lines[i].Length > 0 && lines[i + 1] != null; i++)
            {
                newLinesLength++;
            }
            string[] newLines = new string[newLinesLength];
            for (int i = 0; i < newLines.Length; i++)
            {
                newLines[i] = lines[i + 1];
            }
            Card[] cardList = new Card[newLines.Length];
            for (int i = 0; i < cardList.Length; i++)
            {
                //Getting card amounts of cards
                int cardAmount = 0;
                string cardAmountStr = "";
                for (int j = 0; Char.IsDigit(newLines[i][j]); j++)
                {
                    cardAmountStr += newLines[i][j];
                }
                cardAmount = int.Parse(cardAmountStr);

                //Getting start & end points for names of cards
                int startIndex = 0;
                for (int j = 0; !Char.IsLetter(newLines[i][j]); j++)
                {
                    startIndex++;
                }
                int endIndex = startIndex;
                for (int j = startIndex; newLines[i][j] != '('; j++)
                {
                    endIndex++;
                }
                endIndex -= 2;

                //Getting names of cards
                string cardName = "";
                for (int j = startIndex; j <= endIndex; j++)
                {
                    cardName += newLines[i][j];
                }

                //Creating Card objects
                string json = await client.GetStringAsync($"https://api.scryfall.com/cards/named?fuzzy={cardName}");
                cardList[i] = JsonConvert.DeserializeObject<Card>(json);
                cardList[i].amount = cardAmount;
                cardList[i].maxAmount = cardAmount;
                cardList[i].percent = Math.Round((decimal)(cardList[i].amount / (decimal)libraryCount * 100m), 2);
                progBarLoadDeck.Value = (100 / cardList.Length) * i;
                Thread.Sleep(100);
            }
            return cardList;
        }
        //Displays pictures of cards in pictureBox group
        private async Task DisplayCardsAsync()
        {
            foreach (PictureBox pictureBox in cardImagesGroupBox.Controls.OfType<PictureBox>())
            {
                await Task.Run(() => pictureBox.Image = null);
                try
                {
                    int index = int.Parse(pictureBox.Tag.ToString());
                    if (cards[index].layout == "modal_dfc" || cards[index].layout == "transform" || cards[index].layout == "double_sided")
                    {
                        await Task.Run(() => pictureBox.Load(cards[index].card_faces[0].image_uris.large));
                    }
                    else
                    {
                        await Task.Run(() => pictureBox.Load(cards[index].image_uris.large));
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
            foreach (Label label in cardImagesGroupBox.Controls.OfType<Label>())
            {
                label.Text = string.Empty;
                try
                {
                    int index = int.Parse(label.Tag.ToString());
                    label.Text = $"{cards[index].name} - {cards[index].amount}\n{cards[index].percent}%";
                    label.Visible = true;
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
            cardImagesGroupBox.Visible = true;
        }
        //Just updates the card labels without loading pictures too
        private void UpdateCardLabels()
        {
            foreach (Label label in cardImagesGroupBox.Controls.OfType<Label>())
            {
                try
                {
                    int index = int.Parse(label.Tag.ToString());
                    label.Text = $"{cards[index].name} - {cards[index].amount}\n{cards[index].percent}%";
                    label.Visible = true;
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
        }
        //Checks if array is inside another array in its original order
        private bool IsSubArray(string[] A, string[] B, int n, int m)
        {
            // Two pointers to traverse the arrays 
            int i = 0, j = 0;
            // Traverse both arrays simultaneously 
            while (i < n && j < m)
            {
                // If element matches 
                // increment both pointers 
                if (A[i] == B[j])
                {
                    i++;
                    j++;
                    // If array B is completely 
                    // traversed 
                    if (j == m)
                        return true;
                }
                // If not, 
                // increment i and reset j 
                else
                {
                    i = i - j + 1;
                    j = 0;
                }
            }
            return false;
        }
        //Literally just makes first letter of a string capital
        private string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return char.ToUpper(s[0]) + s.Substring(1);
        }
        //Just updates the card percent since it's based off of the amount property
        private void UpdateCardPercent(int i)
        {
            cards[i].percent = Math.Round((decimal)(cards[i].amount / (decimal)libraryCount * 100m), 2);
        }
        //Notifies user that values are out of date if any cards have been drawn/undrawn
        private void NotifyUpdateValues()
        {
            if (sortTextBox.Visible)
            {
                if (sortTextBox.Lines.Length > 1)
                {
                    lblUpdateValues.Visible = true;
                }
            }
        }
    }

    //Card objects - source of all data for individual cards
    class Card
    {
        public int amount { get; set; }
        public int maxAmount { get; set; }
        public decimal cmc { get; set; }
        public decimal percent { get; set; }
        public string[] color_identity { get; set; }
        public string[] keywords { get; set; }
        public string name { get; set; }
        public string oracle_text { get; set; }
        public string power { get; set; }
        public string rarity { get; set; }
        public string toughness { get; set; }
        public string type_line { get; set; }
        public string layout { get; set; }
        public CardFace[] card_faces { get; set; }
        public ImageURI image_uris { get; set; }
    }
    //Links to different size images for a card
    class ImageURI
    {
        public string small { get; set; }
        public string normal { get; set; }
        public string large { get; set; }
        public string png { get; set; }
    }
    //Only for multiface cards
    class CardFace
    {
        public string name { get; set; }
        public string mana_cost { get; set; }
        public ImageURI image_uris { get; set; }
    }
}
