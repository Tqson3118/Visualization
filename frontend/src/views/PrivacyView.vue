<script setup lang="ts">
// PrivacyView — Màn 12: chính sách bảo mật (1 cột, tối đa 760px).
// View-quality (nhóm A): bỏ hero aurora-soft + gradient icon/title + shadow → surface band
// level-2; TOC raw <button> → native anchor <a href="#sec-N"> (Lenis anchors:true xử lý smooth);
// mục lục + section có index mono (dữ liệu tuần tự → quyết định 4); H1 48px/600/-0.03em.
// GIỮ NGUYÊN nội dung chính sách + id section (anchor).
import { RouterLink } from 'vue-router';
import { Motion } from 'motion-v';
import { ShieldCheck } from 'lucide-vue-next';

const SECTIONS = [
  { id: 'sec-1', title: 'Dữ liệu chúng tôi thu thập' },
  { id: 'sec-2', title: 'Cookie & phiên đăng nhập' },
  { id: 'sec-3', title: 'Chia sẻ dữ liệu' },
  { id: 'sec-4', title: 'Quyền của bạn' },
  { id: 'sec-5', title: 'Bảo mật' },
  { id: 'sec-6', title: 'Liên hệ' },
] as const;
</script>

<template>
  <main class="privacy container">
    <!-- Hero — surface band level-2 (bỏ gradient aurora + shadow, §1/§6) -->
    <Motion
      class="privacy__chrome"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <nav class="privacy__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'home' }">Trang chủ</RouterLink>
        <span aria-hidden="true">/</span>
        <span aria-current="page">Chính sách bảo mật</span>
      </nav>
      <div class="privacy__hero">
        <span class="privacy__icon" aria-hidden="true">
          <ShieldCheck :size="20" />
        </span>
        <div>
          <h1 class="privacy__title">Chính sách bảo mật</h1>
          <p class="privacy__updated">CẬP NHẬT 12/08/2026</p>
        </div>
      </div>
    </Motion>

    <div class="privacy__layout">
      <!-- TOC sticky (desktop) — anchor native, index mono -->
      <nav class="privacy__toc" aria-label="Mục lục">
        <p class="privacy__toc-title">MỤC LỤC</p>
        <ul class="privacy__toc-list">
          <li v-for="(section, idx) in SECTIONS" :key="section.id">
            <a :href="`#${section.id}`" class="privacy__toc-link">
              <span class="privacy__toc-index" aria-hidden="true">{{ String(idx + 1).padStart(2, '0') }}</span>
              <span>{{ section.title }}</span>
            </a>
          </li>
        </ul>
      </nav>

      <article class="privacy__content">
        <section id="sec-1" class="privacy__section">
          <h2 class="privacy__h2">
            <span class="privacy__idx" aria-hidden="true">01</span>
            {{ SECTIONS[0].title }}
          </h2>
          <p>
            Khi bạn đăng ký tài khoản, chúng tôi thu thập: họ tên, email, vai trò (học viên/giảng viên).
            Trong quá trình học, chúng tôi ghi nhận tiến độ học tập (bài đã xem, điểm bài tập, mô phỏng
            đã chạy) để cá nhân hóa lộ trình và tính điểm thưởng (XP, gems).
          </p>
        </section>

        <section id="sec-2" class="privacy__section">
          <h2 class="privacy__h2">
            <span class="privacy__idx" aria-hidden="true">02</span>
            {{ SECTIONS[1].title }}
          </h2>
          <p>
            Chúng tôi dùng cookie HTTP-only để duy trì phiên đăng nhập an toàn (refresh token).
            Access token chỉ tồn tại trong bộ nhớ trình duyệt và tự làm mới qua cookie. Bạn có thể
            đăng xuất bất cứ lúc nào để thu hồi phiên.
          </p>
        </section>

        <section id="sec-3" class="privacy__section">
          <h2 class="privacy__h2">
            <span class="privacy__idx" aria-hidden="true">03</span>
            {{ SECTIONS[2].title }}
          </h2>
          <p>
            Chúng tôi KHÔNG bán dữ liệu cá nhân cho bên thứ ba. Giảng viên của lớp bạn chỉ thấy dữ liệu
            học tập của bạn trong lớp (bài đã xem, điểm, trạng thái nộp) — email được che một phần
            (VD: m***h@truong.edu.vn) trừ khi bạn là người dùng đó.
          </p>
        </section>

        <section id="sec-4" class="privacy__section">
          <h2 class="privacy__h2">
            <span class="privacy__idx" aria-hidden="true">04</span>
            {{ SECTIONS[3].title }}
          </h2>
          <p>
            Bạn có quyền: xem dữ liệu của mình (trang Hồ sơ), đổi mật khẩu (tab Cài đặt), yêu cầu
            xóa tài khoản (liên hệ quản trị viên — tài khoản sẽ được ẩn danh hóa, dữ liệu học tập
            được giữ để phục vụ thống kê nhưng không còn liên kết tới bạn).
          </p>
        </section>

        <section id="sec-5" class="privacy__section">
          <h2 class="privacy__h2">
            <span class="privacy__idx" aria-hidden="true">05</span>
            {{ SECTIONS[4].title }}
          </h2>
          <p>
            Mật khẩu được băm bằng thuật toán mạnh (PBKDF2/Argon2) — chúng tôi không bao giờ lưu
            mật khẩu dạng văn bản thuần. Mọi truyền tải qua HTTPS.
          </p>
        </section>

        <section id="sec-6" class="privacy__section">
          <h2 class="privacy__h2">
            <span class="privacy__idx" aria-hidden="true">06</span>
            {{ SECTIONS[5].title }}
          </h2>
          <p>
            Mọi thắc mắc về chính sách bảo mật, gửi email tới quản trị viên qua trang
            <RouterLink :to="{ name: 'help' }">Trợ giúp</RouterLink>.
          </p>
        </section>
      </article>
    </div>
  </main>
