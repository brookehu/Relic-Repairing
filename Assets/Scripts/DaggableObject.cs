using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject : MonoBehaviour
{
    private Vector3 offset;
    private float zCoord;
    private Rigidbody2D rb;
    private bool isDragging;
    private Vector3 lastPosition;

    // 添加标签属性
    public string tagIdentifier;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D component is not attached to the GameObject.");
        }
    }

    void OnMouseDown()
    {
        if (Camera.main != null)
        {
            zCoord = Camera.main.WorldToScreenPoint(transform.position).z;
            offset = transform.position - GetMouseWorldPos();
            rb.linearVelocity = Vector2.zero; // 停止当前运动
            rb.isKinematic = true;      // 关闭物理模拟
            lastPosition = transform.position;
            isDragging = true;
            Debug.Log("Mouse down detected.");
        }
        else
        {
            Debug.LogError("Main camera is not set in the scene.");
        }
    }

    void OnMouseDrag()
    {
        if (isDragging && Camera.main != null)
        {
            Vector3 newPos = GetMouseWorldPos() + offset;
            // 添加边界检查
            Vector4 screenBounds = GetScreenBounds();
            float objectWidth = GetComponent<Renderer>().bounds.extents.x;
            float objectHeight = GetComponent<Renderer>().bounds.extents.y;

            // 修正边界检查逻辑
            newPos.x = Mathf.Clamp(newPos.x, screenBounds.x + objectWidth, screenBounds.y - objectWidth);
            newPos.y = Mathf.Clamp(newPos.y, screenBounds.z + objectHeight, screenBounds.w - objectHeight);

            // 确保小球不会被卡在边界之外
            float clampedX = Mathf.Clamp(newPos.x, screenBounds.x + objectWidth, screenBounds.y - objectWidth);
            float clampedY = Mathf.Clamp(newPos.y, screenBounds.z + objectHeight, screenBounds.w - objectHeight);
            newPos = new Vector3(clampedX, clampedY, newPos.z);

            transform.position = newPos;
            
            // 实时计算速度（可选）
            // rb.velocity = (newPos - lastPosition) / Time.deltaTime;
            lastPosition = newPos;
            Debug.Log("Mouse drag detected.");
        }
        else
        {
            Debug.LogError("Main camera is not set in the scene or dragging is not active.");
        }
    }

    void OnMouseUp()
    {
        if (isDragging && Camera.main != null)
        {
            rb.isKinematic = false;
            
            // 计算释放时的速度
            Vector3 releasePosition = GetMouseWorldPos() + offset;
            Vector3 throwVector = (releasePosition - lastPosition) * 10f; // 调整系数控制速度
            rb.AddForce(throwVector, ForceMode2D.Impulse);
            isDragging = false;
            Debug.Log("Mouse up detected.");
        }
        else
        {
            Debug.LogError("Main camera is not set in the scene or dragging is not active.");
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        if (Camera.main != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = zCoord;
            return Camera.main.ScreenToWorldPoint(mousePos);
        }
        else
        {
            Debug.LogError("Main camera is not set in the scene.");
            return Vector3.zero; // 或者其他适当的默认值
        }
    }

    // 获取屏幕边界
    private Vector4 GetScreenBounds()
    {
        Camera mainCam = Camera.main;
        Vector3 screenBottomLeft = mainCam.ScreenToWorldPoint(new Vector3(0, 0, zCoord));
        Vector3 screenTopRight = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, zCoord));
        return new Vector4(screenBottomLeft.x, screenTopRight.x, screenBottomLeft.y, screenTopRight.y);
    }

    // 添加碰撞检测方法
    private void OnCollisionEnter2D(Collision2D collision)
    {
        DraggableObject otherObject = collision.gameObject.GetComponent<DraggableObject>();
        if (otherObject != null && otherObject.tagIdentifier == tagIdentifier)
        {
            otherObject.gameObject.SetActive(false);
            Debug.Log("Collision！！");
            // 标签匹配，触发事件
            OnTagMatch(otherObject);
        }
    }

    // 定义标签匹配事件处理方法
    private void OnTagMatch(DraggableObject otherObject)
    {
        Debug.Log("Tag match detected between " + gameObject.name + " and " + otherObject.gameObject.name);
        // 在这里添加你需要的事件处理逻辑
    }
}