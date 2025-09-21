using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [Header("UI Components")]
    public Text numberText;
    public Image background;
    
    [Header("Animation Settings")]
    public float animationDuration = 0.1f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
    
    public int Number { get; private set; }
    private bool isAnimating = false;
    
    public void SetNumber(int number)
    {
        int previousNumber = Number;
        Number = number;
        
        UpdateDisplay();
        
        // 如果數字改變且不是初始設定，播放動畫
        if (previousNumber != number && previousNumber != 0 && number != 0)
        {
            PlayScaleAnimation();
        }
    }
    
    void UpdateDisplay()
    {
        if (Number == 0)
        {
            numberText.text = "";
            background.color = GetEmptyColor();
        }
        else
        {
            numberText.text = Number.ToString();
            background.color = GetColor(Number);
            
            // 根據數字大小調整字體顏色
            numberText.color = GetTextColor(Number);
        }
    }
    
    Color GetEmptyColor()
    {
        return new Color(0.8f, 0.8f, 0.8f, 0.5f); // 淺灰色，半透明
    }
    
    Color GetColor(int number)
    {
        switch (number)
        {
            case 2:     return new Color(0.93f, 0.89f, 0.85f);    // 淺米色
            case 4:     return new Color(0.93f, 0.88f, 0.78f);    // 淺黃色
            case 8:     return new Color(0.95f, 0.69f, 0.47f);    // 橘色
            case 16:    return new Color(0.96f, 0.58f, 0.39f);    // 深橘色
            case 32:    return new Color(0.96f, 0.49f, 0.37f);    // 紅橘色
            case 64:    return new Color(0.96f, 0.37f, 0.23f);    // 紅色
            case 128:   return new Color(0.93f, 0.81f, 0.45f);    // 金黃色
            case 256:   return new Color(0.93f, 0.80f, 0.38f);    // 深金黃色
            case 512:   return new Color(0.93f, 0.78f, 0.31f);    // 更深金黃色
            case 1024:  return new Color(0.93f, 0.77f, 0.25f);    // 深黃色
            case 2048:  return new Color(0.93f, 0.76f, 0.18f);    // 勝利金色
            case 4096:  return new Color(0.24f, 0.47f, 0.85f);    // 藍色
            case 8192:  return new Color(0.20f, 0.33f, 0.73f);    // 深藍色
            default:    return new Color(0.15f, 0.15f, 0.15f);    // 深灰色（超高數字）
        }
    }
    
    Color GetTextColor(int number)
    {
        // 小數字用深色文字，大數字用白色文字
        if (number <= 4)
            return new Color(0.47f, 0.43f, 0.40f); // 深灰色
        else
            return Color.white;
    }
    
    void PlayScaleAnimation()
    {
        if (isAnimating) return;
        
        StartCoroutine(ScaleAnimation());
    }
    
    System.Collections.IEnumerator ScaleAnimation()
    {
        isAnimating = true;
        Vector3 originalScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            float scaleMultiplier = scaleCurve.Evaluate(progress);
            transform.localScale = originalScale * scaleMultiplier;
            yield return null;
        }
        
        transform.localScale = originalScale;
        isAnimating = false;
    }
    
    // 設定新生成格子的動畫
    public void PlaySpawnAnimation()
    {
        StartCoroutine(SpawnAnimation());
    }
    
    System.Collections.IEnumerator SpawnAnimation()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / animationDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, progress);
            yield return null;
        }
        
        transform.localScale = originalScale;
    }
    
    // 獲取格子是否為空
    public bool IsEmpty()
    {
        return Number == 0;
    }
    
    // 重置格子
    public void Reset()
    {
        SetNumber(0);
        transform.localScale = Vector3.one;
        isAnimating = false;
    }
}