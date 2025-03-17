using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ItemFeatures : MonoBehaviour
{
    [Header("Collision Settings")]
    public LayerMask collisionLayers;

    [Header("Item Settings")]
    [Tooltip("如果为true，物体无法被拖拽")]
    public bool IfFixed = false;
    public int type;  // 0-普通物品 1-消耗者

    private Vector3 offset;
    private bool isDragging;
    private float originalZ;
    private Collider2D thisCollider;
    private Rigidbody2D rb;
    private HashSet<Collider2D> currentCollisions = new HashSet<Collider2D>();

    void Awake()
    {
        thisCollider = GetComponent<Collider2D>();
        originalZ = transform.position.z;
        
        rb = GetComponent<Rigidbody2D>() ?? gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnMouseDown()
    {
        if (IfFixed || !thisCollider.enabled) return;

        // 修正z轴坐标为摄像机平面坐标
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x, 
            Input.mousePosition.y,
            Camera.main.nearClipPlane)); // 使用近裁剪面坐标
        
        // 保持物体原有z轴
        mousePos.z = transform.position.z;
        
        offset = transform.position - mousePos;
        isDragging = true;

        // 临时修改刚体属性
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void OnMouseDrag()
    {
        if (!isDragging || IfFixed || !thisCollider.enabled) return;

        // 获取带正确z轴的鼠标坐标
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x,
            Input.mousePosition.y,
            Camera.main.WorldToScreenPoint(transform.position).z));
        
        // 应用偏移计算
        Vector3 newPos = mousePos + offset;
        newPos.z = originalZ;

        // 使用Transform直接移动
        transform.position = newPos;
        
        // 强制物理系统更新
        Physics2D.SyncTransforms();
        DetectCollisions();
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;
        TryConsumeItems();
        ClearAllCollisions();
    }

    void DetectCollisions()
    {
        var filter = new ContactFilter2D
        {
            layerMask = collisionLayers,
            useTriggers = false,
            useLayerMask = true
        };

        List<Collider2D> hits = new List<Collider2D>();
        Physics2D.OverlapCollider(thisCollider, filter, hits);

        // 调试信息优化
        if (hits.Count > 0)
        {
            Debug.Log($"{name} 检测到碰撞: {string.Join(", ", hits.ConvertAll(h => h.name))}");
        }

        ProcessCollisions(new HashSet<Collider2D>(hits));
    }

    void ProcessCollisions(HashSet<Collider2D> newHits)
    {
        newHits.RemoveWhere(col => col == null || col == thisCollider);
        newHits.Remove(thisCollider);

        // 添加新碰撞
        foreach (var hit in newHits)
        {
            if (currentCollisions.Add(hit))
            {
                OnCollisionStart(hit.gameObject);
            }
        }

        // 移除结束的碰撞
        List<Collider2D> expired = new List<Collider2D>();
        foreach (var col in currentCollisions)
        {
            if (!newHits.Contains(col))
            {
                expired.Add(col);
            }
        }
        
        foreach (var col in expired)
        {
            currentCollisions.Remove(col);
            OnCollisionEnd(col?.gameObject); // 安全空值处理
        }
    }

    void TryConsumeItems()
{
        // 创建副本避免枚举时修改集合
        var processingList = new List<Collider2D>(currentCollisions);
        
        foreach (var col in processingList)
        {
            if (col == null || !currentCollisions.Contains(col)) continue;

            var otherItem = col.GetComponent<ItemFeatures>();
            if (otherItem != null && otherItem.enabled)
            {
                ConsumeItem(otherItem);
            }
        }
    }

    void ConsumeItem(ItemFeatures other)
    {
        if (this.type == 1 && other.type == 0)
        {
            Debug.Log($"消耗发生: {name} -> {other.name}");
            
            if (UIManager.instance != null)
            {
                UIManager.instance.ProgressUp();
            }
            
            // 先记录需要移除的碰撞体
            var toRemove = new List<Collider2D>();
            foreach (var col in currentCollisions)
            {
                if (col != null && col.gameObject == other.gameObject)
                {
                    toRemove.Add(col);
                }
            }
            
            // 禁用前先移除碰撞记录
            foreach (var col in toRemove)
            {
                currentCollisions.Remove(col);
                OnCollisionEnd(col?.gameObject);
            }
            
            gameObject.SetActive(false);
        }
    }

    void OnCollisionStart(GameObject other)
    {
        if (other == null) return;

        // 添加组件存在性检查
        var renderer = other.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = Color.red;
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    void OnCollisionEnd(GameObject other)
    {
        if (other == null) return;

        var renderer = other.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = Color.white;
            GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    void ClearAllCollisions()
    {
        foreach (var col in currentCollisions)
        {
            OnCollisionEnd(col?.gameObject);
        }
        currentCollisions.Clear();
    }

    void OnDisable()
    {
        // 禁用时重置状态
        isDragging = false;
        ClearAllCollisions();
    }

    void OnDrawGizmosSelected()
    {
        // 绘制碰撞体形状
        if (thisCollider != null)
        {
            Gizmos.color = Color.yellow;
            if (thisCollider is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(box.bounds.center, box.bounds.size);
            }
            else if (thisCollider is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(circle.bounds.center, circle.radius);
            }
            else if (thisCollider is PolygonCollider2D poly)
            {
                for (int i = 0; i < poly.pathCount; i++)
                {
                    Vector2[] path = poly.GetPath(i);
                    for (int j = 0; j < path.Length; j++)
                    {
                        Gizmos.DrawLine(
                            poly.transform.TransformPoint(path[j]),
                            poly.transform.TransformPoint(path[(j+1)%path.Length])
                        );
                    }
                }
            }
        }
    }
}