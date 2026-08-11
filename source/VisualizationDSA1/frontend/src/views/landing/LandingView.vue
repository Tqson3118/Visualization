<template>
  <div class="landing">
    
    <!-- ── HERO SECTION ── -->
    <section class="hero relative overflow-hidden">
      <!-- Gradient Mesh Background -->
      <div class="hero__mesh" aria-hidden="true">
        <div class="mesh-blob mesh-blob--1"></div>
        <div class="mesh-blob mesh-blob--2"></div>
        <div class="mesh-blob mesh-blob--3"></div>
      </div>
      
      <!-- Particle System -->
      <vue-particles
        id="tsparticles"
        class="absolute inset-0 pointer-events-auto z-0"
        :options="particleOptions"
      />
      
      <div class="hero__glow z-0"></div>
      <div class="hero__particles z-0"></div>
      
      <div class="hero__content" data-aos="fade-up" data-aos-duration="1000">
        <div class="hero__badge spring-hover">
          <span class="pulse-dot"></span>
          <span>Nền tảng học thuật chuẩn bị ra mắt</span>
        </div>
        
        <h1 class="hero__title font-display">
          <span class="hero__prefix text-accent drop-shadow-md">~/</span>
          Khám phá thuật toán theo cách <span class="text-gradient">sống động nhất</span>
        </h1>
        
        <p class="hero__sub font-sans">
          Trực quan hóa cấu trúc dữ liệu, đồ thị, và design patterns. 
          Hệ thống gamification giúp bạn biến việc học code thành một hành trình thú vị.
        </p>
        
        <div class="hero__actions">
          <button class="btn-primary hero-btn spring-hover" @click="handleCta">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <polygon points="5 3 19 12 5 21 5 3"></polygon>
            </svg>
            {{ authStore.isAuthenticated ? 'Vào bảng điều khiển' : 'Bắt đầu học ngay' }}
          </button>
          
          <button class="btn-ghost hero-btn spring-hover" @click="handleBrowse">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="3" width="7" height="7"></rect>
              <rect x="14" y="3" width="7" height="7"></rect>
              <rect x="14" y="14" width="7" height="7"></rect>
              <rect x="3" y="14" width="7" height="7"></rect>
            </svg>
            Khám phá Thư viện thuật toán
          </button>
        </div>
        
        <!-- Trust indicators -->
        <div class="hero__trust" data-aos="fade-up" data-aos-delay="300">
          <div class="trust-item">
            <span class="trust-number">30+</span>
            <span class="trust-label">Thuật toán trực quan hóa</span>
          </div>
          <div class="trust-divider"></div>
          <div class="trust-item">
            <span class="trust-number">4</span>
            <span class="trust-label">Bước học mỗi bài</span>
          </div>
          <div class="trust-divider"></div>
          <div class="trust-item">
            <span class="trust-number">100%</span>
            <span class="trust-label">Tiếng Việt</span>
          </div>
          <div class="trust-divider"></div>
          <div class="trust-item">
            <span class="trust-number">∞</span>
            <span class="trust-label">Thực hành</span>
          </div>
        </div>
      </div>
      
      <!-- Interactive Preview Graphic -->
      <div class="hero__preview z-10" aria-hidden="true" data-aos="fade-up" data-aos-delay="400">
        <div class="glass-panel preview-window spring-hover">
          <div class="preview-header">
            <div class="terminal-dots">
              <span class="terminal-dot terminal-dot--close"></span>
              <span class="terminal-dot terminal-dot--min"></span>
              <span class="terminal-dot terminal-dot--max"></span>
            </div>
            <div class="preview-title font-mono text-muted text-xs">quick-sort.ts</div>
            <div class="preview-actions">
              <button class="preview-btn" aria-label="Run" @click="togglePlay">{{ playing ? '⏸' : '▶' }}</button>
              <button class="preview-btn" aria-label="Step" @click="step">⏭</button>
              <button class="preview-btn" aria-label="Reset" @click="reset">↻</button>
            </div>
          </div>
          <div class="preview-body">
            <div class="bars-container">
              <div
                v-for="(bar, i) in currentPhase"
                :key="i"
                class="bar"
                :class="bar.cls ? `bar--${bar.cls}` : ''"
                :style="{ height: bar.height + '%' }"
              ></div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── BENTO GRID FEATURES ── -->
    <section class="features-section">
      <div class="features-header text-center mb-12" data-aos="fade-up">
        <h2 class="font-display text-3xl mb-4 text-heading">Không chỉ là code, đó là nghệ thuật</h2>
        <p class="text-secondary max-w-2xl mx-auto">Trải nghiệm nền tảng giáo dục kết hợp với giao diện cinematic dark-mode hiện đại, mang lại cảm hứng sáng tạo vô tận.</p>
      </div>
      
      <div class="bento-grid">
        <!-- Large Card: Sorting -->
        <div class="bento-card bento-large glass-panel spring-hover" data-aos="fade-up" data-aos-delay="100">
          <div class="bento-content">
            <div class="bento-icon text-accent"><BaseIcon name="sorting" /></div>
            <h3 class="font-display text-xl text-heading mb-2">Thuật toán Sắp xếp</h3>
            <p class="text-secondary text-sm">7 thuật toán sắp xếp với hệ thống hoạt ảnh VCR điều khiển từng bước tiến trình. Dễ dàng quan sát cách Bubble, Quick, hay Merge Sort hoạt động.</p>
          </div>
          <div class="bento-visual visual-sorting"></div>
        </div>
        
        <!-- Medium Card: Graph -->
        <div class="bento-card bento-medium glass-panel spring-hover" data-aos="fade-up" data-aos-delay="200">
          <div class="bento-content">
            <div class="bento-icon text-accent-warm"><BaseIcon name="graph" /></div>
            <h3 class="font-display text-xl text-heading mb-2">Sân chơi Đồ thị</h3>
            <p class="text-secondary text-sm">Tự do kéo thả các đỉnh đồ thị, kết nối cạnh và chạy BFS/DFS/Dijkstra trực tiếp.</p>
          </div>
        </div>
        
        <!-- Medium Card: Gamification -->
        <div class="bento-card bento-medium glass-panel spring-hover" data-aos="fade-up" data-aos-delay="300">
          <div class="bento-content">
            <div class="bento-icon text-accent-purple"><BaseIcon name="gamification" /></div>
            <h3 class="font-display text-xl text-heading mb-2">Học mà Chơi</h3>
            <p class="text-secondary text-sm">Kiếm điểm XP, nhận huy hiệu và thăng cấp sau mỗi bài học hoàn thành.</p>
          </div>
        </div>
        
        <!-- Small Cards -->
        <div class="bento-card bento-small glass-panel spring-hover" data-aos="fade-up" data-aos-delay="400">
          <div class="bento-icon text-accent-cyan"><BaseIcon name="oop" /></div>
          <h3 class="font-display text-lg text-heading mb-1">OOP</h3>
          <p class="text-secondary text-xs">Làm chủ Lập trình Hướng đối tượng.</p>
        </div>
        
        <div class="bento-card bento-small glass-panel spring-hover" data-aos="fade-up" data-aos-delay="450">
          <div class="bento-icon text-accent-green"><BaseIcon name="solid" /></div>
          <h3 class="font-display text-lg text-heading mb-1">SOLID</h3>
          <p class="text-secondary text-xs">5 nguyên lý thiết kế phần mềm linh hoạt.</p>
        </div>
        
        <div class="bento-card bento-small glass-panel spring-hover" data-aos="fade-up" data-aos-delay="500">
          <div class="bento-icon text-accent-pink"><BaseIcon name="patterns" /></div>
          <h3 class="font-display text-lg text-heading mb-1">Patterns</h3>
          <p class="text-secondary text-xs">Mẫu thiết kế (Design Patterns) phổ biến.</p>
        </div>
        
        <div class="bento-card bento-small glass-panel spring-hover" data-aos="fade-up" data-aos-delay="550">
          <div class="bento-icon text-accent-blue"><BaseIcon name="di" /></div>
          <h3 class="font-display text-lg text-heading mb-1">DI/IoC</h3>
          <p class="text-secondary text-xs">Dependency Injection & Inversion of Control.</p>
        </div>
      </div>
    </section>

    <!-- ── ALGORITHM GRID ── -->
    <section class="algogrid-section">
      <div class="algogrid-header text-center mb-10" data-aos="fade-up">
        <h2 class="font-display text-3xl mb-3 text-heading">Thư viện thuật toán trực quan</h2>
        <p class="text-secondary max-w-2xl mx-auto">30+ thuật toán & cấu trúc dữ liệu, sắp xếp theo nhóm — xem animation từng bước, tự nhập dữ liệu, chạy code của bạn.</p>
      </div>

      <div class="algogrid" data-aos="fade-up">
        <router-link
          v-for="group in algorithmGroups"
          :key="group.category"
          :to="getGroupTarget(group)"
          class="algo-card glass-panel spring-hover"
        >
          <div class="algo-card-head">
            <div class="algo-icon" :class="`algo-icon--${group.iconClass}`">
              <BaseIcon :name="group.icon" />
            </div>
            <div class="algo-meta">
              <h3 class="font-display text-lg text-heading">{{ group.category }}</h3>
              <span class="algo-count text-muted text-xs">{{ group.count }} thuật toán</span>
            </div>
          </div>
          <p class="algo-desc text-secondary text-sm">{{ group.description }}</p>
          <div class="algo-tags">
            <span v-for="tag in group.tags" :key="tag" class="algo-tag">{{ tag }}</span>
          </div>
          <div class="algo-card-foot">
            <span class="algo-cta text-accent">{{ group.premium && !authStore.isPremium ? '🔒 Nâng cấp Premium' : '▶ Xem ngay' }}</span>
            <span v-if="group.premium" class="algo-lock text-accent-warm">
              <BaseIcon name="lock" class="w-3 h-3 inline-block mr-0.5 align-text-bottom" /> Premium
            </span>
          </div>
        </router-link>
      </div>
    </section>

    <!-- ── FREEMIUM ── -->
    <section class="freemium-section">
      <div class="freemium-container">
        <div class="freemium-header text-center mb-10" data-aos="fade-up">
          <h2 class="font-display text-3xl mb-3 text-heading">Miễn phí để bắt đầu, Premium để bứt phá</h2>
          <p class="text-secondary max-w-2xl mx-auto">Mô hình Freemium giúp bạn học thử miễn phí và nâng cấp khi cần thêm sức mạnh.</p>
        </div>

        <div class="freemium-grid" data-aos="fade-up">
          <div class="freemium-card glass-panel">
            <div class="freemium-icon text-accent"><BaseIcon name="heart" /></div>
            <h3 class="font-display text-lg text-heading mb-1">❤️ Hearts</h3>
            <p class="text-sm text-secondary">Mỗi bài học có giới hạn số lần thử. Sai quá nhiều thì hết tim và phải đợi hồi phục — tạo động lực tập trung.</p>
          </div>
          <div class="freemium-card glass-panel">
            <div class="freemium-icon text-accent-cyan"><BaseIcon name="diamond" /></div>
            <h3 class="font-display text-lg text-heading mb-1">💎 Gems</h3>
            <p class="text-sm text-secondary">Tiêu gems mua gợi ý, đóng băng streak, khung avatar — tiêu được chứ không chỉ là số đẹp.</p>
          </div>
          <div class="freemium-card glass-panel freemium-card--premium">
            <div class="freemium-icon text-accent-warm"><BaseIcon name="crown" /></div>
            <h3 class="font-display text-lg text-heading mb-1">👑 Premium</h3>
            <p class="text-sm text-secondary">Mở khóa toàn bộ visualizer cao cấp, AI không giới hạn, không quảng cáo. Thanh toán qua chuyển khoản SePay an toàn.</p>
            <button class="btn-primary hero-btn mt-4 spring-hover" @click="goPremium">Nâng cấp Premium</button>
          </div>
        </div>
      </div>
    </section>

    <!-- ── ROADMAP SECTION ── -->
    <section class="extended-section roadmap-section">
      <div class="extended-container">
        <div class="extended-text" data-aos="fade-right">
          <h2 class="font-display text-3xl mb-4 text-heading">Học tập qua Lộ trình (Roadmap) thay vì Mò mẫm</h2>
          <p class="text-secondary mb-6">Hệ thống bài học được thiết kế chuẩn sư phạm, dẫn dắt bạn qua 4 bước vững chắc: Lý thuyết ➔ Trực quan hoá ➔ Thực hành Code ➔ Trắc nghiệm.</p>
          <ul class="feature-list text-muted">
            <li><span class="text-accent">●</span> Lộ trình từ cơ bản đến nâng cao (Mảng, Cây, Đồ thị).</li>
            <li><span class="text-accent">●</span> Theo dõi tiến độ học tập chi tiết.</li>
            <li><span class="text-accent">●</span> Nhận chứng nhận khi hoàn thành khóa học.</li>
          </ul>
        </div>
        <div class="extended-visual" aria-hidden="true" data-aos="fade-left">
          <div class="roadmap-mockup glass-panel spring-hover">
            <div class="rm-node completed"><div class="rm-icon"><BaseIcon name="check" class="w-3.5 h-3.5" /></div><div class="rm-label font-sans font-medium">Mảng & Chuỗi</div></div>
            <div class="rm-line completed"></div>
            <div class="rm-node active"><div class="rm-icon"><BaseIcon name="zap" class="w-3.5 h-3.5" /></div><div class="rm-label font-sans font-medium">Đồ Thị BFS/DFS</div></div>
            <div class="rm-line"></div>
            <div class="rm-node"><div class="rm-icon"><BaseIcon name="lock" class="w-3.5 h-3.5" /></div><div class="rm-label font-sans font-medium">Quy Hoạch Động</div></div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── TESTIMONIALS SECTION ── -->
    <section class="testimonials-section">
      <div class="testimonials-header text-center mb-12" data-aos="fade-up">
        <h2 class="font-display text-3xl mb-4 text-heading">Người học nói gì về VisualizationDSA?</h2>
        <p class="text-secondary max-w-2xl mx-auto">Trải nghiệm của những người đã học thuật toán theo cách trực quan.</p>
      </div>
      
      <div class="testimonials-carousel" data-aos="fade-up">
        <div class="testimonial-track" ref="testimonialTrack" :style="{ transform: `translateX(-${currentTestimonial * 100}%)` }">
          <div v-for="(testimonial, index) in testimonials" :key="index" class="testimonial-card glass-panel spring-hover">
            <div class="testimonial-rating">
              <BaseIcon v-for="i in 5" :key="i" name="star" class="w-5 h-5 text-accent-yellow fill-current" />
            </div>
            <blockquote class="testimonial-quote">
              {{ testimonial.quote }}
            </blockquote>
            <div class="testimonial-author">
              <div class="testimonial-avatar">
                <div class="avatar-placeholder" :style="{ background: testimonial.avatarColor }">
                  {{ testimonial.initials }}
                </div>
              </div>
              <div class="testimonial-info">
                <cite class="testimonial-name font-medium">{{ testimonial.name }}</cite>
                <p class="testimonial-role text-muted text-xs">{{ testimonial.role }}</p>
              </div>
            </div>
          </div>
        </div>
        
        <div class="testimonial-controls">
          <button class="carousel-btn spring-hover" @click="prevTestimonial" aria-label="Previous testimonial">
            <BaseIcon name="chevron-left" class="w-5 h-5" />
          </button>
          <div class="testimonial-dots">
            <button v-for="(_, index) in testimonials" :key="index" 
                    class="testimonial-dot" 
                    :class="{ active: currentTestimonial === index }"
                    @click="goToTestimonial(index)"
                    :aria-label="`Go to testimonial ${index + 1}`">
            </button>
          </div>
          <button class="carousel-btn spring-hover" @click="nextTestimonial" aria-label="Next testimonial">
            <BaseIcon name="chevron-right" class="w-5 h-5" />
          </button>
        </div>
      </div>
    </section>

    <!-- ── CODELAB SECTION ── -->
    <section class="extended-section codelab-section reverse">
      <div class="extended-container">
        <div class="extended-text" data-aos="fade-right">
          <h2 class="font-display text-3xl mb-4 text-heading">Thực hành Code Trực tiếp (Codelab)</h2>
          <p class="text-secondary mb-6">Không chỉ dừng lại ở việc xem animation. Bạn sẽ được cấp ngay một trình soạn thảo Monaco chuyên nghiệp và hệ thống chấm điểm tự động đa ngôn ngữ ngay trên trình duyệt.</p>
          <ul class="feature-list text-muted">
            <li><span class="text-accent-green">●</span> Chấm code tự động y hệt LeetCode (Judge0 API).</li>
            <li><span class="text-accent-green">●</span> Hỗ trợ C#, TypeScript, Python, Java.</li>
            <li><span class="text-accent-green">●</span> Đánh giá Time & Space Complexity thực tế.</li>
          </ul>
        </div>
        <div class="extended-visual" aria-hidden="true" data-aos="fade-left">
          <div class="codelab-mockup clay-card spring-hover">
            <div class="terminal-header">
              <div class="terminal-dots"><span class="terminal-dot terminal-dot--close"></span><span class="terminal-dot terminal-dot--min"></span><span class="terminal-dot terminal-dot--max"></span></div>
              <div class="terminal-title font-mono text-muted text-xs">two-sum.ts</div>
            </div>
            <div class="terminal-body font-mono text-xs">
              <div class="code-line"><span class="text-accent-purple">function</span> <span class="text-accent-blue">twoSum</span>(nums, target) {</div>
              <div class="code-line indent">  <span class="text-accent-purple">const</span> map = <span class="text-accent-purple">new</span> <span class="text-accent-warm">Map</span>();</div>
              <div class="code-line indent">  <span class="text-accent-purple">for</span> (<span class="text-accent-purple">let</span> i = <span class="text-accent-warm">0</span>; i < nums.length; i++) {</div>
              <div class="code-line indent-2 text-muted">    // Code logic here...</div>
              <div class="code-line indent">  }</div>
              <div class="code-line">}</div>
              <div class="code-line mt-4 text-accent-green">>> All 15/15 Test Cases Passed! (12ms) <BaseIcon name="check-circle" class="w-3.5 h-3.5 inline-block align-text-bottom" /></div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── AI ASSISTANT SECTION ── -->
    <section class="extended-section ai-section">
      <div class="extended-container">
        <div class="extended-text" data-aos="fade-right">
          <h2 class="font-display text-3xl mb-4 text-heading">AI Assistant - Người Mentor Tận Tụy</h2>
          <p class="text-secondary mb-6">Mắc kẹt ở một bài toán khó? Trợ lý ảo AI luôn túc trực 24/7 để gợi ý hướng giải quyết, giải thích lỗi sai (Bug) và tối ưu hóa đoạn code của bạn mà không hề tiết lộ đáp án.</p>
        </div>
        <div class="extended-visual" aria-hidden="true" data-aos="fade-left">
          <div class="ai-mockup clay-card spring-hover">
            <div class="ai-chat user">Tại sao Quick Sort bị O(N²) ở TH xấu nhất?</div>
            <div class="ai-chat bot">
              <span class="ai-icon text-accent-primary">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/></svg>
              </span>
              <div class="ai-msg typing-effect">Trường hợp xấu nhất <strong>O(N²)</strong> xảy ra khi mảng đã được sắp xếp sẵn và bạn luôn chọn pivot là phần tử cuối/đầu. Khi đó, mảng bị chia thành 1 phần có N-1 phần tử và 1 phần có 0 phần tử.</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── CTA SECTION ── -->
    <section class="cta-section" data-aos="fade-up">
      <div class="cta-card glass-panel spring-hover">
        <h2 class="font-display text-3xl mb-4 text-heading">Sẵn sàng nâng cao trình độ?</h2>
        <p class="text-secondary mb-8 max-w-lg mx-auto">Gia nhập cộng đồng sinh viên lập trình Việt Nam và làm chủ Cấu trúc Dữ liệu & Giải thuật ngay hôm nay.</p>
        <button class="btn-primary hero-btn mx-auto spring-hover" @click="handleCta">
          Tạo tài khoản miễn phí
        </button>
      </div>
    </section>

    <!-- ── FOOTER ── -->
    <footer class="landing-footer border-t border-border-default mt-16 py-8 px-6 text-center text-sm text-muted">
      <div class="max-w-4xl mx-auto flex flex-col md:flex-row justify-between items-center gap-4">
        <div>&copy; 2026 VisualizationDSA. Nền tảng học tập thuật toán.</div>
        <div class="flex gap-4">
          <router-link to="/docs/intro/intro" class="hover:text-accent-primary transition-colors">Tài liệu</router-link>
          <router-link to="/courses" class="hover:text-accent-primary transition-colors">Thư viện Lộ trình</router-link>
          <router-link to="/checkout" class="hover:text-accent-primary transition-colors">Nâng cấp Premium</router-link>
        </div>
      </div>
    </footer>
    
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../../features/auth/store/useAuthStore';
import gsap from 'gsap';
import { ScrollTrigger } from 'gsap/ScrollTrigger';

