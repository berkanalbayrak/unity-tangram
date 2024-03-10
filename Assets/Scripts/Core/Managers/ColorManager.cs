using System.Collections.Generic;
using System.Linq;
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
    
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
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