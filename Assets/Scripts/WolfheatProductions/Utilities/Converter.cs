using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
        
        /// <summary>
        /// Makes sure there is a space between all words.
        /// </summary>
        /// <param name="s">source string.</param>
        /// <returns>Resulting string.</returns>
        public static string SpaceOutString(string s)
        {
            char[] chars = s.ToCharArray();
            char last = 'x';
            StringBuilder sb = new StringBuilder();
            foreach (char c in chars) {
                if (char.IsUpper(c) && char.IsLower(last))
                    sb.Append(' ');
                sb.Append(c);
            }
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
        public static Vector2Int GetMouseLocalPositionIndex(RectTransform rectTransform, int BoxSize, PointerEventData eventData = null)
        {
            Vector2 pos = GetMouseLocalPosition(rectTransform, eventData);

            int xPos = (int)pos.x / BoxSize;
            int yPos = (int)-pos.y / BoxSize;

            return new Vector2Int(xPos, yPos);
        }
        
        /// <summary>
        /// Converts the local mouse position inside a recttransform into a Vector2Int index as if its tiles on a grid of size Boxsize.
        /// </summary>
        /// <param name="eventData">Need the Click eventdata to read the mouse position.</param>
        /// <param name="rectTransform">Recttransform of the local displayobject.</param>
        /// <param name="BoxSize">The size of the grid or tiles.</param>
        /// <returns>Vector with the local index position.</returns>
        // get the position inside a rect - supply the rect and eventdata and boxSize
        public static Vector2 GetMouseLocalPosition(RectTransform rectTransform, PointerEventData eventData = null)
        {
            Vector2 pos = new();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData?.position ?? Mouse.current.position.ReadValue(),
                eventData?.pressEventCamera,
                out pos
            );
            return pos;
        }
        
        public static Vector2 GetMouseLocalPositionSpecificCamera(Camera cam,RectTransform rectTransform, PointerEventData eventData = null)
        {
            Vector2 pos = new();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                eventData?.position ?? Mouse.current.position.ReadValue(),
                cam,
                out pos
            );
            return pos;
        }

        /// <summary>
        /// Converts the Sent in position inside a recttransform into a Vector2Int index as if its tiles on a grid of size Boxsize.
        /// </summary>
        /// <param name="eventData">Need the Click eventdata to read the mouse position.</param>
        /// <param name="rectTransform">Recttransform of the local displayobject.</param>
        /// <param name="BoxSize">The size of the grid or tiles.</param>
        /// <returns>Vector with the local index position.</returns>
        // get the position inside a rect - supply the rect and eventdata and boxSize
        public static Vector2 GetMouseLocalPosition(RectTransform rectTransform, Vector2 localPos)
        {
            Vector2 pos = new();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                localPos,
                null,
                out pos
            );
            return pos;
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
