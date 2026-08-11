import sharp from 'sharp';
import { stat } from 'fs/promises';
import { readdir } from 'fs/promises';
import { resolve, extname } from 'path';

const inputDir = 'D:\\FPT\\og\\VisualizationDSA\\tailieu\\images';

async function convertToWebP() {
  const files = await readdir(inputDir);
  
  let totalSaved = 0;
  let totalFiles = 0;

  for (const file of files) {
    const ext = extname(file).toLowerCase();
    if (!['.png', '.jpg', '.jpeg'].includes(ext)) continue;
    
    const filePath = resolve(inputDir, file);
    const webpPath = filePath.replace(/\.(png|jpg|jpeg)$/i, '.webp');
    
    try {
      const originalStat = await stat(filePath);
      const originalSize = originalStat.size;
      
      await sharp(filePath)
        .webp({ quality: 80, effort: 6 })
        .toFile(webpPath);
      
      const webpStat = await stat(webpPath);
      const webpSize = webpStat.size;
      const saved = originalSize - webpSize;
      const savedPercent = ((saved / originalSize) * 100).toFixed(1);
      
      console.log(`✓ ${file} → ${file.replace(ext, '.webp')} (${(originalSize/1024).toFixed(1)}KB → ${(webpSize/1024).toFixed(1)}KB, saved ${savedPercent}%)`);
      totalSaved += saved;
      totalFiles++;
    } catch (err) {
      console.error(`✗ Failed to convert ${file}:`, err.message);
    }
  }
  
  console.log(`\n=== Summary ===`);
  console.log(`Files converted: ${totalFiles}`);
  console.log(`Total space saved: ${(totalSaved/1024/1024).toFixed(2)} MB`);
}

convertToWebP().catch(console.error);