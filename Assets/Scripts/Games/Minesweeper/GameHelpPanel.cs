using System;
using TMPro;
using UnityEngine;

public class GameHelpPanel : MonoBehaviour
{

    [SerializeField] private GameObject[] textHolders;

    [SerializeField] private Transform rewardTextPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateInfo();    
    }

    private void UpdateInfo()
    {
        // Get from stats

        int[] baseRewardValues = USerInfo.NormalDifficultys;
        float[] levels = USerInfo.MinesweeperRatingDropLevels;
        float[] drops = USerInfo.MinesweeperRatingDrops;

        // Clear the Holders and populate with new info
        foreach (var holder in textHolders) {
            foreach(Transform t in holder.transform)
                Destroy(t);
        }

        for (int i = 0; i < levels.Length; i++) {
            
            // Player Rating
            Transform textfield = Instantiate(rewardTextPrefab, textHolders[0].transform);            
            textfield.GetComponent<TextMeshProUGUI>().text = i== 0 ? "" :( levels[i-1].ToString()+"+");
            
            // Multiplier
            textfield = Instantiate(rewardTextPrefab, textHolders[1].transform);
            textfield.GetComponent<TextMeshProUGUI>().text = ""+drops[i];

            // Values
            for (int j = 0; j < baseRewardValues.Length; j++) {
                textfield = Instantiate(rewardTextPrefab, textHolders[j+2].transform);
                textfield.GetComponent<TextMeshProUGUI>().text = ""+(baseRewardValues[j] * drops[i]);
            }

        }
        
        /*
        // Multipliers
        for (int i = 0; i < multiplierTexts.Length; i++) {
            ratingTexts[i].text = levels[i].ToString();
            multiplierTexts[i].text = drops[i].ToString();
        } */       

    }

}
