using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace _UkiyoStudio.Scripts
{
    public class ThirdPersonShooterController : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
        [SerializeField] private LayerMask aimColliderMask = new LayerMask();
        [SerializeField] private Transform debugTransform;
        [SerializeField] private Transform pfBulletProjectile;
        [SerializeField] private Transform spawnBulletPosition;
        [SerializeField] private Transform vfxHitGreen;
        [SerializeField] private Transform vfxHitRed;
        [SerializeField] private Transform Crosshair;
        [SerializeField] private float lookSensitivity;
        [SerializeField] private float aimSensitivity;
        [SerializeField] private bool useHitScan;
        [SerializeField] private Rig aimRig;

        private StarterAssetsInputs _starterAssetsInputs;
        private ThirdPersonController _thirdPersonController;
        private Animator _animator;

        private float _aimRigWeight;

        // Start is called before the first frame update
        void Awake()
        {
            _animator = GetComponent<Animator>();
            _thirdPersonController = GetComponent<ThirdPersonController>();
            _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector3 mouseWorldPosition = Vector3.zero;
            Vector2 screenCenterPoint = GetScreenCenter();
            
            HandleLooking(out mouseWorldPosition, out Transform hitTransform, screenCenterPoint);
            
            HandleAiming(mouseWorldPosition);

            HandleShooting(hitTransform, mouseWorldPosition);
        }

        /// <summary>
        /// Get the mouse to world postion and raycast hit transform of where the player is looking
        /// within the game world
        /// </summary>
        /// <param name="mouseWorldPosition"></param>
        /// <param name="hitTransform"></param>
        /// <param name="screenCenterPoint"></param>
        private void HandleLooking(out Vector3 mouseWorldPosition, out Transform hitTransform,
            Vector2 screenCenterPoint)
        {
            hitTransform = null;

            Ray ray = Camera.main.ScreenPointToRay((screenCenterPoint));

            if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
            {
                debugTransform.position = raycastHit.point;
                mouseWorldPosition = raycastHit.point;
                hitTransform = raycastHit.transform;
            }
            else
            {
                debugTransform.position = ray.GetPoint(20);
                mouseWorldPosition = ray.GetPoint(20);
            }
        }

        /// <summary>
        /// Logic to handle shooting depending on shoot mechanic (hit scan or projectile)
        /// </summary>
        /// <param name="hitTransform"></param>
        /// <param name="mouseWorldPosition"></param>
        private void HandleShooting(Transform hitTransform, Vector3 mouseWorldPosition)
        {
            if (_starterAssetsInputs.shoot && _starterAssetsInputs.aim)
            {
                if (useHitScan)
                {
                    ShootHitScan(hitTransform, mouseWorldPosition);
                }
                else
                {
                    ShootProjectile(mouseWorldPosition);
                }
            }

            _starterAssetsInputs.shoot = false;
        }

        /// <summary>
        /// Instantiate vfx when we hit a target using hitscan
        /// </summary>
        /// <param name="hitTransform"></param>
        /// <param name="mouseWorldPosition"></param>
        private void ShootHitScan(Transform hitTransform, Vector3 mouseWorldPosition)
        {
            if(hitTransform == null) return;
            Instantiate(hitTransform.gameObject.CompareTag("Target") ? vfxHitGreen : vfxHitRed,
                mouseWorldPosition, Quaternion.identity);
        }

        /// <summary>
        /// Instantiate a bullet projectile prefab in the direction the player is aiming
        /// </summary>
        /// <param name="mouseWorldPosition"></param>
        private void ShootProjectile(Vector3 mouseWorldPosition)
        {
            Vector3 aimDirection = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            Instantiate(pfBulletProjectile, spawnBulletPosition.position,
                Quaternion.LookRotation(aimDirection, Vector3.up));
        }
        
        /// <summary>
        /// Logic to perform while the player is aiming their weapon
        /// - Lower sensitivity for better accurary
        /// - Rotate the player armature alongside player aiming direction
        /// - Enable Animation Layer to invoke aiming animation
        /// - Enable Aiming Camera for closer over the shoulder camera angle
        /// </summary>
        /// <param name="mouseWorldPosition"></param>
        private void HandleAiming(Vector3 mouseWorldPosition)
        {
            if (_starterAssetsInputs.aim)
            {
                Crosshair.gameObject.SetActive(true);
                debugTransform.gameObject.SetActive(true);
                aimVirtualCamera.gameObject.SetActive(true);
                _thirdPersonController.SetSensitivity(aimSensitivity);
                _thirdPersonController.SetRotateOnMove(false);
                _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));
                aimRig.weight = Mathf.Lerp(aimRig.weight, 1f, Time.deltaTime * 20f);

                Vector3 worldAimTarget = mouseWorldPosition;
                worldAimTarget.y = transform.position.y;
                Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

                transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            }
            else
            {
                Crosshair.gameObject.SetActive(false);
                debugTransform.gameObject.SetActive(false);
                aimVirtualCamera.gameObject.SetActive(false);
                _thirdPersonController.SetSensitivity(lookSensitivity);
                _thirdPersonController.SetRotateOnMove(true);
                _animator.SetLayerWeight(1, Mathf.Lerp(_animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
                aimRig.weight = Mathf.Lerp(aimRig.weight, 0f, Time.deltaTime * 20f);
            }
        }

        
        private Vector2 GetScreenCenter()
        {
            return new Vector2(Screen.width / 2f, Screen.height / 2f);
        }
    }
}