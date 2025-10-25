using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum AnimationChangeType {Stop,Merge,Delete};

public class AnimationInfo
{
    public Vector3 animationStart;
    public Vector3 animationEnd;
    public AnimationChangeType type;
}
public class MergeTile : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private MergeTileData data;

    private const float AnimationSpeed = 2000f;

    private AnimationInfo animationInfo = null;
    private Coroutine animationCoroutine = null;
    public bool MergedThisFrame { get; set; } = false;

    public int Value { get; private set; }
    public int Level { get; private set; }

    private void Update()
    {
        // Starts the coroutine the ifrt frame where the data is set and no routine is running
        if (animationInfo != null && animationCoroutine == null)
            animationCoroutine = StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        yield return null;
        float distance = Vector2.Distance(animationInfo.animationEnd, animationInfo.animationStart);
        float animationTime = distance / AnimationSpeed;

        float timer = 0;

        // Animate the piece from the start position to the end position at a linear velocity
        while (timer < animationTime) {
            float t = timer / animationTime;
            transform.localPosition = Vector3.Lerp(animationInfo.animationStart,animationInfo.animationEnd, t);
            yield return null;
            timer += Time.deltaTime;
        }

        // Force piece into position
        transform.localPosition = animationInfo.animationEnd;

        // Do any Change here
        switch (animationInfo.type) {
            case AnimationChangeType.Stop:
                Stop();
                break;
            case AnimationChangeType.Merge:
                Merge();
                break;
            default:
            case AnimationChangeType.Delete:
                Delete();
                break;
        }

        // Unset the animation data
        animationInfo = null;
        animationCoroutine = null;
        MergedThisFrame = false;
    }

    public void SetLevel(int level)
    {
        Level = level;
        Value = 1 << level;
        // Get divider
        UpdateTile();
    }
    
    public void UpdateTile()
    {
        image.color = data.BlockColors[Math.Min(data.BlockColors.Length - 1, Level - 1)];
        numberText.text = Value.ToString();
        numberText.color = data.BlockColors[Level<=2?0:1];
    }

    internal int ChangeAnimationToMerge()
    {
        // Allow changing the type if it exists
        if (animationInfo == null) return Level;

        MergedThisFrame = true;
        animationInfo.type = AnimationChangeType.Merge;
        return Level + 1;
    }
    internal void SetAnimation(Vector3 localPosition, AnimationChangeType type)
    {
        animationInfo = new AnimationInfo() {animationStart = transform.localPosition, animationEnd = localPosition, type = type};        
    }

    private void Delete()
    {
        Debug.Log("Delete Tile at ");
        Destroy(gameObject);
    }

    private void Merge()
    {
        Debug.Log("Merge Tile at ");
        SetLevel(Level + 1);
    }

    private void Stop()
    {
        Debug.Log("Stop Tile here ");
    }
}