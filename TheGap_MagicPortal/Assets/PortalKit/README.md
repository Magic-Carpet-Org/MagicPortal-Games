# Kilimanjaro PortalKit

One drop-in folder that connects a Unity WebGL game to every portal we ship on.
The game makes two calls; PortalKit works out which portal it's running on.

| Portal | How it's detected | What it does |
|---|---|---|
| MagicGames | `?sessionId=` in the URL | postMessage bridge + signed REST (init, score) |
| Lagos Life | inside an iframe and `Vatar.init()` succeeds | Vatar SDK: score, leaderboard, pause/resume |
| None | anything else (Editor, direct link) | logs only |

## Folder layout

```
Assets/PortalKit/
  README.md
  Runtime/
    IGamePortal.cs
    PortalManager.cs
    PortalSettings.cs
    NullPortal.cs
    MagicGames/MagicGamesBridge.cs
    MagicGames/MagicGamesApi.cs
    MagicGames/MagicGamesPortal.cs
    LagosLife/LagosLifeBridge.cs
    LagosLife/LagosLifePortal.cs
  Plugins/WebGL/
    PortalKit_MagicGames.jslib
    PortalKit_LagosLife.jslib
```

## Adding it to a game

1. Copy the `PortalKit` folder into `Assets/`.
2. Remove any older portal code from the game (old `PortalBridge`, `ApiManager`,
   `.jslib` files, or a previous `Portals` folder), and delete any
   `PortalBridge` GameObject from scenes. Two copies will clash.
3. Create the settings asset: **Assets > Create > Kilimanjaro > Portal Settings**.
   Name it `PortalSettings` and put it in a `Resources` folder.
   Fill in this game's MagicGames webhook secret.
4. In the game code, add `using Kilimanjaro.Portals;` and make two calls:

   ```csharp
   // While playing (safe every frame - PortalKit throttles it):
   PortalManager.ReportProgress(currentScore);

   // Once, when the run ends:
   PortalManager.GameOver(finalScore);
   ```

Nothing needs to be added to scenes. PortalKit creates its own objects at startup.

### Loading screen and Play Again

If the game has a splash/loading screen that should wait for the portal, or a
Play Again button that reloads the scene:

```csharp
// Splash: hold the loading screen until the portal is known and the session is checked.
PortalManager.WhenReady(ok => { if (ok) StartGame(); else ShowNetworkError(); });

// Play Again: on MagicGames this waits for the new session (usually already
// fetched by GameOver); on Lagos Life / no portal it runs straight away.
PortalManager.PlayAgain(() => SceneManager.LoadSceneAsync("Game"));
```

Don't read `sessionId` from the URL or call portal APIs from game code - PortalKit owns that.

### Testing MagicGames from the Editor

Put a real MagicGames session id in `PortalSettings > Editor Test Session Id`.
Play mode then uses it for init, score submission and simulated restarts.
Leave it empty to run the Editor with no portal session. It is never used in builds.

### Optional

```csharp
PortalManager.RequestLeaderboard(entries => { /* null if the portal has none */ });
PortalManager.PauseChanged += paused => { /* show/hide a pause overlay */ };
PortalManager.Current.Player   // id / name / avatar, or null
```

Pause and resume already freeze `Time.timeScale` and mute `AudioListener`.
If the game uses unscaled time (`Time.unscaledDeltaTime`, UI tweens), stop
those yourself in `PauseChanged`.

## Hosting

The host must allow both portals to frame the game, and must **not** send
`X-Frame-Options`:

```
Content-Security-Policy: frame-ancestors https://game-portal-cyan.vercel.app https://lagoslife.app https://www.lagoslife.app https://naijalife.app https://www.naijalife.app https://lagoslife.eliysites.com
```

On Vercel (`vercel.json`):

```json
{ "headers": [{ "source": "/(.*)", "headers": [{ "key": "Content-Security-Policy",
  "value": "frame-ancestors https://game-portal-cyan.vercel.app https://lagoslife.app https://www.lagoslife.app https://naijalife.app https://www.naijalife.app https://lagoslife.eliysites.com" }] }] }
```

## Per-game checklist

- [ ] Editor: console shows `[PortalManager] Portal: MagicGames` and `Not on Lagos Life (not_webgl)`
- [ ] MagicGames build: `Portal: MagicGames`, session init OK, score submitted after a run
- [ ] Lagos Life sandbox (`/developers/sandbox/<draft>`): `Connected (test)`, `Portal: LagosLife`, `Score sent`
- [ ] Pause/resume from the Lagos Life player freezes and resumes the game
- [ ] Works at 375 x 812 portrait with touch
- [ ] Sound starts only after the first tap
- [ ] Loads quickly on mobile data (Brotli/Gzip on, small build)
- [ ] Lagos Life listing: 512 px square icon, 3-5 screenshots, description up to 120 characters

## Notes

- Lagos Life accepts one score every 20 s per player and keeps the best. If a
  run ends sooner, PortalKit holds the best score and sends it when allowed.
- No Lagos Life secret is ever in the build. Lagos Life **purchases** need a
  server to verify and deliver them; PortalKit doesn't do purchases yet.
- The MagicGames webhook secret in `PortalSettings` does ship inside the WebGL
  build, so a player can forge scores. Moving that REST call to a server fixes it.

## Adding another portal

Write a class that implements `IGamePortal` (plus a `.jslib` if it needs
JavaScript), then add its detection to `PortalManager.Detect()`. Games don't change.
