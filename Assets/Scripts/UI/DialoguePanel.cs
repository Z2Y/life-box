using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        public float typingSpeed = 0.05f;

        private DialogueLine currentDialogue;
        private bool isDialogueActive;
        private bool isTyping;
        private string currentText = "";

        public DialogueLine CurrentDialogue => currentDialogue;

        private void Awake()
        {
            transform.SetParent(UIManager.Instance.worldRoot);
        }

        public async UniTask StartDialogue(DialogueLine dialogue)
        {
            // 继承原先对话的取消回调
            if (dialogue.onCancel == null && gameObject.activeSelf)
            {
                dialogue.onCancel = currentDialogue?.onCancel;
            }
            
            Show();
            isDialogueActive = true;
            nameText.text = "";
            dialogueText.text = "";
            choicesPanel.SetActive(false);

            // Load first line of dialogue
            currentDialogue = dialogue;

            await LoadCurrentLine();
        }

        private async UniTask LoadCurrentLine()
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

        private async UniTask LoadCharacterImageAsync(string imagePath)
        {
            var loader = Resources.LoadAsync<Sprite>(imagePath);
            await loader;
            characterImage.sprite = loader.asset as Sprite;
        }

        private async UniTask TypeTextAsync()
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

        private void onCancel()
        {
            Hide();
            currentDialogue.onCancel?.Invoke();
        }

        private void UpdateChoices(List<DialogueChoice> choices)
        {
            // Remove previous buttons
            foreach (Transform child in choicesPanel.transform)
            {
                Destroy(child.gameObject);
            }

            if (!currentDialogue.uninterruptible)
            {
                choices.Add(new DialogueChoice()
                {
                    text = "结束对话",
                    onSelect = onCancel
                });
            }

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

        public static async UniTask<DialoguePanel> Show(DialogueLine dialogue)
        {
            if (await UIManager.Instance.FindOrCreateAsync<DialoguePanel>(true) is DialoguePanel panel)
            {
                await panel.StartDialogue(dialogue);
                return panel;
            }

            dialogue.onCancel();

            return null;
        }
    }
    
    [Serializable]
    public class DialogueLine
    {
        public string speakerName;
        public string text;
        public string spritePath;
        public bool uninterruptible;
        public List<DialogueChoice> choices = new ();
        public Action onCancel;
    }

    [Serializable]
    public class DialogueChoice
    {
        public string text;
        public Action onSelect;
    }
}