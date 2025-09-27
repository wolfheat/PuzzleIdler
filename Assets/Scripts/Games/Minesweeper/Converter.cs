using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WolfheatProductions
{
    public class Converter
    {
        // Takes an int array and scrambles it using FisherYates
        public static string RemoveAllNonCharacters(string s)
        {
            char[] chars = s.ToCharArray();
            StringBuilder sb = new StringBuilder();
            foreach (char c in chars)
                if (char.IsLetter(c))
                    sb.Append(c);
            return sb.ToString();
        }
        // Takes an int array and scrambles it using FisherYates
        public static int[] FisherYatesScramble(int[] allPos)
        {
            int n = allPos.Length;
            for (int i = 0; i < n; i++)
            {
                int temp = allPos[i];
                // Rendom pos
                int random = Random.Range(0, n);
                allPos[i] = allPos[random];
                allPos[random] = temp;

            }
            return allPos;
        }


        // get the position inside a rect - supply the rect and eventdata and boxSize
        public static Vector2Int GetMouseLocalPositionIndex(PointerEventData eventData,RectTransform rectTransform, int BoxSize)
        {
            Vector2 pos = new();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out pos
            );

            int xPos = (int)pos.x / BoxSize;
            int yPos = (int)-pos.y / BoxSize;

            return new Vector2Int(xPos, yPos);
        }



    }
}
