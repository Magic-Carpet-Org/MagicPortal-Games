using UnityEngine;
using UnityEngine.UI; // Required for the UI Image component

namespace TechJuego.GetOut.Visuals
{
	public class RandomBackground : MonoBehaviour
	{
		[Header("Background Settings")]
		[Tooltip("Drag your UI Image component here")]
		public Image backgroundImage; 

		[Tooltip("Add all your different background sprites to this array")]
		public Sprite[] possibleBackgrounds;

		private void Start()
		{
			SetRandomBackground();
		}

		private void SetRandomBackground()
		{
			// Failsafe: Make sure we actually have sprites and an image component assigned
			if (backgroundImage == null)
			{
				Debug.LogWarning("[RandomBackground] No Image component assigned!");
				return;
			}

			if (possibleBackgrounds == null || possibleBackgrounds.Length == 0)
			{
				Debug.LogWarning("[RandomBackground] No background sprites assigned to the array!");
				return;
			}

			// Pick a random number between 0 and the total number of sprites
			int randomIndex = Random.Range(0, possibleBackgrounds.Length);

			// Apply the randomly chosen sprite to the UI Image
			backgroundImage.sprite = possibleBackgrounds[randomIndex];
		}
	}
}