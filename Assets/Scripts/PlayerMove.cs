using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public int health = 5;
    public float maxSpeed = 3.5f;
    public float jumpPower = 2;
    float h;
    float j;
    int playerDamagedLayer;
    int enemyLayer;
    public Rigidbody2D rigid;
    public SpriteRenderer sprite;
    public Animator animator;
    public GameManager gameManager;
    public AudioSource audioSource;

    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip damagedSound;
    public AudioClip itemSound;
    public AudioClip dieSound;
    public AudioClip clearSound;
    
    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        playerDamagedLayer = LayerMask.NameToLayer("PlayerDamaged");
        enemyLayer = LayerMask.NameToLayer("Obstacle");
    }

    void Update()
    {
        //움직일 때 로직. d키 입력시 h에 1반환, a키 입력시 h에 -1  반환. 다른경우 0 반환
        h = Input.GetAxisRaw("Horizontal");

        //멈출때 로직. a와 d키 누른 상태에서 땔 시 똑같은 방향과 0.5 크기 가진 벡터 반환
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.normalized.x * 0.5f, rigid.linearVelocity.y);
        }

        //점프 로직. space키 입력시 점프함
        if (Input.GetButtonDown("Jump") && j == 1)
        {
            PlaySound("jump");
            animator.SetBool("isJumping", true);
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
        }
        else if (Input.GetButtonUp("Jump") && rigid.linearVelocity.y >= -0.1f)
        {
            rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, rigid.linearVelocity.y * 0.1f);
        }

        //걷는 애니메이션
        if (rigid.linearVelocity.x != 0)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        //방향 전환
        if (h <= -1)
        {
            sprite.flipX = true;
        }
        else if (h >= 1)
        {
            sprite.flipX = false;
        }

        if (j == 0)
        {
            animator.SetBool("isFalling", true);
        }
        else
        {
            animator.SetBool("isFalling", false);
        }
    }

    void FixedUpdate()
    {
        //h값을 벡터에 곱해서 함수 실행하는 횟수마다 물체에 힘을 가함
        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
        
        // 최대 속력
        if(rigid.linearVelocity.x > maxSpeed)
        {
            rigid.linearVelocity = new Vector2(maxSpeed, rigid.linearVelocity.y);
        }
        else if (rigid.linearVelocity.x < (-maxSpeed))
        {
            rigid.linearVelocity = new Vector2(-maxSpeed, rigid.linearVelocity.y);
        }

        // 바닥 착지
        Debug.DrawLine(rigid.position + new Vector2(-0.5f, 0), rigid.position + new Vector2(0.5f, 0f), new Color(0, 1, 0));
        Debug.DrawLine(rigid.position + new Vector2(-0.5f, 0), rigid.position + new Vector2(-0.5f, -1f), new Color(0, 1, 0));
        Debug.DrawLine(rigid.position + new Vector2(-0.5f, -1), rigid.position + new Vector2(0.5f, -1f), new Color(0, 1, 0));
        Debug.DrawLine(rigid.position + new Vector2(0.5f, 0), rigid.position + new Vector2(0.5f, -1f), new Color(0, 1, 0));
        RaycastHit2D rayHit = Physics2D.BoxCast(rigid.position, new Vector2(1.5f, 1f), 0f, Vector2.down, 1f, LayerMask.GetMask("Platform"));
        RaycastHit2D rayHit2 = Physics2D.BoxCast(rigid.position + new Vector2(0, -0.5f), new Vector2(1.5f, 1f), 0f, Vector2.down, 1f, LayerMask.GetMask("Obstacle"));
        if (rayHit.collider != null && rayHit.collider.gameObject.layer == 6)
        {
            if (rayHit.distance <= 0.5f && (rigid.linearVelocity.y <= 0)) // 착지중 혹은 착지
            {
                j = 1f;
                animator.SetBool("isJumping", false);
            }
            else // 점프중
            {
                j = 0f;
            }
        }

        if (rayHit.collider == null)
        {
            j = 0f;
        }

        if (rayHit2.collider != null && rayHit2.collider.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreLayerCollision(playerDamagedLayer, enemyLayer, false);
        }
        else
        {
            Physics2D.IgnoreLayerCollision(playerDamagedLayer, enemyLayer, true);
        }


        if (transform.position.y <= -10)
        {
            OnDamaged();
            Invoke("OffDamaged", 2f);
        }
    }

    //충돌 시 이벤트
    void OnCollisionEnter2D(Collision2D collision)
    {
        float contactX = collision.contacts[0].point.x;

        if (collision.gameObject.tag == "Enemy")
        {
            if (rigid.linearVelocity.y < 0 && rigid.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            else
            {
                OnDamaged(contactX);
                Invoke("OffDamaged", 2f);
            }
        }

        if (collision.gameObject.tag == "Obstacle")
        {
            OnDamaged(contactX);
            Invoke("OffDamaged", 2f);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            Item item = collision.GetComponent<Item>();
            gameManager.stagePoint += item.point;
            PlaySound("item");
            collision.gameObject.SetActive(false);
        }
        else if (collision.gameObject.tag == "Finish")
        {
            gameManager.NextStage();
        }
    }

    //피격무적
    void OnDamaged(float? targetPos = null)
    {
        if (targetPos.HasValue) // 피격로직
        {
        gameObject.layer = 9; // PlayerDamaged로 레이어 교체
        health -= 1;
         if (health > 0)
            {
                PlaySound("damaged");
                gameManager.UIHealth[health].gameObject.SetActive(false);
                int knockBackdir = (targetPos - rigid.position.x) > 0 ? -1 : 1; 
                rigid.linearVelocity = Vector2.zero;
                rigid.AddForce(new Vector2(knockBackdir*20, 1), ForceMode2D.Impulse);
                InvokeRepeating("Lighting", 0f, 0.2f);    
            }
            else if (health == 0)
            {
                PlaySound("damaged");
                gameManager.UIHealth[health].gameObject.SetActive(false);
                OnDie();
            }
        
        }
        else // 낙사로직
        {
            gameObject.layer = 9;
            health -= 1;
            if (health > 0)
            {
                PlaySound("damaged");
                gameManager.UIHealth[health].gameObject.SetActive(false);
                rigid.linearVelocity = Vector2.zero;
                transform.position = new Vector3(-8, 1, -5);
                InvokeRepeating("Lighting", 0f, 0.2f);   
            }
            else if (health == 0)
            {
                PlaySound("damaged");
                gameManager.UIHealth[health].gameObject.SetActive(false);
                OnDie();
            }
        }
    }

    void OnDie()
    {
        Time.timeScale = 0;
        PlaySound("die");
        gameManager.OnDieTitle.gameObject.SetActive(true);
        gameManager.Retry.gameObject.SetActive(true);
    }

    void OnAttack(Transform enemy)
    {
        PlaySound("attack");
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        gameManager.stagePoint += 1;
        rigid.AddForce(Vector2.up * 3, ForceMode2D.Impulse);
        enemyMove.OnDamaged();
    }

    void Lighting()
    {
        sprite.color = new Color(1, 1, 1, 0.4f); 
        Invoke("Lighting2", 0.1f);
    }

    void Lighting2()
    {
        sprite.color = new Color(1, 1, 1, 1);
    }

    void OffDamaged()
    {
        gameObject.layer = 8;
        CancelInvoke();
        sprite.color = new Color(1,1,1,1);
    }

    public void PlaySound(string action)
    {
        switch (action)
        {
            case "jump":
                audioSource.clip = jumpSound;
                audioSource.Play();
                break;
            case "attack":
                audioSource.clip = attackSound;
                audioSource.Play();
                break;
            case "damaged":
                audioSource.clip = damagedSound;
                audioSource.Play();
                break;
            case "item":
                audioSource.clip = itemSound;
                audioSource.Play();
                break;
            case "die":
                audioSource.clip = dieSound;
                audioSource.Play();
                break;
            case "clear":
                audioSource.clip = clearSound;
                audioSource.Play();
                break;
        }
    }   
}
