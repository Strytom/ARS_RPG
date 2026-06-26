# Project Overview
- **Game Title**: ARS RPG Prototype
- **High-Level Concept**: A third-person action RPG prototype demonstrating responsive non-targeting character movement, standard light and heavy attacks, and robust health/damage systems with direct combat feedback in the style of Dragon Nest.
- **Players**: Single player
- **Inspiration / Reference Games**: Dragon Nest (smooth non-targeting action combat, mouse-driven orbit camera, combat lock)
- **Tone / Art Direction**: Stylized prototype using clean primitives and standard URP materials.
- **Target Platform**: PC (Standalone)
- **Screen Orientation / Resolution**: Landscape 1920x1080
- **Render Pipeline**: Universal Render Pipeline (URP)

# Game Mechanics
## Core Gameplay Loop
1. **Explore**: Move the character in a test arena with fluid, camera-relative third-person WASD controls and mouse-controlled orbit view.
2. **Combat**: Engage static target dummies using combo attacks. Deliver fast **Light Attacks** (Left Click) and powerful **Heavy Attacks** (Right Click).
3. **Hazard Survival**: Avoid active traps/hazards in the arena to preserve Health (HP), with visual damage feedback (screen flash, player pushback/invincibility frames).

## Controls and Input Methods (Dragon Nest Style)
- **WASD / Left Stick**: Move character relative to camera look direction.
- **Mouse Delta**: Orbit camera around player character. The mouse cursor is locked to the screen center during gameplay to act as a target reticle.
- **Left Click (LMB)**: Light Attack (Trigger fast attack combo/slash).
- **Right Click (RMB)**: Heavy Attack (Trigger slower, heavy swing dealing higher damage and larger knockback).
- **H Key**: Test damage key (simulates taking 15 points of damage to test health UI).
- **Escape Key**: Toggle mouse cursor lock/unlock.

# UI
- **Crosshair / Reticle**: A clean crosshair placed at the dead-center of the screen.
- **Health Bar (HUD)**: A stylized red health bar (Slider / Image Fill) in the top-left corner displaying `Current HP / Max HP`.
- **Damage Numbers / Floating Text**: Temporary text popups spawning above targets when hit to indicate damage dealt, or red screen flash overlay when player takes damage.

# Key Asset & Context

### 1. `ThirdPersonCamera.cs` (New Script)
A robust follow-orbit camera script. Locks the cursor and orbits the player using mouse axis movement. Avoids clipping using simple sphere casting or raycasting.
```csharp
public class ThirdPersonCamera : MonoBehaviour {
    public Transform target;
    public float distance = 4.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 80.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    // ...
}
```

### 2. `Player_Controller.cs` (Modify Script)
Handles:
- Ground-relative WASD movement mapped to camera orientation.
- RigidBody physics for smooth motion without jitter.
- Rotation facing direction: turns to match running direction, but snaps to face camera look direction during attacks.
- Input Action integration for Movement, Camera Look, Light/Heavy Attack.

### 3. `Health.cs` (New Script)
Manages HP, invincibility frames (i-frames), taking damage, and visual recovery flash.
```csharp
public class Health : MonoBehaviour, IDamageable {
    public float maxHP = 100f;
    public float currentHP;
    public float invincibilityDuration = 0.5f;
    // ...
    public void TakeDamage(float amount, Vector3 pushDirection);
}
```

### 4. `IDamageable.cs` (New Script)
A simple, reusable interface for any object that can receive combat damage:
```csharp
public interface IDamageable {
    void TakeDamage(float amount, Vector3 pushDirection);
}
```

### 5. `CombatDummy.cs` (New Script)
A target dummy script that implements `IDamageable`. When hit, it plays a hit effect, spawns floating damage numbers, and triggers a slight wobble or shake.

### 6. `DamageHazard.cs` (New Script)
An obstacle/hazard in the scene (e.g., spinning red blade or damage field) that applies damage to any overlapping `IDamageable` object with a tag "Player".