gsap.registerPlugin(ScrollTrigger);

const emit = defineEmits<{ openLogin: [] }>();
const authStore = useAuthStore();
const router = useRouter();

// ── HOTFIX-3: quick-sort preview animation (loop, không tĩnh) ──
interface PreviewBar {
  height: number;
  cls: '' | 'pivot' | 'compare' | 'swap' | 'sorted';
}

// Mô phỏng Quick Sort trên [8,3,9,4,7,5] → sorted [3,4,5,7,8,9]
const QUICK_SORT_PHASES: PreviewBar[][] = [
  // 0 — khởi tạo, chọn pivot (vàng)
  [{ height: 80, cls: '' }, { height: 30, cls: '' }, { height: 90, cls: '' }, { height: 40, cls: '' }, { height: 70, cls: '' }, { height: 50, cls: 'pivot' }],
  // 1 — so sánh cặp (tím)
  [{ height: 80, cls: 'compare' }, { height: 30, cls: 'compare' }, { height: 90, cls: '' }, { height: 40, cls: '' }, { height: 70, cls: '' }, { height: 50, cls: 'pivot' }],
  // 2 — swap (đỏ)
  [{ height: 30, cls: 'swap' }, { height: 80, cls: 'swap' }, { height: 90, cls: '' }, { height: 40, cls: '' }, { height: 70, cls: '' }, { height: 50, cls: 'pivot' }],
  // 3 — so sánh cặp tiếp (tím)
  [{ height: 30, cls: '' }, { height: 80, cls: '' }, { height: 90, cls: 'compare' }, { height: 40, cls: 'compare' }, { height: 70, cls: '' }, { height: 50, cls: 'pivot' }],
  // 4 — swap tiếp (đỏ)
  [{ height: 30, cls: '' }, { height: 80, cls: '' }, { height: 40, cls: 'swap' }, { height: 90, cls: 'swap' }, { height: 70, cls: '' }, { height: 50, cls: 'pivot' }],
  // 5 — pivot hạ cánh đúng vị trí → vùng sorted (xanh)
  [{ height: 30, cls: 'sorted' }, { height: 40, cls: '' }, { height: 50, cls: 'pivot' }, { height: 70, cls: '' }, { height: 80, cls: '' }, { height: 90, cls: 'sorted' }],
  // 6 — so sánh phần trái (tím)
  [{ height: 30, cls: 'sorted' }, { height: 40, cls: 'compare' }, { height: 50, cls: 'compare' }, { height: 70, cls: '' }, { height: 80, cls: '' }, { height: 90, cls: 'sorted' }],
  // 7 — swap phần trái (đỏ)
  [{ height: 30, cls: 'sorted' }, { height: 50, cls: 'swap' }, { height: 40, cls: 'swap' }, { height: 70, cls: '' }, { height: 80, cls: '' }, { height: 90, cls: 'sorted' }],
  // 8 — toàn bộ sorted (xanh)
  [{ height: 30, cls: 'sorted' }, { height: 40, cls: 'sorted' }, { height: 50, cls: 'sorted' }, { height: 70, cls: 'sorted' }, { height: 80, cls: 'sorted' }, { height: 90, cls: 'sorted' }],
];

