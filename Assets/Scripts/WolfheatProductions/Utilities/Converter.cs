using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WolfheatProductions
{
    public class Converter
    {
        
        /// <summary>
        /// Removes all non characters from a string.
        /// </summary>
        /// <param name="s">source string.</param>
        /// <returns>Resulting string.</returns>
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
        /// <summary>
        /// Scramble an array using the FisherYates method
        /// </summary>
        /// <param name="allPos">The array to scramble.</param>
        /// <returns>The resulting scrambled array.</returns>
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

        /// <summary>
        /// Converts the local mouse position inside a recttransform into a Vector2Int index as if its tiles on a grid of size Boxsize.
        /// </summary>
        /// <param name="eventData">Need the Click eventdata to read the mouse position.</param>
        /// <param name="rectTransform">Recttransform of the local displayobject.</param>
        /// <param name="BoxSize">The size of the grid or tiles.</param>
        /// <returns>Vector with the local index position.</returns>
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
    public static class TextColor
    {

        /// <summary>
        /// Color a string red.
        /// </summary>
        public static string ColorStringRed(string s) => $"<color=red>{s}</color>";



    }
}
