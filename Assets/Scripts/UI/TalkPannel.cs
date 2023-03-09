using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    [PrefabResource("Prefabs/ui/DialoguePanel")]
    public class DialoguePanel : UIBase
    {
        public Text nameText;
        public Text dialogueText;
        public Image characterImage;
        public GameObject choicesPanel;
        public Button choiceButtonPrefab;
        public float typingSpeed = 0.02f;

        private DialogueLine currentDialogue;
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

        public void StartDialogue(DialogueLine dialogue)
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
            if (!string.IsNullOrEmpty(currentDialogue.spritePath))
            {
                await LoadCharacterImageAsync(currentDialogue.spritePath);
            }
            else
            {
                characterImage.sprite = null;
            }

            // Start typing dialogue text
            currentText = currentDialogue.text;
            nameText.text = currentDialogue.speakerName;

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
            if (currentDialogue.choices.Count > 0)
            {
                ShowChoices();
                UpdateChoices(currentDialogue.choices);
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
            // currentLineIndex = choice.nextLineIndex;

            var next = choice.onSelect();

            // Update dialogue
            if (next != null)
            {
                currentDialogue = next;
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

        public static async void Show(DialogueLine dialogue)
        {
            var panel = await UIManager.Instance.FindOrCreateAsync<DialoguePanel>() as DialoguePanel;
            if (!ReferenceEquals(panel, null))
            {
                panel.StartDialogue(dialogue);
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
        public Func<DialogueLine> onSelect;
    }
}