const phaseIndex = ref(0);
const playing = ref(true);
const currentPhase = computed(() => QUICK_SORT_PHASES[phaseIndex.value] ?? QUICK_SORT_PHASES[0]);

let previewTimer: ReturnType<typeof setInterval> | null = null;

function advancePreview(): void {
  phaseIndex.value = (phaseIndex.value + 1) % QUICK_SORT_PHASES.length;
}

function startPreview(): void {
  if (previewTimer) clearInterval(previewTimer);
  previewTimer = setInterval(advancePreview, 1200);
}

function stopPreview(): void {
  if (previewTimer) {
    clearInterval(previewTimer);
    previewTimer = null;
  }
}

function togglePlay(): void {
  playing.value = !playing.value;
  if (playing.value) startPreview();
  else stopPreview();
}

function step(): void {
  playing.value = false;
  stopPreview();
  advancePreview();
}

function reset(): void {
  phaseIndex.value = 0;
  playing.value = true;
  startPreview();
}


function handleCta(): void {
  if (authStore.isAuthenticated) {
    router.push('/dashboard');
  } else {
    emit('openLogin');
  }
}

function handleBrowse(): void {
  router.push('/courses');
}

function goPremium(): void {
  router.push('/checkout');
}

// Premium gating thật: card premium với user không premium → đưa tới Checkout
function getGroupTarget(group: AlgorithmGroup): string {
  if (group.premium && !authStore.isPremium) return '/checkout';
  return group.route;
}

