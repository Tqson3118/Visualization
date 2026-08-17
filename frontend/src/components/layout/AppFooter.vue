<script setup lang="ts">
// AppFooter — bê NGUYÊN CẤU TRÚC footer VisualizationDSA3 (AppFooter.vue):
// brand + mô tả + social → 3 cột link → bottom bar bản quyền.
// Đổi namespace màu sang bối cảnh web hiện tại (tím vũ trụ #0D0C14/#A855F7)
// và link trỏ route THẬT của DSA Visual.
import { RouterLink } from 'vue-router';
</script>

<template>
  <footer class="app-footer">
    <div class="container app-footer__inner">
      <div class="app-footer__grid">
        <!-- Brand Info -->
        <div class="app-footer__brand">
          <RouterLink to="/" class="app-footer__logo-wrap">
            <span class="app-footer__logo" aria-hidden="true">~/</span>
            <span class="app-footer__logo-name">DSA Visual</span>
          </RouterLink>
          <p class="app-footer__desc">
            Nền tảng học tập trực quan hóa Cấu trúc Dữ liệu &amp; Giải thuật tiếng Việt đầu tiên — xem
            thuật toán chạy thật từng bước, luyện tập theo lộ trình và chấm code tự động ngay trên trình duyệt.
          </p>
          <div class="app-footer__social">
            <a href="#" aria-label="Cộng đồng" class="app-footer__social-btn">
              <svg width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.8" viewBox="0 0 24 24">
                <circle cx="12" cy="12" r="3.2" />
                <path d="M2 12h4.5M17.5 12H22M12 2v4.5M12 17.5V22" stroke-linecap="round" />
              </svg>
            </a>
            <a href="#" aria-label="Diễn đàn" class="app-footer__social-btn">
              <svg width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.8" viewBox="0 0 24 24">
                <path d="M21 11.5a8.38 8.38 0 0 1-8.5 8.5 8.5 8.5 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 8.5-8.5 8.38 8.38 0 0 1 8.5 8.5z" stroke-linejoin="round" />
              </svg>
            </a>
            <a href="#" aria-label="GitHub" class="app-footer__social-btn">
              <svg width="18" height="18" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0 0 24 12c0-6.63-5.37-12-12-12z" />
              </svg>
            </a>
          </div>
        </div>

        <!-- Khóa học -->
        <div>
          <h3 class="app-footer__heading">Khóa học</h3>
          <ul class="app-footer__list">
            <li><RouterLink to="/courses" class="app-footer__link">Lộ trình Grokking DSA</RouterLink></li>
            <li><RouterLink to="/simulations" class="app-footer__link">Mô phỏng thuật toán</RouterLink></li>
            <li><RouterLink to="/classes" class="app-footer__link">Lớp học trực tuyến</RouterLink></li>
            <li><RouterLink to="/quests" class="app-footer__link">Thử thách &amp; Quest</RouterLink></li>
            <li><RouterLink to="/leaderboard" class="app-footer__link">Bảng xếp hạng</RouterLink></li>
          </ul>
        </div>

        <!-- Công cụ -->
        <div>
          <h3 class="app-footer__heading">Công cụ</h3>
          <ul class="app-footer__list">
            <li><RouterLink to="/simulations" class="app-footer__link">Trình mô phỏng từng bước</RouterLink></li>
            <li><RouterLink to="/simulations" class="app-footer__link">Phân tích độ phức tạp (Big O)</RouterLink></li>
            <li><RouterLink to="/courses" class="app-footer__link">Code Lab chấm tự động</RouterLink></li>
            <li><RouterLink to="/courses" class="app-footer__link">Trắc nghiệm lộ trình</RouterLink></li>
          </ul>
        </div>

        <!-- Về chúng tôi -->
        <div>
          <h3 class="app-footer__heading">Về chúng tôi</h3>
          <ul class="app-footer__list">
            <li><RouterLink to="/" class="app-footer__link">Giới thiệu nền tảng</RouterLink></li>
            <li><RouterLink to="/premium" class="app-footer__link">Gói cước Premium</RouterLink></li>
            <li><RouterLink to="/help" class="app-footer__link">Trung tâm trợ giúp</RouterLink></li>
            <li><RouterLink to="/privacy" class="app-footer__link">Chính sách bảo mật</RouterLink></li>
          </ul>
        </div>
      </div>

      <!-- Bottom Bar -->
      <div class="app-footer__bottom">
        <p class="app-footer__copy">
          &copy; {{ new Date().getFullYear() }} DSA Visual. All rights reserved.
        </p>
        <div class="app-footer__legal">
          <RouterLink to="/privacy" class="app-footer__legal-link">Điều khoản</RouterLink>
          <RouterLink to="/privacy" class="app-footer__legal-link">Quyền riêng tư</RouterLink>
          <RouterLink to="/" class="app-footer__legal-link">Sitemap</RouterLink>
        </div>
      </div>
    </div>
  </footer>
