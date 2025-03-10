using UnityEngine;

// 创建名为"Boundary"的空物体，添加以下脚本
public class ScreenBoundary : MonoBehaviour
{
    void Start()
    {
        AddCollider();
    }

    void AddCollider()
    {
        Camera mainCam = Camera.main;
        var colliderTop = gameObject.AddComponent<BoxCollider2D>();
        var colliderBottom = gameObject.AddComponent<BoxCollider2D>();
        var colliderLeft = gameObject.AddComponent<BoxCollider2D>();
        var colliderRight = gameObject.AddComponent<BoxCollider2D>();

        Vector2 screenSize = mainCam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        
        // 顶部
        colliderTop.size = new Vector2(screenSize.x * 2, 0.1f);
        colliderTop.offset = new Vector2(0, screenSize.y);
        
        // 底部
        colliderBottom.size = new Vector2(screenSize.x * 2, 0.1f);
        colliderBottom.offset = new Vector2(0, -screenSize.y);
        
        // 左侧
        colliderLeft.size = new Vector2(0.1f, screenSize.y * 2);
        colliderLeft.offset = new Vector2(-screenSize.x, 0);
        
        // 右侧
        colliderRight.size = new Vector2(0.1f, screenSize.y * 2);
        colliderRight.offset = new Vector2(screenSize.x, 0);
    }
}