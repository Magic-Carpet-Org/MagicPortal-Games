using UnityEngine;
using System.Collections;
using UnityEditor;
namespace TechJuego.GetOut.Utils
{
	public class TakeScreenShot : MonoBehaviour
	{
#if UNITY_EDITOR
		[MenuItem("Tech Juego/Take Screen Shot")]
		static void TakeScreen()
		{
			string resolution = string.Empty;
			resolution = "" + Screen.width + "X" + Screen.height;
			ScreenCapture.CaptureScreenshot("ScreenShot-" + resolution + "-" + PlayerPrefs.GetInt("number", 0) + ".png");
			PlayerPrefs.SetInt("number", PlayerPrefs.GetInt("number", 0) + 1);
		}
#endif
	}
}