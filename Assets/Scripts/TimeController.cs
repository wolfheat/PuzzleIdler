using System.Collections;
using UnityEngine;

public class TimeController : MonoBehaviour
{

    WaitForSeconds wait = new WaitForSeconds(1);

    void Start()
    {
        Debug.Log("TimeController - Start");
        StartCoroutine(Tick());
    }

    private IEnumerator Tick()
    {
        while (true) {
            //Debug.Log("Tick");
            
            // Update the text
            Stats.Tick();

            // Update visuals
            CoinsVisuals.Instance.UpdateTexts();

            yield return wait;
        }
    }
}
