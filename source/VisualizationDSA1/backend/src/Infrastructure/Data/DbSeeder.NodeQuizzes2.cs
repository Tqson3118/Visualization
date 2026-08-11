using System.Collections.Generic;

namespace VisualizationDSA.Infrastructure.Data
{
    public partial class DbSeeder
    {
        // ── Quiz theo node — OOP & SOLID ──
        private static List<QuizQuestionSeed> OopQuizQuestions(string name)
        {
            if (name == "Encapsulation")
                return new List<QuizQuestionSeed>
                {
                    Q("Đóng gói (Encapsulation) bảo vệ dữ liệu bằng cách nào?", new[]{"Dùng private + getter/setter kiểm soát","Dùng toàn biến public","Không cần kiểm soát","Dùng biến static"},0,"Ẩn field private, truy cập qua phương thức công khai."),
                    Q("Access modifier nào cho phép truy cập từ mọi nơi?", new[]{"public","private","protected","internal"},0,"public phơi bày toàn cục."),
                    Q("Access modifier nào chỉ truy cập trong cùng lớp?", new[]{"private","public","protected","internal"},0,"private giới hạn trong lớp."),
                    Q("Lợi ích của đóng gói?", new[]{"Giữ bất biến và kiểm soát dữ liệu","Tăng tốc chạy","Giảm class","Không có lợi ích"},0,"Ngăn sửa dữ liệu sai trực tiếp từ ngoài."),
                    Q("Property trong C# gồm những phần nào?", new[]{"getter và setter","chỉ getter","chỉ setter","không có"},0,"Property gồm accessor get/set."),
                    Q("Để ngăn sửa sau khi khởi tạo, dùng gì?", new[]{"Chỉ có getter (read-only)","setter public","field public","biến tĩnh"},0,"Read-only property chỉ cho đọc."),
                    Q("BankAccount.Đóng gói balance giúp gì?", new[]{"Chặn rút tiền khi số dư âm","Tăng lãi suất","Giảm code","Không giúp gì"},0,"Ràng buộc nghiệp vụ ngay trong setter/method."),
                    Q("Đóng gói liên quan chặt đến nguyên lý nào?", new[]{"Information hiding","Open/Closed","Liskov","Interface Segregation"},0,"Ẩn thông tin là nền tảng đóng gói."),
                    Q("Cách đọc private field từ bên ngoài?", new[]{"Qua public getter/property","Truy cập trực tiếp","Reflection duy nhất","Không đọc được"},0,"Phơi bày qua getter kiểm soát."),
                    Q("Vi phạm đóng gói thường gặp?", new[]{"Để field public","Dùng class","Dùng namespace","Viết method"},0,"Public field cho phép sửa bừa bãi."),
                };
            if (name == "Inheritance")
                return new List<QuizQuestionSeed>
                {
                    Q("Kế thừa cho phép lớp con làm gì?", new[]{"Tái sử dụng thành viên lớp cha","Xóa lớp cha","Truy cập private lớp cha","Đổi tên lớp cha"},0,"Lớp con nhận lại thành viên public/protected."),
                    Q("Từ khóa biểu thị kế thừa trong C#?", new[]{":","extends","implements","uses"},0,"C# dùng dấu hai chấm khi khai báo."),
                    Q("protected cho phép truy cập ở đâu?", new[]{"Lớp hiện tại và lớp con","Mọi nơi","Chỉ cùng namespace","Chỉ cùng assembly"},0,"protected mở cho lớp con kế thừa."),
                    Q("Base class vs derived class?", new[]{"Lớp cha vs lớp con","Lớp con vs lớp cha","Hai lớp độc lập","Lớp trừu tượng"},0,"Base là lớp cha, derived là lớp con."),
                    Q("Override method trong C# dùng từ khóa gì?", new[]{"override (method phải virtual)","new","static","readonly"},0,"override yêu cầu method cha virtual."),
                    Q("Ưu điểm của kế thừa?", new[]{"Tái sử dụng và mở rộng code","Giảm coupling","Tự tối ưu","Chống lỗi tuyệt đối"},0,"Kế thừa chia sẻ hành vi chung."),
                    Q("Nhược điểm của lạm dụng kế thừa?", new[]{"Tăng coupling, khó thay đổi","Chạy chậm","Tốn RAM","Không có nhược điểm"},0,"Quan hệ is-a chặt làm khó refactor."),
                    Q("Composition (has-a) khác Inheritance (is-a)?", new[]{"Composition chứa đối tượng thay vì kế thừa hành vi","Giống hệt nhau","Composition chỉ dùng interface","Không khác"},0,"Has-a linh hoạt hơn is-a."),
                    Q("Lớp Dog kế thừa Animal — phát biểu nào đúng?", new[]{"Dog là một Animal (is-a)","Animal là một Dog","Chúng độc lập","Dog chứa Animal"},0,"is-a thể hiện quan hệ kế thừa."),
                    Q("Constructor của lớp con gọi constructor cha bằng gì?", new[]{"base(...)","super(...)","this(...)","parent(...)"},0,"C# dùng base() để gọi constructor cha."),
                };
            if (name == "Polymorphism")
                return new List<QuizQuestionSeed>
                {
                    Q("Đa hình cho phép điều gì?", new[]{"Cùng interface, hành vi khác nhau theo runtime type","Nhiều hàm cùng tên tham số giống hệt","Truy cập private","Tăng tốc"},0,"Runtime polymorphism qua override/interface."),
                    Q("Compile-time polymorphism còn gọi là gì?", new[]{"Method overloading","Method overriding","Interface","Abstract class"},0,"Overloading (nạp chồng) quyết định lúc compile."),
                    Q("Runtime polymorphism dựa vào gì?", new[]{"Override + virtual/dispatch","Overloading","Namespace","Biến toàn cục"},0,"Virtual dispatch chọn method theo kiểu runtime."),
                    Q("Interface cho phép đa hình như thế nào?", new[]{"Nhiều lớp cài cùng interface, gọi chung","Chỉ một lớp cài","Không hỗ trợ","Chỉ struct"},0,"Đa hình qua interface."),
                    Q("Khi gọi shape.Draw() với shape là Circle runtime?", new[]{"Chạy Draw() của Circle","Chạy của Shape","Lỗi","Ngẫu nhiên"},0,"Dispatch theo runtime type."),
                    Q("Từ khóa nào bắt buộc method có thể override?", new[]{"virtual","new","sealed","readonly"},0,"virtual cho phép override."),
                    Q("Từ khóa nào ngăn tiếp tục override?", new[]{"sealed","virtual","abstract","public"},0,"sealed override chặn override cấp dưới."),
                    Q("Abstract method là gì?", new[]{"Chỉ khai báo, không thân — lớp con phải cài","Có thân đầy đủ","Tự động chạy","Bị cấm dùng"},0,"Abstract bắt buộc cài ở lớp con."),
                    Q("Đa hình giúp gì trong thiết kế?", new[]{"Mở rộng dễ, ít sửa code cũ","Chạy nhanh hơn","Ít class hơn","Không cần interface"},0,"Thêm loại mới không sửa code gọi chung."),
                    Q("Ví dụ đa hình điển hình?", new[]{"Shape + Circle/Rectangle/Triangle","Một lớp duy nhất","Enum lớn","Biến static"},0,"Các hình cùng Draw() khác hành vi."),
                };
            if (name == "Abstraction")
                return new List<QuizQuestionSeed>
                {
                    Q("Trừu tượng hóa tập trung vào gì?", new[]{"Phơi bày giao diện, ẩn phức tạp","Chi tiết triển khai","Bộ nhớ","Tốc độ"},0,"Che giấu phức tạp, lộ cái cần dùng."),
                    Q("Khác biệt abstract class và interface?", new[]{"Abstract class có thể có field/thân method; interface khai báo hợp đồng","Giống hệt nhau","Interface có field","Abstract không có method"},0,"Abstract cho phần triển khai chung, interface là hợp đồng."),
                    Q("Interface trong C# chỉ chứa gì (truyền thống)?", new[]{"Khai báo method/property không thân","Code hoàn chỉnh","Field private","Constructor"},0,"Interface khai báo hợp đồng, lớp cài cung cấp thân."),
                    Q("Khi nào nên dùng abstract class?", new[]{"Có state dùng chung + hành vi mặc định","Chỉ cần hợp đồng thuần","Không có state","Cần nhiều kế thừa"},0,"Abstract chia sẻ state/implement chung."),
                    Q("Khi nào nên dùng interface?", new[]{"Nhiều kiểu không liên quan cần chung hành vi","Cần field","Cần constructor","Chỉ một lớp"},0,"Interface cho hợp đồng chung nhiều loại."),
                    Q("Abstract method bắt buộc gì?", new[]{"Lớp con cài đặt (override)","Tự chạy","Có thân","Không dùng"},0,"Abstract method không thân, lớp con override."),
                    Q("Lớp chứa abstract method phải là gì?", new[]{"abstract class","sealed class","static class","struct"},0,"Lớp abstract mới chứa abstract method."),
                    Q("Có khởi tạo được abstract class trực tiếp không?", new[]{"Không","Có","Chỉ trong package","Khi có override"},0,"Abstract không new trực tiếp được."),
                    Q("Interface giúp giảm gì?", new[]{"Coupling giữa module","Số class","Bộ nhớ","Thời gian compile"},0,"Phụ thuộc vào hợp đồng thay vì chi tiết."),
                    Q("Ví dụ abstraction trong thực tế?", new[]{"IPayment — Paypal/Visa/Momo cài chung","Một class gộp tất cả","Không có","Chỉ CRUD"},0,"Giao diện thanh toán che giấu nhà cung cấp."),
                };
            if (name == "SOLID - SRP")
                return new List<QuizQuestionSeed>
                {
                    Q("SRP nghĩa là gì?", new[]{"Mỗi lớp một trách nhiệm duy nhất","Một lớp làm mọi thứ","Mỗi package một class","Không có interface"},0,"Một lớp một lý do để thay đổi."),
                    Q("Dấu hiệu vi phạm SRP?", new[]{"Lớp God class làm quá nhiều việc","Lớp nhỏ","Nhiều class","Dùng interface"},0,"Quá nhiều chức năng trong một lớp."),
                    Q("Hệ quả khi vi phạm SRP?", new[]{"Khó bảo trì, lỗi lan truyền","Chạy nhanh","Ít code","Dễ test hơn"},0,"Đổi 1 việc ảnh hưởng việc khác."),
                    Q("Lớp UserService gộp: lưu DB + gửi email + log →?", new[]{"Vi phạm SRP","Tuân thủ SRP","Đúng chuẩn","Tối ưu"},0,"Nên tách repository/email/logger."),
                    Q("SRP giúp gì cho testing?", new[]{"Dễ unit test từng trách nhiệm","Khó test","Không test được","Chỉ integration test"},0,"Trách nhiệm nhỏ dễ cô lập test."),
                    Q("\"Lý do để thay đổi\" trong SRP là gì?", new[]{"Một actor/yêu cầu nghiệp vụ duy nhất","Số dòng code","Tên lớp","Vị trí file"},0,"Mỗi trách nhiệm gắn một actor."),
                    Q("Tách class theo SRP thường dẫn đến?", new[]{"Nhiều class nhỏ rõ ràng","Ít file","Một file khổng lồ","Ít interface"},0,"Class nhỏ, nhiệm vụ rõ."),
                    Q("Vi phạm SRP có quan hệ với gì?", new[]{"God class, shotgun surgery","Singleton","Bitmask","Recursion"},0,"God class là ví dụ điển hình."),
                    Q("Để tuân thủ SRP, ta nên?", new[]{"Tách trách nhiệm thành class riêng","Gộp hết","Dùng biến toàn cục","Copy-paste"},0,"Mỗi trách nhiệm một class/service."),
                    Q("SRP nằm trong nhóm nào?", new[]{"SOLID","GRASP chỉ","GoF patterns","XP practices"},0,"S là chữ đầu của SOLID."),
                };
            if (name == "SOLID - OCP")
                return new List<QuizQuestionSeed>
                {
                    Q("OCP nghĩa là gì?", new[]{"Mở cho mở rộng, đóng cho sửa đổi","Đóng mọi thứ","Mở mọi thứ","Không thay đổi"},0,"Thêm hành vi mới không sửa code cũ."),
                    Q("Cách tuân thủ OCP?", new[]{"Dùng interface/strategy và đa hình","Sửa trực tiếp class cũ","Thêm if/switch dài","Copy-paste"},0,"Extension qua interface thay vì sửa cũ."),
                    Q("Thêm kiểu mới mà phải sửa switch cũ →?", new[]{"Vi phạm OCP","Tuân thủ OCP","Đúng chuẩn","Không liên quan"},0,"Switch/if cho từng kiểu phá vỡ OCP."),
                    Q("OCP giúp giảm rủi ro gì?", new[]{"Làm hỏng code đã chạy khi thêm tính năng","Chạy chậm","Tốn RAM","Ít class"},0,"Không đụng code cũ → ít regression."),
                    Q("Strategy pattern liên quan OCP thế nào?", new[]{"Thêm strategy mới mà không sửa context","Phải sửa context mỗi lần","Không liên quan","Làm context dài"},0,"Context dùng interface, strategy mới cài thêm."),
                    Q("Khi giao diện cố định, thêm tính năng qua?", new[]{"Class mới cài interface","Sửa interface tùy ý","Sửa mọi lớp cũ","Dùng static"},0,"Mở rộng bằng class mới."),
                    Q("\"Đóng cho sửa đổi\" nghĩa là gì?", new[]{"Không sửa code class đã chạy ổn định","Không ai được code","File chỉ đọc","Không test"},0,"Tránh sửa code đã verified."),
                    Q("Vi phạm OCP thường xuất hiện ở?", new[]{"Chuỗi if-else kiểm tra type","Lớp nhỏ","Interface","Abstract"},0,"Type checking dài = cần thêm loại phải sửa."),
                    Q("OCP + polymorphism giúp gì?", new[]{"Thêm loại mới không đụng code cũ","Chạy nhanh hơn","Ít code hơn","Tự fix lỗi"},0,"Polymorphism cho phép extension."),
                    Q("Ví dụ vi phạm OCP?", new[]{"function Area() switch theo type","Class AreaStrategy","Interface IShape","Factory"},0,"Area() switch theo từng loại → phải sửa khi thêm loại."),
                };
            if (name == "SOLID - LSP")
                return new List<QuizQuestionSeed>
                {
                    Q("LSP yêu cầu gì?", new[]{"Lớp con thay thế được lớp cha không phá vỡ hành vi","Lớp con không có method","Lớp cha abstract","Chỉ interface"},0,"Đối tượng lớp con dùng được như lớp cha."),
                    Q("Ví dụ kinh điển vi phạm LSP?", new[]{"Square kế thừa Rectangle thay đổi hành vi setWidth","Dog kế thừa Animal","Paypal cài IPayment","Circle cài IShape"},0,"Square phá bất biến Rectangle."),
                    Q("Khi code gọi setWidth(10) trên Rectangle nhưng nhận Square?", new[]{"Hành vi sai: chiều cao cũng đổi","Vẫn đúng","Không sao","Tăng tốc"},0,"Square override làm bất biến của Rectangle vỡ."),
                    Q("Hệ quả vi phạm LSP?", new[]{"Bug chỉ lộ khi dùng đa hình","Bug lúc compile","Không bug","Chỉ chậm"},0,"Lỗi tinh vi khi thay thế ở runtime."),
                    Q("Cách tránh vi phạm LSP?", new[]{"Chỉ kế thừa khi is-a thật sự, giữ contract cha","Kế thừa bừa bãi","Sửa lớp cha tùy ý","Dùng static"},0,"Tôn trọng contract của lớp cha."),
                    Q("Contract của lớp cha gồm gì?", new[]{"Bất biến + tiền/điều kiện hậu","Tên method","Thứ tự file","Comment"},0,"Bất biến và hành vi kỳ vọng."),
                    Q("Thay Rectangle bằng Square trong hàm dùng Rectangle?", new[]{"Không an toàn","An toàn tuyệt đối","Luôn tốt","Không nên thử"},0,"Vi phạm LSP làm hàm cha sai."),
                    Q("LSP gắn với kỹ thuật nào?", new[]{"Design by contract","Code review","Pair programming","TDD"},0,"Contract của cha phải được con giữ."),
                    Q("Lớp con ném exception mới không có ở cha →?", new[]{"Vi phạm LSP","Tuân thủ","Bình thường","Tối ưu"},0,"Phá vỡ kỳ vọng hành vi của cha."),
                    Q("Cách thay thế an toàn khi is-a không rõ ràng?", new[]{"Dùng composition/interface","Ép kế thừa","Dùng enum","Dùng static"},0,"Ưu tiên has-a khi không thực sự is-a."),
                };
            if (name == "SOLID - DIP")
                return new List<QuizQuestionSeed>
                {
                    Q("DIP nghĩa là gì?", new[]{"Module cao phụ thuộc trừu tượng, không phụ thuộc chi tiết","Module cao phụ thuộc module thấp","Không dùng interface","Chi tiết quyết định"},0,"Phụ thuộc abstraction."),
                    Q("DIP khác DI (Dependency Injection)?", new[]{"DIP là nguyên lý, DI là kỹ thuật triển khai","Giống hệt nhau","DI là nguyên lý","Không liên quan"},0,"DI (constructor injection) là cách thực hiện DIP."),
                    Q("Code `new EmailService()` trong UserService →?", new[]{"Vi phạm DIP","Tuân thủ DIP","Đúng chuẩn","Tối ưu"},0,"Phụ thuộc chi tiết cụ thể."),
                    Q("Để tuân thủ DIP, UserService nên phụ thuộc vào?", new[]{"INotificationService (interface)","EmailService cụ thể","SmsService","Database trực tiếp"},0,"Phụ thuộc interface."),
                    Q("Constructor injection giúp gì?", new[]{"Dễ swap implementation và test","Không cần interface","Chạy nhanh","Ít code"},0,"Truyền interface qua constructor."),
                    Q("Lợi ích của DIP?", new[]{"Giảm coupling, dễ mở rộng/test","Tăng coupling","Khó test","Chỉ đẹp code"},0,"Đổi implementation không sửa code dùng."),
                    Q("Trong DIP, ai định nghĩa interface?", new[]{"Module cấp cao (policy)","Module cấp thấp","Database","Không ai"},0,"Abstraction thuộc về policy."),
                    Q("Ví dụ vi phạm DIP?", new[]{"Lớp Report gọi trực tiếp PdfExporter","Report dùng IExporter","Dùng factory","Dùng DI container"},0,"Phụ thuộc trực tiếp chi tiết."),
                    Q("Với DIP, thay Email → SMS cần sửa gì?", new[]{"Đăng ký DI cho implementation mới","Sửa mọi nơi gọi","Đổi interface","Xóa code"},0,"Đổi binding ở composition root."),
                    Q("Tầng nơi lắp ghép dependency gọi là gì?", new[]{"Composition root","Business layer","UI layer","Repository"},0,"Composition root nơi DI bind."),
                };
            return new List<QuizQuestionSeed>
            {
                Q("Strategy pattern cho phép gì?", new[]{"Đổi thuật toán runtime mà không sửa context","Gộp mọi thuật toán","Chỉ một thuật toán","Tự tối ưu"},0,"Strategy đóng gói từng thuật toán riêng."),
                Q("Observer pattern làm gì?", new[]{"Thông báo các subscriber khi trạng thái đổi","Đồng bộ dữ liệu","Chống vòng lặp","Tăng tốc"},0,"Một-nhiều, subscriber được notify."),
                Q("Factory pattern dùng khi nào?", new[]{"Tạo đối tượng tách khỏi nơi dùng","Xóa đối tượng","Clone","Serialize"},0,"Factory che giấu logic khởi tạo."),
                Q("Singleton đảm bảo gì?", new[]{"Một instance toàn cục","Nhiều instance","Instance bất biến","Instance tự hủy"},0,"Một instance dùng chung."),
                Q("Nhược điểm Singleton?", new[]{"Khó test, trạng thái toàn cục","Nhanh","Tiết kiệm RAM","Không có"},0,"Ẩn phụ thuộc toàn cục."),
                Q("Decorator pattern?", new[]{"Bọc đối tượng để thêm hành vi","Thay thế đối tượng","Xóa đối tượng","Sắp xếp"},0,"Thêm trách nhiệm linh hoạt."),
                Q("Adapter pattern?", new[]{"Chuyển giao diện sang giao diện mong đợi","Tăng tốc","Nén dữ liệu","Cache"},0,"Làm hợp giao diện không tương thích."),
                Q("Builder pattern hữu ích khi?", new[]{"Đối tượng nhiều tham số khởi tạo","Singleton","Đối tượng không constructor","Bitmask"},0,"Xây dựng từng bước rõ ràng."),
                Q("Pattern nào giảm coupling nhất giữa module?", new[]{"Observer","Singleton","Prototype","Flyweight"},0,"Event-driven decouple hoàn toàn."),
                Q("Pattern đóng gói yêu cầu thành đối tượng?", new[]{"Command","Singleton","Observer","Factory"},0,"Command biến hành động thành object."),
            };
        }
    }
}
