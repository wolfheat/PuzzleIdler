using System;
using System.Collections;
using UnityEngine;

public class SavingNotice : MonoBehaviour
{
    [SerializeField] private GameObject notice; 

    void Start()
    {

        PlayerGameData.SaveNeeded += Show;
    }

    private void Show()
    {
        StopAllCoroutines();
        StartCoroutine(ShowNotice());
    }

    private IEnumerator ShowNotice()
    {
        notice.SetActive(true);
        yield return new WaitForSeconds(2);
        notice.SetActive(false);
    }
}
