using Skills;
        using UnityEngine;
        using UnityEngine.Serialization;
        
        public class SwordSkill : Skill
        {
            [Header("Skill Info")] 
            [SerializeField] private GameObject swordPrefab;
            [SerializeField] private Vector2 launchForce;
            [SerializeField] private float swordGravity = 1f; // 改为1作为默认值，表示正常重力
            
            private Vector2 finalDirection;
            private Vector2 currentAimDirection;
        
            [Header("Aim dots")]
            [SerializeField] private int numberOfDots = 10;
            [SerializeField] private float spaceBetweenDots = 0.5f;
            [SerializeField] private GameObject dotPrefab;
            [SerializeField] private GameObject dotsParent;
            
            private GameObject[] dots;
            
            protected override void Start()
            {
                base.Start();
                GenerateDots();
            }
            
            protected override void Update()
            {
                base.Update();
                
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    DotsActive(true);
                }
                
                if (Input.GetKey(KeyCode.Mouse1))
                {
                    // 存储当前瞄准方向，避免多次计算
                    currentAimDirection = AimDirection();
                    
                    for (int i = 0; i < dots.Length; i++)
                    {
                        dots[i].transform.position = DotsPosition(i * spaceBetweenDots);
                    }
                }
                
                if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    // 在释放鼠标时确定最终方向
                    finalDirection = new Vector2(currentAimDirection.x * launchForce.x, currentAimDirection.y * launchForce.y);
                    DotsActive(false);
                }
            }
            
            public void CreateSword()
            {
                GameObject newSword = Instantiate(swordPrefab, player.swordPoint.transform.position, transform.rotation);
                SwordSkillController swordSkillController = newSword.GetComponent<SwordSkillController>();
                
                swordSkillController.SetUpSword(finalDirection, swordGravity);
                swordSkillController.ShowSwordAnimation();
            }

            # region Draw Aim Line
            private Vector2 AimDirection()
            {
                Vector2 playerPosition = player.swordPoint.transform.position;
                Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                Vector2 direction = mousePosition - playerPosition;
                return direction.normalized;
            }
        
            public void DotsActive(bool isActive)
            {
                dotsParent.SetActive(isActive);
            }
        
            private void GenerateDots()
            {
                dots = new GameObject[numberOfDots];
                for (int i = 0; i < numberOfDots; i++)
                {
                    dots[i] = Instantiate(dotPrefab, player.swordPoint.transform.position, Quaternion.identity, dotsParent.transform);
                }
                dotsParent.SetActive(false);
            }
            
            private Vector2 DotsPosition(float t)
            {
                Vector2 initialVelocity = new Vector2(
                    currentAimDirection.x * launchForce.x,
                    currentAimDirection.y * launchForce.y);
                    
                Vector2 gravity = Physics2D.gravity * swordGravity;
                
                Vector2 position = (Vector2)player.swordPoint.transform.position + 
                    initialVelocity * t + 
                    0.5f * gravity * t * t;
                    
                return position;
            }
            #endregion
        }