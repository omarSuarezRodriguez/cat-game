using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WhiskerHaven.Core;

namespace WhiskerHaven.UI
{
    [System.Serializable]
    public class TutorialStep
    {
        public string title;
        [TextArea(2, 5)] public string body;
        public Sprite illustration;
        public string highlightPanelId;  // optional: which UI element to arrow-point at
    }

    public class TutorialView : MonoBehaviour
    {
        [Header("Steps")]
        public List<TutorialStep> steps = new();

        [Header("UI Elements")]
        [SerializeField] private Image            illustration;
        [SerializeField] private TextMeshProUGUI  titleText;
        [SerializeField] private TextMeshProUGUI  bodyText;
        [SerializeField] private TextMeshProUGUI  stepCounterText;
        [SerializeField] private Button           nextButton;
        [SerializeField] private Button           prevButton;
        [SerializeField] private Button           skipButton;
        [SerializeField] private Slider           progressDots;
        [SerializeField] private CanvasGroup      backdrop;

        private int _currentStep = 0;

        private void Awake()
        {
            nextButton?.onClick.AddListener(NextStep);
            prevButton?.onClick.AddListener(PrevStep);
            skipButton?.onClick.AddListener(CompleteTutorial);

            // Default steps if none assigned in Inspector
            if (steps == null || steps.Count == 0)
                BuildDefaultSteps();
        }

        public void StartTutorial()
        {
            _currentStep = GameManager.Instance?.Save?.tutorialStep ?? 0;
            gameObject.SetActive(true);
            ShowStep(_currentStep);
        }

        private void ShowStep(int idx)
        {
            if (steps == null || steps.Count == 0) { CompleteTutorial(); return; }
            idx = Mathf.Clamp(idx, 0, steps.Count - 1);
            var step = steps[idx];

            if (illustration)    illustration.sprite  = step.illustration;
            if (titleText)       titleText.text        = step.title;
            if (bodyText)        bodyText.text         = step.body;
            if (stepCounterText) stepCounterText.text  = $"{idx + 1} / {steps.Count}";
            if (progressDots)    progressDots.value    = (float)idx / (steps.Count - 1);

            if (prevButton) prevButton.interactable = idx > 0;
            if (nextButton) nextButton.GetComponentInChildren<TextMeshProUGUI>().text =
                idx == steps.Count - 1 ? "Start!" : "Next";

            EventBus.Publish(new OnTutorialStep { Step = idx });

            // Save progress
            if (GameManager.Instance?.Save != null)
                GameManager.Instance.Save.tutorialStep = idx;
        }

        private void NextStep()
        {
            AudioManager.Instance?.PlaySFX("ui_click");
            if (_currentStep >= steps.Count - 1) { CompleteTutorial(); return; }
            _currentStep++;
            ShowStep(_currentStep);
        }

        private void PrevStep()
        {
            AudioManager.Instance?.PlaySFX("ui_click");
            if (_currentStep <= 0) return;
            _currentStep--;
            ShowStep(_currentStep);
        }

        private void CompleteTutorial()
        {
            if (GameManager.Instance?.Save != null)
                GameManager.Instance.Save.tutorialComplete = true;

            SaveSystem.Instance?.Save();
            AudioManager.Instance?.PlaySFX("tutorial_complete");
            gameObject.SetActive(false);
        }

        private void BuildDefaultSteps()
        {
            steps = new List<TutorialStep>
            {
                new() {
                    title = "Welcome to Whisker Haven!",
                    body  = "You've inherited a forgotten cat sanctuary. It's a bit rough around the edges, but with your help — and some very enthusiastic cats — it'll become something magical."
                },
                new() {
                    title = "Collect Snuggles 🐾",
                    body  = "Snuggles are the lifeblood of your haven. Cats produce them automatically over time. The happier your cats, the more Snuggles you earn!"
                },
                new() {
                    title = "Rescue Cats 🐱",
                    body  = "Visit the Cat Collection tab to rescue cats. Each cat has a unique personality and production rate. Start with common cats and work your way up to rare breeds!"
                },
                new() {
                    title = "Build Habitats 🏠",
                    body  = "Habitats are where your cats live and work. Assign cats to habitats to boost their production. Upgrade habitats to unlock more cat slots and multipliers."
                },
                new() {
                    title = "Purr Power ✨",
                    body  = "The Purr Power bar shows your global happiness multiplier. Keep your cats happy and your production can reach 5× the base rate!"
                },
                new() {
                    title = "Come Back Often 🌙",
                    body  = "Your cats keep purring even when you're away! Return daily for offline earnings, daily missions, and login streak bonuses.\n\nNow go build the ultimate cat sanctuary!"
                },
            };
        }
    }
}
