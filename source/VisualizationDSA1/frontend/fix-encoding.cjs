const fs = require('fs');
const path = require('path');

const filesToFix = [
  'src/views/teacher/TheoryArticleLibraryTab.vue',
  'src/views/teacher/TeacherClassroomCurriculumTab.vue',
  'src/views/teacher/QuizBuilderTab.vue',
  'src/views/teacher/CodelabBuilderTab.vue'
];

for (const file of filesToFix) {
  const filePath = path.join(__dirname, file);
  try {
    const content = fs.readFileSync(filePath, 'utf8');
    // Buffer.from(string, 'latin1') takes the characters and creates bytes from their char codes.
    // Then toString('utf8') decodes those bytes back into real characters.
    const fixedContent = Buffer.from(content, 'latin1').toString('utf8');
    
    // Safety check: only write if it actually fixed things (contains proper Vietnamese)
    if (fixedContent.includes('Quản lý')) {
      fs.writeFileSync(filePath, fixedContent, 'utf8');
      console.log(`Fixed ${file}`);
    } else {
      console.log(`Skipped ${file} (didn't find 'Quản lý' in fixed content)`);
    }
  } catch (err) {
    console.error(`Error processing ${file}:`, err);
  }
}
