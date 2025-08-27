using TMPro;
using UnityEngine;

public class ResearchTierItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tier;

    public void SetTierInfo(int currentTier) => tier.text = "Tier " + (currentTier+1);

}
