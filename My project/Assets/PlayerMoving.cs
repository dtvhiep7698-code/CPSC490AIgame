// PlayerMoving.cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMoving : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("References")]
    public randomMap mazeGenerator;
    public Tilemap tilemap;

    [Header("Visual")]
    public SpriteRenderer heroRenderer;

    [Header("Effects")]
    public GameObject smokeEffect;
    public Animator smokeAnimator;

    private Vector3 targetPos;
    private bool isMoving = false;
    private Vector2Int currentDir = Vector2Int.zero;

    private float stuckTimer = 0f;
    private float stuckThreshold = 2f;
    private Vector3 lastPosition;
    private bool isReady = false;

    void Start()
    {
        targetPos = transform.position;
        lastPosition = transform.position;
        StartCoroutine(WaitReady());
    }

    System.Collections.IEnumerator WaitReady()
    {
        yield return null;
        SnapToCell();
        isReady = true;
    }

    // Tách riêng để dùng lại nhiều chỗ
    void SnapToCell()
    {
        Vector3Int offset = mazeGenerator.GetOffset();
        // Dùng RoundToInt để tránh sai lệch float khi tính ô
        Vector3Int cell = new Vector3Int(
            Mathf.RoundToInt(transform.position.x - 0.5f) - offset.x,
            Mathf.RoundToInt(transform.position.y - 0.5f) - offset.y,
            0
        );
        // Clamp để không ra ngoài map
        cell.x = Mathf.Clamp(cell.x, 0, mazeGenerator.width - 1);
        cell.y = Mathf.Clamp(cell.y, 0, mazeGenerator.height - 1);

        transform.position = tilemap.CellToWorld(cell + offset) + new Vector3(0.5f, 0.5f, 0);
        targetPos = transform.position;
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!isReady) return;
        HandleInput();
        MoveToTarget();
        CheckStuck();
    }

    void CheckStuck()
    {
        if (!isMoving)
        {
            stuckTimer = 0f;
            return;
        }

        if (Vector3.Distance(transform.position, lastPosition) < 0.001f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer >= stuckThreshold)
            {
                Debug.LogWarning("Player stuck! Resetting...");
                isMoving = false;
                stuckTimer = 0f;
                SnapToCell();
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    void HandleInput()
    {
        if (isMoving) return;

        Vector2Int dir = Vector2Int.zero;

        if (Keyboard.current.wKey.wasPressedThisFrame)      dir = Vector2Int.up;
        else if (Keyboard.current.sKey.wasPressedThisFrame) dir = Vector2Int.down;
        else if (Keyboard.current.aKey.wasPressedThisFrame) dir = Vector2Int.left;
        else if (Keyboard.current.dKey.wasPressedThisFrame) dir = Vector2Int.right;

        if (dir == Vector2Int.zero) return;

        currentDir = dir;
        FlipSprite(dir);

        Vector3Int offset = mazeGenerator.GetOffset();
        // Dùng RoundToInt thay vì WorldToCell để tránh sai lệch float
        Vector3Int currentCell = new Vector3Int(
            Mathf.RoundToInt(transform.position.x - 0.5f) - offset.x,
            Mathf.RoundToInt(transform.position.y - 0.5f) - offset.y,
            0
        );
        Vector3Int nextCell = currentCell;

        while (true)
        {
            Vector3Int check = nextCell + new Vector3Int(dir.x, dir.y, 0);
            if (mazeGenerator.IsWall(check.x, check.y)) break;
            nextCell = check;
        }

        if (nextCell != currentCell)
        {
            targetPos = tilemap.CellToWorld(nextCell + offset) + new Vector3(0.5f, 0.5f, 0);
            isMoving = true;
            stuckTimer = 0f;
            lastPosition = transform.position;
        }
    }

    void FlipSprite(Vector2Int dir)
    {
        if (heroRenderer == null) return;
        if (dir == Vector2Int.right) heroRenderer.flipX = false;
        else if (dir == Vector2Int.left) heroRenderer.flipX = true;
    }

    void RotateToDirection(Vector2Int dir)
    {
        float angle = 0f;
        if (dir == Vector2Int.up)         angle = 90f;
        else if (dir == Vector2Int.down)  angle = 270f;
        else if (dir == Vector2Int.left)  angle = 180f;
        else if (dir == Vector2Int.right) angle = 0f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    Vector2Int TurnLeft(Vector2Int dir)
    {
        if (dir == Vector2Int.up)    return Vector2Int.left;
        if (dir == Vector2Int.left)  return Vector2Int.down;
        if (dir == Vector2Int.down)  return Vector2Int.right;
        if (dir == Vector2Int.right) return Vector2Int.up;
        return Vector2Int.zero;
    }

    void MoveToTarget()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            transform.position = targetPos; // Snap chính xác
            isMoving = false;
            stuckTimer = 0f;

            Vector2Int faceDir = TurnLeft(currentDir);
            RotateToDirection(faceDir);
            currentDir = faceDir;

            PlaySmokeEffect(faceDir);
            CheckCell();
        }
    }

    void CheckCell()
    {
        Vector3Int offset = mazeGenerator.GetOffset();
        Vector3Int cell = new Vector3Int(
            Mathf.RoundToInt(transform.position.x - 0.5f) - offset.x,
            Mathf.RoundToInt(transform.position.y - 0.5f) - offset.y,
            0
        );

        if (cell.x == mazeGenerator.GetGoalCell().x && cell.y == mazeGenerator.GetGoalCell().y)
        {
            Debug.Log("Win! New map...");
            mazeGenerator.RegenerateMap();
            Respawn();
        }
    }

    public void Respawn()
    {
        Vector3Int offset = mazeGenerator.GetOffset();
        Vector3 spawn = tilemap.CellToWorld(new Vector3Int(1, 1, 0) + offset) + new Vector3(0.5f, 0.5f, 0);
        transform.position = spawn;
        targetPos = spawn;
        currentDir = Vector2Int.zero;
        stuckTimer = 0f;
        transform.rotation = Quaternion.identity;
        lastPosition = spawn;
    }

    void PlaySmokeEffect(Vector2Int dir)
    {
        if (smokeEffect == null || smokeAnimator == null) return;

        smokeEffect.transform.position = transform.position;

        float angle = 0f;
        if (dir == Vector2Int.up)         angle = 90f;
        else if (dir == Vector2Int.down)  angle = 270f;
        else if (dir == Vector2Int.left)  angle = 180f;
        else if (dir == Vector2Int.right) angle = 0f;

        smokeEffect.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        smokeEffect.SetActive(true);
        smokeAnimator.SetTrigger("PlaySmoke");

        StartCoroutine(HideSmokeAfterDelay());
    }

    System.Collections.IEnumerator HideSmokeAfterDelay()
    {
        yield return new WaitForSeconds(0.4f);
        smokeEffect.SetActive(false);
    }
}