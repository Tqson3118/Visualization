#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=60;";
using var conn = new SqlConnection(cs);
conn.Open();

Console.WriteLine("==================================================================");
Console.WriteLine("     THỰC THI CHUẨN HÓA DỮ LIỆU LỚP HỌC & BÌNH HỒI TIM           ");
Console.WriteLine("==================================================================");

using var tx = conn.BeginTransaction();

try
{
    var now = DateTime.UtcNow;

    // 1. Sửa CurriculumPublished = 1 cho cả 4 lớp (18, 19, 20, 21)
    using (var cmd = new SqlCommand("UPDATE [Classes] SET CurriculumPublished = 1 WHERE Id IN (18, 19, 20, 21)", conn, tx))
    {
        var n = cmd.ExecuteNonQuery();
        Console.WriteLine($"1. Đã cập nhật CurriculumPublished = 1 cho {n} lớp học.");
    }

    // 2. Sửa Node 153: Đổi tên thành 'Quiz: Chuyên sâu Cây BST & AVL', SortOrder = 6
    using (var cmd = new SqlCommand("UPDATE [LearningPathNodes] SET Title = N'Quiz: Chuyên sâu Cây BST & AVL', SortOrder = 6 WHERE Id = 153", conn, tx))
    {
        var n = cmd.ExecuteNonQuery();
        Console.WriteLine($"2. Đã sửa tiêu đề Node 153: {n} dòng.");
    }

    // 3. Đồng bộ Hearts & HeartsMax cho Users
    using (var cmd = new SqlCommand("UPDATE [Users] SET HeartsMax = 31 WHERE PremiumUntil > SYSUTCDATETIME()", conn, tx))
    {
        Console.WriteLine($"3.1 Đã nâng HeartsMax = 31 cho user có Premium: {cmd.ExecuteNonQuery()} dòng.");
    }

    using (var cmd = new SqlCommand("UPDATE [Users] SET HeartsMax = 10, Hearts = CASE WHEN Hearts > 10 THEN 10 ELSE Hearts END WHERE PremiumUntil IS NULL OR PremiumUntil <= SYSUTCDATETIME()", conn, tx))
    {
        Console.WriteLine($"3.2 Đã hạ HeartsMax = 10 cho user Free: {cmd.ExecuteNonQuery()} dòng.");
    }

    using (var cmd = new SqlCommand(@"
        UPDATE [Users] 
        SET Hearts = 28, HeartsMax = 31, Gems = 450, Xp = 2600, StreakDays = 5, PremiumUntil = DATEADD(month, 1, SYSUTCDATETIME())
        WHERE Email = 'student@demo.local'", conn, tx))
    {
        cmd.ExecuteNonQuery();
        Console.WriteLine("3.3 Đã phục hồi student@demo.local: 28/31 tim, 450 gems, 2600 XP, 5 streak.");
    }

    // Xóa UserInventory vật phẩm 9, 10 của student@demo.local nếu còn để test lại từ đầu
    using (var cmd = new SqlCommand("DELETE FROM [UserInventory] WHERE UserId = 3 AND ItemId IN (9, 10)", conn, tx))
    {
        Console.WriteLine($"3.4 Đã dọn bình hồi tim cũ của student@demo.local: {cmd.ExecuteNonQuery()} dòng.");
    }

    // 4. Lấy danh sách thành viên của 4 lớp
    List<int> GetMembers(int cId)
    {
        var list = new List<int>();
        using var c = new SqlCommand("SELECT UserId FROM [ClassMembers] WHERE ClassId = @cId ORDER BY UserId", conn, tx);
        c.Parameters.AddWithValue("@cId", cId);
        using var r = c.ExecuteReader();
        while (r.Read()) list.Add(r.GetInt32(0));
        return list;
    }

    var m18 = GetMembers(18); // 20
    var m19 = GetMembers(19); // 22
    var m20 = GetMembers(20); // 12
    var m21 = GetMembers(21); // 22

    Console.WriteLine($"Thành viên: Lớp 18={m18.Count}, Lớp 19={m19.Count}, Lớp 20={m20.Count}, Lớp 21={m21.Count}");

    // 5. Cấu hình lại ClassAssignments cho 4 lớp
    using (var cmd = new SqlCommand(@"
        DELETE FROM [ExerciseSubmissions] WHERE ClassAssignmentId IN (SELECT Id FROM [ClassAssignments] WHERE ClassId IN (18, 19, 20, 21));
        DELETE FROM [ClassAssignments] WHERE ClassId IN (18, 19, 20, 21);", conn, tx))
    {
        Console.WriteLine($"5.1 Đã dọn ExerciseSubmissions lớp học cũ và ClassAssignments cũ: {cmd.ExecuteNonQuery()} dòng.");
    }

    int InsertAssignment(int cId, int? pathItemId, int? lessonId, int? exId, DateTime? dueAt, int sortOrder)
    {
        using var cmd = new SqlCommand(@"
            INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt)
            OUTPUT INSERTED.Id
            VALUES (@cId, @pId, @lId, @eId, @due, 1, @sort, 0, @created)", conn, tx);
        cmd.Parameters.AddWithValue("@cId", cId);
        cmd.Parameters.AddWithValue("@pId", (object?)pathItemId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@lId", (object?)lessonId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@eId", (object?)exId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@due", (object?)dueAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@sort", sortOrder);
        cmd.Parameters.AddWithValue("@created", now.AddDays(-30));
        return (int)cmd.ExecuteScalar();
    }

    // Class 18 (PRO192 - Đã kết thúc hoàn hảo)
    var a18_1 = InsertAssignment(18, 1, 1, 84, now.AddDays(-75), 1);
    var a18_203 = InsertAssignment(18, 203, 167, 142, now.AddDays(-65), 2);
    var a18_2 = InsertAssignment(18, 2, 2, 87, now.AddDays(-55), 3);
    var a18_3 = InsertAssignment(18, 3, 103, null, now.AddDays(-45), 4);
    var a18_4 = InsertAssignment(18, 4, 104, 1, now.AddDays(-35), 5);

    // Class 19 (CSD201 - Chính khóa đang học rất tích cực)
    var a19_1 = InsertAssignment(19, 1, 1, 84, now.AddDays(-10), 1);      // Đã hết hạn 10 ngày
    var a19_203 = InsertAssignment(19, 203, 167, 142, now.AddDays(-3), 2); // Đã hết hạn 3 ngày
    var a19_2 = InsertAssignment(19, 2, 2, 87, now.AddDays(5), 3);        // Còn 5 ngày
    var a19_3 = InsertAssignment(19, 3, 103, null, now.AddDays(10), 4);   // Còn 10 ngày
    var a19_4 = InsertAssignment(19, 4, 104, 1, now.AddDays(15), 5);      // Còn 15 ngày

    // Class 20 (ALGO301 - Chuyên đề ICPC Cây)
    var a20_140 = InsertAssignment(20, 140, 136, null, now.AddDays(-14), 1);
    var a20_141 = InsertAssignment(20, 141, 137, null, now.AddDays(-10), 2);
    var a20_142 = InsertAssignment(20, 142, 138, null, now.AddDays(-7), 3);
    var a20_143 = InsertAssignment(20, 143, 139, 129, now.AddDays(-3), 4);
    var a20_144 = InsertAssignment(20, 144, null, 129, now.AddDays(4), 5); // Quiz 1 (Node 144)
    var a20_153 = InsertAssignment(20, 153, null, 131, now.AddDays(11), 6); // Quiz 2 (Node 153)

    // Class 21 (DSA202 - Lớp cảnh báo deadline & lagging learners)
    var a21_5 = InsertAssignment(21, 5, 3, 90, now.AddDays(-2), 1); // Stack - Quá hạn 2 ngày
    var a21_6 = InsertAssignment(21, 6, 4, 93, now.AddDays(-1), 2); // Linked List - Quá hạn 1 ngày
    var a21_7 = InsertAssignment(21, 7, 105, null, now.AddDays(4), 3);
    var a21_8 = InsertAssignment(21, 8, 106, 2, now.AddDays(10), 4);

    Console.WriteLine("5.2 Đã thiết lập ClassAssignments chuẩn cho 4 lớp.");

    // 6. Định nghĩa Code Mẫu JavaScript Thật và Chuẩn Chỉnh
    var codeBubbleSort = @"function bubbleSort(arr) {
  if (!Array.isArray(arr) || arr.length <= 1) return arr;
  const n = arr.length;
  let swapped;
  for (let i = 0; i < n - 1; i++) {
    swapped = false;
    for (let j = 0; j < n - 1 - i; j++) {
      if (arr[j] > arr[j + 1]) {
        const temp = arr[j];
        arr[j] = arr[j + 1];
        arr[j + 1] = temp;
        swapped = true;
      }
    }
    if (!swapped) break;
  }
  return arr;
}";

    var codeBinarySearch = @"function binarySearch(arr, target) {
  if (!Array.isArray(arr) || arr.length === 0) return -1;
  let left = 0;
  let right = arr.length - 1;
  while (left <= right) {
    const mid = Math.floor(left + (right - left) / 2);
    if (arr[mid] === target) return mid;
    if (arr[mid] < target) left = mid + 1;
    else right = mid - 1;
  }
  return -1;
}";

    var codeStack = @"class MyStack {
  constructor() {
    this._items = [];
  }
  push(x) {
    this._items.push(x);
  }
  pop() {
    if (this.isEmpty()) return null;
    return this._items.pop();
  }
  peek() {
    if (this.isEmpty()) return null;
    return this._items[this._items.length - 1];
  }
  isEmpty() {
    return this._items.length === 0;
  }
  size() {
    return this._items.length;
  }
}";

    var codeLinkedList = @"class Node {
  constructor(val, next = null) {
    this.val = val;
    this.next = next;
  }
}

class MyLinkedList {
  constructor() {
    this.head = null;
    this.size = 0;
  }
  insertHead(val) {
    const node = new Node(val, this.head);
    this.head = node;
    this.size++;
  }
  insertTail(val) {
    const node = new Node(val);
    if (!this.head) {
      this.head = node;
    } else {
      let cur = this.head;
      while (cur.next) cur = cur.next;
      cur.next = node;
    }
    this.size++;
  }
  delete(val) {
    if (!this.head) return;
    if (this.head.val === val) {
      this.head = this.head.next;
      this.size--;
      return;
    }
    let cur = this.head;
    while (cur.next && cur.next.val !== val) {
      cur = cur.next;
    }
    if (cur.next) {
      cur.next = cur.next.next;
      this.size--;
    }
  }
  toArray() {
    const res = [];
    let cur = this.head;
    while (cur) {
      res.push(cur.val);
      cur = cur.next;
    }
    return res;
  }
}";

    var codeCodelab142 = @"function solveChallenge(input) {
  // Thử thách: Kiểm tra dấu ngoặc hợp lệ (Valid Parentheses) & xử lý chuỗi
  if (typeof input !== 'string') return false;
  const stack = [];
  const map = { ')': '(', ']': '[', '}': '{' };
  for (let i = 0; i < input.length; i++) {
    const ch = input[i];
    if (ch === '(' || ch === '[' || ch === '{') {
      stack.push(ch);
    } else if (map[ch]) {
      if (stack.length === 0 || stack.pop() !== map[ch]) {
        return false;
      }
    }
  }
  return stack.length === 0;
}";

    // Helper chèn CodeSubmission
    void InsertCodeSub(int uId, int exId, string code, int score, int passed, int total, DateTime subAt)
    {
        using var cmd = new SqlCommand(@"
            IF NOT EXISTS (SELECT 1 FROM [CodeSubmissions] WHERE UserId = @uId AND ExerciseId = @exId)
            BEGIN
                INSERT INTO [CodeSubmissions] (UserId, ExerciseId, Code, Score, PassedTests, TotalTests, ResultJson, SubmittedAt, IsClientDeclared, ClientRequestId)
                VALUES (@uId, @exId, @code, @score, @passed, @total, @resJson, @subAt, 1, NEWID())
            END
            ELSE
            BEGIN
                UPDATE [CodeSubmissions] 
                SET Code = @code, Score = @score, PassedTests = @passed, TotalTests = @total, ResultJson = @resJson, SubmittedAt = @subAt
                WHERE UserId = @uId AND ExerciseId = @exId
            END", conn, tx);

        var resJsonList = new List<string>();
        for (int i = 1; i <= total; i++)
        {
            bool p = i <= passed;
            resJsonList.Add($"{{\"testId\":\"t{i}\",\"passed\":{(p ? "true" : "false")}}}");
        }
        var resJson = "[" + string.Join(",", resJsonList) + "]";

        cmd.Parameters.AddWithValue("@uId", uId);
        cmd.Parameters.AddWithValue("@exId", exId);
        cmd.Parameters.AddWithValue("@code", code);
        cmd.Parameters.AddWithValue("@score", score);
        cmd.Parameters.AddWithValue("@passed", passed);
        cmd.Parameters.AddWithValue("@total", total);
        cmd.Parameters.AddWithValue("@resJson", resJson);
        cmd.Parameters.AddWithValue("@subAt", subAt);
        cmd.ExecuteNonQuery();
    }

    // Helper chèn ExerciseSubmission (Quiz)
    void InsertQuizSub(int uId, int exId, int? aId, int score, DateTime subAt)
    {
        using var cmd = new SqlCommand(@"
            INSERT INTO [ExerciseSubmissions] (UserId, ExerciseId, ClassAssignmentId, Score, AnswersJson, ResultJson, DurationSeconds, SubmittedAt, ClientRequestId)
            VALUES (@uId, @exId, @aId, @score, @ans, @res, 90, @subAt, NEWID())", conn, tx);
        cmd.Parameters.AddWithValue("@uId", uId);
        cmd.Parameters.AddWithValue("@exId", exId);
        cmd.Parameters.AddWithValue("@aId", (object?)aId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@score", score);
        cmd.Parameters.AddWithValue("@ans", "[{\"QuestionId\":1,\"Selected\":[0]},{\"QuestionId\":2,\"Selected\":[1]}]");
        cmd.Parameters.AddWithValue("@res", "[{\"QuestionId\":1,\"Correct\":true},{\"QuestionId\":2,\"Correct\":true}]");
        cmd.Parameters.AddWithValue("@subAt", subAt);
        cmd.ExecuteNonQuery();
    }

    // Helper chèn UserNodeProgress
    void InsertNodeProg(int uId, int nId, int score, DateTime subAt)
    {
        using var cmd = new SqlCommand(@"
            IF NOT EXISTS (SELECT 1 FROM [UserNodeProgress] WHERE UserId = @uId AND NodeId = @nId)
            BEGIN
                INSERT INTO [UserNodeProgress] (UserId, NodeId, Status, Stars, NodeScore, UnlockedAt, PassedAt, UpdatedAt)
                VALUES (@uId, @nId, 2, 3, @score, DATEADD(day, -5, @subAt), @subAt, @subAt)
            END
            ELSE
            BEGIN
                UPDATE [UserNodeProgress] 
                SET Status = 2, Stars = 3, NodeScore = @score, PassedAt = @subAt, UpdatedAt = @subAt
                WHERE UserId = @uId AND NodeId = @nId
            END", conn, tx);
        cmd.Parameters.AddWithValue("@uId", uId);
        cmd.Parameters.AddWithValue("@nId", nId);
        cmd.Parameters.AddWithValue("@score", score);
        cmd.Parameters.AddWithValue("@subAt", subAt);
        cmd.ExecuteNonQuery();
    }

    // Helper chèn UserProgress (Lesson)
    void InsertLessonProg(int uId, int lId, int score, DateTime subAt)
    {
        using var cmd = new SqlCommand(@"
            IF NOT EXISTS (SELECT 1 FROM [UserProgress] WHERE UserId = @uId AND LessonId = @lId)
            BEGIN
                INSERT INTO [UserProgress] (UserId, LessonId, Viewed, SimulationCount, BestScore, CompletedAt, UpdatedAt)
                VALUES (@uId, @lId, 1, 3, @score, @subAt, @subAt)
            END
            ELSE
            BEGIN
                UPDATE [UserProgress] 
                SET Viewed = 1, SimulationCount = 3, BestScore = @score, CompletedAt = @subAt, UpdatedAt = @subAt
                WHERE UserId = @uId AND LessonId = @lId
            END", conn, tx);
        cmd.Parameters.AddWithValue("@uId", uId);
        cmd.Parameters.AddWithValue("@lId", lId);
        cmd.Parameters.AddWithValue("@score", score);
        cmd.Parameters.AddWithValue("@subAt", subAt);
        cmd.ExecuteNonQuery();
    }

    // ──────────────────────────────────────────────────────────────────
    // 7. SEEDER CHO LỚP 18 (PRO192 - Đã học xong, 100% hoàn thành)
    // ──────────────────────────────────────────────────────────────────
    Console.WriteLine("Đang seed dữ liệu cho Lớp 18 (PRO192)...");
    foreach (var uId in m18)
    {
        int scoreBase = 85 + (uId % 16);
        // Node 1: Bubble Sort
        InsertNodeProg(uId, 1, scoreBase, now.AddDays(-80));
        InsertLessonProg(uId, 1, scoreBase, now.AddDays(-80));
        InsertCodeSub(uId, 84, codeBubbleSort, 100, 11, 11, now.AddDays(-80));
        InsertQuizSub(uId, 82, a18_1, scoreBase, now.AddDays(-80));

        // Node 203: Codelab
        InsertNodeProg(uId, 203, scoreBase, now.AddDays(-70));
        InsertLessonProg(uId, 167, scoreBase, now.AddDays(-70));
        InsertCodeSub(uId, 142, codeCodelab142, 100, 5, 5, now.AddDays(-70));

        // Node 2: Binary Search
        InsertNodeProg(uId, 2, scoreBase, now.AddDays(-60));
        InsertLessonProg(uId, 2, scoreBase, now.AddDays(-60));
        InsertCodeSub(uId, 87, codeBinarySearch, 100, 11, 11, now.AddDays(-60));

        // Node 3: Luyện tập
        InsertNodeProg(uId, 3, scoreBase, now.AddDays(-50));
        InsertLessonProg(uId, 103, scoreBase, now.AddDays(-50));

        // Node 4: Kiểm tra cuối
        InsertNodeProg(uId, 4, scoreBase, now.AddDays(-40));
        InsertLessonProg(uId, 104, scoreBase, now.AddDays(-40));
        InsertQuizSub(uId, 1, a18_4, scoreBase, now.AddDays(-40));
    }

    // ──────────────────────────────────────────────────────────────────
    // 8. SEEDER CHO LỚP 19 (CSD201 - Chính khóa đang học rất tốt)
    // ──────────────────────────────────────────────────────────────────
    Console.WriteLine("Đang seed dữ liệu cho Lớp 19 (CSD201)...");
    foreach (var uId in m19)
    {
        bool isStudentSample = (uId == 3);
        int score = isStudentSample ? 100 : (85 + (uId * 7 % 16));

        // Node 1: Bubble Sort (Hết hạn 10 ngày trước) -> 100% nộp đúng hạn
        var sub1At = now.AddDays(-12);
        InsertNodeProg(uId, 1, score, sub1At);
        InsertLessonProg(uId, 1, score, sub1At);
        InsertCodeSub(uId, 84, codeBubbleSort, 100, 11, 11, sub1At);
        InsertQuizSub(uId, 82, a19_1, score, sub1At);

        // Node 203: Codelab 142 (Hết hạn 3 ngày trước)
        // 20 bạn nộp đúng hạn, 2 bạn (26, 27) nộp sau hạn -> TOÀN BỘ 22 BẠN ĐỀU CÓ BÀI NỘP TRONG MODAL!
        DateTime sub203At = (uId == 26 || uId == 27) ? now.AddDays(-1) : now.AddDays(-5);
        int sub203Score = (uId == 26 || uId == 27) ? 80 : score;
        InsertNodeProg(uId, 203, sub203Score, sub203At);
        InsertLessonProg(uId, 167, sub203Score, sub203At);
        InsertCodeSub(uId, 142, codeCodelab142, 100, 5, 5, sub203At);

        // Node 2: Binary Search (Còn hạn 5 ngày) -> 19 bạn đã xong, 3 bạn đang làm
        if (uId != 24 && uId != 25 && uId != 26)
        {
            var sub2At = now.AddDays(-2);
            InsertNodeProg(uId, 2, score, sub2At);
            InsertLessonProg(uId, 2, score, sub2At);
            InsertCodeSub(uId, 87, codeBinarySearch, 100, 11, 11, sub2At);
            InsertQuizSub(uId, 1, a19_2, score, sub2At);
        }

        // Node 3: Luyện tập -> 18 bạn đã xem
        if (uId != 23 && uId != 24 && uId != 25 && uId != 26)
        {
            InsertNodeProg(uId, 3, score, now.AddDays(-1));
            InsertLessonProg(uId, 103, score, now.AddDays(-1));
        }

        // Node 4: Kiểm tra cuối -> 16 bạn đã làm sớm
        if (uId <= 18 || isStudentSample)
        {
            InsertNodeProg(uId, 4, score, now.AddHours(-10));
            InsertLessonProg(uId, 104, score, now.AddHours(-10));
            InsertQuizSub(uId, 1, a19_4, score, now.AddHours(-10));
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // 9. SEEDER CHO LỚP 20 (ALGO301 - Chuyên đề Cây & ICPC)
    // ──────────────────────────────────────────────────────────────────
    Console.WriteLine("Đang seed dữ liệu cho Lớp 20 (ALGO301)...");
    foreach (var uId in m20)
    {
        bool isSample = (uId == 3);
        int score = isSample ? 100 : (88 + (uId % 13));

        // Lý thuyết Nodes 140, 141, 142, 143: Đạt 100% cả 12 bạn
        InsertNodeProg(uId, 140, score, now.AddDays(-12));
        InsertLessonProg(uId, 136, score, now.AddDays(-12));

        InsertNodeProg(uId, 141, score, now.AddDays(-9));
        InsertLessonProg(uId, 137, score, now.AddDays(-9));

        InsertNodeProg(uId, 142, score, now.AddDays(-6));
        InsertLessonProg(uId, 138, score, now.AddDays(-6));

        InsertNodeProg(uId, 143, score, now.AddDays(-2));
        InsertLessonProg(uId, 139, score, now.AddDays(-2));

        // Node 144 (Quiz 129: Cây Nhị Phân & Tự Cân Bằng) -> 11/12 bạn đã nộp (trừ uId 14)
        if (uId != 14)
        {
            InsertNodeProg(uId, 144, score, now.AddDays(-1));
            InsertQuizSub(uId, 129, a20_144, score, now.AddDays(-1));
        }

        // Node 153 (Quiz 131: Chuyên sâu Cây BST & AVL) -> 10/12 bạn đã nộp (trừ uId 13, 14)
        if (uId != 13 && uId != 14)
        {
            InsertNodeProg(uId, 153, score, now.AddHours(-6));
            InsertQuizSub(uId, 131, a20_153, score, now.AddHours(-6));
        }
    }

    // ──────────────────────────────────────────────────────────────────
    // 10. SEEDER CHO LỚP 21 (DSA202 - Kịch bản Deadline & Lagging Learners)
    // ──────────────────────────────────────────────────────────────────
    Console.WriteLine("Đang seed dữ liệu cho Lớp 21 (DSA202)...");
    foreach (var uId in m21)
    {
        // 3 học viên chưa nộp (Quá hạn cả 2 bài) -> Lagging Learners:
        // uId 23 (Ngô Minh Hiếu), 24 (Đinh Công Minh), 25 (Nguyễn Văn Tuấn)
        if (uId == 23 || uId == 24 || uId == 25)
        {
            // Không nộp bài 5 và bài 6! Sẽ xuất hiện cờ đỏ trong LaggingLearners!
            continue;
        }

        // 4 học viên nộp trễ (uId 19, 20, 21, 22) -> Nộp sau deadline
        // Deadline bài 5: -2 ngày. Late: -1.7 ngày.
        // Deadline bài 6: -1 ngày. Late: -0.7 ngày.
        bool isLate = (uId >= 19 && uId <= 22);

        DateTime sub5At = isLate ? now.AddDays(-1.7) : now.AddDays(-2.5);
        DateTime sub6At = isLate ? now.AddDays(-0.7) : now.AddDays(-1.5);
        int score5 = isLate ? 75 : (85 + (uId % 16));
        int score6 = isLate ? 70 : (85 + (uId % 16));

        // Node 5: Stack (Lab 90, Final 88)
        InsertNodeProg(uId, 5, score5, sub5At);
        InsertLessonProg(uId, 3, score5, sub5At);
        InsertCodeSub(uId, 90, codeStack, 100, 11, 11, sub5At);
        InsertQuizSub(uId, 88, a21_5, score5, sub5At);

        // Node 6: Linked List (Lab 93, Final 2)
        InsertNodeProg(uId, 6, score6, sub6At);
        InsertLessonProg(uId, 4, score6, sub6At);
        InsertCodeSub(uId, 93, codeLinkedList, 100, 11, 11, sub6At);
        InsertQuizSub(uId, 2, a21_6, score6, sub6At);
    }

    tx.Commit();
    Console.WriteLine("==================================================================");
    Console.WriteLine("  QUY TRÌNH CHUẨN HÓA DỮ LIỆU LỚP HỌC & TIM ĐÃ HOÀN TẤT 100%!   ");
    Console.WriteLine("==================================================================");
}
catch (Exception ex)
{
    tx.Rollback();
    Console.WriteLine("LỖI XẢY RA - ĐÃ ROLLBACK: " + ex.Message);
    Console.WriteLine(ex.StackTrace);
}
