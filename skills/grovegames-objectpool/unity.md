## Unity

- Pool GameObjects with `GameObjectPool` and components with `ComponentPool<T>`. They activate on rent and deactivate on return; do not call `SetActive` yourself.
- Never `Destroy` a pooled object; return it. Never `Instantiate` what a pool can rent.
- An asmdef that overrides references lists `GroveGames.ObjectPool.dll` in its precompiled references.
