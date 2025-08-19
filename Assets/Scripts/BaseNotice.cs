using System.Collections;
using UnityEngine;

public class BaseNotice : MonoBehaviour
{
    protected float noticeTime = 1f;
    virtual protected void Start()
    {
        StartCoroutine(SaveNoticeCO());
    }
    private IEnumerator SaveNoticeCO()
    {
        yield return new WaitForSeconds(noticeTime);
        Destroy(gameObject);
    }
}
