using System;
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
        int cardAmount = 60;
        public Form1()
        {
            InitializeComponent();
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
            lblDeckText.Visible = true;
            lblDeckText.Text = "Just a moment . . .";
            try
            {
                lines = File.ReadAllLines(fileTextBox.Text);
                if (Path.GetExtension(fileTextBox.Text) != ".txt")
                {
                    lblDeckText.Text = "Invalid file type. Try again.";
                    return;
                }
                else if (lines.Length < 2)
                {
                    lblDeckText.Text = "Invalid file. Try again.";
                    return;
                }
            }
            catch (ArgumentException)
            {
                lblDeckText.Text = "File not found. Try again.";
                return;
            }
            catch (FileNotFoundException)
            {
                lblDeckText.Text = "File not found. Try again.";
                return;
            }

            try
            {
                cards = await GetCards();
            }
            catch (IndexOutOfRangeException)
            {
                lblDeckText.Text = "Invalid file. Try again.";
                return;
            }
            await DisplayCards();
            lblDeckText.Visible = false;
        }

        //Reads deck file and converts it to list of card objects
        //Just don't even try to understand how this works. I don't even know anymore.
        private async Task<Card[]> GetCards()
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
                cardList[i].percent = Math.Round((decimal)((cardList[i].amount / cardAmount) * 100), 2);
                Thread.Sleep(100);
            }
            return cardList;
        }

        //Displays pictures of cards in pictureBox group
        private Task DisplayCards()
        {
            foreach (var pictureBox in groupBox1.Controls.OfType<PictureBox>())
            {
                try
                {
                    int index = int.Parse(pictureBox.Tag.ToString());
                    if (cards[index].layout == "modal_dfc" || cards[index].layout == "transform" || cards[index].layout == "double_sided")
                    {
                        pictureBox.Load(cards[index].card_faces[0].image_uris.large);
                    }
                    else
                    {
                        pictureBox.Load(cards[index].image_uris.large);
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    continue;
                }
            }
            return Task.CompletedTask;
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
    }

    class Card
    {
        public int amount { get; set; }
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
    class ImageURI
    {
        public string small { get; set; }
        public string normal { get; set; }
        public string large { get; set; }
        public string png { get; set; }
    }
    class CardFace
    {
        public string name { get; set; }
        public string mana_cost { get; set; }
        public ImageURI image_uris { get; set; }
    }
}
