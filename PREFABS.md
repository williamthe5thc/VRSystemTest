# VR Interview System - Prefab Setup Guide

This guide describes how to create the essential prefabs for the VR Interview System.

## Required Prefabs

1. **LoadingScreen**
2. **PersistentSystems**
3. **XRRig**
4. **Avatar**
5. **UI Elements**

## Prefab Creation Instructions

### 1. LoadingScreen Prefab

The LoadingScreen prefab is used for scene transitions.

1. **Create a new GameObject:**
   - Create a new empty GameObject
   - Name it "LoadingScreen"

2. **Add UI components:**
   ```
   LoadingScreen
   ├── Canvas (World Space)
   │   ├── Panel (Background)
   │   ├── Logo
   │   ├── LoadingText
   │   ├── ProgressBarBackground
   │   │   └── ProgressBarFill
   │   └── ProgressText
   ```

3. **Add LoadingScreen script:**
   - Add the `LoadingScreen.cs` script to the root GameObject
   - Configure the references:
     - ProgressBar: Assign the ProgressBarFill Image
     - LoadingText: Assign the LoadingText TMP component
     - ProgressText: Assign the ProgressText TMP component
   - Set loading messages in the array (e.g., "Setting up virtual interview...", "Connecting to interview server...", etc.)

4. **Create the prefab:**
   - Drag the GameObject from the Hierarchy to the Prefabs folder

### 2. PersistentSystems Prefab

The PersistentSystems prefab contains core systems that persist between scenes.

1. **Create a new GameObject:**
   - Create a new empty GameObject
   - Name it "PersistentSystems"

2. **Add child components:**
   ```
   PersistentSystems
   ├── AppManager
   ├── SettingsManager
   └── AudioListener
   ```

3. **Add scripts:**
   - Add `AppManager.cs` to the AppManager GameObject
   - Add `SettingsManager.cs` to the SettingsManager GameObject

4. **Configure scripts:**
   - In AppManager, set references to the SettingsManager
   - Configure default settings in the SettingsManager

5. **Create the prefab:**
   - Drag the GameObject from the Hierarchy to the Prefabs folder

### 3. XRRig Prefab

The XRRig prefab handles VR tracking and interactions.

1. **Create using XR Origin:**
   - Right-click in the Hierarchy and select `XR > XR Origin (VR)`
   - This creates the standard XR Rig structure

2. **Configure components:**
   ```
   XRRig
   ├── Camera Offset
   │   └── Main Camera (with Hand tracking)
   ├── LeftHand Controller
   │   └── Left Ray Interactor
   └── RightHand Controller
       └── Right Ray Interactor
   ```

3. **Add scripts:**
   - Add `VRRigSetup.cs` to the XRRig root GameObject
   - Add `VRInputHandler.cs` to the XRRig root GameObject

4. **Configure controllers:**
   - Set up action references for controllers
   - Configure interaction layers
   - Set up teleportation if needed

5. **Create the prefab:**
   - Drag the GameObject from the Hierarchy to the Prefabs folder

### 4. Avatar Prefab

The Avatar prefab represents the interviewer.

1. **Import a 3D model:**
   - Import a humanoid avatar model
   - Configure the rig as "Humanoid" in import settings

2. **Setup the hierarchy:**
   ```
   Avatar
   ├── Model
   │   └── (Imported character mesh/rig)
   ├── AnimationController
   ├── IKTargets
   │   ├── HeadTarget
   │   ├── LeftHandTarget
   │   └── RightHandTarget
   └── LipSyncVisualizer (optional debug)
   ```

3. **Add scripts:**
   - Add `AvatarController.cs` to the Avatar root GameObject
   - Add `LipSync.cs` to the Avatar root GameObject
   - Add `FacialExpressions.cs` to the Avatar root GameObject
   - Add `GestureSystem.cs` to the Avatar root GameObject

4. **Configure the Animator:**
   - Create an Animator Controller with states for:
     - Idle
     - Listening
     - Thinking
     - Speaking
     - Gesturing
   - Set up transitions between states

5. **Configure the scripts:**
   - Set up blend shape indices for lip-sync
   - Configure facial expression mappings
   - Set up gesture animations

6. **Create the prefab:**
   - Drag the GameObject from the Hierarchy to the Prefabs folder

### 5. UI Element Prefabs

Create prefabs for common UI elements used across scenes.

#### 5.1 StatePanel Prefab

1. **Create a new GameObject:**
   - Create a new empty GameObject
   - Name it "StatePanel"

2. **Add UI components:**
   ```
   StatePanel
   ├── Canvas (World Space)
   │   ├── Background
   │   ├── StateIcon
   │   ├── StateText
   │   └── AudioLevelIndicator
   ```

3. **Create the prefab:**
   - Drag the GameObject from the Hierarchy to the Prefabs folder

#### 5.2 DebugPanel Prefab

1. **Create a new GameObject:**
   - Create a new empty GameObject
   - Name it "DebugPanel"

2. **Add UI components:**
   ```
   DebugPanel
   ├── Canvas (World Space)
   │   ├── Background
   │   ├── TitleText
   │   ├── DebugText
   │   ├── ScrollView (for logs)
   │   └── ToggleButton
   ```

