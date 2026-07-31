using Unity.VisualScripting;
using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    Rigidbody2D rigid;
    SpriteRenderer sprite;
    Animator anim;
    CapsuleCollider2D cap;
    public int nextMove;
    void Think()
    {
        nextMove = Random.Range(-1, 2);
        anim.SetInteger("WalkSpeed", nextMove);
        //재귀
        Invoke("Think", Random.Range(2, 5));
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        cap = GetComponent<CapsuleCollider2D>();
        Think();
    }

    void Update()
    {      
        if (nextMove == 1)
        {
            sprite.flipX = true;
        }
        else if (nextMove == -1)
        {
            sprite.flipX = false;
        }
    }

    void FixedUpdate()
    {
        rigid.linearVelocity = new Vector2(nextMove, rigid.linearVelocity.y);
        //지형 체크
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.3f, rigid.position.y);
        Debug.DrawRay(frontVec, new Vector3(0, -1f, 0), new Color(0,1,0));
        RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));

        if (rayHit.collider == null)
        {
            nextMove = -nextMove;
            CancelInvoke();
            Invoke("Think", Random.Range(2,5));
        }
    }

    public void OnDamaged()
    {
        sprite.color = new Color(1,1,1,0.4f);
        sprite.flipY = true;
        cap.enabled = false;
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        nextMove = 3;
        CancelInvoke();
        Invoke("DeActive", 5);
    }

    void DeActive()
    {
        gameObject.SetActive(false);
    }
}
