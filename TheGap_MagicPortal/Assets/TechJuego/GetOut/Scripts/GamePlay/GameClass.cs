using System.Collections.Generic;

namespace TechJuego.GetOut
{
	[System.Serializable]
	public class LevelData
	{
		public int maxCols;
		public int maxRows;
		public List<int> fixedNumbers = new List<int>();
		public List<string> Level = new List<string>();
	}
}