# Implementation Steps

## Step 1: Input Setup
- **Description**: Add `HeavyAttack` (Right Mouse Button) and `TestDamage` (H Key) actions to the `InputSystem_Actions.inputactions` file. Regenerate the `InputSystem_Actions.cs` C# class wrapper.
- **Assigned Role**: developer
- **Dependencies**: None
- **Parallelizable**: No

## Step 2: Camera System
- **Description**: Create `ThirdPersonCamera.cs`. Attach it to the Main Camera. Configure it to follow the player, rotate around the player with Mouse Look inputs, and lock/unlock the cursor with Escape.
- **Assigned Role**: developer
- **Dependencies**: None
- **Parallelizable**: Yes

## Step 3: Character Movement
- **Description**: Refactor `Player_Controller.cs` to handle:
  - Third-person WASD movement (relative to Camera forward/right vectors).
  - Rotation toward movement direction.
  - Integration of Rigidbody physics.
- **Assigned Role**: developer
- **Dependencies**: Step 2
- **Parallelizable**: No

## Step 4: Health & Damage Interface
- **Description**: Create `IDamageable.cs` and `Health.cs`. Attach `Health.cs` to the player character.
  - Implement HP decrease, invincibility timer (i-frames) after taking damage, and character material flashing (red) as damage feedback.
  - Bind `H Key` to deal test damage of 15 to the player for instant testing.
- **Assigned Role**: developer
- **Dependencies**: Step 3
- **Parallelizable**: Yes

## Step 5: Combat & Attacks
- **Description**: Expand `Player_Controller.cs` to perform attacks:
  - **Light Attack (LMB)**: Instantly rotates the character to face camera look direction. Triggers a short spherecast/raycast forward. Displays a temporary white slash visual placeholder (Trail or temporary expandable swing arc). Deals 10 damage.
  - **Heavy Attack (RMB)**: Slower windup, rotates to look direction, performs a longer-range spherecast. Displays a red swing placeholder. Deals 25 damage and applies a larger knockback.
- **Assigned Role**: developer
- **Dependencies**: Step 3, Step 4
- **Parallelizable**: No

## Step 6: Target Dummy & Hazard Setup
- **Description**: Create `CombatDummy.cs` and `DamageHazard.cs`. 
  - Create a Combat Dummy prefab (e.g., capsule with a visual shield/target indicator) that shows floating hit numbers or text popups.
  - Create a Hazard prefab (e.g., a rotating red bar) that deals periodic damage to the player if they touch it.
- **Assigned Role**: developer
- **Dependencies**: Step 4, Step 5
- **Parallelizable**: Yes

## Step 7: UI HUD & Level Integration
- **Description**: Build a clean Canvas UI:
  - Center Reticle (crosshair).
  - Slider Health Bar linked to the player's `Health.cs` events (using standard Action subscription pattern).
  - Set up a test arena in `SampleScene` containing the Player, Main Camera, some ground terrain, multiple Combat Dummies, and a Damage Hazard.
- **Assigned Role**: developer
- **Dependencies**: Step 2, Step 3, Step 4, Step 5, Step 6
- **Parallelizable**: No

# Verification & Testing
- **Movement Check**: Verify the player moves smoothly with WASD in the direction the camera faces. Verify the mouse rotates the camera smoothly, and the cursor locks properly.
- **Light Attack Test**: Attack a dummy with LMB. Verify the dummy registers 10 damage, wobbles, and spawns a floating text of "10". Verify the player snaps to look forward before attacking.
- **Heavy Attack Test**: Attack a dummy with RMB. Verify the dummy registers 25 damage, receives a larger knockback push, and spawns "25" text.
- **Damage Test**: Walk into the hazard or press `H`. Verify the player takes damage, health bar decreases, screen flashes, and player is temporarily invincible (flashes red/transparent) for 0.5s.
- **No Console Errors**: Ensure compilation is 100% clean and zero NullReferenceExceptions are logged at runtime.
