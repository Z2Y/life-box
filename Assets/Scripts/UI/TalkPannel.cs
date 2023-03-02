using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DialoguePanel : UIBase
    {
        public Text nameText;
        public Text dialogueText;
        public Image characterImage;
        public GameObject choicesPanel;
        public Button choiceButtonPrefab;
        public float typingSpeed = 0.02f;

        private List<DialogueLine> currentDialogue;
        private int currentLineIndex;
        private bool isDialogueActive;
        private bool isTyping;
        private string currentText = "";
        
        private void Awake()
        {
            nameText = transform.Find("Name").GetComponent<Text>();
            dialogueText = transform.Find("Dialogue").GetComponent<Text>();
            characterImage = transform.Find("CharacterImage").GetComponent<Image>();
            choicesPanel = transform.Find("ChoicesPanel").gameObject;
        }

        public async Task StartDialogueAsync(List<DialogueLine> dialogue)
        {
            // Reset dialogue box
            Show();
            isDialogueActive = true;
            nameText.text = "";
            dialogueText.text = "";
            choicesPanel.SetActive(false);

            // Load first line of dialogue
            currentDialogue = dialogue;
            currentLineIndex = 0;
            LoadCurrentLine();
        }

        private async void LoadCurrentLine()
        {
            // Clear previous text
            dialogueText.text = "";

            // Load speaker image
            if (!string.IsNullOrEmpty(currentDialogue[currentLineIndex].spritePath))
            {
                await LoadCharacterImageAsync(currentDialogue[currentLineIndex].spritePath);
            }
            else
            {
                characterImage.sprite = null;
            }

            // Start typing dialogue text
            currentText = currentDialogue[currentLineIndex].text;
            nameText.text = currentDialogue[currentLineIndex].speakerName;

            await TypeTextAsync();
        }

        private async Task LoadCharacterImageAsync(string imagePath)
        {
            var loader = Resources.LoadAsync<Sprite>(imagePath);
            await YieldCoroutine.WaitForInstruction(loader);
            characterImage.sprite = loader.asset as Sprite;
        }

        private async Task TypeTextAsync()
        {
            dialogueText.text = "";
            isTyping = true;
            foreach (var c in currentText)
            {
                dialogueText.text += c;
                await YieldCoroutine.WaitForSeconds(typingSpeed);
            }

            isTyping = false;
            if (currentDialogue[currentLineIndex].choices.Count > 0)
            {
                ShowChoices();
                UpdateChoices(currentDialogue[currentLineIndex].choices);
            }
        }

        private void ShowChoices()
        {
            choicesPanel.SetActive(true);
        }

        private void HideChoices()
        {
            choicesPanel.SetActive(false);
        }

        private void UpdateChoices(List<DialogueChoice> choices)
        {
            // Remove previous buttons
            foreach (Transform child in choicesPanel.transform)
            {
                Destroy(child.gameObject);
            }

            // Create choice buttons
            foreach (var choice in choices)
            {
                var button = Instantiate(choiceButtonPrefab, choicesPanel.transform);
                button.GetComponentInChildren<Text>().text = choice.text;
                button.onClick.AddListener(() => OnChoiceSelected(choice));
            }
        }

        private void OnChoiceSelected(DialogueChoice choice)
        {
            // Hide choices
            HideChoices();

            // Go to the corresponding dialogue line
            currentLineIndex = choice.nextLineIndex;

            // Update dialogue
            if (currentLineIndex < currentDialogue.Count)
            {
                LoadCurrentLine();
            }
            else
            {
                // End of dialogue
                Hide();
                isDialogueActive = false;

                // Reset choices
                HideChoices();
            }
        }
    }
    
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string text;
        public string spritePath;
        public List<DialogueChoice> choices = new ();
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public int nextLineIndex;
    }
}