using System;
using System.Collections.Generic;
using MidnightObituary.Gameplay.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MidnightObituary.UI
{
    public sealed class CallGreyboxView : MonoBehaviour
    {
        [SerializeField] private TMP_Text npcText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private TMP_Text hudText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Transform choicesRoot;
        [SerializeField] private Button choiceButtonPrefab;
        [SerializeField] private Button returnButton;

        public Button ReturnButton => returnButton;

        public void SetNpc(string npcName, string personalityTag)
        {
            npcText.text = $"{npcName} [{personalityTag}]";
        }

        public void SetHud(int stress, int stressMax, int cigarettes, int breakdown, int breakdownMax, int count, int target)
        {
            hudText.text = $"Stress: {stress}/{stressMax} | Cigarettes: {cigarettes}\nBreakdown: {breakdown}/{breakdownMax} | Count: {count}/{target}";
        }

        public void ShowNode(DialogueNodeDefinition node, Action<string> onChoiceClicked)
        {
            dialogueText.text = node.Text;

            foreach (Transform child in choicesRoot)
            {
                Destroy(child.gameObject);
            }

            foreach (DialogueChoiceDefinition choice in node.Choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choicesRoot);
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                label.text = choice.Text;
                string choiceId = choice.ChoiceId;
                button.onClick.AddListener(() => onChoiceClicked(choiceId));
            }
        }

        public void ShowResult(string result)
        {
            resultText.text = result;
        }
    }
}