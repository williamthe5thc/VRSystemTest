# TranscriptPanel Prefab Setup Instructions

## Overview
The TranscriptPanel prefab displays the user's speech transcript and the LLM's response in a clean, readable format.

## Creating the Prefab in Unity

1. In your Hierarchy, create a new GameObject under your Canvas called "TranscriptPanel"

2. Set its RectTransform properties:
   - Anchor Preset: Middle Center
   - Position: X=0, Y=100, Z=0
   - Width: 500
   - Height: 300

3. Add an Image component:
   - Color: Black with Alpha=0.8 (#000000CC)
   - Raycast Target: Enabled

4. Add a Vertical Layout Group component:
   - Padding: Left=10, Right=10, Top=10, Bottom=10
   - Child Alignment: Upper Center
   - Spacing: 10
   - Child Force Expand Width: True
   - Child Force Expand Height: False
   - Child Control Width: True
   - Child Control Height: True

5. Create a Header section:
   - Create a child GameObject called "Header"
   - Add a Horizontal Layout Group component
   - Add the following children:
     a. "TitleText" (TextMeshProUGUI): "Conversation Transcript"
     b. "DismissButton" (Button): "X" for closing the panel

6. Create User Transcript display:
   - Create a child GameObject called "UserTranscript"
   - Add TextMeshProUGUI component
   - Set Font Size: 16
   - Set Font Style: Normal
   - Set Color: White
   - Set Alignment: Top Left
   - Set Overflow: Overflow (to allow scrolling if needed)
   - Reference this in the UIManager's "userTranscriptText" field

7. Create LLM Response display:
   - Create a child GameObject called "LLMResponse"
   - Add TextMeshProUGUI component
   - Set Font Size: 16
   - Set Font Style: Normal
   - Set Color: Light Blue (#ADD8E6)
   - Set Alignment: Top Left
   - Set Overflow: Overflow (to allow scrolling if needed)
   - Reference this in the UIManager's "llmResponseText" field

8. For the DismissButton:
   - Add an onClick event pointing to the UIManager.HideTranscriptPanel() method
   - Reference this in the UIManager's "dismissTranscriptButton" field

## Adding to Your Scene

1. Connect the components in the Inspector:
   - Drag the button reference to the UIManager's "dismissTranscriptButton" field
   - Drag the TextMeshProUGUI components to "userTranscriptText" and "llmResponseText" fields
   - Set the TranscriptPanel reference in UIManager

2. Make sure the TranscriptPanel is initially disabled (unchecked in the Inspector)

3. Test by sending a message and confirming the display shows correctly
