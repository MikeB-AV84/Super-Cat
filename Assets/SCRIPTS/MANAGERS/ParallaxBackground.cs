using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [Header("References (Assign in Inspector)")]
    [Tooltip("The first child GameObject displaying one part of this background layer.")]
    public Transform backgroundPartA;
    [Tooltip("The second child GameObject displaying the other part of this background layer.")]
    public Transform backgroundPartB;

    [Header("Movement Settings")]
    [Tooltip("How fast this layer scrolls relative to the main game speed. 0 = no scroll, 1 = same speed as foreground. Distant layers should have smaller values (e.g., 0.1 for sky).")]
    public float parallaxFactor = 0.5f;

    private float _spriteWidth;             // Width of one background part in world units
    private Camera _mainCamera;
    private float _cameraHalfWidthWorld;

    void Start()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError($"ParallaxBackground on '{gameObject.name}': Main Camera not found! Disabling script.", this.gameObject);
            enabled = false;
            return;
        }
        if (!_mainCamera.orthographic)
        {
            Debug.LogWarning($"ParallaxBackground on '{gameObject.name}': Main Camera is not orthographic. Parallax effect might not work as intended.", this.gameObject);
        }
        _cameraHalfWidthWorld = _mainCamera.orthographicSize * _mainCamera.aspect;


        if (backgroundPartA == null || backgroundPartB == null)
        {
            Debug.LogError($"ParallaxBackground on '{gameObject.name}': backgroundPartA or backgroundPartB not assigned in the Inspector! Disabling script.", this.gameObject);
            enabled = false;
            return;
        }

        SpriteRenderer srA = backgroundPartA.GetComponent<SpriteRenderer>();
        if (srA == null || srA.sprite == null)
        {
            Debug.LogError($"ParallaxBackground on '{gameObject.name}': backgroundPartA is missing its SpriteRenderer or Sprite! Disabling script.", this.gameObject);
            enabled = false;
            return;
        }

        // Calculate sprite width based on the sprite's bounds and the local scale of the child transform
        _spriteWidth = srA.sprite.bounds.size.x * backgroundPartA.transform.localScale.x;

        if (_spriteWidth <= 0)
        {
            Debug.LogError($"ParallaxBackground on '{gameObject.name}': Calculated sprite width is zero or negative. Check Part_A's sprite, PPU, and scale. Disabling script.", this.gameObject);
            enabled = false;
            return;
        }

        // Initialize positions: A is centered on this parent, B is to the right of A.
        // The parent GameObject itself will be centered with the camera (or at your desired starting X).
        backgroundPartA.localPosition = Vector3.zero; // A is at the parent's origin
        backgroundPartB.localPosition = new Vector3(_spriteWidth, 0, 0); // B is one width to the right of A
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver())
        {
            return; // Stop scrolling if GameManager is missing or the game is over
        }

        // Calculate how much this layer should move based on game speed and parallax factor
        float deltaX = GameManager.Instance.CurrentGameSpeed * Time.deltaTime * parallaxFactor;

        // Move this parent GameObject (which carries Part_A and Part_B with it)
        transform.Translate(Vector3.left * deltaX, Space.World);

        // Check if Part_A needs to leapfrog to the right
        // Condition: If Part_A's right edge (world position) is now to the left of the camera's left edge.
        float partA_RightEdge_WorldX = backgroundPartA.position.x + (_spriteWidth / 2f);
        float cameraLeftEdge_WorldX = _mainCamera.transform.position.x - _cameraHalfWidthWorld;

        if (partA_RightEdge_WorldX < cameraLeftEdge_WorldX)
        {
            // Part_A is off-screen to the left. Move its local position to be to the right of Part_B.
            // Part_B is currently at localX = _spriteWidth (relative to Part_A's original localX=0).
            // So, new localX for A will be _spriteWidth + _spriteWidth.
            backgroundPartA.localPosition += new Vector3(2 * _spriteWidth, 0, 0);
            //Debug.Log($"{gameObject.name} - Part A jumped right.");
        }

        // Check if Part_B needs to leapfrog to the right
        // Condition: If Part_B's right edge (world position) is now to the left of the camera's left edge.
        float partB_RightEdge_WorldX = backgroundPartB.position.x + (_spriteWidth / 2f);
        // cameraLeftEdge_WorldX is already calculated

        if (partB_RightEdge_WorldX < cameraLeftEdge_WorldX)
        {
            // Part_B is off-screen to the left. Move its local position to be to the right of Part_A.
            backgroundPartB.localPosition += new Vector3(2 * _spriteWidth, 0, 0);
            //Debug.Log($"{gameObject.name} - Part B jumped right.");
        }
    }
}