interface AlgorithmGroup {
  category: string;
  route: string;
  icon: string;
  iconClass: string;
  count: number;
  description: string;
  tags: string[];
  premium?: boolean;
}

const algorithmGroups: AlgorithmGroup[] = [
  {
    category: 'Sorting',
    route: '/sorting',
    icon: 'sorting',
    iconClass: 'sorting',
    count: 7,
    description: 'Bubble, Quick, Merge, Heap, Radix, Counting, Bucket Sort — quan sát từng phép so sánh và tráo đổi.',
    tags: ['Sắp xếp', 'Array'],
  },
  {
    category: 'Graph',
    route: '/graph',
    icon: 'graph',
    iconClass: 'graph',
    count: 9,
    description: 'BFS, DFS, Dijkstra, Bellman-Ford, Kruskal, Prim, A* — sân chơi đồ thị kéo thả tương tác.',
    tags: ['Đồ thị', 'Tương tác'],
    premium: true,
  },
  {
    category: 'Searching',
    route: '/algorithms',
    icon: 'search',
    iconClass: 'search',
    count: 3,
    description: 'Tìm kiếm tuần tự, nhị phân và kỹ thuật cửa sổ trượt trên mảng.',
    tags: ['Tìm kiếm', 'Array'],
  },
  {
    category: 'Stack & Queue',
    route: '/algorithms',
    icon: 'code-ide',
    iconClass: 'sq',
    count: 3,
    description: 'Stack LIFO, Queue FIFO và Monotonic Stack — cấu trúc dữ liệu nền tảng.',
    tags: ['Cấu trúc dữ liệu'],
  },
  {
    category: 'Tree',
    route: '/algorithms',
    icon: 'graph',
    iconClass: 'tree',
    count: 1,
    description: 'Cây nhị phân tìm kiếm (BST) với thao tác chèn, xóa, duyệt trực quan.',
    tags: ['Cây'],
    premium: true,
  },
  {
    category: 'OOP & Design',
    route: '/algorithms',
    icon: 'oop',
    iconClass: 'oop',
    count: 5,
    description: 'OOP, SOLID, Design Patterns, DI/IoC — khái niệm trừu tượng hóa thành bài giảng trực quan.',
    tags: ['Kiến trúc', 'Markdown'],
  },
];

