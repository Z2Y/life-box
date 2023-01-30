using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LifeActionCard : UIBase, IPointerClickHandler {
    private Model.Command actionCommand;
    private Text nameText;
    private Text descriptionText;

    private void Awake() {
        nameText = transform.Find("Name").GetComponent<Text>();
        descriptionText = transform.Find("Description").GetComponent<Text>();
    }

    public void SetCommand(Model.Command command) {
        actionCommand = command;
        nameText.text = command.Name;
        descriptionText.text = command.Description;
    }

    public void OnPointerClick(PointerEventData eventData) {
        actionCommand?.Expression.ExecuteExpression();
    }
}