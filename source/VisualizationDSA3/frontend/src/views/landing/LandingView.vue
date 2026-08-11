<template>
  <div class="landing bg-bg-primary text-text-primary min-h-screen font-sans overflow-x-hidden pb-20">
    
    <!-- Udemy-style Hero Banner -->
    <section class="max-w-7xl mx-auto px-6 py-8 md:py-12">
      <div class="relative bg-bg-surface border border-border-subtle rounded-2xl overflow-hidden shadow-2xl flex flex-col lg:flex-row items-center h-auto lg:h-[450px]">
        <!-- Geometric abstract background shape (Udemy style) -->
        <div class="absolute inset-0 bg-gradient-to-r from-bg-surface via-bg-surface to-transparent z-10 lg:w-2/3"></div>
        <div class="absolute right-0 top-0 bottom-0 w-full lg:w-2/3 bg-accent-purple/10" style="clip-path: polygon(20% 0, 100% 0, 100% 100%, 0% 100%);"></div>
        <div class="absolute right-0 top-0 bottom-0 w-1/2 opacity-30 lg:opacity-100 flex items-center justify-center p-8 z-0">
          <img src="../../assets/code_to_viz_hero.png" alt="Hero Illustration" class="max-h-[120%] w-auto object-cover rounded-xl transform rotate-3 scale-110 shadow-2xl mix-blend-screen" />
        </div>
        
        <!-- Text Box Box Overlay -->
        <div class="relative z-20 w-full lg:w-1/2 p-8 lg:p-16 lg:ml-8">
          <div class="bg-bg-primary/95 backdrop-blur-xl p-8 rounded-2xl border border-border-default shadow-[0_8px_32px_rgba(0,0,0,0.5)] max-w-lg">
            <h1 class="text-3xl md:text-4xl font-extrabold mb-4 leading-tight">
              Làm chủ thuật toán <br/>
              <span class="text-transparent bg-clip-text bg-gradient-to-r from-accent-purple to-accent-cyan">Trực quan và dễ hiểu</span>
            </h1>
            <p class="text-text-secondary text-base mb-6 leading-relaxed">
              Từ lý thuyết khô khan đến hoạt ảnh sống động. Tham gia ngay nền tảng học Cấu trúc dữ liệu thế hệ mới với công nghệ AI Code-to-Viz.
            </p>
            <button @click="handleCta" class="w-full bg-accent hover:bg-accent-dark text-white font-bold py-4 rounded-xl transition-colors shadow-lg shadow-accent/20">
              <template v-if="authStore.isAuthenticated">
                Tiếp tục học ngay
              </template>
              <template v-else>
                Đăng ký học trải nghiệm
              </template>
            </button>
          </div>
        </div>
      </div>
    </section>

    <!-- Udemy-style Categories / Skills -->
    <section class="max-w-7xl mx-auto px-6 py-16 border-b border-border-subtle">
      <div class="grid lg:grid-cols-4 gap-12 items-center">
        <!-- Left Text -->
        <div class="lg:col-span-1">
          <h2 class="text-3xl font-extrabold mb-4 leading-tight">Học các kỹ năng <span class="italic text-accent-purple">cốt lõi</span> cho sự nghiệp IT</h2>
          <p class="text-text-muted text-sm leading-relaxed mb-6">
            Khám phá phương pháp học bằng thị giác (Visual Learning) giúp bạn vượt qua mọi kỳ phỏng vấn thuật toán tại các tập đoàn công nghệ lớn.
          </p>
          <button class="text-accent-purple font-bold hover:underline flex items-center gap-1">Khám phá lộ trình <BaseIcon name="arrow-right" class="w-4 h-4" /></button>
        </div>
        
        <!-- Right Cards -->
        <div class="lg:col-span-3 grid sm:grid-cols-3 gap-6">
          <div class="skill-card group">
            <div class="skill-card-img bg-gradient-to-br from-bg-secondary to-bg-active group-hover:from-accent-purple/20 group-hover:to-accent-cyan/10">
              <BaseIcon name="sorting" class="w-16 h-16 text-accent group-hover:scale-110 transition-transform duration-500" />
            </div>
            <h3 class="skill-card-title group-hover:text-accent-purple transition-colors">Thuật toán Nền tảng <span class="float-right opacity-0 group-hover:opacity-100">&rarr;</span></h3>
          </div>
          
          <div class="skill-card group">
            <div class="skill-card-img bg-gradient-to-br from-bg-secondary to-bg-active group-hover:from-accent-yellow/20 group-hover:to-accent-red/10">
              <BaseIcon name="graph" class="w-16 h-16 text-accent-yellow group-hover:scale-110 transition-transform duration-500" />
            </div>
            <h3 class="skill-card-title group-hover:text-accent-yellow transition-colors">Đồ thị & Cây Nhị Phân <span class="float-right opacity-0 group-hover:opacity-100">&rarr;</span></h3>
          </div>

          <div class="skill-card group">
            <div class="skill-card-img bg-gradient-to-br from-bg-secondary to-bg-active group-hover:from-accent-cyan/20 group-hover:to-accent-blue/10">
              <BaseIcon name="oop" class="w-16 h-16 text-accent-cyan group-hover:scale-110 transition-transform duration-500" />
            </div>
            <h3 class="skill-card-title group-hover:text-accent-cyan transition-colors">Tư duy OOP & Design Patterns <span class="float-right opacity-0 group-hover:opacity-100">&rarr;</span></h3>
          </div>
        </div>
      </div>
    </section>

    <!-- Udemy-style Trending Courses List -->
    <section class="max-w-7xl mx-auto px-6 py-16">
      <div class="flex justify-between items-end mb-8">
        <h2 class="text-2xl md:text-3xl font-extrabold">Các khóa học thịnh hành</h2>
        <a href="#/courses" class="text-sm font-bold text-accent hover:underline">Xem tất cả</a>
      </div>
      
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <div v-for="course in courses" :key="course.id" class="course-thumb-card group cursor-pointer" @click="router.push('/courses')">
          <div class="course-img-wrapper">
            <div class="absolute inset-0 bg-gradient-to-t from-bg-primary to-transparent opacity-60 z-10"></div>
            <BaseIcon :name="course.icon" class="absolute inset-0 m-auto w-12 h-12 text-white/50 z-20 group-hover:scale-125 transition-transform duration-500" />
            <div class="w-full h-full bg-bg-active group-hover:bg-bg-hover transition-colors"></div>
          </div>
          
          <div class="mt-3">
            <h3 class="font-bold text-base line-clamp-2 leading-tight group-hover:text-accent-purple transition-colors">{{ course.title }}</h3>
            <p class="text-xs text-text-muted mt-1">{{ course.author }}</p>
            
            <div class="flex items-center gap-1 mt-1 text-sm">
              <span class="font-bold text-accent-yellow">{{ course.rating }}</span>
              <div class="flex text-accent-yellow">
                <BaseIcon name="star" class="w-3.5 h-3.5" v-for="i in 5" :key="i" />
              </div>
              <span class="text-xs text-text-muted">({{ course.reviews }})</span>
            </div>
            
            <div class="mt-1 flex items-baseline gap-2">
              <span class="font-bold text-lg">{{ course.price }} ₫</span>
              <span class="text-xs text-text-muted line-through">{{ course.oldPrice }} ₫</span>
            </div>
            
            <div class="mt-2" v-if="course.isBestseller">
              <span class="bg-accent-yellow/20 text-accent-yellow border border-accent-yellow/30 text-[10px] font-bold px-2 py-0.5 rounded-sm uppercase tracking-wide">
                Bán chạy nhất
              </span>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Udemy-style Deep Feature Banner (AI Focus) -->
    <section class="max-w-7xl mx-auto px-6 py-16">
      <div class="bg-bg-surface border border-border-default rounded-3xl p-8 lg:p-12 shadow-2xl flex flex-col lg:flex-row gap-12 items-center overflow-hidden relative">
        <!-- Abstract glowing orb -->
        <div class="absolute top-0 right-0 w-96 h-96 bg-accent-purple/20 rounded-full blur-[100px] pointer-events-none"></div>
        
        <div class="lg:w-1/2 relative z-10">
          <h2 class="text-3xl md:text-4xl font-extrabold mb-4 leading-tight">
            Tái định hình tư duy với <br/><span class="text-transparent bg-clip-text bg-gradient-to-r from-accent-purple to-accent-cyan">Trợ giảng AI 24/7</span>
          </h2>
          <p class="text-text-secondary text-base mb-8 leading-relaxed">
            Chuẩn bị kỹ năng cho tương lai. Trình mô phỏng mã nguồn (Code Sandbox) của chúng tôi được tích hợp AI giúp nhận diện lỗi, phân tích thuật toán và sinh ảnh động tương tác ngay lập tức.
          </p>
          
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-y-4 gap-x-6 mb-8">
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-full bg-accent-purple/20 text-accent-purple flex items-center justify-center"><BaseIcon name="hub" class="w-4 h-4" /></div>
              <span class="text-sm font-medium">Render đồ thị thời gian thực</span>
            </div>
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-full bg-accent-cyan/20 text-accent-cyan flex items-center justify-center"><BaseIcon name="forum" class="w-4 h-4" /></div>
              <span class="text-sm font-medium">Hỏi đáp AI giải thích Code</span>
            </div>
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-full bg-accent-green/20 text-accent-green flex items-center justify-center"><BaseIcon name="play" class="w-4 h-4" /></div>
              <span class="text-sm font-medium">VCR Playback tua ngược</span>
            </div>
            <div class="flex items-center gap-3">
              <div class="w-8 h-8 rounded-full bg-accent-yellow/20 text-accent-yellow flex items-center justify-center"><BaseIcon name="military-tech" class="w-4 h-4" /></div>
              <span class="text-sm font-medium">Gamification đua Top</span>
            </div>
          </div>
          
          <button class="bg-bg-primary border border-border-strong hover:bg-bg-hover text-white font-bold py-3 px-8 rounded-xl transition-colors">
            Tìm hiểu thêm về AI Visualizer
          </button>
        </div>
        
        <!-- Collage of abstract visuals -->
        <div class="lg:w-1/2 w-full h-[350px] grid grid-cols-2 gap-4 relative z-10">
          <div class="bg-gradient-to-br from-accent-purple/20 to-accent-cyan/10 rounded-2xl border border-accent-purple/20 p-6 flex flex-col justify-between h-full">
            <BaseIcon name="code-ide" class="w-12 h-12 text-accent-purple opacity-50" />
            <div class="space-y-2">
              <div class="h-2 bg-accent-purple/30 rounded w-3/4"></div>
              <div class="h-2 bg-accent-purple/20 rounded w-1/2"></div>
              <div class="h-2 bg-accent-purple/20 rounded w-5/6"></div>
            </div>
          </div>
          <div class="flex flex-col gap-4 h-full">
            <div class="bg-gradient-to-br from-accent-cyan/20 to-accent-blue/10 rounded-2xl border border-accent-cyan/20 p-6 flex-1 flex items-center justify-center">
               <div class="w-16 h-16 rounded-full border-4 border-accent-cyan border-t-transparent animate-spin"></div>
            </div>
            <div class="bg-gradient-to-br from-accent-yellow/20 to-accent-red/10 rounded-2xl border border-accent-yellow/20 p-6 flex-1 flex items-end">
               <div class="w-full flex items-end gap-1 h-12">
                 <div class="w-1/4 bg-accent-yellow/40 rounded-t h-1/3"></div>
                 <div class="w-1/4 bg-accent-yellow/60 rounded-t h-2/3"></div>
                 <div class="w-1/4 bg-accent-yellow/80 rounded-t h-full"></div>
                 <div class="w-1/4 bg-accent-yellow rounded-t h-1/2"></div>
               </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Udemy-style Testimonials -->
    <section class="max-w-7xl mx-auto px-6 py-16 mb-12">
      <h2 class="text-2xl font-bold mb-8">Học viên nói gì về chúng tôi</h2>
      <div class="grid md:grid-cols-3 gap-6">
        <div class="testimonial-card">
          <div class="text-4xl text-accent-purple font-serif mb-2 leading-none">"</div>
          <p class="text-sm text-text-secondary leading-relaxed mb-6 flex-1">Khóa học đã giải thích thuật toán bề sâu (DFS) rất rõ ràng. Tính năng vẽ đồ thị bằng AI theo thời gian thực giúp tôi hình dung chính xác cách Call Stack hoạt động bên trong bộ nhớ.</p>
          <div class="flex items-center gap-3 pt-4 border-t border-border-default">
            <div class="w-10 h-10 rounded-full bg-accent text-white flex items-center justify-center font-bold">N</div>
            <div>
              <p class="font-bold text-sm text-white">Nguyễn Văn A</p>
              <p class="text-xs text-accent-purple mt-0.5">Sinh viên Bách Khoa</p>
            </div>
          </div>
        </div>

        <div class="testimonial-card">
          <div class="text-4xl text-accent-cyan font-serif mb-2 leading-none">"</div>
          <p class="text-sm text-text-secondary leading-relaxed mb-6 flex-1">Thực sự là một công cụ mang tính đột phá! Thay vì phải vẽ nháp thuật toán bằng tay, giờ đây tôi chỉ cần viết code và xem các khối mảng tự động hoán vị. Rất ấn tượng!</p>
          <div class="flex items-center gap-3 pt-4 border-t border-border-default">
            <div class="w-10 h-10 rounded-full bg-accent-cyan text-white flex items-center justify-center font-bold">T</div>
            <div>
              <p class="font-bold text-sm text-white">Trần Thị B</p>
              <p class="text-xs text-accent-purple mt-0.5">Software Engineer tại FPT</p>
            </div>
          </div>
        </div>

        <div class="testimonial-card">
          <div class="text-4xl text-accent-yellow font-serif mb-2 leading-none">"</div>
          <p class="text-sm text-text-secondary leading-relaxed mb-6 flex-1">Phần giải thích Design Patterns bằng hoạt hình thật sự xuất sắc. Nhờ VisualizationDSA, tôi đã qua được vòng phỏng vấn System Design của một công ty lớn với điểm số tối đa.</p>
          <div class="flex items-center gap-3 pt-4 border-t border-border-default">
            <div class="w-10 h-10 rounded-full bg-accent-yellow text-bg-primary flex items-center justify-center font-bold">L</div>
            <div>
              <p class="font-bold text-sm text-white">Lê Văn C</p>
              <p class="text-xs text-accent-purple mt-0.5">Học viên khóa OOP</p>
            </div>
          </div>
        </div>
      </div>
    </section>

    <AppFooter />
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import AppFooter from '../../components/AppFooter.vue';
import { useAuthStore } from '../../features/auth/store/useAuthStore';

