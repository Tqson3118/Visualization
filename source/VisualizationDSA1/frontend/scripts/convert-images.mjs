import sharp from 'sharp';
import { glob } from 'glob';
import { resolve } from 'path';

async function convertToWebP() {
  const inputDirs = [
    'public/assets/avatars',
    'public/assets/frames',
    'tailieu/images'
  ];

  let totalSaved = 0;
  let totalFiles = 0;

  for (const dir of inputDirs) {
    const files = await glob(`${dir}/*.{png,jpg,jpeg}`, { absolute: true });
    
    for (const file of files) {
      const webpPath = file.replace(/\.(png|jpg|jpeg)$/i, '.webp');
      
      try {
        const stats = await sharp(file).toFile(webpPath, {
          quality: 80,
          effort: 6
        });
        
        const originalSize = (await import('fs')).promises.stat(file).then(s => s.size);
        const webpSize = (await import('fs')).promises.stat(webpPath).then(s => s.size);
        const saved = originalSize - webpSize;
        const savedPercent = ((saved / originalSize) * 100).toFixed(1);
        
        console.log(`✓ ${file} → ${webpPath} (${(originalSize/1024).toFixed(1)}KB → ${(webpSize/1024).toFixed(1)}KB, saved ${savedPercent}%)`);
        totalSaved += saved;
        totalFiles++;
      } catch (err) {
        console.error(`✗ Failed to convert ${file}:`, err.message);
      }
    }
  }
  
  console.log(`\n=== Summary ===`);
  console.log(`Files converted: ${totalFiles}`);
  console.log(`Total space saved: ${(totalSaved/1024/1024).toFixed(2)} MB`);
}

convertToWebP().catch(console.error);