using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    public Transform rayDown, rayLeft, rayRight;
    public LayerMask platformLayer, obstacleLayer;
    private bool isMoveLeft = false;
    private Vector3 nextPlatformLeft, nextPlatformRight;
    // to stop infinite jumping, see if character is jumping
    private bool isJumping = false;
    private bool isMove = false;
    private ManagerVars vars;
    private Rigidbody2D my_Body;
    private SpriteRenderer spriteRenderer;
    private AudioSource m_AudioSource;
    private void Awake()
    {
        EventCenter.AddListener<bool>(EventDefine.IsMusicOn, IsMusicOn);
        EventCenter.AddListener<int>(EventDefine.ChangeSkin, ChangeSkin);
        vars = ManagerVars.GetManagerVars();
        my_Body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        m_AudioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        ChangeSkin(GameManager.Instance.GetCurrentSelectedSkin());
    }
    private void OnDestroy()
    {
        EventCenter.RemoveListener<int>(EventDefine.ChangeSkin, ChangeSkin);
        EventCenter.RemoveListener<bool>(EventDefine.IsMusicOn, IsMusicOn);
    }
    private void ChangeSkin(int skinIndex)
    {
        spriteRenderer.sprite = vars.characterSkinSpriteList[skinIndex];
    }
    private void IsMusicOn(bool value)
    {
        m_AudioSource.mute = !value;
    }
    private bool IsPointerOverGameObject(Vector2 mousePosition)
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raycastResults);
        return raycastResults.Count > 0;
    }
    private void Update()
    {
        if (IsPointerOverGameObject(Input.mousePosition)) return;
        if (GameManager.Instance.IsGameStarted == false || GameManager.Instance.IsGameOver || GameManager.Instance.IsPause)
            return;
        if (Input.GetMouseButtonDown(0) && isJumping == false && nextPlatformLeft != Vector3.zero)
        {
            if (isMove == false)
            {
                EventCenter.Broadcast(EventDefine.PlayerMove);
                isMove = true;
            }
            m_AudioSource.PlayOneShot(vars.jumpClip);
            EventCenter.Broadcast(EventDefine.DecidePath);
            isJumping = true;

            //Determine click right or left side of the screen.
            Vector3 mousePos = Input.mousePosition;
            if (mousePos.x <= Screen.width / 2)
            {
                isMoveLeft = true;
            }
            else if (mousePos.x > Screen.width / 2)
            {
                isMoveLeft = false;
            }
            Jump();
        }
        
        //Game Over 1
        if (my_Body.velocity.y < 0 && IsRayPlatform() == false && GameManager.Instance.IsGameOver == false)
        {
            m_AudioSource.PlayOneShot(vars.fallClip);
            spriteRenderer.sortingLayerName = "Default";
            GetComponent<BoxCollider2D>().enabled = false;
            GameManager.Instance.IsGameOver = true;
            StartCoroutine(DealyShowGameOverPanel());
        }
        //Game Over 2
        if (isJumping && IsRayObstacle() && GameManager.Instance.IsGameOver == false)
        {
            m_AudioSource.PlayOneShot(vars.hitClip);
            GameObject go = ObjectPool.Instance.GetDeathEffect();
            go.SetActive(true);
            go.transform.position = transform.position;
            GameManager.Instance.IsGameOver = true;
            spriteRenderer.enabled = false;
            StartCoroutine(DealyShowGameOverPanel());
        }
        //Game over 3
        if (transform.position.y - Camera.main.transform.position.y < -5 && GameManager.Instance.IsGameOver == false)
        {
            m_AudioSource.PlayOneShot(vars.fallClip);
            GameManager.Instance.IsGameOver = true;
            StartCoroutine(DealyShowGameOverPanel());
        }
    }
    IEnumerator DealyShowGameOverPanel()
    {
        yield return new WaitForSeconds(1f);
        EventCenter.Broadcast(EventDefine.ShowGameOverPanel);
    }
    private void Jump()
    {
        if (isMoveLeft)
        {
            //character facing direction
            transform.localScale = new Vector3(-1, 1, 1);
            transform.DOMoveX(nextPlatformLeft.x, 0.2f);
            transform.DOMoveY(nextPlatformLeft.y + 0.8f, 0.15f);
        }
        else
        {
            transform.DOMoveX(nextPlatformRight.x, 0.2f);
            transform.DOMoveY(nextPlatformRight.y + 0.8f, 0.15f);
            transform.localScale = Vector3.one;
        }
    }
    //avoid addscore in the same platform
    private GameObject lastHitGo = null;
    // detect platform
    private bool IsRayPlatform()
    {
        RaycastHit2D hit = Physics2D.Raycast(rayDown.position, Vector2.down, 1f, platformLayer);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Platform")
            {
                if (lastHitGo != hit.collider.gameObject)
                {
                    if (lastHitGo == null)
                    {
                        lastHitGo = hit.collider.gameObject;
                        return true;
                    }
                    // broadcast addscore
                    EventCenter.Broadcast(EventDefine.AddScore);
                    lastHitGo = hit.collider.gameObject;
                }
                return true;
            }
        }
        return false;
    }
    //detect obstacle
    private bool IsRayObstacle()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(rayLeft.position, Vector2.left, 0.15f, obstacleLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(rayRight.position, Vector2.right, 0.15f, obstacleLayer);

        if (leftHit.collider != null)
        {
            if (leftHit.collider.tag == "Obstacle")
            {
                return true;
            }
        }

        if (rightHit.collider != null)
        {
            if (rightHit.collider.tag == "Obstacle")
            {
                return true;
            }
        }
        return false;
    }
    
    // see if the character collide platform
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Platform")
        {
            isJumping = false;
            Vector3 currentPlatformPos = collision.gameObject.transform.position;
            nextPlatformLeft = new Vector3(currentPlatformPos.x -
                vars.nextXPos, currentPlatformPos.y + vars.nextYPos, 0);
            nextPlatformRight = new Vector3(currentPlatformPos.x +
                vars.nextXPos, currentPlatformPos.y + vars.nextYPos, 0);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Pickup")
        {
            m_AudioSource.PlayOneShot(vars.diamondClip);
            EventCenter.Broadcast(EventDefine.AddDiamond);
            collision.gameObject.SetActive(false);
        }
    }
}
