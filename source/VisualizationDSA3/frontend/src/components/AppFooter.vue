<template>
  <footer class="bg-bg-secondary border-t border-border-default mt-auto">
    <div class="max-w-7xl mx-auto px-6 py-12 md:py-16">
      <div class="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-5 gap-8">
        
        <!-- Brand Info -->
        <div class="col-span-2 lg:col-span-2 pr-8">
          <router-link to="/" class="flex items-center gap-2 mb-4">
             <div class="w-8 h-8 rounded bg-accent-purple/20 flex items-center justify-center text-accent-purple font-bold font-mono">
                ~/
             </div>
             <span class="font-bold text-xl tracking-tight">VisualizationDSA</span>
          </router-link>
          <p class="text-text-muted text-sm leading-relaxed mb-6">
            Nền tảng EdTech tiên phong trong việc sử dụng trí tuệ nhân tạo (AI) để trực quan hóa cấu trúc dữ liệu và thuật toán, giúp việc học lập trình trở nên sinh động và dễ hiểu hơn bao giờ hết.
          </p>
          <div class="flex gap-4">
            <a href="#" class="w-10 h-10 rounded-full bg-bg-surface border border-border-subtle flex items-center justify-center text-text-muted hover:text-accent-purple hover:border-accent-purple transition-colors">
              <BaseIcon name="hub" class="w-5 h-5" />
            </a>
            <a href="#" class="w-10 h-10 rounded-full bg-bg-surface border border-border-subtle flex items-center justify-center text-text-muted hover:text-accent-cyan hover:border-accent-cyan transition-colors">
              <BaseIcon name="forum" class="w-5 h-5" />
            </a>
            <a href="https://github.com/maitieubao/VisualizationDSA" target="_blank" class="w-10 h-10 rounded-full bg-bg-surface border border-border-subtle flex items-center justify-center text-text-muted hover:text-white hover:border-white transition-colors">
              <svg width="20" height="20" fill="currentColor" viewBox="0 0 24 24"><path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z"/></svg>
            </a>
          </div>
        </div>

        <!-- Khóa học -->
        <div>
          <h3 class="font-bold text-white mb-4">Khóa học</h3>
          <ul class="space-y-3">
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-purple transition-colors">Cấu trúc dữ liệu cơ bản</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-purple transition-colors">Thuật toán Sắp xếp</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-purple transition-colors">Cây & Đồ thị nâng cao</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-purple transition-colors">Design Patterns (OOP)</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-purple transition-colors">Luyện thi Phỏng vấn IT</a></li>
          </ul>
        </div>

        <!-- Công cụ -->
        <div>
          <h3 class="font-bold text-white mb-4">Công cụ AI</h3>
          <ul class="space-y-3">
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-cyan transition-colors">Trình biên dịch Online</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-cyan transition-colors">Code-to-Viz Generator</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-cyan transition-colors">Phân tích độ phức tạp (Big O)</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-accent-cyan transition-colors">AI Code Tutor</a></li>
          </ul>
        </div>

        <!-- Hỗ trợ -->
        <div>
          <h3 class="font-bold text-white mb-4">Về chúng tôi</h3>
          <ul class="space-y-3">
            <li><a href="#" class="text-sm text-text-muted hover:text-white transition-colors">Giới thiệu nền tảng</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-white transition-colors">Gói cước Premium</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-white transition-colors">Trung tâm trợ giúp</a></li>
            <li><a href="#" class="text-sm text-text-muted hover:text-white transition-colors">Điều khoản bảo mật</a></li>
          </ul>
        </div>

      </div>

      <!-- Bottom Bar -->
      <div class="mt-12 pt-8 border-t border-border-default flex flex-col md:flex-row justify-between items-center gap-4">
        <p class="text-xs text-text-disabled">
          &copy; {{ new Date().getFullYear() }} VisualizationDSA. Phát triển bởi Mai Tiêu Bảo. All rights reserved.
        </p>
        <div class="flex items-center gap-6">
          <a href="#" class="text-xs text-text-disabled hover:text-white transition-colors">Điều khoản</a>
          <a href="#" class="text-xs text-text-disabled hover:text-white transition-colors">Quyền riêng tư</a>
          <a href="#" class="text-xs text-text-disabled hover:text-white transition-colors">Sitemap</a>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
// Component không chứa logic phức tạp
</script>
