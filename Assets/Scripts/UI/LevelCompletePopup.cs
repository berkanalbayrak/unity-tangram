using System;
using _3rdParty.git_amend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LevelCompletePopup : MonoBehaviour
    {
        [SerializeField] private Image popupImage;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI text;
        
        private EventBinding<LevelCompletedEvent> _levelCompletedEventBinding;

        private void Awake()
        {
            popupImage.enabled = false;
            button.interactable = false;
            text.enabled = false;
        }

        private void OnEnable()
        {
            _levelCompletedEventBinding = new EventBinding<LevelCompletedEvent>(OnLevelCompleted);
            EventBus<LevelCompletedEvent>.Register(_levelCompletedEventBinding);
            button.onClick.AddListener(OnButtonClicked);
        }
        
        private void OnDisable()
        {
            EventBus<LevelCompletedEvent>.Deregister(_levelCompletedEventBinding);
            button.onClick.RemoveListener(OnButtonClicked);
        }
        
        private void OnLevelCompleted(LevelCompletedEvent obj)
        {
            popupImage.enabled = true;
            button.interactable = true;
            text.enabled = true;
        }
        
        private void OnButtonClicked()
        {
            popupImage.enabled = false;
            button.interactable = false;
            text.enabled = false;
            EventBus<NextLevelEvent>.Raise(new NextLevelEvent());
        }
    }
}