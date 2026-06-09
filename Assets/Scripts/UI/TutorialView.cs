using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhiskerHaven.Audio;
using WhiskerHaven.Core;
using WhiskerHaven.Utils;
using static WhiskerHaven.UI.UIFactory;

namespace WhiskerHaven.UI
{
    public class TutorialView : MonoBehaviour
    {
        private TextMeshProUGUI _titleText, _bodyText, _counterText;
        private Button          _nextBtn, _prevBtn, _skipBtn;
        private Slider          _progressDots;
        private TextMeshProUGUI _nextBtnLabel;
        private Image           _illustration;

        private int _step;
        private List<(string title, string body, Color accent)> _steps;

        private void Awake()
        {
            BuildSteps();
            BuildUI();
        }

        private void BuildSteps()
        {
            _steps = new List<(string, string, Color)>
            {
                ("Welcome to Whisker Haven! 🐱",
                 "You've inherited an abandoned cat sanctuary.\n\nWith your love and a little Snuggle magic, it'll become the coziest place for cats in the world.",
                 AMBER),

                ("Earn Snuggles 🐾",
                 "Snuggles are your main currency.\n\nCats produce Snuggles automatically over time. The happier your cats, the more they produce — even while you're away!",
                 GOLD),

                ("Rescue Cats 🐱",
                 "Visit the Cats tab to see all 15 rescuable cats.\n\nEach cat has a unique personality, rarity, and production rate. Start with common cats and work your way up to the legendary Astrid!",
                 UIFactory.UNCOMMON),

                ("Manage Habitats 🏠",
                 "Cats produce more when assigned to a Habitat.\n\nEach habitat has a preferred cat personality — matching them gives a 2× production bonus. Upgrade habitats to add more slots and multipliers.",
                 SUCCESS),

                ("Purr Power ✨",
                 "The Purr Power bar at the top shows your global multiplier.\n\nA happy sanctuary can reach 5× production! Keep cats in their preferred habitats and assign volunteers to maximize it.",
                 PURR_CLR),

                ("Come Back Daily 🌙",
                 "Your cats keep purring even while you're away!\n\nReturn every day for daily missions, login streak bonuses, and to collect your offline earnings.\n\nNow go build the ultimate Whisker Haven!",
                 CREAM),
            };
        }

        private void BuildUI()
        {
            // Dark backdrop
            var backdrop = Panel(transform, "Backdrop", new Color(0, 0, 0, 0.8f));
            Stretch(backdrop);

            // Card
            var card = Panel(transform, "Card", CREAM);
            Center(card, new Vector2(560, 440));
            VLayout(card, 12, new RectOffset(36, 36, 28, 24));

            // Header accent bar
            var accentBar = Panel(card.transform, "AccentBar", AMBER);
            LE(accentBar, prefH: 6);

            // Illustration area (placeholder colored panel)
            var illus = Panel(card.transform, "Illustration", BG_DARK);
            LE(illus, prefH: 120, minH: 120);
            _illustration = illus.GetComponent<Image>();
            Text(illus.transform, "Emoji", "🐱", 64, CREAM, TextAlignmentOptions.Center).raycastTarget = false;

            // Title
            _titleText = Text(card.transform, "Title", "", 22, TEXT_D, TextAlignmentOptions.Center, FontStyles.Bold);
            _titleText.enableWordWrapping = true;
            LE(_titleText.gameObject, prefH: 56);

            // Body
            _bodyText = Text(card.transform, "Body", "", 14, TEXT_D, TextAlignmentOptions.Center);
            _bodyText.enableWordWrapping = true;
            LE(_bodyText.gameObject, prefH: 100);

            // Progress
            _progressDots = SliderH(card.transform, "Progress", AMBER, 12);
            LE(_progressDots.gameObject, prefH: 12);
            _progressDots.interactable = false;

            // Counter + navigation
            var nav = Group(card.transform, "Nav");
            LE(nav, prefH: 44);
            HLayout(nav, 12, new RectOffset(0, 0, 0, 0));

            _prevBtn = Btn(nav.transform, "Prev", "← Prev", BG_LIGHT, TEXT_L, 13);
            LE(_prevBtn.gameObject, prefW: 100, prefH: 40);
            _prevBtn.onClick.AddListener(Prev);

            var center = Group(nav.transform, "Center"); LE(center, flexW: 1);
            _counterText = Text(center.transform, "Counter", "1 / 6", 12, BG_LIGHT, TextAlignmentOptions.Center);

            _nextBtn = Btn(nav.transform, "Next", "Next →", AMBER, TEXT_L, 13);
            LE(_nextBtn.gameObject, prefW: 110, prefH: 40);
            var nextLabel = _nextBtn.GetComponentInChildren<TextMeshProUGUI>();
            _nextBtnLabel = nextLabel;
            _nextBtn.onClick.AddListener(Next);

            // Skip
            _skipBtn = Btn(card.transform, "Skip", "Skip tutorial", new Color(0, 0, 0, 0), new Color(0.5f, 0.5f, 0.5f), 11);
            LE(_skipBtn.gameObject, prefH: 24);
            _skipBtn.onClick.AddListener(Complete);
        }

        public void StartTutorial()
        {
            _step = GameManager.Instance?.Save?.tutorialStep ?? 0;
            ShowStep(_step);
        }

        private void ShowStep(int idx)
        {
            if (_steps == null || _steps.Count == 0) { Complete(); return; }
            idx = Mathf.Clamp(idx, 0, _steps.Count - 1);
            var (title, body, accent) = _steps[idx];

            if (_titleText)    _titleText.text   = title;
            if (_bodyText)     _bodyText.text    = body;
            if (_counterText)  _counterText.text = $"{idx + 1} / {_steps.Count}";
            if (_progressDots) _progressDots.value = _steps.Count > 1 ? (float)idx / (_steps.Count - 1) : 1f;
            if (_illustration) _illustration.color = accent.WithAlpha(0.15f);

            if (_prevBtn)     _prevBtn.interactable = idx > 0;
            if (_nextBtnLabel) _nextBtnLabel.text    = idx == _steps.Count - 1 ? "Start!" : "Next →";

            EventBus.Publish(new OnTutorialStep { Step = idx });
            if (GameManager.Instance?.Save != null) GameManager.Instance.Save.tutorialStep = idx;
        }

        private void Next()
        {
            AudioManager.Instance?.PlaySFX("ui_click");
            if (_step >= (_steps?.Count ?? 1) - 1) Complete();
            else { _step++; ShowStep(_step); }
        }

        private void Prev()
        {
            AudioManager.Instance?.PlaySFX("ui_click");
            if (_step > 0) { _step--; ShowStep(_step); }
        }

        private void Complete()
        {
            if (GameManager.Instance?.Save != null) GameManager.Instance.Save.tutorialComplete = true;
            SaveSystem.Instance?.Save();
            AudioManager.Instance?.PlaySFX("tutorial_complete");
            gameObject.SetActive(false);
        }
    }
}
