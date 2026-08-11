import re

with open(r'd:\FPT\og\VisualizationDSA\backend\src\Infrastructure\Data\DbSeeder.cs', 'r', encoding='utf-8') as f:
    content = f.read()

# I will replace itemOrder++ with the correct spaced ordering
content = content.replace("int itemOrder = 1000;", "int lessonIndex = 1;")
content = content.replace("var lessonItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Lesson, lesson.Id, null, null, sl.Title, itemOrder++, true);", "int itemOrder = lessonIndex * 1000;\n                    var lessonItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Lesson, lesson.Id, null, null, sl.Title, itemOrder, true);")
content = content.replace("var quizItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Quiz, null, quiz.Id, null, \"Quiz: \" + sl.Title, itemOrder++, true);", "var quizItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Quiz, null, quiz.Id, null, \"Quiz: \" + sl.Title, itemOrder + 500, true);")
content = content.replace("var codelabItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Codelab, null, null, codelab.Id, \"Codelab: \" + sl.Title, itemOrder++, true);", "var codelabItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Codelab, null, null, codelab.Id, \"Codelab: \" + sl.Title, itemOrder + 750, true);\n                    lessonIndex++;")

with open(r'd:\FPT\og\VisualizationDSA\backend\src\Infrastructure\Data\DbSeeder.cs', 'w', encoding='utf-8') as f:
    f.write(content)
print("Patched 2 successfully")