const testimonials = [
  {
    quote: "VisualizationDSA hoàn toàn thay đổi cách tôi tiếp cận thuật toán. Thay vì học vẹt code, giờ tôi thấy được dữ liệu chuyển động từng bước. BFS/DFS không còn là lý thuyết khô khan nữa!",
    name: "Minh Anh",
    role: "Sinh viên ĐH Bách Khoa HN",
    initials: "MA",
    avatarColor: "linear-gradient(135deg, #06b6d4, #3b82f6)"
  },
  {
    quote: "Hệ thống gamification thực sự gây nghiện tích cực. Mỗi lần hoàn thành bài học nhận XP, unlock badge, thấy streak tăng đều ngày khiến tôi muốn học thêm mỗi ngày. 3 tháng liên tiếp không nghỉ streak!",
    name: "Duy Khánh",
    role: "Sinh viên ĐH FPT",
    initials: "DK",
    avatarColor: "linear-gradient(135deg, #f97316, #ec4899)"
  },
  {
    quote: "Tính năng Codelab với Monaco Editor và Judge0 API giống hệt LeetCode nhưng tích hợp sẵn trong bài học. Chạy code, thấy kết quả real-time, debug từng dòng - trải nghiệm cực đã so với việc copy-paste code trên web.",
    name: "Thảo Vy",
    role: "Sinh viên ĐH Công Nghệ TP.HCM",
    initials: "TV",
    avatarColor: "linear-gradient(135deg, #8b5cf6, #06b6d4)"
  },
  {
    quote: "Sân chơi đồ thị interactive là phần tôi thích nhất. Kéo thả vertex, thêm edge, chạy Dijkstra trực quan thấy đường đi ngắn nhất sáng lên từng bước - giúp tôi hiểu bản chất thuật toán thay vì chỉ nhớ pseudo-code.",
    name: "Quang Huy",
    role: "Sinh viên ĐH Khoa Học Tự Nhiên",
    initials: "QH",
    avatarColor: "linear-gradient(135deg, #22c55e, #10b981)"
  },
  {
    quote: "Làm TA môn Cấu trúc dữ liệu, tôi giới thiệu VisualizationDSA cho cả lớp. Kết quả: điểm trung bình bài tập lớn tăng 15%, sinh viên hiểu sâu hơn về pointer, recursion, heap. Công cụ tuyệt vời cho giảng viên!",
    name: "Thúy Linh",
    role: "Trợ giảng ĐH Sư Phạm Kỹ Thuật",
    initials: "TL",
    avatarColor: "linear-gradient(135deg, #ec4899, #f43f5e)"
  },
];

