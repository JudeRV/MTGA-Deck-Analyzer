# MTG Arena Deck Analyzer Application
A small MTG Arena deck analyzer app I built to learn WinForms
---

**How to use:**
1) Download all the files from this repository and extract them to a single folder.
2) Run `Winforms Test.exe`.
3) Open MTG Arena and export one of your deck files if you haven't already. This will only copy the text to your clipboard, so you must then paste the text into a text document (must end in .txt). Make sure that, if your deck has a Companion card, you remove it from the top; the top line must always read "Deck". Save this file wherever you'd like.
4) Use the app to load the deck file into the program, and enjoy!

**Some notes:**
- You can draw a card by left-clicking on its picture on the right-hand side of the window. You can put that card back into the library by right-clicking on it.
- This program will NOT work with any deck that is not copied from or made exactly like it is in MTG Arena.
- This program will NOT work with any deck that includes more than 32 unique cards.
- [This](https://pastebin.com/dHWF1nhc) is an example of a deck that will properly work in the app. The Sideboard section is not necessary, and the file can end at the last line of the "Deck" cards.
- When sorting by official Keywords, you must enter in each keyword on a separate line in the textbox.
- When sorting by custom keywords, each word you input into the textbox must match directly with the word on the card, including any punctuation that may be attached to that word. Capitalization does not apply to this rule.
- When drawing or undrawing a card from the library, you must update the values in the results box manually. This can be achieved simply by making a small change in the sort and then reverting it back.
- Be sure to send me a message for any bugs or feature requests that you might want for the application that aren't already listed here.
- Most of these notes include bugs or capabilities that I'd like to remove and improve, respectively. I may improve the application in the future, but this stands as a v1.0 working application.
