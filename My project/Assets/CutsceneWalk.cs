// CutsceneWalk.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CutsceneWalk : MonoBehaviour
{
    [Header("Scene next")]
    public string nextSceneName = "GameScene";

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float exitX = 12f;

    [Header("Visual")]
    public SpriteRenderer heroRenderer;  // Drag SpriteRenderer here

    private AsyncOperation asyncLoad;
    private bool sceneReady = false;
    private bool hasTriggered = false;

    void Start()
    {
        // Place the character off-screen to the left
        float startX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).x - 1.5f;
        float startY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.2f, 0)).y;
        transform.position = new Vector3(startX, startY, 0);

        // The sprite looks to the right by default, so ensure it's not flipped
        if (heroRenderer != null) heroRenderer.flipX = false;

        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
            yield return null;

        sceneReady = true;
    }

    void Update()
    {
        // The character always moves to the right
        transform.Translate(Vector2.right * walkSpeed * Time.deltaTime);

        // When the character has moved off the right side of the screen AND the scene is loaded → switch scene
        if (!hasTriggered && transform.position.x >= exitX && sceneReady)
        {
            hasTriggered = true;
            asyncLoad.allowSceneActivation = true;
        }

        // If the character has moved off the screen but the scene is not yet loaded
        // → stop at the right edge and wait
        if (transform.position.x >= exitX - 0.5f && !sceneReady)
        {
            transform.position = new Vector3(exitX - 0.5f, transform.position.y, 0);
        }
    }
}