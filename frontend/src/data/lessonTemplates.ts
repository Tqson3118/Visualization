export interface LessonTemplate {
  id: string;
  name: string;
  description: string;
  content: string;
}

export const LESSON_TEMPLATES: LessonTemplate[] = [
  {
    id: 'algo-standard',
    name: '🎓 1. Bài giảng Giải thuật chuẩn (Chuẩn sư phạm)',
    description: 'Mục tiêu, Cơ chế hoạt động, Mã giả Pseudocode, Phân tích Big-O và Ví dụ từng bước.',
    content: `# Thuật toán [Tên Giải Thuật]

> [!NOTE]
> Khóa học: **Cấu trúc dữ liệu & Giải thuật** | Trình độ: **Cơ bản / Trung cấp**

## 1. Mục tiêu bài học
- Hiểu rõ bản chất và ý tưởng cốt lõi của thuật toán.
- Nắm vững các bước thực thi từng bước (Step-by-step Execution).
- Tự cài đặt được thuật toán bằng ngôn ngữ lập trình (C++, Java, Python).
- Phân tích và đánh giá độ phức tạp thời gian ($O$) và không gian ($Space$).

---

## 2. Ý tưởng & Cơ chế hoạt động
Mô tả chi tiết nguyên lý hoạt động của giải thuật bằng ngôn ngữ trực quan dễ hiểu:
1. **Bước 1**: Khởi tạo các con trỏ / biến đếm cần thiết.
2. **Bước 2**: Lặp qua từng phần tử và so sánh điều kiện.
3. **Bước 3**: Cập nhật trạng thái và hoán đổi vị trí nếu cần.
4. **Bước 4**: Kết thúc khi toàn bộ cấu trúc đã thỏa mãn điều kiện.

> [!TIP]
> Hãy mở tab **Mô phỏng (Simulation)** bên cạnh để chạy thử từng bước bằng hình ảnh trực quan!

---

## 3. Mã giả (Pseudocode)
\`\`\`cpp
// Mã giả minh họa
function solveAlgorithm(arr, n) {
    for i from 0 to n - 1 {
        // Xử lý từng phần tử
        if (arr[i] > arr[i + 1]) {
            swap(arr[i], arr[i + 1]);
        }
    }
    return arr;
}
\`\`\`

---

## 4. Đánh giá Độ phức tạp (Complexity Analysis)
| Trường hợp | Độ phức tạp thời gian (Time) | Không gian bộ nhớ (Space) | Ghi chú |
| :--- | :--- | :--- | :--- |
| **Tốt nhất (Best Case)** | \`O(N)\` | \`O(1)\` | Khi mảng đã có thứ tự sẵn |
| **Trung bình (Average Case)** | \`O(N log N)\` | \`O(1)\` | Trường hợp dữ liệu ngẫu nhiên |
| **Xấu nhất (Worst Case)** | \`O(N^2)\` | \`O(1)\` | Khi mảng ngược thứ tự hoàn toàn |

---

## 5. Tổng kết & Bài tập ứng dụng
- **Điểm mấu chốt**: Ghi nhớ quy tắc chia để trị và điều kiện dừng đệ quy.
- **Thực hành**: Hoàn thành bài tập trắc nghiệm và thử thách lập trình testcase trong mục Bài tập!
`,
  },
  {
    id: 'ds-standard',
    name: '🧱 2. Cấu trúc dữ liệu & Thao tác CRUD',
    description: 'Định nghĩa, Biểu diễn bộ nhớ, Thao tác Thêm/Xóa/Tìm kiếm, So sánh ưu nhược điểm.',
    content: `# Cấu trúc dữ liệu: [Tên Cấu Trúc Dữ Liệu]

> [!NOTE]
> Phân loại: **Tuyến tính (Linear) / Phi tuyến tính (Non-linear)** | Đơn vị: **Node / Array-based**

## 1. Định nghĩa & Cấu trúc lưu trữ
Mô tả khái niệm, các phần tử cấu thành và cơ chế cấp phát bộ nhớ (liên tục hay liên kết con trỏ).

\`\`\`cpp
// Định nghĩa cấu trúc một Node
struct Node {
    int data;
    Node* next;
    Node(int val) : data(val), next(nullptr) {}
};
\`\`\`

---

## 2. Các thao tác cơ bản (Core Operations)
- **Truy cập (Access)**: Lấy giá trị tại vị trí chỉ định.
- **Tìm kiếm (Search)**: Duyệt tìm kiếm phần tử có giá trị $X$.
- **Chèn (Insertion)**: Thêm phần tử mới vào đầu / cuối / giữa.
- **Xóa (Deletion)**: Loại bỏ phần tử khỏi cấu trúc dữ liệu.

> [!WARNING]
> Luôn kiểm tra điều kiện rỗng (\`null\` hoặc \`isEmpty\`) trước khi thực hiện thao tác xóa để tránh lỗi Crash bộ nhớ!

---

## 3. Bảng tổng kết hiệu năng
| Thao tác | Thời gian (Time) | Không gian (Space) | Giải thích |
| :--- | :--- | :--- | :--- |
| **Thêm đầu** | \`O(1)\` | \`O(1)\` | Cập nhật con trỏ đầu |
| **Thêm cuối** | \`O(1)\` | \`O(1)\` | Khi có lưu con trỏ tail |
| **Tìm kiếm** | \`O(N)\` | \`O(1)\` | Cần duyệt tuần tự qua các node |
| **Xóa** | \`O(N)\` | \`O(1)\` | Phải tìm vị trí trước khi ngắt liên kết |
`,
  },
  {
    id: 'lab-standard',
    name: '🧪 3. Hướng dẫn Thực hành Lab / Codelab',
    description: 'Mô tả bài toán thực tế, Yêu cầu I/O, Hướng tiếp cận giải thuật, Testcases mẫu.',
    content: `# Hướng dẫn Lab: [Tên Bài Lab / Thử Thách]

## 1. Đề bài
Cho một dãy số nguyên gồm $N$ phần tử... Hãy tìm cách tối ưu để...

### Đầu vào (Input)
- Dòng 1: Số nguyên $N$ ($1 \le N \le 10^5$).
- Dòng 2: Gồm $N$ số nguyên cách nhau bởi dấu cách.

### Đầu ra (Output)
- In ra kết quả duy nhất thỏa mãn yêu cầu bài toán.

---

## 2. Ví dụ (Examples)
| Input | Output | Giải thích |
| :--- | :--- | :--- |
| \`5\`<br/>\`1 3 2 5 4\` | \`5\` | Phần tử lớn nhất tìm được là 5 |
| \`3\`<br/>\`-1 -5 0\` | \`0\` | Số lớn nhất là 0 |

---

## 3. Gợi ý hướng giải thuật
> [!TIP]
> Sử dụng kỹ thuật hai con trỏ (Two Pointers) hoặc mảng cộng dồn (Prefix Sum) để giảm độ phức tạp từ \`O(N^2)\` xuống còn \`O(N)\`.
`,
  },
];