3. **Add the DebugDisplay script:**
   - Add `DebugDisplay.cs` to the DebugPanel GameObject
   - Configure the references to the DebugText

4. **Create the prefab:**
   - Drag the GameObject from the Hierarchy to the Prefabs folder

#### 5.3 VRMenu Prefab

1. **Create a new GameObject:**
   - Create a new empty GameObject
   - Name it "VRMenu"

2. **Add UI components:**
   ```
   VRMenu
   ├── Canvas (World Space)
   │   ├── Background
   │   ├── TitleText
   │   ├── PauseButton
   │   ├── ResumeButton
   │   ├── RestartButton
   │   ├── FeedbackButton
   │   └── ExitButton
   ```

3. **Add the VRInteractionUI script:**
   - Add `VRInteractionUI.cs` to the VRMenu GameObject
   - Configure button references

4. **Create the prefab:**
   - Drag the GameObject from the Hierarchy to the Prefabs folder

## Using the Prefabs

After creating these prefabs, they can be used in your scenes:

1. **In SceneInitializer:**
   - Assign the LoadingScreen prefab to the loadingScreenPrefab field
   - Assign the PersistentSystems prefab to the persistentSystemsPrefab field

2. **In All Scenes:**
   - Drag the XRRig prefab into the scene
   - Configure any scene-specific settings

3. **In Interview Scenes:**
   - Drag the Avatar prefab into the scene
   - Position it at an appropriate distance and height
   - Configure it to face the user's starting position

4. **UI Setup:**
   - Add the appropriate UI prefabs to each scene
   - Configure their positions and visibility states

## Testing Prefabs

Before finalizing prefabs, it's a good practice to test them in isolation:

1. **Create a test scene** with minimal objects
2. **Add the prefab** to test
3. **Run the scene** and verify functionality
4. **Adjust the prefab** as needed
5. **Apply prefab changes** when satisfied

## Prefab Variants

For different environments or avatar types, consider using Prefab Variants:

1. **Create the base prefab** first
2. **Right-click** on the prefab in the Project window
3. Select **Create > Prefab Variant**
4. Customize the variant for specific needs

## Avatar Variants

Consider creating multiple avatar variants for different interview styles:

1. **Corporate Interviewer**
   - Professional appearance
   - Formal animations
   - Structured feedback style

2. **Startup Interviewer**
   - Casual appearance
   - More relaxed animations
   - Conversational feedback style

3. **Technical Interviewer**
   - Professional but technical-focused appearance
   - More analytical gestures
   - Technical feedback focus

## Environment Integration

When adding prefabs to your environment scenes:

1. **Position the Avatar** at an appropriate distance (1.5-2m from user)
2. **Align the XRRig** with the intended starting position
3. **Position UI elements** at comfortable viewing distances and angles
4. **Configure lighting** to highlight the avatar appropriately
5. **Test in VR** to ensure comfortable scale and positioning

## Performance Considerations

When creating prefabs, keep performance in mind for the Oculus Quest:

1. **Avatar Optimization:**
   - Keep poly count under 50k triangles for the avatar
   - Use a single material where possible
   - Limit blend shape count to essential expressions

2. **UI Optimization:**
   - Use sprite atlases for UI textures
   - Keep world-space UI elements simple
   - Use efficient text rendering (TextMeshPro)

3. **Scene Loading:**
   - Consider asynchronous loading for large prefabs
   - Use LOD (Level of Detail) for complex prefabs
   - Instance shared prefabs rather than duplicating

## Animation Setup

The avatar prefab requires proper animation setup:

1. **Create an Animator Controller** (AvatarAnimator.controller)
2. **Define Animation States:**
   - Idle
   - Listening
   - Processing
   - Speaking
   - Various gestures

3. **Create Transitions** between states with appropriate conditions
4. **Set Up Parameters:**
   - isListening (bool)
   - isProcessing (bool)
   - isSpeaking (bool)
   - gestureType (int)

5. **Configure Blend Trees** for smooth transitions between animations

## Lip Sync Setup

The avatar's lip sync system needs proper configuration:

1. **Identify Blend Shapes** on your avatar model:
   - Jaw_Open
   - Mouth_Wide
   - Mouth_Narrow
   - Etc.

2. **Map Blend Shapes** to phonemes in the LipSync component
3. **Test with sample audio** to verify the mapping works correctly
4. **Adjust Sensitivity** for natural-looking speech

## Folder Organization

Organize your prefabs in a structured folder hierarchy:

```
Assets/
├── Prefabs/
│   ├── Core/
│   │   ├── LoadingScreen.prefab
│   │   ├── PersistentSystems.prefab
│   │   └── XRRig.prefab
│   ├── Avatars/
│   │   ├── CorporateInterviewer.prefab
│   │   ├── StartupInterviewer.prefab
│   │   └── TechnicalInterviewer.prefab
│   ├── Environments/
│   │   ├── CorporateOfficeProps.prefab
│   │   ├── StartupOfficeProps.prefab
│   │   └── CasualOfficeProps.prefab
│   └── UI/
│       ├── StatePanel.prefab
│       ├── DebugPanel.prefab
│       ├── VRMenu.prefab
│       └── FeedbackPanel.prefab
```

This organization makes it easier to manage and locate prefabs as the project grows.
