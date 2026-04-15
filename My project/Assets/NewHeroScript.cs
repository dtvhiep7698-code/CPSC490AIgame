using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class NewHeroScript : MonoBehaviour
{
    public float speed = 5f;
    public float minX;
    public float maxX;
    // Thêm biến này vào đầu
    private float npcX = 4f; // Vị trí cố định của NPC

    [Header("UI Hội thoại")]
    public GameObject interactPrompt;    // UI "[Space] Nói chuyện"
    public GameObject dialogueBox;       // Khung hội thoại
    public TextMeshProUGUI dialogueText; // Text hiển thị câu thoại

    [Header("Hội thoại NPC")]
    [TextArea(2, 5)]
    public string[] dialogueLines = {
        "Hello, traveler!",
        "Nice weather today, isn't it?",
        "Wishing you a safe journey!"
    };

    public float interactRange = 1f; // khoảng cách tính từ maxX

    private SpriteRenderer sr;
    private Animator animator;
    private bool isTalking = false;
    private int dialogueIndex = 0;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        interactPrompt?.SetActive(false);
        dialogueBox?.SetActive(false);
    }

    void Update()
    {
        HandleMovement();
        CheckNearMaxX();
        HandleDialogue();
    }

    void HandleMovement()
    {
        if (isTalking) return;

        float move = 0f;
        if (Keyboard.current.aKey.isPressed) move = -1f;
        else if (Keyboard.current.dKey.isPressed) move = 1f;

        float newX = transform.position.x + move * speed * Time.deltaTime;
        newX = Mathf.Clamp(newX, minX, maxX);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        if (animator != null)
            animator.SetBool("isWalking", move != 0);

        if (move > 0) sr.flipX = false;
        else if (move < 0) sr.flipX = true;
    }

    // ── Kiểm tra hero có đứng gần maxX không ───────────
   // NewHeroScript.cs
    void CheckNearMaxX()
    {
        if (isTalking) return;

        bool nearNPC = Mathf.Abs(transform.position.x - npcX) <= interactRange;
        interactPrompt?.SetActive(nearNPC);
    }

    void HandleDialogue()
    {
        if (!Keyboard.current.spaceKey.wasPressedThisFrame) return;

        bool nearNPC = Mathf.Abs(transform.position.x - npcX) <= interactRange;

        if (!isTalking && nearNPC)
        {
            // Tìm NPC gần maxX và lấy dialogueLines từ nó
            NPCscript npc = FindAnyObjectByType<NPCscript>();
            if (npc != null)
            {
                dialogueLines = npc.dialogueLines; // Dùng data từ NPC
            }
            StartDialogue();
        }
        else if (isTalking)
        {
            NextLine(); // Lật dòng, nếu hết thì NextLine() tự gọi EndDialogue()
        }
    }

    void StartDialogue()
    {
        isTalking = true;
        dialogueIndex = 0;
        interactPrompt?.SetActive(false);
        dialogueBox?.SetActive(true);
        if (animator != null) animator.SetBool("isWalking", false);
        ShowLine();
    }

    void NextLine()
    {
        dialogueIndex++;
        if (dialogueIndex < dialogueLines.Length)
            ShowLine();
        else
            EndDialogue();
    }

    void ShowLine()
    {
        if (dialogueText != null)
            dialogueText.text = dialogueLines[dialogueIndex];
    }

    void EndDialogue()
    {
        isTalking = false;
        dialogueBox?.SetActive(false);
        dialogueText?.SetText("");
    }
}
