<script setup lang="ts">
// PrivacyView — Màn 12: chính sách bảo mật (1 cột, tối đa 760px)
// H-E2: hero Aurora soft + breadcrumb + TOC sticky (Lenis anchors:true → anchor #sec-N cuộn mượt).
// GIỮ NGUYÊN nội dung chính sách.
import { RouterLink } from 'vue-router';
import { ShieldCheck } from 'lucide-vue-next';

const SECTIONS = [
  { id: 'sec-1', title: '1. Dữ liệu chúng tôi thu thập' },
  { id: 'sec-2', title: '2. Cookie & phiên đăng nhập' },
  { id: 'sec-3', title: '3. Chia sẻ dữ liệu' },
  { id: 'sec-4', title: '4. Quyền của bạn' },
  { id: 'sec-5', title: '5. Bảo mật' },
  { id: 'sec-6', title: '6. Liên hệ' },
] as const;

function scrollToSection(id: string): void {
  document.getElementById(id)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
}
</script>

<template>
  <main class="privacy container">
    <!-- Hero — Aurora soft -->
    <header class="privacy__chrome">
      <nav class="privacy__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'home' }">Trang chủ</RouterLink>
        <span aria-hidden="true">/</span>
        <span>Chính sách bảo mật</span>
      </nav>
      <div class="privacy__hero">
        <span class="privacy__icon" aria-hidden="true">
          <ShieldCheck :size="22" />
        </span>
        <div>
          <h1 class="privacy__title">Chính sách bảo mật</h1>
          <p class="privacy__updated">Cập nhật lần cuối: 12/08/2026</p>
        </div>
      </div>
    </header>

    <div class="privacy__layout">
      <!-- TOC sticky (desktop) -->
      <nav class="privacy__toc" aria-label="Mục lục">
        <p class="privacy__toc-title">Mục lục</p>
        <ul class="privacy__toc-list">
          <li v-for="section in SECTIONS" :key="section.id">
            <button type="button" class="privacy__toc-link" @click="scrollToSection(section.id)">
              {{ section.title }}
            </button>
          </li>
        </ul>
      </nav>

      <article class="privacy__content">
        <section id="sec-1" class="privacy__section">
          <h2>{{ SECTIONS[0].title }}</h2>
          <p>
            Khi bạn đăng ký tài khoản, chúng tôi thu thập: họ tên, email, vai trò (học viên/giảng viên).
            Trong quá trình học, chúng tôi ghi nhận tiến độ học tập (bài đã xem, điểm bài tập, mô phỏng
            đã chạy) để cá nhân hóa lộ trình và tính điểm thưởng (XP, gems).
          </p>
        </section>

        <section id="sec-2" class="privacy__section">
          <h2>{{ SECTIONS[1].title }}</h2>
          <p>
            Chúng tôi dùng cookie HTTP-only để duy trì phiên đăng nhập an toàn (refresh token).
            Access token chỉ tồn tại trong bộ nhớ trình duyệt và tự làm mới qua cookie. Bạn có thể
            đăng xuất bất cứ lúc nào để thu hồi phiên.
          </p>
        </section>

        <section id="sec-3" class="privacy__section">
          <h2>{{ SECTIONS[2].title }}</h2>
          <p>
            Chúng tôi KHÔNG bán dữ liệu cá nhân cho bên thứ ba. Giảng viên của lớp bạn chỉ thấy dữ liệu
            học tập của bạn trong lớp (bài đã xem, điểm, trạng thái nộp) — email được che một phần
            (VD: m***h@truong.edu.vn) trừ khi bạn là người dùng đó.
          </p>
        </section>

        <section id="sec-4" class="privacy__section">
          <h2>{{ SECTIONS[3].title }}</h2>
          <p>
            Bạn có quyền: xem dữ liệu của mình (trang Hồ sơ), đổi mật khẩu (tab Cài đặt), yêu cầu
            xóa tài khoản (liên hệ quản trị viên — tài khoản sẽ được ẩn danh hóa, dữ liệu học tập
            được giữ để phục vụ thống kê nhưng không còn liên kết tới bạn).
          </p>
        </section>

        <section id="sec-5" class="privacy__section">
          <h2>{{ SECTIONS[4].title }}</h2>
          <p>
            Mật khẩu được băm bằng thuật toán mạnh (PBKDF2/Argon2) — chúng tôi không bao giờ lưu
            mật khẩu dạng văn bản thuần. Mọi truyền tải qua HTTPS.
          </p>
        </section>

        <section id="sec-6" class="privacy__section">
          <h2>{{ SECTIONS[5].title }}</h2>
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

/* ── Hero — Aurora soft ── */
.privacy__chrome {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 22%, var(--color-border));
  border-radius: var(--radius-xl);
  background-color: var(--aurora-soft);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.privacy__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.privacy__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.privacy__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.privacy__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: #fff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.privacy__title {
  font-size: clamp(var(--text-2xl), 4vw, var(--text-3xl));
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.privacy__updated {
  font-size: var(--text-xs);
  /* H-E2: foreground 92% — text-muted chỉ 4.26:1 trên aurora-soft light (sát fail AA) */
  color: color-mix(in srgb, var(--color-foreground) 92%, transparent);
  margin-top: 2px;
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
  background: var(--color-surface);
  box-shadow: var(--shadow-sm);
}

.privacy__toc-title {
  font-size: var(--text-xs);
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-muted);
}

.privacy__toc-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.privacy__toc-link {
  width: 100%;
  text-align: left;
  background: none;
  border: none;
  padding: 6px 8px;
  border-radius: var(--radius-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  cursor: pointer;
  transition: background 150ms ease, color 150ms ease;
  line-height: 1.4;
}

.privacy__toc-link:hover {
  background: var(--color-surface-hover);
  color: var(--color-foreground);
  text-decoration: none;
}

.privacy__content {
  max-width: 760px;
  display: flex;
  flex-direction: column;
}

.privacy__section {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  padding-block: var(--space-md);
  scroll-margin-top: 24px;
}

.privacy__section + .privacy__section {
  border-top: 1px solid var(--color-border);
}

.privacy__content h2 {
  font-size: var(--text-md);
  color: var(--color-foreground);
}

.privacy__content p {
  font-size: var(--text-sm);
  line-height: 1.8;
  color: var(--color-foreground);
  max-width: 72ch;
}

@media (max-width: 800px) {
  .privacy__chrome { padding: var(--space-md); }
  .privacy__layout { grid-template-columns: 1fr; gap: var(--space-md); }
  .privacy__toc { position: static; }
}
</style>
