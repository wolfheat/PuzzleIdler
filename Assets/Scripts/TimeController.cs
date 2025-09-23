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
        int minute = 0;
        while (true) {

            // Take care of minuteTick
            minute++;
            if (minute == 60) {
                Stats.RatingDropEachMinute();
                minute = 0;
            }

            // Update the text
            Stats.Tick();

            // Update visuals
            CoinsVisuals.Instance.UpdateTexts();

            yield return wait;
        }
    }
}
