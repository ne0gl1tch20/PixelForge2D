# PixelForge2D 🎮

A small, touch-first Unity 2D Android game scaffold built to be managed from GitHub.

## Game

Move the little explorer with your finger, collect cyan shards and dodge falling hazards. The game is generated from C# so the starter project does not depend on imported art assets.

## Project

- Unity 6 (6000.0) project layout
- Android portrait build target
- Runtime-generated 2D game scene and simple UI
- GitHub Actions Android debug APK build on pushes to `main` and manual runs
- APK artifact retention: 7 days

## APK build setup

GitHub Actions needs a Unity Personal or Plus/Pro license configured for GameCI. Add the license XML as the repository Actions secret `UNITY_LICENSE`. GameCI may also require `UNITY_EMAIL` and `UNITY_PASSWORD` depending on the license setup. Never commit license files or credentials.

Open **Actions → Android Debug APK** to view builds. Successful runs upload `PixelForge2D-debug.apk` as an artifact for seven days.

## Local editing

Open this folder with Unity Hub using Unity **6000.0 LTS** with Android Build Support, Android SDK & NDK Tools, and OpenJDK installed.

This is a starter scaffold, not a published or signed release build.
