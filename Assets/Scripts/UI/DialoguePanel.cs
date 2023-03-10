using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.HeroEditor.Common.Scripts.Common;
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
        public GameObject choiceButtonPrefab;
        public float typingSpeed = 0.02f;

        private DialogueLine currentDialogue;
        private bool isDialogueActive;
        private bool isTyping;
        private string currentText = "";

        public DialogueLine CurrentDialogue => currentDialogue;

        private void Awake()
        {
            transform.SetParent(UIManager.Instance.worldRoot);
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

            ShowChoices();
            UpdateChoices(currentDialogue.choices);
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
            
            choices.Add(new DialogueChoice()
            {
                text = "结束对话",
                onSelect = Hide
            });

            // Create choice buttons
            foreach (var choice in choices)
            {
                var button = Instantiate(choiceButtonPrefab, choicesPanel.transform).GetComponent<Button>();
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

            choice.onSelect();

            // Update dialogue
        }

        public static async void Show(DialogueLine dialogue)
        {
            if (await UIManager.Instance.FindOrCreateAsync<DialoguePanel>(true) is DialoguePanel panel)
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
        public Action onSelect;
    }
}