</template>

<style scoped>
/* Palette tím vũ trụ — khớp bối cảnh web hiện tại (#0D0C14 nền, #A855F7 accent) */
.app-footer {
  position: relative;
  margin-top: var(--space-2xl);
  border-top: 1px solid rgba(255, 255, 255, 0.07);
  background:
    radial-gradient(ellipse 60% 80% at 50% 120%, rgba(168, 85, 247, 0.07), transparent 70%),
    #0d0c14;
}

.app-footer__inner {
  padding-block: var(--space-2xl) var(--space-lg);
}

.app-footer__grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--space-xl);
}

@media (min-width: 768px) {
  .app-footer__grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}

@media (min-width: 1080px) {
  .app-footer__grid {
    grid-template-columns: 2fr 1fr 1fr 1fr;
    gap: var(--space-xl);
  }
}

/* Brand */
.app-footer__brand {
  padding-right: var(--space-lg);
}

.app-footer__logo-wrap {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  margin-bottom: var(--space-md);
  text-decoration: none;
}

.app-footer__logo {
  width: 34px;
  height: 34px;
  border-radius: var(--radius-md);
  background: rgba(168, 85, 247, 0.15);
  border: 1px solid rgba(168, 85, 247, 0.35);
  color: #c084fc;
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: var(--text-sm);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.app-footer__logo-name {
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: var(--text-lg);
  letter-spacing: 0.02em;
  color: #fff;
}

.app-footer__desc {
  margin: 0 0 var(--space-md);
  font-size: var(--text-sm);
  line-height: 1.7;
  color: rgba(255, 255, 255, 0.5);
}

.app-footer__social {
  display: flex;
  gap: 10px;
}

.app-footer__social-btn {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.08);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  color: rgba(255, 255, 255, 0.45);
  transition: color 150ms ease, border-color 150ms ease, box-shadow 150ms ease;
}

.app-footer__social-btn:hover {
  color: #c084fc;
  border-color: rgba(168, 85, 247, 0.5);
  box-shadow: 0 0 14px rgba(168, 85, 247, 0.2);
}

/* Cột link */
.app-footer__heading {
  margin: 0 0 var(--space-md);
  font-size: var(--text-sm);
  font-weight: 700;
  letter-spacing: 0.03em;
  color: #fff;
}

.app-footer__list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.app-footer__link {
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.45);
  text-decoration: none;
  transition: color 150ms ease;
}

.app-footer__link:hover {
  color: #c084fc;
}

/* Bottom bar */
.app-footer__bottom {
  margin-top: var(--space-xl);
  padding-top: var(--space-md);
  border-top: 1px solid rgba(255, 255, 255, 0.07);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
}

@media (min-width: 768px) {
  .app-footer__bottom {
    flex-direction: row;
  }
}

.app-footer__copy {
  margin: 0;
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.3);
}

.app-footer__legal {
  display: flex;
  gap: var(--space-lg);
}

.app-footer__legal-link {
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.3);
  text-decoration: none;
  transition: color 150ms ease;
}

.app-footer__legal-link:hover {
  color: #fff;
}
</style>
