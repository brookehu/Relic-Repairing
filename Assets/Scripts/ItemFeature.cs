using UnityEngine;

public class ItemFeature : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Rigidbody rb;

    void Start()
    {
        // 获取物体的 Rigidbody 组件
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }

    void OnMouseDown()
    {
        // 计算鼠标点击位置与物体中心的偏移量
        offset = transform.position - GetMouseWorldPosition();
        isDragging = true;

        // 禁用重力以便拖拽
        rb.useGravity = false;
        rb.isKinematic = true;
    }

    void OnMouseUp()
    {
        // 恢复重力并启用物理效果
        isDragging = false;
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    void Update()
    {
        if (isDragging)
        {
            // 更新物体位置以跟随鼠标
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 碰撞检测逻辑
        Debug.Log($"Collision detected with {collision.gameObject.name}");
        // 可根据需求扩展碰撞处理逻辑
    }

    private Vector3 GetMouseWorldPosition()
    {
        // 将屏幕坐标转换为世界坐标
        Vector3 mouseScreenPosition = Input.mousePosition;
        mouseScreenPosition.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPosition);
    }
}