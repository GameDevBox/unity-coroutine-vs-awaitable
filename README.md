# Coroutine vs Awaitable in Unity 6

The two bomb scripts from the GameDevBox video **"Coroutine vs Awaitable in Unity 6: Which One Should You Use?"**

▶️ Watch the video: https://youtu.be/VIDEO_ID

The same bomb is written twice so you can compare them round by round:

| Script | What it shows |
| --- | --- |
| [`BombCoroutine.cs`](BombCoroutine.cs) | The **red corner**: `IEnumerator`, `yield return`, `StartCoroutine`, `WaitUntil`, a callback for the result |
| [`BombAwaitable.cs`](BombAwaitable.cs) | The **blue corner**: `async Awaitable`, `WaitForSecondsAsync`, `NextFrameAsync`, `FixedUpdateAsync`, `EndOfFrameAsync`, `BackgroundThreadAsync` / `MainThreadAsync`, `Awaitable<bool>`, `AwaitableCompletionSource`, and `destroyCancellationToken` |

## Quick comparison

| | Coroutine | Awaitable |
| --- | --- | --- |
| Start it | `StartCoroutine(Routine())` | just call it (even `Start` can be `async`) |
| Wait seconds | `yield return new WaitForSeconds(1f)` | `await Awaitable.WaitForSecondsAsync(1f)` |
| Wait one frame | `yield return null` | `await Awaitable.NextFrameAsync()` |
| Return a value | callback or field | `async Awaitable<bool>` |
| Wait for an event | `WaitUntil` (checks every frame) | `AwaitableCompletionSource` |
| Heavy work off the main thread | no | `BackgroundThreadAsync` → `MainThreadAsync` |
| Object destroyed | stops automatically | keeps running, so pass `destroyCancellationToken` |
| `try`/`catch` around the wait | not allowed (CS1626) | works normally |
| Unity version | any | 2023.1+ / Unity 6 |

**Rule of thumb:** use a coroutine for small loops that live and die with one object (blink, fade, patrol). Use Awaitable when you need a result, an event, a background thread or a real try-catch, and always pass `destroyCancellationToken` inside a MonoBehaviour.

## Requirements

- Unity 6 (or 2023.1+) for `BombAwaitable.cs`. `BombCoroutine.cs` works in any version.
- The **Input System** package for the E-to-defuse key (`Keyboard.current`). It's the default in new Unity 6 projects.
- Assign the serialized fields in the Inspector: the fuse transform, the explosion prefab and the spark particles. `BombAwaitable` loads a scene named `NextLevel` at the end, so rename it to one of yours.

## License

MIT. Use these in your own projects.

---

More Unity tutorials on [GameDevBox](https://www.youtube.com/channel/UCgXs2PTiL19Rv1qOn1SI7XQ?sub_confirmation=1).
