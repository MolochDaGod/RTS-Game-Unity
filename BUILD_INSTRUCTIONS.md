# 🎮 Build Instructions for Unity WebGL

## ⚠️ IMPORTANT: Unity is Currently Running

Multiple Unity instances are detected. You have **two options** to build:

---

## ✅ Option 1: Manual Build in Unity Editor (EASIEST)

Since Unity is already open, use this method:

### Steps:
1. **In Unity Editor**, go to the top menu
2. Click **Build → Build WebGL**
   - This custom menu item was just created for you
   - OR use: **File → Build Settings**
3. If using Build Settings:
   - Select **WebGL** platform
   - Click **"Switch Platform"** if not already selected
   - Click **"Build"**
   - Choose folder: `E:\GrudgeDefense\RTS-Game-master\RTS-Game-master\Build`
4. Wait 10-30 minutes for the build to complete
5. Unity will create the `Build` folder with all WebGL files

### After Build Completes:
Run these commands in PowerShell:
```powershell
cd "E:\GrudgeDefense\RTS-Game-master\RTS-Game-master"
git add Build/
git commit -m "Add WebGL build for Vercel deployment

Co-Authored-By: Warp <agent@warp.dev>"
git push origin master
```

---

## Option 2: Automated Build (Close Unity First)

### Steps:
1. **Close ALL Unity Editor windows**
2. Run this command in PowerShell:
   ```powershell
   cd "E:\GrudgeDefense\RTS-Game-master\RTS-Game-master"
   .\build-webgl.ps1
   ```
3. Wait for the automated build to complete

---

## 🚀 After Build is Complete

Once the `Build` folder exists with `index.html`, follow these steps:

### 1. Verify Build
```powershell
# Check if build was successful
Test-Path "E:\GrudgeDefense\RTS-Game-master\RTS-Game-master\Build\index.html"
# Should return: True
```

### 2. Push to GitHub
```powershell
git add Build/
git add Assets/Editor/WebGLBuilder.cs
git add Assets/Editor/WebGLBuilder.cs.meta
git add build-webgl.ps1
git commit -m "Add WebGL build and build scripts

Co-Authored-By: Warp <agent@warp.dev>"
git push origin master
```

### 3. Deploy to Vercel

**Via Vercel Website (Recommended):**
1. Go to [https://vercel.com](https://vercel.com)
2. Click **"Sign in with GitHub"**
3. Click **"Add New Project"**
4. Find **"RTS-Game-Unity"** in your repository list
5. Click **"Import"**
6. Vercel will automatically detect `vercel.json` configuration
7. Click **"Deploy"**
8. Wait 2-5 minutes

**Via Vercel CLI:**
```powershell
npm install -g vercel
vercel login
vercel --prod
```

### 4. Access Your Game
After deployment, you'll get a URL like:
- `https://rts-game-unity.vercel.app`
- Or `https://rts-game-unity-yourusername.vercel.app`

---

## 📊 Build Information

- **Unity Version**: 2021.3.42f1
- **Target Platform**: WebGL
- **Compression**: Gzip
- **Expected Build Size**: 100-500 MB
- **Build Time**: 10-30 minutes (depending on your system)

---

## 🔧 Troubleshooting

### Build Fails in Unity
- Check **Console** window for errors
- Ensure **WebGL Build Support** is installed via Unity Hub
- Try **File → Build Settings → Player Settings** and adjust memory settings

### "Multiple Unity Instances" Error
- Close all Unity windows
- Check Task Manager for `Unity.exe` processes
- Kill any remaining Unity processes:
  ```powershell
  Get-Process -Name Unity | Stop-Process -Force
  ```

### Vercel Deployment Fails
- Ensure `Build/index.html` exists
- Check file size (Vercel free tier has 100MB limit per file)
- Check Vercel deployment logs

---

## 📚 Files Created

- `Assets/Editor/WebGLBuilder.cs` - Unity build script
- `build-webgl.ps1` - PowerShell automation script
- `vercel.json` - Vercel configuration
- `DEPLOYMENT.md` - Detailed deployment guide

---

**Repository**: https://github.com/MolochDaGod/RTS-Game-Unity
**Current Status**: Ready for build → Push build → Deploy to Vercel