</template>

<style scoped>
.privacy {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Hero — surface band level-2 (§6): card-raised + border-subtle, KHÔNG shadow ── */
.privacy__chrome {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.privacy__breadcrumb {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

.privacy__breadcrumb a {
  color: var(--color-primary);
  font-weight: 600;
  padding-block: var(--space-xs);
}

.privacy__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.privacy__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.privacy__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.privacy__updated {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  color: var(--color-text-tertiary);
  margin-top: var(--space-xs);
}

/* ── Layout: TOC + content ── */
.privacy__layout {
  display: grid;
  grid-template-columns: 240px minmax(0, 1fr);
  gap: var(--space-xl);
  align-items: start;
}

.privacy__toc {
  position: sticky;
  top: 88px;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-card);
}

.privacy__toc-title {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}

.privacy__toc-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  margin: 0;
  padding: 0;
}

.privacy__toc-link {
  display: flex;
  align-items: baseline;
  gap: var(--space-sm);
  width: 100%;
  text-align: left;
  padding: var(--space-sm) 12px;
  border-radius: var(--radius-sm);
  font-size: var(--text-sm);
  line-height: 1.4;
  color: var(--color-text-secondary);
  transition: background 150ms cubic-bezier(0.16, 1, 0.3, 1), color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.privacy__toc-link:hover {
  background: var(--color-surface-hover);
  color: var(--color-foreground);
  text-decoration: none;
}

.privacy__toc-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  flex-shrink: 0;
}

.privacy__content {
  max-width: 760px;
  display: flex;
  flex-direction: column;
}

.privacy__section {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding-block: var(--space-md);
  scroll-margin-top: 24px;
}

.privacy__section + .privacy__section {
  border-top: 1px solid var(--color-border);
}

.privacy__h2 {
  display: flex;
  align-items: baseline;
  gap: var(--space-sm);
  font-size: var(--text-xl);
  font-weight: 600;
  line-height: 1.25;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
  margin: 0;
}

.privacy__idx {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  color: var(--color-text-tertiary);
  flex-shrink: 0;
}

.privacy__content p {
  font-size: var(--text-sm);
  line-height: 1.8;
  color: var(--color-foreground);
  max-width: 72ch;
  margin: 0;
}

@media (max-width: 800px) {
  .privacy__chrome { padding: var(--space-md); }
  .privacy__layout { grid-template-columns: 1fr; gap: var(--space-md); }
  .privacy__toc { position: static; }
}
</style>
