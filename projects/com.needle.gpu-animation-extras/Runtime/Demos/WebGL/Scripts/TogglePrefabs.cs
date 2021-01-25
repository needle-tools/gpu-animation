using System;
using UnityEngine;
using UnityEngine.UI;

namespace Demos.WebGL.Scripts
{
	public class TogglePrefabs : MonoBehaviour
	{
		public string Prefix;
		public GameObject[] Prefabs;
		public Text Text;
		public Button Button;
		private int _index;

		private void Start()
		{
			Toggle();
		}

		private void OnEnable()
		{
			Button.onClick.AddListener(this.OnClicked);
		}

		private void OnDisable()
		{
			Button.onClick.RemoveListener(this.OnClicked);
		}

		private void OnClicked()
		{
			Toggle();
		}

		private void Toggle()
		{
			if (Prefabs.Length <= 0) return;
			var activeIndex = _index++;
			activeIndex %= Prefabs.Length;
			for (var index = 0; index < Prefabs.Length; index++)
			{
				var pref = Prefabs[index];
				if (pref)
				{
					pref.SetActive(index == activeIndex);
					if (index == activeIndex) Text.text = Prefix + pref.name;
				}
			}
		}
	}
}