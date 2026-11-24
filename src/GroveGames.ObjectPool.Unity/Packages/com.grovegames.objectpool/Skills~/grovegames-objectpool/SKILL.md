---
name: grovegames-objectpool
description: Rules for pooling objects and collections in game code with GroveGames.ObjectPool. Use when writing or reviewing code that creates and destroys objects repeatedly, needs a temporary list or dictionary, or spawns and despawns GameObjects.
---

# Using GroveGames.ObjectPool

Rules for game code that uses this package. They hold in every game. The API is in the package's `README.md`.

- Pool anything that is created and discarded repeatedly during play. Do not pool what is created once.
- Give every pool a maximum size and a return callback that resets the object, so a rented object never carries old state.
- Return every rented object exactly once, and never use it after returning it.
- For a temporary collection, rent it with `using (pool.Rent(out var items))` so it is returned on every path.
- Use the plain pools on the main thread. Use the `Concurrent` pools only when several threads share the pool.
- Use `KeyedObjectPool` or `IndexedObjectPool` when one kind of object has several variants, instead of a dictionary of pools.
- Dispose a pool in `Dispose` of the class that created it.

## Unity

- Pool GameObjects with `GameObjectPool` and components with `ComponentPool<T>`. They activate on rent and deactivate on return; do not call `SetActive` yourself.
- Never `Destroy` a pooled object; return it. Never `Instantiate` what a pool can rent.
- An asmdef that overrides references lists `GroveGames.ObjectPool.dll` in its precompiled references.
