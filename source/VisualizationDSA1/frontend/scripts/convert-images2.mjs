import sharp from 'sharp';
import { glob } from 'glob';
import { stat } from 'fs/promises';

async function convertToWebP() {
  const inputDirs = [
    'tailieu/images'
  ];

  let totalSaved = 0;
  let totalFiles = 0;

  for (const dir of inputDirs) {
    const files = await glob(`${dir}/*.{png,jpg,jpeg}`, { absolute: true });
    
    for (const file of files) {
      const webpPath = file.replace(/\.(png|jpg|jpeg)$/i, '.webp');
      
      try {
        const originalStat = await stat(file);
        const originalSize = originalStat.size;
        
        await sharp(file)
          .webp({ quality: 80, effort: 6 })
          .toFile(webpPath);
        
        const webpStat = await stat(webpPath);
        const webpSize = webpStat.size;
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