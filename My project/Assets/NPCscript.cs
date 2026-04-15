// NPCDialogue.cs
using UnityEngine;

public class NPCscript : MonoBehaviour
{
    [Header("Hội thoại")]
    [TextArea(2, 5)]
    public string[] dialogueLines = {
       "Hello, traveler!",
       "Nice weather today, isn't it?",
       "Wishing you a safe journey!"
    };

    [Header("Khoảng cách kích hoạt")]
    public float interactRange = 10f;

    [HideInInspector] public bool playerInRange = false;

    void OnDrawGizmosSelected()
    {
        // Hiển thị vùng tương tác trong Editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}