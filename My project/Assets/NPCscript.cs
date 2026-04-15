// NPCDialogue.cs
using UnityEngine;

public class NPCscript : MonoBehaviour
{
    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] dialogueLines = {
       "Hello, traveler!",
       "Nice weather today, isn't it?",
       "Wishing you a safe journey!"
    };

    [Header("Interaction Range")]
    public float interactRange = 10f;

    [HideInInspector] public bool playerInRange = false;

    void OnDrawGizmosSelected()
    {
        // Show interactive area in Editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}