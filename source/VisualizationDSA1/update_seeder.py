import os

file_path = 'backend/src/Infrastructure/Data/DbSeeder.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

content = content.replace(
    'private void AddLessonToCourse(Course course, string title, string contentMd, string sandboxType, string sandboxConfig, Guid? quizId, int xpReward, int index)',
    'private void AddLessonToCourse(Course course, string title, string contentMd, string sandboxType, string sandboxConfig, Guid? quizId, int xpReward, int index, Guid? codelabId = null)'
)

codelab_item_code = '''            if (quizId.HasValue)
            {
                var quizItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Quiz, null, quizId.Value, null, "Quiz: " + title, itemOrder + 500, true);
                module.Items.Add(quizItem);
            }

            if (codelabId.HasValue)
            {
                var codelabItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Codelab, null, null, codelabId.Value, "Codelab: " + title, itemOrder + 750, true);
                module.Items.Add(codelabItem);
            }'''
content = content.replace('''            if (quizId.HasValue)
            {
                var quizItem = new ModuleItem(module.Id, null, VisualizationDSA.Domain.Enums.ModuleItemType.Quiz, null, quizId.Value, null, "Quiz: " + title, itemOrder + 500, true);
                module.Items.Add(quizItem);
            }''', codelab_item_code)

codelab_instances_code = '''            var systemQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Title.Contains("System Design"));

            if (!_context.Codelabs.Any())
            {
                var bubbleSortCodelabEntity = new Codelab(
                    "Th?c hành: Kh?i T?o Thu?t Toán S?p X?p Bubble Sort",
                    "Vi?t hàm s?p x?p m?ng s? nguyên tang d?n b?ng thu?t toán Bubble Sort. Ð?m b?o d?t d? ph?c t?p O(N²).",
                    "public class Solution {\\n    public void BubbleSort(int[] arr) {\\n        // Vi?t code t?i dây\\n    }\\n}",
                    1, 50, 1000, 128000000, "csharp", "N <= 1000", "Input: [5, 2, 9, 1, 5, 6]\\nOutput: [1, 2, 5, 5, 6, 9]", "S? d?ng 2 vòng l?p l?ng nhau.", "sorting,array"
                );
                bubbleSortCodelabEntity.TestCases.Add(new CodelabTestCase(bubbleSortCodelabEntity.Id, "[5, 2, 9, 1, 5, 6]", "[1, 2, 5, 5, 6, 9]", false, 50, 1));
                bubbleSortCodelabEntity.TestCases.Add(new CodelabTestCase(bubbleSortCodelabEntity.Id, "[10, 9, 8, 7, 6, 5]", "[5, 6, 7, 8, 9, 10]", true, 50, 2));

                var twoSumCodelabEntity = new Codelab(
                    "Th?c hành: Tìm 2 s? có t?ng b?ng Target",
                    "Cho m?ng s? nguyên 
ums và s? nguyên 	arget. Tr? v? ch? s? c?a 2 s? sao cho t?ng c?a chúng b?ng 	arget.\\nVí d?: nums = [2,7,11,15], target = 9 => [0,1]",
                    "public class Solution {\\n    public int[] TwoSum(int[] nums, int target) {\\n        // Vi?t code t?i dây\\n        return new int[0];\\n    }\\n}",
                    1, 50, 1000, 128000000, "csharp", "2 <= nums.length <= 10^4", "Input: nums = [2,7,11,15], target = 9\\nOutput: [0,1]", "Dùng HashSet ho?c Dictionary d? luu giá tr? dã duy?t.", "array,hashmap"
                );
                twoSumCodelabEntity.TestCases.Add(new CodelabTestCase(twoSumCodelabEntity.Id, "[2,7,11,15]\\n9", "[0,1]", false, 50, 1));
                twoSumCodelabEntity.TestCases.Add(new CodelabTestCase(twoSumCodelabEntity.Id, "[3,2,4]\\n6", "[1,2]", true, 50, 2));

                var dpCodelabEntity = new Codelab(
                    "Th?c hành: Fibonacci (Quy ho?ch d?ng)",
                    "Vi?t hàm tìm s? Fibonacci th? n s? d?ng Quy ho?ch d?ng (Tabulation ho?c Memoization) d? d?t d? ph?c t?p O(N).",
                    "public class Solution {\\n    public int Fib(int n) {\\n        // Vi?t code t?i dây\\n        return 0;\\n    }\\n}",
                    1, 50, 1000, 128000000, "csharp", "0 <= n <= 30", "Input: n = 4\\nOutput: 3 (vì 0,1,1,2,3...)", "Luu l?i k?t qu? dã tính d? tránh tính toán l?i.", "dp"
                );
                dpCodelabEntity.TestCases.Add(new CodelabTestCase(dpCodelabEntity.Id, "4", "3", false, 50, 1));
                dpCodelabEntity.TestCases.Add(new CodelabTestCase(dpCodelabEntity.Id, "10", "55", true, 50, 2));

                await _context.Codelabs.AddRangeAsync(bubbleSortCodelabEntity, twoSumCodelabEntity, dpCodelabEntity);
                await _context.SaveChangesAsync();
            }

            var bubbleSortCodelab = await _context.Codelabs.FirstOrDefaultAsync(c => c.Title.Contains("Bubble"));
            var twoSumCodelab = await _context.Codelabs.FirstOrDefaultAsync(c => c.Title.Contains("TwoSum") || c.Title.Contains("Target"));
            var dpCodelab = await _context.Codelabs.FirstOrDefaultAsync(c => c.Title.Contains("Fibonacci"));'''
content = content.replace('            var systemQuiz = await _context.Quizzes.FirstOrDefaultAsync(q => q.Title.Contains("System Design"));', codelab_instances_code)

content = content.replace(
    '"dsa", "{\\"array\\":[5,12,8,25,3]}", dsaBasicsQuiz?.Id, 30, 1);',
    '"dsa", "{\\"array\\":[5,12,8,25,3]}", dsaBasicsQuiz?.Id, 30, 1, twoSumCodelab?.Id);'
)

content = content.replace(
    '"sorting", "{\\"array\\":[29,10,14,37,13]}", bubbleSortQuiz?.Id, 45, 1);',
    '"sorting", "{\\"array\\":[29,10,14,37,13]}", bubbleSortQuiz?.Id, 45, 1, bubbleSortCodelab?.Id);'
)

content = content.replace(
    '"dsa", "{}", dpQuiz?.Id, 65, 1);',
    '"dsa", "{}", dpQuiz?.Id, 65, 1, dpCodelab?.Id);'
)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(content)
print("Updated DbSeeder.cs successfully.")
