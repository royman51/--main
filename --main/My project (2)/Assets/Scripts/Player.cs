using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 6f;

    public GameObject bullet;
    public Transform shootPos;

    public KeyCode shootKey = KeyCode.X;

    public float chargeStartJudgeTime = 0.25f;

    public float chargeNeedTime = 0.8f;

    public float normalBulletScale = 1f;
    public float bigBulletScale = 1.55f;

    public int normalBulletDamage = 1;
    public int bigBulletDamage = 2;

    private bool isHoldingShootKey = false;
    private bool isRealCharging = false;
    private float chargeTimer = 0f;

    public bool useCameraBounds = true;
    public Camera boundaryCamera;

    public float cameraWallPadding = 0.15f;

    public bool useColliderSizeForBounds = true;

    private Collider2D playerCollider;

    public float turnAngle = 18f;
    public float turnSpeed = 8f;

    public bool useChargeShake = true;

    public Transform visualRoot;

    public float minChargeShake = 0.02f;
    public float maxChargeShake = 0.18f;

    private Vector3 visualStartLocalPosition;
    private Vector3 rootShakeOffset = Vector3.zero;

    private bool canControl = true;

    void Start()
    {
        if (visualRoot != null)
        {
            visualStartLocalPosition = visualRoot.localPosition;
        }

        if (boundaryCamera == null)
        {
            boundaryCamera = Camera.main;
        }

        playerCollider = GetComponent<Collider2D>();

        if (playerCollider == null)
        {
            playerCollider = GetComponentInChildren<Collider2D>();
        }
    }

    void Update()
    {
        // 플레이어 본체를 흔드는 경우, 이전 프레임 흔들림을 먼저 제거
        if (visualRoot == null && rootShakeOffset != Vector3.zero)
        {
            transform.position = transform.position - rootShakeOffset;
            rootShakeOffset = Vector3.zero;
        }

        if (!canControl)
        {
            StopChargeShake();

            isHoldingShootKey = false;
            isRealCharging = false;
            chargeTimer = 0f;

            return;
        }

        float x = 0;
        float y = 0;

        if (Input.GetKey(KeyCode.W))
        {
            y = y + 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            y = y - 1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            x = x - 1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            x = x + 1;
        }

        Move(x, y);
        TurnAnimation(x);
        ChargeAndShoot();
        ChargeShake();
    }

    void Move(float x, float y)
    {
        Vector3 move = new Vector3(x, y, 0);

        if (move.magnitude > 1)
        {
            move = move.normalized;
        }

        transform.position = transform.position + move * speed * Time.deltaTime;

        ClampPlayerPositionByCamera();
    }

    void ClampPlayerPositionByCamera()
    {
        if (!useCameraBounds)
        {
            return;
        }

        if (boundaryCamera == null)
        {
            boundaryCamera = Camera.main;
        }

        if (boundaryCamera == null)
        {
            return;
        }

        Vector3 playerPos = transform.position;

        float distanceFromCamera = Mathf.Abs(playerPos.z - boundaryCamera.transform.position.z);

        Vector3 bottomLeft = boundaryCamera.ViewportToWorldPoint(new Vector3(0f, 0f, distanceFromCamera));
        Vector3 topRight = boundaryCamera.ViewportToWorldPoint(new Vector3(1f, 1f, distanceFromCamera));

        float halfWidth = 0f;
        float halfHeight = 0f;

        if (useColliderSizeForBounds && playerCollider != null)
        {
            halfWidth = playerCollider.bounds.extents.x;
            halfHeight = playerCollider.bounds.extents.y;
        }

        float minX = bottomLeft.x + halfWidth + cameraWallPadding;
        float maxX = topRight.x - halfWidth - cameraWallPadding;

        float minY = bottomLeft.y + halfHeight + cameraWallPadding;
        float maxY = topRight.y - halfHeight - cameraWallPadding;

        playerPos.x = Mathf.Clamp(playerPos.x, minX, maxX);
        playerPos.y = Mathf.Clamp(playerPos.y, minY, maxY);

        transform.position = playerPos;
    }

    void TurnAnimation(float x)
    {
        float targetZ = 0f;

        if (x < 0)
        {
            targetZ = turnAngle;
        }

        if (x > 0)
        {
            targetZ = -turnAngle;
        }

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetZ);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime
        );
    }

    void ChargeAndShoot()
    {
        if (Input.GetKeyDown(shootKey))
        {
            isHoldingShootKey = true;
            isRealCharging = false;
            chargeTimer = 0f;
        }

        if (Input.GetKey(shootKey) && isHoldingShootKey)
        {
            chargeTimer = chargeTimer + Time.deltaTime;

            if (!isRealCharging && chargeTimer >= chargeStartJudgeTime)
            {
                isRealCharging = true;
                Debug.Log("차지 시작");
            }
        }

        if (Input.GetKeyUp(shootKey) && isHoldingShootKey)
        {
            StopChargeShake();

            if (chargeTimer >= chargeNeedTime)
            {
                FireBigBullet();
            }
            else
            {
                FireNormalBullet();
            }

            isHoldingShootKey = false;
            isRealCharging = false;
            chargeTimer = 0f;
        }
    }

    void ChargeShake()
    {
        if (!useChargeShake)
        {
            return;
        }

        if (!isHoldingShootKey || !isRealCharging)
        {
            StopChargeShake();
            return;
        }

        float chargePower = 0f;

        if (chargeNeedTime > chargeStartJudgeTime)
        {
            chargePower = (chargeTimer - chargeStartJudgeTime) / (chargeNeedTime - chargeStartJudgeTime);
        }

        if (chargePower < 0f)
        {
            chargePower = 0f;
        }

        if (chargePower > 1f)
        {
            chargePower = 1f;
        }

        float shakeAmount = Mathf.Lerp(minChargeShake, maxChargeShake, chargePower);

        float randomX = Random.Range(-shakeAmount, shakeAmount);
        float randomY = Random.Range(-shakeAmount, shakeAmount);

        Vector3 shake = new Vector3(randomX, randomY, 0);

        if (visualRoot != null)
        {
            visualRoot.localPosition = visualStartLocalPosition + shake;
        }
        else
        {
            transform.position = transform.position + shake;
            rootShakeOffset = shake;
        }
    }

    void StopChargeShake()
    {
        if (visualRoot != null)
        {
            visualRoot.localPosition = visualStartLocalPosition;
        }

        if (visualRoot == null && rootShakeOffset != Vector3.zero)
        {
            transform.position = transform.position - rootShakeOffset;
            rootShakeOffset = Vector3.zero;
        }
    }

    void FireNormalBullet()
    {
        FireBullet(normalBulletDamage, normalBulletScale);
    }

    void FireBigBullet()
    {
        FireBullet(bigBulletDamage, bigBulletScale);
    }

    void FireBullet(int damage, float scale)
    {
        if (bullet == null)
        {
            Debug.Log("총알이 없음");
            return;
        }

        if (shootPos == null)
        {
            Debug.Log("발사 위치가 없음");
            return;
        }

        GameObject newBullet = Instantiate(bullet, shootPos.position, Quaternion.identity);

        newBullet.transform.localScale = new Vector3(scale, scale, scale);

        // 플레이어 탄환 무작위 회전
        float randomZ = Random.Range(0f, 360f);
        newBullet.transform.rotation = Quaternion.Euler(0f, 0f, randomZ);

        Bullet bulletScript = newBullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.damage = damage;
        }

        if (scale >= bigBulletScale)
        {
            Debug.Log("큰 탄환 발사 피해량: " + damage);
        }
        else
        {
            Debug.Log("일반 탄환 발사 피해량: " + damage);
        }
    }

    public void SetControlEnabled(bool value)
    {
        canControl = value;

        if (!canControl)
        {
            StopChargeShake();

            isHoldingShootKey = false;
            isRealCharging = false;
            chargeTimer = 0f;
        }
    }
}