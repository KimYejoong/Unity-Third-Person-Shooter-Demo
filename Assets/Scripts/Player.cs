using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Transform characterTransform;

    [SerializeField] 
    private Transform cameraAnchor; // 카메라 위치 설정을 위한 빈 오브젝트의 트랜스폼

    [SerializeField]
    private float Sensitivity = 1.0f; // 시점 조절 마우스 감도

    [SerializeField]
    private float moveSpeed; // 캐릭터 이동 속도(걷기)

    public float MinX = -25f; // X축 회전각 제한(내려다 볼 때)
    public float MaxX = 70; // X축 회전각 제한(올려다 볼 때)

    public GameObject grenadeObj;

    public int ammo;
    public int grenades;
    public float health;

    public int maxAmmo;
    public int maxGrenades;
    public float maxHealth;

    bool IsDead;

    private AudioSource audioSourceFootstep;

    Animator animator;
    Camera characterCamera;

    bool IsFocused = true; // false == 메뉴 등 UI 조작 위해 마우스 고정 해제, 시점 조작 제한 상태
    bool IsZoomed = false; // 마우스 우클릭으로 확대/축소 토글
    bool IsRunning = false; // LShift로 달리기

    [SerializeField]
    float zoomedFOV = 30; // 확대 시 시야각
    [SerializeField]
    float normalFOV = 60; // 일반 상태 시야각

    [SerializeField]
    float FireRate; // delay between each shot
    float FireCooldownTimer;

    [SerializeField]
    Rigidbody rigid;

    float currentFOV; // 현재 시야각
    float currentSpeed;

    Vector3 CharOriginalLocalPos;
    

    // Start is called before the first frame update
    void Start()
    {
        animator = characterTransform.GetComponentInChildren<Animator>();
        characterCamera = cameraAnchor.GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked; // 기본적으론 마우스 중앙 고정

        currentFOV = normalFOV;
        currentSpeed = 0;
        CharOriginalLocalPos = characterTransform.localPosition;

        Initialize();
    }

    private void Initialize() // 체력 및 탄약 초기화
    {
        IsDead = false;
        maxHealth = 100;
        SetHP(maxHealth);

        maxAmmo = 60;
        ammo = maxAmmo;

        maxGrenades = 3;
        grenades = maxGrenades;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDead)
        {
            LookAround();
            Move();
            Zoom();
            Shoot();
            ThrowGrenade();
        }
        UpdateFocus();
        
    }

    private void LateUpdate()
    {
        // 충돌 판정은 characterTransform에서 일어나는데, 실제 이동은 이(Characater 게임 오브젝트) trasnform을 기준으로 이뤄지므로,
        // 장애물에 의해 characterTrasnform이 이동하지 못하는 경우 두 transform의 위치에 괴리가 생기게 됨.
        // 따라서 chracterTrasnform의 원래 localPosition을 CharOriginalLocalPos에 기록해두고, 오차가 발생할 경우 이 transform의 정위치를 역산하여 재설정
        // 실질적인 이동이 완료된 후 보정해주기 위해 LateUpdate에서 처리

        if (characterTransform.localPosition != CharOriginalLocalPos) 
        {
            transform.position = characterTransform.position - CharOriginalLocalPos;
            characterTransform.localPosition = CharOriginalLocalPos;
        }
    }

    private void LookAround()
    {
        if (!IsFocused) return;

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 camAngle = cameraAnchor.rotation.eulerAngles;

        float x = camAngle.x - mouseDelta.y * Sensitivity;
        float y = camAngle.y + mouseDelta.x * Sensitivity;


        if (x < 180f)
        {
            x = Mathf.Clamp(x, -1f, MaxX); // 아래로 시점 조절이 안되는 현상 피하기 위해 0f 대신 -1f 사용
        }
        else
        {
            x = Mathf.Clamp(x, 360 + MinX, 361f); // 유사한 이유로 위로 시점 회전 시 360f 대신 361f 사용
        }

        cameraAnchor.rotation = Quaternion.Euler(x, y, camAngle.z);        
    }

    private void Move()
    {
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool IsMove = moveInput.magnitude != 0; // 이동 키 입력 여부로 판단, 이동 중에 false로 전환되더라도 실제 이동이 즉각 멈추진 않고 모멘텀에 의해 좀 더 이동
        IsRunning = Input.GetKey(KeyCode.LeftShift);

        if (IsRunning) // 달릴 때 강제로 줌인 상태 해제
        {
            IsZoomed = false;
        }

        // Obsolete: 블랜드 트리 적용 전에 사용한 애니메이터 제어
        // animator.SetBool("IsRunning", IsRunning);
        // animator.SetBool("IsWalking", IsMove);        
        
        Vector3 lookForward = new Vector3(cameraAnchor.forward.x, 0f, cameraAnchor.forward.z).normalized;
        Vector3 lookRight = new Vector3(cameraAnchor.right.x, 0f, cameraAnchor.right.z).normalized;
        Vector3 moveDir = lookForward * moveInput.y + lookRight * moveInput.x;


        if (IsMove)
        {
            if (IsRunning) // 아무 상태에서 달리기 상태로
            {
                if (audioSourceFootstep == null)
                { // 현재 오디오소스에 할당된 발걸음 효과음이 없어서(정지 상태) 바로 달리기 효과음 재생 가능한 경우
                    audioSourceFootstep = SoundManager.Instance.PlaySFX("Run");
                    audioSourceFootstep.loop = true;
                }
                else if (audioSourceFootstep.clip == SoundManager.Instance.audioWalk)
                { // 걷기 상태에서 달리기로 전환하는 경우에 해당하여 걷기 루프 재생부터 멈춰줘야 하는 경우
                    audioSourceFootstep.Stop();
                    audioSourceFootstep = SoundManager.Instance.PlaySFX("Run");
                    audioSourceFootstep.loop = true;
                }


                currentSpeed = Mathf.Clamp(currentSpeed + 0.1f, 1f, moveSpeed * 1.6f);
            }
            else
            {
                if (audioSourceFootstep == null) // 현재 오디오소스에 할당된 발걸음 효과음이 없어서(정지 상태) 바로 걷기 효과음 재생 가능한 경우
                {
                    audioSourceFootstep = SoundManager.Instance.PlaySFX("Walk");
                    audioSourceFootstep.loop = true;
                }
                else if (audioSourceFootstep.clip == SoundManager.Instance.audioRun) // 달리기 상태에서 걷기로 전환하는 경우에 해당하여 걷기 루프 재생부터 멈춰줘야 하는 경우
                {
                    audioSourceFootstep.Stop();
                    audioSourceFootstep = SoundManager.Instance.PlaySFX("Walk");
                    audioSourceFootstep.loop = true;
                }

                if (currentSpeed > moveSpeed) // 달리기에서 걷기로
                    currentSpeed = Mathf.Max(currentSpeed - 0.1f, moveSpeed);
                else // 대기에서 걷기로
                    currentSpeed = Mathf.Clamp(currentSpeed + 0.1f, 1f, moveSpeed);
            }
        }
        else
        {
            currentSpeed = Mathf.Max(currentSpeed - 0.1f, 0); // 감속 후 정지
            if (audioSourceFootstep != null) // 정지 시 발걸음 효과음 재생 멈춤(걷기, 달리기 종류 무관)
                audioSourceFootstep.Stop();
        }

        if (moveDir != Vector3.zero)
            characterTransform.forward = moveDir;
        else // 옆으로 이동하다가 정지할 경우, 다시 천천히 정면 바라보게 함
            characterTransform.forward = Vector3.Lerp(characterTransform.forward, lookForward, Time.deltaTime * 5f);        

        transform.position += moveDir * Time.deltaTime * currentSpeed;
        
        animator.SetFloat("MoveSpeed", currentSpeed); // 애니메이션 블랜드 트리에 현재 상태 반영
    }

    void UpdateFocus()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // ESC키 누를 경우 커서 고정 해제하고 시점 조절 불가한 상태로 변경, 다시 누를 경우 토글
        {
            if (IsFocused)
            {
                Cursor.lockState = CursorLockMode.None;
                IsFocused = false;                
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                IsFocused = true;                
            }
        }
    }

    void Zoom() // 화면 줌인/아웃
    {
        if (Input.GetMouseButtonDown(1) && !IsRunning) // 마우스 우클릭으로 줌인/아웃, 달릴 땐 줌인 불가능
        {
            IsZoomed = !IsZoomed;            
        }

        if (IsZoomed)
            currentFOV = Mathf.Max(currentFOV * 0.9f, zoomedFOV); // 줌인
        else
            currentFOV = Mathf.Min(currentFOV * 1.1f, normalFOV); // 줌아웃(원래대로)

        characterCamera.fieldOfView = currentFOV; // 실질적인 FOV 조절은 여기서
    }

    void Shoot()
    {
        FireCooldownTimer = Mathf.Max(0, FireCooldownTimer - Time.deltaTime);

        if (FireCooldownTimer > 0) // 발사 딜레이에 걸렸을 때
            return;

        if (ammo <= 0) // 총알이 없을 때 리턴하되, 이 상태에서 좌클릭 시 짤깍거리는 소리 재생
        {
            if (Input.GetMouseButtonDown(0))
                SoundManager.Instance.PlaySFX("Dryfire");
            
            return;            
        }

        if (Input.GetMouseButton(0))
        {
            RaycastHit hitResult;
                        
            Ray ray = characterCamera.ScreenPointToRay(Input.mousePosition);
            float maxDistance = 1000f;
            Vector3 vec = ray.origin + ray.direction * maxDistance - characterTransform.position; // 조준 가능한 최대 사거리의 탄착점을 향하는 방향 벡터

            int layerMask = 1 << LayerMask.NameToLayer("Item");
            layerMask = ~layerMask; // Item 레이어는 레이캐스트 대상에서 제외함
            Physics.Raycast(ray, out hitResult, maxDistance, layerMask); // 화면 상의 조준점 기준으로 미리 탄착점 계산
            
            var bullet = BulletManager.GetObject(); // BulletManager의 오브젝트 풀에 요청 보냄
            bullet.Initialize(); // 라이프타임 초기화도 같이 진행

            Vector3 direction;            
            if (hitResult.collider == null) // 사선에 충돌할 대상 없을 경우 조준 Ray의 최대 거리(여기선 1000) 지점을 향해 발사
                direction = vec;
            else // 사선에 뭔가 걸릴 경우 그곳을 향해 발사
            {                
                direction = hitResult.point - transform.position;                
            }

            bullet.transform.position = this.transform.position + direction.normalized * 1f;
            // Debug.DrawRay(this.transform.position, direction);
            bullet.Fire(direction);

            FireCooldownTimer = FireRate;
            SoundManager.Instance.PlaySFX("Shoot");
            ammo--;
        }
    }

    void ThrowGrenade()
    {
        if (grenades == 0) // 잔여 수류탄이 없으면 리턴
            return;

        if (Input.GetKeyDown(KeyCode.G))
        {
            RaycastHit hitResult;

            Ray ray = characterCamera.ScreenPointToRay(Input.mousePosition);
            float maxDistance = 50f;
            Vector3 vec = ray.origin + ray.direction * maxDistance - characterTransform.position;

            int layerMask = 1 << LayerMask.NameToLayer("Item");
            layerMask = ~layerMask; // Item 레이어는 레이캐스트 대상에서 제외함
            Physics.Raycast(ray, out hitResult, maxDistance, layerMask);                      

            Vector3 direction;
            if (hitResult.collider == null)
                direction = vec;
            else
            {
                direction = hitResult.point - transform.position;
            }

            direction = Mathf.Min(direction.magnitude, maxDistance / 5) * direction.normalized + Vector3.up * 2f; // 조준 위치에 따라 약하게 던질 수 있도록 하되, 위쪽으로 약하게 힘을 추가로 줌
            // * 정확한 계산으로 탄착점을 구한 게 아니기 때문에, 일단 거리를 적당히 맞추기 위해서 임의로 /5 해준 상태 → Magic number

            GameObject greandeInstant = Instantiate(grenadeObj, transform.position, transform.rotation);
            Rigidbody rigidGrenade = greandeInstant.GetComponent<Rigidbody>();
            rigidGrenade.AddForce(direction + rigid.velocity , ForceMode.Impulse); // 캐릭터의 이동 속도를 추가 반영
            rigidGrenade.AddTorque(Vector3.back * 8, ForceMode.Impulse);

            SoundManager.Instance.PlaySFX("ThrowGrenade");
            grenades--;
        }
    }

    public void SetHP(float hp)
    {
        health = hp;
        // Debug.Log(HP);
    }

    public void ChangeHP(float amount)
    {
        health = Mathf.Clamp(0, health + amount, maxHealth);
        //Debug.Log(HP);

        if (health == 0) // HP 증감 결과 0이 되면 제거(오브젝트 풀에 반환)
        {
            Debug.Log("Dead");
        }
    }
    
}
