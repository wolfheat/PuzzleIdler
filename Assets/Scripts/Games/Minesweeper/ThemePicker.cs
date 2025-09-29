using System.Linq;
using UnityEngine;

public class ThemePicker : MonoBehaviour
{
    public NumberThemeSO currentMinesweeper;
    [SerializeField] NumberThemeSO[] themes;

    public static ThemePicker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public string[] GetThemes()
    {
        return themes.Select(x => x.name).ToArray();
        //return themes.Select(x => x.ThemeName).ToArray();
    }
    public void SetTheme(int index)
    {
        if (index >= themes.Length) return;

        currentMinesweeper = themes[index];
    }
}
