using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// BLUE CORNER: the bomb written with Awaitable (Unity 6).
// Compare round by round with BombCoroutine.cs.
public class BombAwaitable : MonoBehaviour
{
    [SerializeField] float fuseSeconds = 3f;
    [SerializeField] Transform fuse;
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] ParticleSystem sparkParticles;
    [SerializeField] float blastRadius = 5f;
    [SerializeField] float blastForce = 800f;

    // Lets us await something that isn't time: here, a match being struck.
    readonly AwaitableCompletionSource lit = new();

    // Call this from anything: a match, a button, a trigger.
    public void Light() => lit.TrySetResult();

    // In Unity 6, Start itself can be async.
    async Awaitable Start()
    {
        CancellationToken token = destroyCancellationToken;

        try
        {
            // 1. Wait for an event (AwaitableCompletionSource).
            await lit.Awaitable;

            // 2. Every frame + a return value (NextFrameAsync + Awaitable<bool>).
            bool defused = await BurnFuseAsync(token);
            if (defused)
            {
                Destroy(gameObject);
                return;
            }

            // 3. A timed pause (WaitForSecondsAsync).
            await Awaitable.WaitForSecondsAsync(1f, token);

            // 4. Heavy math off the main thread (BackgroundThreadAsync / MainThreadAsync).
            Vector3[] sparks = await CalculateSparksAsync(1000, transform.position);

            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            var emit = new ParticleSystem.EmitParams();
            foreach (Vector3 spark in sparks)
            {
                emit.position = spark;
                sparkParticles.Emit(emit, 1);
            }

            // 5. Physics on the physics step (FixedUpdateAsync).
            await Awaitable.FixedUpdateAsync(token);
            foreach (Collider hit in Physics.OverlapSphere(transform.position, blastRadius))
            {
                if (hit.attachedRigidbody != null)
                    hit.attachedRigidbody.AddExplosionForce(blastForce, transform.position, blastRadius);
            }

            // 6. After the whole frame is finished (EndOfFrameAsync).
            await Awaitable.EndOfFrameAsync(token);
            ScreenCapture.CaptureScreenshot("boom.png");

            // 7. Unity's own async operations can be awaited directly.
            await SceneManager.LoadSceneAsync("NextLevel");
        }
        catch (OperationCanceledException)
        {
            // Bomby was destroyed mid-wait. Stop quietly.
        }
    }

    async Awaitable<bool> BurnFuseAsync(CancellationToken token)
    {
        float timeLeft = fuseSeconds;

        while (timeLeft > 0f)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                return true; // defused!

            timeLeft -= Time.deltaTime;
            fuse.localScale = new Vector3(1f, timeLeft / fuseSeconds, 1f);

            await Awaitable.NextFrameAsync(token);
        }

        return false; // boom
    }

    async Awaitable<Vector3[]> CalculateSparksAsync(int count, Vector3 origin)
    {
        await Awaitable.BackgroundThreadAsync();

        // Background thread: plain C# math only. No transforms, no GameObjects,
        // no UnityEngine.Random. That's why origin is passed in.
        var random = new System.Random();
        var sparks = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            var direction = new Vector3(
                (float)random.NextDouble() * 2f - 1f,
                (float)random.NextDouble() * 2f - 1f,
                (float)random.NextDouble() * 2f - 1f);
            sparks[i] = origin + direction.normalized * (float)random.NextDouble() * blastRadius;
        }

        await Awaitable.MainThreadAsync();
        return sparks;
    }

    void OnDestroy()
    {
        // A completion source doesn't know about the token, so cancel it ourselves.
        lit.TrySetCanceled();
    }

    // Rule: Awaitables are pooled. Await each one only once.
    // DON'T:
    //   Awaitable wait = Awaitable.WaitForSecondsAsync(1f);
    //   await wait;
    //   await wait; // undefined behaviour
}
