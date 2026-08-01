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

    CapsuleCollider2D cap;
    public PhysicsMaterial2D OnFriction;
    public PhysicsMaterial2D OffFriction;
    
    void Awake()
    {
        cap = GetComponent<CapsuleCollider2D>();
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
        } // 점프키를 뗄 때 속도를 줄여서 점프 높이를 조절
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

        //낙하 애니메이션
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

        //캐릭터 아래에 레이박스를 쏴서 바닥이 감지되는지 확인
        RaycastHit2D rayHit = Physics2D.BoxCast(rigid.position, new Vector2(0.7f, 0.6f), 0f, Vector2.down, 0.6f, LayerMask.GetMask("Platform"));
        //캐릭터 0.4 아래에 레이박스를 쏴서 적이 감지되는지 확인
        RaycastHit2D rayHit2 = Physics2D.BoxCast(rigid.position + new Vector2(0, 0.4f), new Vector2(1.8f, 2f), 0f, Vector2.down, 2f, LayerMask.GetMask("Obstacle"));
        
        //바닥에 닿았는지 확인
        if (rayHit.collider != null && rayHit.collider.gameObject.layer == 6)
        {
            if (rayHit.distance <= 0.2f && (rigid.linearVelocity.y <= 0)) // 착지중 혹은 착지
            {
                cap.sharedMaterial = OnFriction;
                j = 1f;
                animator.SetBool("isJumping", false);
            }
            else // 점프중
            {
                j = 0f;
            }
        }

        if (rayHit.collider == null) // 허공으로 떨어지고 있는 경우 (아무것도 감지되지 않음)
        {
            j = 0f;
            cap.sharedMaterial = OffFriction;
        }

        //적이 감지되는지 확인
        if (rayHit2.collider != null && rayHit2.collider.gameObject.CompareTag("Enemy"))
        {
            Physics2D.IgnoreLayerCollision(playerDamagedLayer, enemyLayer, false);
        }
        else
        {
            Physics2D.IgnoreLayerCollision(playerDamagedLayer, enemyLayer, true);
        }

        //낙사
        if (transform.position.y <= -10)
        {
            OnDamaged();
            Invoke("OffDamaged", 2f);
        }
    }

    //충돌 시 이벤트
    void OnCollisionEnter2D(Collision2D collision)
    {   
        //장애물의 x좌표를 가져옴
        float contactX = collision.contacts[0].point.x;

        //장애물이 적일 경우
        if (collision.gameObject.tag == "Enemy")
        {
            //적의 y좌표보다 플레이어의 y좌표가 높고, 플레이어가 아래로 떨어지고 있을 때 공격
            if (rigid.linearVelocity.y < 0 && rigid.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
            }
            else // 그 외의 경우 피격
            {
                OnDamaged(contactX);
                Invoke("OffDamaged", 2f);
            }
        }
        
        //장애물이 장애물일 경우
        if (collision.gameObject.tag == "Obstacle")
        {
            // 피격
            OnDamaged(contactX);
            Invoke("OffDamaged", 2f);
        }
    }

    //트리거로 충돌 시 이벤트
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item")
        {
            Item item = collision.GetComponent<Item>();
            //아이템 획득
            if (item.itemType == Item.ItemType.Coin)
            {
                gameManager.stagePoint += item.point;
                PlaySound("item");
                collision.gameObject.SetActive(false);
            }

            else if (item.itemType == Item.ItemType.Apple)
            {
                health += 1;
                gameManager.stagePoint += item.point;
                if (health > 5)
                {
                    health = 5;
                }
                PlaySound("item");
                gameManager.UIHealth[health - 1].gameObject.SetActive(true);
                collision.gameObject.SetActive(false);
            }
        }
        
        
        if (collision.gameObject.tag == "Finish") //피니시 라인에 도달했을 때 다음 스테이지로 이동
        {
            gameManager.NextStage();
        }
    }

    //피격무적 (targetPos가 null이면 낙사로직, null이 아니면 적과 충돌로직)
    void OnDamaged(float? targetPos = null)
    {
        if (targetPos.HasValue) 
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
        gameManager.stagePoint += 150;
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
