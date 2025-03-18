# Git Repository Setup Guide

This document outlines the steps to properly set up and use the git repository for the VR Interview System Unity client.

## Repository Setup

The git repository has already been initialized. To complete the setup:

1. **Configure User Information**
   ```bash
   git config user.name "Your Name"
   git config user.email "your.email@example.com"
   ```

2. **Add Remote Repository**
   ```bash
   # For GitHub
   git remote add origin https://github.com/yourusername/vr-interview-system-client.git
   # For GitLab
   git remote add origin https://gitlab.com/yourusername/vr-interview-system-client.git
   # For Azure DevOps
   git remote add origin https://dev.azure.com/yourusername/vr-interview-system/_git/client
   ```

3. **Verify Remote Setup**
   ```bash
   git remote -v
   ```

## Initial Commit

To create your initial commit:

1. **Check Status**
   ```bash
   git status
   ```

2. **Add Files to Staging**
   ```bash
   git add .
   ```

3. **Commit Changes**
   ```bash
   git commit -m "Initial commit of VR Interview System Unity client"
   ```

4. **Push to Remote**
   ```bash
   git push -u origin main
   ```

## Working with Large Files

Unity projects often contain large binary files. Consider using Git LFS (Large File Storage) for handling these files:

1. **Install Git LFS**
   ```bash
   git lfs install
   ```

2. **Track Large File Types**
   ```bash
   git lfs track "*.fbx"
   git lfs track "*.png"
   git lfs track "*.jpg"
   git lfs track "*.wav"
   git lfs track "*.mp3"
   git lfs track "*.asset"
   ```

3. **Commit .gitattributes**
   ```bash
   git add .gitattributes
   git commit -m "Add Git LFS tracking for large files"
   ```

## Branch Strategy

Consider using the following branch strategy:

- **main**: Stable production releases
- **develop**: Active development branch
- **feature/name**: New features
- **bugfix/name**: Bug fixes
- **release/version**: Release preparation

## Unity-Specific Considerations

1. **Scene and Prefab Merging**
   - Use Unity Smart Merge for handling scene and prefab conflicts
   - Configure git to use the UnityYAMLMerge tool

2. **Meta Files**
   - Always commit .meta files alongside their corresponding assets
   - Never ignore .meta files as they contain important Unity references

3. **Project Version Control Settings**
   - Ensure Unity is set to "Visible Meta Files" and "Force Text" in Editor Settings
   - This makes version control with git much more effective

## Workflow Recommendations

1. **Pull Before Push**
   Always pull changes before pushing your own:
   ```bash
   git pull origin main
   ```

2. **Regular Commits**
   Make small, focused commits with descriptive messages

3. **Code Reviews**
   Use pull/merge requests for code reviews before merging into main branches

4. **Unity Project Backup**
   Consider making a backup copy of your project before major git operations