const currentTestimonial = ref(0);
const testimonialTrack = ref<HTMLElement | null>(null);

function nextTestimonial(): void {
  currentTestimonial.value = (currentTestimonial.value + 1) % testimonials.length;
}

function prevTestimonial(): void {
  currentTestimonial.value = (currentTestimonial.value - 1 + testimonials.length) % testimonials.length;
}

function goToTestimonial(index: number): void {
  currentTestimonial.value = index;
}

// Particle options
const particleOptions = {
  background: { color: { value: 'transparent' } },
  fpsLimit: 120,
  particles: {
    color: { value: '#06b6d4' },
    links: {
      color: '#3b82f6',
      distance: 150,
      enable: true,
      opacity: 0.3,
      width: 1
    },
    move: {
      enable: true,
      speed: 1.5,
      direction: 'none',
      random: false,
      straight: false,
      outModes: 'bounce'
    },
    number: { value: 50, density: { enable: true, area: 800 } },
    opacity: { value: 0.5 },
    shape: { type: 'circle' },
    size: { value: { min: 1, max: 3 } }
  },
  interactivity: {
    events: {
      onHover: { enable: true, mode: 'grab' },
      onClick: { enable: true, mode: 'push' }
    },
    modes: {
      grab: { distance: 140, links: { opacity: 0.8 } },
      push: { quantity: 4 }
    }
  },
  detectRetina: true
};

