using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// RED CORNER: the bomb written with coroutines.
// Compare round by round with BombAwaitable.cs.
public class BombCoroutine : MonoBehaviour
{
    [SerializeField] float fuseSeconds = 3f;
    [SerializeField] Transform fuse;
    [SerializeField] GameObject explosionPrefab;

    bool isLit;
    public void Light() => isLit = true;

    // Round 1: a coroutine needs StartCoroutine to run.
    void Start() => StartCoroutine(BombRoutine());

    IEnumerator BombRoutine()
    {
        // Round 3: WaitUntil re-checks the condition every frame.
        yield return new WaitUntil(() => isLit);

        // Round 2: no return values, so the answer comes back through a callback.
        bool defused = false;
        yield return BurnFuse(result => defused = result);
        if (defused)
        {
            Destroy(gameObject);
            yield break;
        }

        // Lightning round: the four classic waits.
        yield return new WaitForSeconds(1f);     // seconds
        yield return null;                       // next frame
        yield return new WaitForFixedUpdate();   // physics step
        yield return new WaitForEndOfFrame();    // after the frame is rendered

        // Round 4: a heavy loop here runs on the main thread and freezes the game.

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }

    IEnumerator BurnFuse(Action<bool> onDone)
    {
        float timeLeft = fuseSeconds;

        while (timeLeft > 0f)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                onDone(true);
                yield break;
            }

            timeLeft -= Time.deltaTime;
            fuse.localScale = new Vector3(1f, timeLeft / fuseSeconds, 1f);
            yield return null;
        }

        onDone(false);
    }

    // Round 5: destroy or deactivate this GameObject and the coroutine simply stops.

    // Round 6: this does NOT compile (error CS1626: cannot yield a value
    // in the body of a try block with a catch clause):
    //
    // try { yield return new WaitForSeconds(1f); }
    // catch (Exception e) { Debug.LogException(e); }
}
