using System;
using System.Collections.Generic;
using System.Linq;
using _3rdParty.git_amend;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Core.Managers
{
    public class ColorManager : MonoBehaviour
    {
        public static ColorManager Instance { get; private set; }

        [SerializeField, ListDrawerSettings(ShowIndexLabels = true)]
        private List<Color> availableColors = new List<Color>();
    
        private HashSet<Color> _usedColors = new HashSet<Color>();
        
        private EventBinding<LevelCompletedEvent> _levelCompletedEventBinding;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            _levelCompletedEventBinding = new EventBinding<LevelCompletedEvent>(OnLevelCompleted);
            EventBus<LevelCompletedEvent>.Register(_levelCompletedEventBinding);
        }

        private void OnLevelCompleted(LevelCompletedEvent @event)
        {
            _usedColors.Clear();
        }

        private void OnDisable()
        {
            EventBus<LevelCompletedEvent>.Deregister(_levelCompletedEventBinding);
        }

        public Color GetAvailableColor()
        {
            foreach (var color in availableColors.Where(color => _usedColors.Add(color)))
            {
                return color;
            }

            Debug.LogWarning("All predefined colors are in use.");
            return Color.black;
        }

        public void ReleaseColor(Color color)
        {
            _usedColors.Remove(color);
        }
    }
}