let ctx: gsap.Context;

onMounted(() => {
  ctx = gsap.context(() => {
    // Hero Title Stagger
    gsap.from('.hero__badge, .hero__title, .hero__sub, .hero__actions, .hero__trust', {
      y: 30,
      opacity: 0,
      duration: 1,
      stagger: 0.15,
      ease: 'power3.out',
      delay: 0.2
    });

    // Mesh blob animations
    gsap.to('.mesh-blob--1', {
      x: 50,
      y: -30,
      rotation: 180,
      duration: 20,
      ease: 'none',
      repeat: -1,
      yoyo: true
    });
    gsap.to('.mesh-blob--2', {
      x: -40,
      y: 40,
      rotation: -120,
      duration: 25,
      ease: 'none',
      repeat: -1,
      yoyo: true
    });
    gsap.to('.mesh-blob--3', {
      x: 30,
      y: 20,
      rotation: 90,
      duration: 18,
      ease: 'none',
      repeat: -1,
      yoyo: true
    });

    // Roadmap scrub
    gsap.from('.roadmap-mockup .rm-node:not(.completed)', {
      opacity: 0.3,
      scale: 0.9,
      stagger: 0.3,
      scrollTrigger: {
        trigger: '.roadmap-section',
        start: 'top 70%',
        end: 'bottom 40%',
        scrub: 1
      }
    });

    // Testimonial carousel auto-play
    let testimonialTimer = setInterval(() => {
      nextTestimonial();
    }, 5000);

    // Pause on hover — giữ tham chiếu timer để clear đúng
    const carousel = document.querySelector('.testimonials-carousel');
    const stopAutoPlay = () => clearInterval(testimonialTimer);
    const startAutoPlay = () => {
      stopAutoPlay();
      testimonialTimer = setInterval(() => {
        nextTestimonial();
      }, 5000);
    };
    if (carousel) {
      carousel.addEventListener('mouseenter', stopAutoPlay);
      carousel.addEventListener('mouseleave', startAutoPlay);
      testimonialCleanup = () => {
        stopAutoPlay();
        carousel.removeEventListener('mouseenter', stopAutoPlay);
        carousel.removeEventListener('mouseleave', startAutoPlay);
      };
    }

    // HOTFIX-3 — preview quick-sort tự chạy loop khi vào trang
    startPreview();
  });
});

let testimonialCleanup: (() => void) | null = null;

onUnmounted(() => {
  if (testimonialCleanup) testimonialCleanup();
  stopPreview();
  if (ctx) ctx.revert();
});
</script>

<style scoped>
@import "./LandingView.css";
</style>