const emit = defineEmits<{ openLogin: [] }>();
const authStore = useAuthStore();
const router = useRouter();

function handleCta(): void {
  if (authStore.isAuthenticated) {
    router.push('/dashboard');
  } else {
    emit('openLogin');
  }
}

const courses = [
  { id: 1, icon: 'sorting', title: 'Giải mã Thuật toán Sắp xếp toàn tập', author: 'Mai Tiêu Bảo', rating: '4.9', reviews: '1,452', price: '259.000', oldPrice: '500.000', isBestseller: true },
  { id: 2, icon: 'graph', title: 'Khám phá Đồ thị & Cây nhị phân từ Zero', author: 'Khoa Công Nghệ', rating: '4.8', reviews: '890', price: '279.000', oldPrice: '550.000', isBestseller: true },
  { id: 3, icon: 'oop', title: 'Tư duy Lập trình Hướng đối tượng (OOP)', author: 'Mai Tiêu Bảo', rating: '4.7', reviews: '2,104', price: '199.000', oldPrice: '399.000', isBestseller: false },
  { id: 4, icon: 'patterns', title: 'Làm chủ Design Patterns trong 30 ngày', author: 'Advanced IT', rating: '4.9', reviews: '3,211', price: '399.000', oldPrice: '800.000', isBestseller: true },
];
</script>

<style scoped>
@import "./LandingView.css";
</style>
