using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ItemFeatures : MonoBehaviour
{
    [Header("Drag Settings")]
    
    [Header("Collision Settings")]
    [Tooltip("可碰撞的层级")]
    public LayerMask collisionLayers;

    private Vector3 offset;
    private bool isDragging = false;
    private float originalZ;
    private Collider2D thisCollider;
    private List<Collider2D> currentCollisions = new List<Collider2D>();

    void Awake()
    {
        thisCollider = GetComponent<Collider2D>();
        originalZ = transform.position.z;
        
        if (!GetComponent<Rigidbody2D>())
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void OnMouseDown()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mouseWorldPos;
        offset.z = 0;
    
        isDragging = true;
        thisCollider.enabled = false;
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
        newPosition.z = originalZ;
        transform.position = newPosition;

        DetectCollisions();
    }

    void OnMouseUp()
    {
        isDragging = false;
        thisCollider.enabled = true;
        ClearAllCollisions();
    }

    void DetectCollisions()
    {
        // 使用碰撞体实际形状进行检测
        List<Collider2D> hits = new List<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(collisionLayers);
        filter.useTriggers = false;

        // 获取当前碰撞体接触的所有碰撞体
        Physics2D.OverlapCollider(thisCollider, filter, hits);

        // 处理新碰撞
        foreach (Collider2D hit in hits)
        {
            if (hit != thisCollider && !currentCollisions.Contains(hit))
            {
                currentCollisions.Add(hit);
                OnCollisionStart(hit.gameObject);
            }
        }

        // 处理结束的碰撞
        for (int i = currentCollisions.Count - 1; i >= 0; i--)
        {
            if (!hits.Contains(currentCollisions[i]))
            {
                OnCollisionEnd(currentCollisions[i].gameObject);
                currentCollisions.RemoveAt(i);
            }
        }
    }

    void ClearAllCollisions()
    {
        foreach (var col in currentCollisions)
        {
            OnCollisionEnd(col.gameObject);
        }
        currentCollisions.Clear();
    }

    void OnCollisionStart(GameObject other)
    {
        Debug.Log($"开始碰撞: {other.name}");
        other.GetComponent<SpriteRenderer>().color = Color.red;
        GetComponent<SpriteRenderer>().color = Color.red;
    }

    void OnCollisionEnd(GameObject other)
    {
        Debug.Log($"结束碰撞: {other.name}");
        other.GetComponent<SpriteRenderer>().color = Color.white;
        GetComponent<SpriteRenderer>().color = Color.white;
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