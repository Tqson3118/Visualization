<script setup lang="ts">
// PrivacyView — Màn 12: chính sách bảo mật (1 cột, tối đa 760px)
</script>

<template>
  <main class="privacy container">
    <article class="privacy__content">
      <h1 class="privacy__title">Chính sách bảo mật</h1>
      <p class="text-muted privacy__updated">Cập nhật lần cuối: 12/08/2026</p>

      <section>
        <h2>1. Dữ liệu chúng tôi thu thập</h2>
        <p>
          Khi bạn đăng ký tài khoản, chúng tôi thu thập: họ tên, email, vai trò (học viên/giảng viên).
          Trong quá trình học, chúng tôi ghi nhận tiến độ học tập (bài đã xem, điểm bài tập, mô phỏng
          đã chạy) để cá nhân hóa lộ trình và tính điểm thưởng (XP, gems).
        </p>
      </section>

      <section>
        <h2>2. Cookie & phiên đăng nhập</h2>
        <p>
          Chúng tôi dùng cookie HTTP-only để duy trì phiên đăng nhập an toàn (refresh token).
          Access token chỉ tồn tại trong bộ nhớ trình duyệt và tự làm mới qua cookie. Bạn có thể
          đăng xuất bất cứ lúc nào để thu hồi phiên.
        </p>
      </section>

      <section>
        <h2>3. Chia sẻ dữ liệu</h2>
        <p>
          Chúng tôi KHÔNG bán dữ liệu cá nhân cho bên thứ ba. Giảng viên của lớp bạn chỉ thấy dữ liệu
          học tập của bạn trong lớp (bài đã xem, điểm, trạng thái nộp) — email được che một phần
          (VD: m***h@truong.edu.vn) trừ khi bạn là người dùng đó.
        </p>
      </section>

      <section>
        <h2>4. Quyền của bạn</h2>
        <p>
          Bạn có quyền: xem dữ liệu của mình (trang Hồ sơ), đổi mật khẩu (tab Cài đặt), yêu cầu
          xóa tài khoản (liên hệ quản trị viên — tài khoản sẽ được ẩn danh hóa, dữ liệu học tập
          được giữ để phục vụ thống kê nhưng không còn liên kết tới bạn).
        </p>
      </section>

      <section>
        <h2>5. Bảo mật</h2>
        <p>
          Mật khẩu được băm bằng thuật toán mạnh (PBKDF2/Argon2) — chúng tôi không bao giờ lưu
          mật khẩu dạng văn bản thuần. Mọi truyền tải qua HTTPS.
        </p>
      </section>

      <section>
        <h2>6. Liên hệ</h2>
        <p>
          Mọi thắc mắc về chính sách bảo mật, gửi email tới quản trị viên qua trang
          <RouterLink :to="{ name: 'help' }">Trợ giúp</RouterLink>.
        </p>
      </section>
    </article>
  </main>
</template>

<style scoped>
.privacy {
  padding-block: var(--space-lg) var(--space-2xl);
}

.privacy__content { max-width: 760px; margin-inline: auto; display: flex; flex-direction: column; gap: var(--space-lg); }

.privacy__title { font-size: var(--text-2xl); }
.privacy__updated { font-size: var(--text-xs); }

.privacy__content section { display: flex; flex-direction: column; gap: var(--space-xs); }

.privacy__content h2 { font-size: var(--text-md); }

.privacy__content p { font-size: var(--text-sm); line-height: 1.8; color: var(--color-foreground); }
</style>
