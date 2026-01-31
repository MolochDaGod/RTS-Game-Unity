# Deployment Instructions

## Building for Web

1. Open the project in Unity 2021.3.42f1
2. Go to **File → Build Settings**
3. Select **WebGL** platform and click "Switch Platform"
4. Click **Player Settings** and configure:
   - **Compression Format**: Gzip or Brotli
   - **Publishing Settings → Decompression Fallback**: Enable if needed
5. Click **Build** and select the `Build` folder
6. Wait for Unity to complete the build

## Deploying to Vercel

### Via GitHub (Recommended)

1. Push your code to GitHub:
   ```bash
   git add .
   git commit -m "Initial commit with Unity WebGL build"
   git push origin main
   ```

2. Go to [vercel.com](https://vercel.com) and sign in with GitHub
3. Click "Add New Project"
4. Import your GitHub repository
5. Vercel will auto-detect the configuration from `vercel.json`
6. Click "Deploy"

### Via Vercel CLI

```bash
npm install -g vercel
vercel login
vercel
```

## Post-Deployment

Your Unity game will be available at: `https://your-project.vercel.app`

Note: WebGL builds can be large. Ensure your build is optimized for web delivery.
