<template>
  <div class="auth-page">
    <div class="auth-container">
      <!-- Left Panel: Visual/Artwork -->
      <div class="auth-left">
        <div class="auth-visual-content">
          <div class="brand-badge" data-aos="fade-down" data-aos-delay="100">
            <BaseIcon name="gamification" />
            <span>Nền tảng VisualizationDSA</span>
          </div>
          <h1 class="auth-heading font-display" data-aos="fade-right" data-aos-delay="200">Học thuật toán <br/><span class="text-gradient">không còn nhàm chán</span></h1>
          <p class="auth-subtext text-secondary" data-aos="fade-right" data-aos-delay="300">Hệ thống mô phỏng, chấm bài tự động và theo dõi lộ trình học tập hàng đầu.</p>
          
          <div class="auth-features" data-aos="fade-up" data-aos-delay="400">
            <div class="feature-item">
              <BaseIcon name="sorting" class="text-accent-cyan" />
              <span>Hàng chục thuật toán được trực quan hoá 3D</span>
            </div>
            <div class="feature-item">
              <BaseIcon name="solid" class="text-accent-green" />
              <span>Sân chơi đồ thị (Graph Sandbox) & Codelab tự động</span>
            </div>
            <div class="feature-item">
              <BaseIcon name="patterns" class="text-accent-purple" />
              <span>Hệ thống Gamification: Tích điểm, nâng rank</span>
            </div>
          </div>
        </div>
        
        <!-- Decorative background elements -->
        <div class="bg-glow"></div>
        <div class="bg-glow bg-glow-2"></div>
        <div class="particles-container" ref="particlesContainer"></div>
      </div>

      <!-- Right Panel: Form -->
      <div class="auth-right">
        <div class="auth-form-wrapper" data-aos="fade-left" data-aos-delay="200">
          <div class="auth-header">
            <h2 class="font-display text-2xl mb-2">{{ isRegisterMode ? 'Tạo tài khoản mới' : 'Chào mừng trở lại' }}</h2>
            <p class="text-muted text-sm">{{ isRegisterMode ? 'Vui lòng điền thông tin để bắt đầu hành trình của bạn.' : 'Vui lòng đăng nhập để tiếp tục.' }}</p>
          </div>

          <div v-if="authStore.authError" class="auth-error-banner mb-4">
            <BaseIcon name="warning" class="w-4 h-4 shrink-0" />
            <span>{{ authStore.authError }}</span>
          </div>

          <form @submit.prevent="handleSubmit" class="auth-form">
            <div class="form-group floating-label">
              <input id="auth-email" v-model="email" type="email" required autocomplete="email"
                class="form-input" placeholder=" " />
              <label for="auth-email" class="floating-label-text">Địa chỉ Email</label>
              <span class="input-focus-bar"></span>
            </div>

            <div v-if="isRegisterMode" class="form-group floating-label slide-down">
              <input id="auth-username" v-model="username" type="text" required minlength="3" maxlength="100"
                class="form-input" placeholder=" " />
              <label for="auth-username" class="floating-label-text">Tên hiển thị</label>
              <span class="input-focus-bar"></span>
            </div>

            <div class="form-group floating-label password-field">
              <div class="flex justify-between items-center mb-1">
                <label class="form-label" for="auth-password">Mật khẩu</label>
                <a v-if="!isRegisterMode" href="#" class="text-xs text-accent-primary hover:underline">Quên mật khẩu?</a>
              </div>
              <div class="password-input-wrapper">
                <input 
                  id="auth-password" 
                  v-model="password" 
                  :type="showPassword ? 'text' : 'password'" 
                  required 
                  minlength="8"
                  class="form-input" 
                  placeholder=" " 
                  @input="checkPasswordStrength"
                />
                <label for="auth-password" class="floating-label-text">Mật khẩu</label>
                <button type="button" class="password-toggle" @click="showPassword = !showPassword" :aria-label="showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'">
                  <BaseIcon :name="showPassword ? 'eye-off' : 'eye'" class="w-5 h-5" />
                </button>
                <span class="input-focus-bar"></span>
              </div>
              <PasswordStrengthMeter :strength="passwordStrength" v-if="isRegisterMode && password.length > 0" />
            </div>

            <button type="submit" :disabled="authStore.isLoading" class="btn-primary w-full py-3 mt-4 flex justify-center items-center gap-2">
              <span v-if="authStore.isLoading" class="spinner"><BaseIcon name="solid" /></span>
              <span>{{ isRegisterMode ? 'Tạo tài khoản' : 'Đăng nhập' }}</span>
            </button>
          </form>

          <div class="auth-divider">
            <span>hoặc</span>
          </div>

          <button class="btn-google w-full" @click="handleGoogleLogin">
            <svg class="google-icon" viewBox="0 0 24 24">
              <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"/>
              <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"/>
              <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"/>
              <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"/>
            </svg>
            Tiếp tục với Google
          </button>

          <!-- Dev fallback: khi chưa có VITE_GOOGLE_CLIENT_ID -->
          <div v-if="googleDevFallback" class="google-dev-fallback mt-4 p-4 rounded-xl border border-border-default bg-bg-elevated/50">
            <p class="text-xs text-muted font-semibold mb-3">Chế độ Dev — nhập email/name Google (chưa cấu hình Client ID)</p>
            <div class="flex flex-col gap-3">
              <div class="form-group floating-label">
                <input id="google-fallback-email" v-model="googleFallbackEmail" type="email" class="form-input" placeholder=" " />
                <label for="google-fallback-email" class="floating-label-text">Email Google</label>
              </div>
              <div class="form-group floating-label">
                <input id="google-fallback-name" v-model="googleFallbackName" type="text" class="form-input" placeholder=" " />
                <label for="google-fallback-name" class="floating-label-text">Tên hiển thị (tùy chọn)</label>
              </div>
              <button type="button" @click="submitGoogleDevFallback" :disabled="authStore.isLoading"
                class="btn-google w-full">
                <BaseIcon name="solid" class="w-4 h-4" />
                {{ authStore.isLoading ? 'Đang đăng nhập...' : 'Đăng nhập Google (Dev)' }}
              </button>
            </div>
          </div>

          <p class="auth-footer-text mt-8 text-center text-sm text-secondary">
            {{ isRegisterMode ? 'Đã có tài khoản?' : 'Chưa có tài khoản?' }}
            <RouterLink :to="isRegisterMode ? '/login' : '/register'" class="text-accent-primary font-medium hover:underline">
              {{ isRegisterMode ? 'Đăng nhập ngay' : 'Tạo tài khoản' }}
            </RouterLink>
          </p>

          <div v-if="demoCredentials" class="demo-info text-center mt-6 p-4 border border-border-default rounded-lg bg-bg-elevated/50">
            <span class="text-xs text-muted block mb-1">Tài khoản Demo (dùng thử)</span>
            <code class="text-xs font-mono text-accent-primary-text bg-accent-primary-dim px-2 py-1 rounded">{{ demoCredentials.email }}</code>
            <span class="text-muted mx-1">/</span>
            <code class="text-xs font-mono text-accent-primary-text bg-accent-primary-dim px-2 py-1 rounded">{{ demoCredentials.password }}</code>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useToastStore } from '@/composables/useToast';
import { ref, computed, watch, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '../../features/auth/store/useAuthStore';
import { statelessAuthApi } from '../../features/auth/services/statelessAuthApi';

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const toastStore = useToastStore();

interface GsiCredentialResponse {
  credential: string;
}

interface GsiAccounts {
  id: {
    initialize: (config: {
      client_id: string;
      callback: (response: GsiCredentialResponse) => void;
      auto_select?: boolean;
    }) => void;
    prompt: () => void;
  };
}

const email = ref('');
const username = ref('');
const password = ref('');
const showPassword = ref(false);

const googleDevFallback = ref(false);
const googleFallbackEmail = ref('');
const googleFallbackName = ref('');

const demoCredentials = ref<{ email: string; password: string } | null>(null);

onMounted(async () => {
  try {
    demoCredentials.value = await statelessAuthApi.getDemoCredentials();
  } catch {
    demoCredentials.value = null;
  }
});

const passwordStrength = ref(0);

// Xử lý mode dựa trên URL (/login hay /register)
const isRegisterMode = computed(() => route.path === '/register');

// Password strength checker
function checkPasswordStrength(): void {
  let strength = 0;
  const val = password.value;
  
  if (val.length >= 8) strength += 1;
  if (val.length >= 12) strength += 1;
  if (/[A-Z]/.test(val)) strength += 1;
  if (/[a-z]/.test(val)) strength += 1;
  if (/[0-9]/.test(val)) strength += 1;
  if (/[^A-Za-z0-9]/.test(val)) strength += 1;
  
  passwordStrength.value = Math.min(5, strength);
}

function getStrengthLabel(): string {
  const labels = ['Rất yếu', 'Yếu', 'Trung bình', 'Khá', 'Mạnh', 'Rất mạnh'];
  return labels[passwordStrength.value] || '';
}

function getStrengthColor(): string {
  if (passwordStrength.value <= 1) return 'var(--color-accent-red)';
  if (passwordStrength.value <= 2) return 'var(--color-accent-red)';
  if (passwordStrength.value <= 3) return 'var(--color-accent-warm)';
  if (passwordStrength.value <= 4) return 'var(--color-accent-cyan)';
  return 'var(--color-accent-green)';
}

function getStrengthLabelClass(): string {
  const classes = ['strength-very-weak', 'strength-weak', 'strength-fair', 'strength-good', 'strength-strong', 'strength-very-strong'];
  return classes[passwordStrength.value] || '';
}

// Reset error khi đổi trang
watch(isRegisterMode, () => {
  authStore.authError = null;
  password.value = '';
  passwordStrength.value = 0;
});

async function handleSubmit(): Promise<void> {
  try {
    if (isRegisterMode.value) {
      await authStore.register(email.value, username.value, password.value);
    } else {
      await authStore.logIn(email.value, password.value);
    }
    // Nếu login thành công, chuyển hướng về Dashboard
    router.push('/dashboard');
  } catch {
    // Error đã được lưu trong authStore.authError
  }
}

async function handleGoogleLogin(): Promise<void> {
  const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID;
  if (!clientId) {
    googleDevFallback.value = true;
    return;
  }
  try {
    await loadGsiScript();
    const accounts = (window as unknown as { google?: { accounts?: GsiAccounts } }).google?.accounts;
    if (!accounts) throw new Error('Không thể tải Google Identity Services.');
    accounts.id.initialize({
      client_id: clientId,
      callback: handleGoogleCredential,
      auto_select: false,
    });
    accounts.id.prompt();
  } catch (err) {
    authStore.authError = err instanceof Error ? err.message : 'Không thể khởi tạo Google OAuth.';
  }
}

function handleGoogleCredential(response: GsiCredentialResponse): void {
  if (!response?.credential) {
    authStore.authError = 'Không nhận được thông tin xác thực Google.';
    return;
  }
  void performGoogleLogin(response.credential, undefined, undefined, undefined);
}

async function performGoogleLogin(idToken: string, email?: string, name?: string, googleSubject?: string): Promise<void> {
  authStore.authError = null;
  try {
    await authStore.googleLogin(idToken, email, name, googleSubject);
    toastStore.success('Đăng nhập Google thành công.');
    router.push('/dashboard');
  } catch {
    // Error đã được lưu trong authStore.authError
  }
}

async function submitGoogleDevFallback(): Promise<void> {
  const emailVal = googleFallbackEmail.value.trim();
  const nameVal = googleFallbackName.value.trim();
  if (!emailVal) {
    authStore.authError = 'Vui lòng nhập email để đăng nhập Google (chế độ dev).';
    return;
  }
  await performGoogleLogin('', emailVal, nameVal || emailVal.split('@')[0], undefined);
}

function loadGsiScript(): Promise<void> {
  return new Promise((resolve, reject) => {
    if ((window as unknown as { google?: { accounts?: unknown } }).google?.accounts) {
      resolve();
      return;
    }
    const existing = document.querySelector<HTMLScriptElement>('script[src="https://accounts.google.com/gsi/client"]');
    if (existing) {
      existing.addEventListener('load', () => resolve());
      existing.addEventListener('error', () => reject(new Error('Không thể tải Google Identity Services.')));
      return;
    }
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = () => resolve();
    script.onerror = () => reject(new Error('Không thể tải Google Identity Services.'));
    document.head.appendChild(script);
  });
}

// Nếu đã đăng nhập rồi thì redirect về dashboard
onMounted(() => {
  if (authStore.isAuthenticated) {
    router.push('/dashboard');
  }
});
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: var(--color-bg-base);
  padding: 1rem;
}

.auth-container {
  display: flex;
  width: 100%;
  max-width: 1100px;
  min-height: 650px;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  border-radius: var(--radius-2xl);
  overflow: hidden;
  box-shadow: var(--shadow-2xl);
}

.auth-left {
  flex: 1;
  min-width: 0;
  display: none;
  background: var(--color-bg-elevated);
  padding: 3rem;
  position: relative;
  border-right: 1px solid var(--color-border-default);
  overflow: hidden;
}

@media (min-width: 900px) {
  .auth-left {
    display: flex;
    flex-direction: column;
    justify-content: center;
  }
}

.auth-visual-content {
  position: relative;
  z-index: 10;
}

.brand-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: var(--color-bg-active);
  border-radius: var(--radius-full);
  font-size: 0.875rem;
  font-weight: 500;
  margin-bottom: 2rem;
  border: 1px solid var(--color-border-strong);
}

.auth-heading {
  font-size: 2.5rem;
  line-height: 1.2;
  margin-bottom: 1rem;
  color: var(--color-text-primary);
  overflow-wrap: break-word;
}

.auth-subtext {
  font-size: 1.125rem;
  margin-bottom: 3rem;
  max-width: 80%;
  overflow-wrap: break-word;
}

.auth-features {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.feature-item {
  display: flex;
  align-items: center;
  gap: 1rem;
  font-size: 1rem;
  color: var(--color-text-secondary);
  background: var(--color-bg-base);
  padding: 1rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border-default);
}

/* Giới hạn icon trong feature-item đúng 24px */
.feature-item :deep(svg),
.feature-item svg {
  width: 24px !important;
  height: 24px !important;
  flex-shrink: 0;
}

.bg-glow {
  position: absolute;
  top: -10%;
  left: -10%;
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, var(--color-accent-primary-dim) 0%, transparent 70%);
  filter: blur(80px);
  z-index: 0;
  opacity: 0.5;
}

.bg-glow-2 {
  top: auto;
  bottom: -10%;
  left: auto;
  right: -10%;
  background: radial-gradient(circle, rgba(16, 185, 129, 0.15) 0%, transparent 70%);
}

.auth-right {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  background: var(--color-bg-surface);
}

.auth-form-wrapper {
  width: 100%;
  max-width: 400px;
}

.auth-header {
  margin-bottom: 2rem;
}

.auth-error-banner {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.3);
  padding: 0.75rem 1rem;
  border-radius: var(--radius-lg);
  color: #ef4444;
  font-size: 0.875rem;
}

.form-group {
  margin-bottom: 1.25rem;
}

.form-label {
  display: block;
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--color-text-secondary);
  margin-bottom: 0.5rem;
}

.form-input {
  width: 100%;
  background: var(--color-bg-base);
  border: 1px solid var(--color-border-strong);
  border-radius: var(--radius-lg);
  padding: 0.75rem 1rem;
  font-size: 0.9375rem;
  color: var(--color-text-primary);
  outline: none;
  transition: all 0.2s;
}

.form-input:focus {
  border-color: var(--color-accent-primary);
  box-shadow: 0 0 0 3px rgba(66, 85, 255, 0.15);
}

.auth-divider {
  display: flex;
  align-items: center;
  margin: 2rem 0;
  color: var(--color-text-muted);
  font-size: 0.875rem;
}

.auth-divider::before,
.auth-divider::after {
  content: "";
  flex: 1;
  border-bottom: 1px solid var(--color-border-default);
}

.auth-divider span {
  padding: 0 1rem;
}

.btn-google {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  background: var(--color-bg-base);
  border: 1px solid var(--color-border-strong);
  color: var(--color-text-primary);
  border-radius: var(--radius-lg);
  padding: 0.75rem 1.5rem;
  font-size: 0.9375rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-google:hover {
  background: var(--color-bg-elevated);
  border-color: var(--color-text-secondary);
}

.google-icon {
  width: 20px;
  height: 20px;
}

.spinner {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* ── ENHANCED AUTH STYLES ── */

.slide-down {
  animation: slideDown 0.3s ease-out;
}

@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Floating Label Inputs */
.form-group.floating-label {
  position: relative;
}

.form-group.floating-label .form-input {
  padding: 1rem 1rem 0.5rem;
  transition: all 0.2s;
}

.form-group.floating-label .form-input:focus,
.form-group.floating-label .form-input:not(:placeholder-shown) {
  padding-top: 1.25rem;
  padding-bottom: 0.25rem;
}

.form-group.floating-label .form-input:focus + .floating-label-text,
.form-group.floating-label .form-input:not(:placeholder-shown) + .floating-label-text {
  transform: translateY(-1.5rem) scale(0.85);
  color: var(--color-accent-primary);
}

.floating-label-text {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-muted);
  font-size: 0.9375rem;
  pointer-events: none;
  transition: all 0.2s ease;
  background: var(--color-bg-surface);
  padding: 0 0.25rem;
  z-index: 1;
}

.input-focus-bar {
  position: absolute;
  bottom: 0;
  left: 50%;
  width: 0;
  height: 2px;
  background: linear-gradient(90deg, var(--color-accent-primary), var(--color-accent-cyan));
  transition: all 0.3s ease;
  transform: translateX(-50%);
  border-radius: 1px;
}

.form-group.floating-label .form-input:focus + .floating-label-text + .input-focus-bar,
.form-group.floating-label .form-input:not(:placeholder-shown) + .floating-label-text + .input-focus-bar {
  width: 100%;
}

/* Password Field Enhancements */
.password-field {
  position: relative;
}

.password-input-wrapper {
  position: relative;
  display: flex;
  align-items: flex-start;
}

.password-input-wrapper .form-input {
  flex: 1;
  padding-right: 3.5rem;
}

.password-toggle {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: 0.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color 0.2s ease;
  z-index: 2;
}

.password-toggle:hover {
  color: var(--color-accent-primary);
}

.password-toggle:focus {
  outline: none;
  color: var(--color-accent-primary);
}

/* Password Strength Meter */
.password-strength-meter {
  margin-top: 0.5rem;
  padding: 0.5rem 0.75rem;
  background: var(--color-bg-base);
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border-default);
}

.strength-bar-container {
  height: 6px;
  background: var(--color-bg-base);
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 0.5rem;
}

.strength-bar-fill {
  height: 100%;
  border-radius: 3px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  background: var(--strength-color, var(--color-accent-red));
}

.strength-labels {
  display: flex;
  justify-content: space-between;
  font-size: 0.625rem;
  color: var(--color-text-muted);
  font-family: var(--font-mono);
}

.strength-label {
  flex: 1;
  text-align: center;
  font-size: 0.5625rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  opacity: 0.3;
  transition: opacity 0.2s;
}

.strength-label.active {
  opacity: 1;
  color: var(--strength-color, var(--color-accent-red));
  font-weight: 700;
}

/* Enhanced Form Input Focus */
.form-input:focus {
  border-color: var(--color-accent-primary);
  box-shadow: 0 0 0 3px rgba(66, 85, 255, 0.15);
  outline: none;
}

.form-input::placeholder {
  color: transparent;
}

/* Particle Background for Auth Left Panel */
.particles-container {
  position: absolute;
  inset: 0;
  overflow: hidden;
  pointer-events: none;
  z-index: 0;
}

.particle {
  position: absolute;
  border-radius: 50%;
  background: var(--color-accent-primary);
  opacity: 0.1;
  animation: float 20s infinite linear;
}

@keyframes float {
  0% {
    transform: translateY(100vh) scale(0);
    opacity: 0;
  }
  10% {
    opacity: 0.1;
  }
  90% {
    opacity: 0.1;
  }
  100% {
    transform: translateY(-100vh) scale(1);
    opacity: 0;
  }
}

/* Enhanced Auth Left Panel */
.auth-left {
  position: relative;
  overflow: hidden;
}

.auth-left::before {
  content: '';
  position: absolute;
  inset: 0;
  background: 
    radial-gradient(ellipse 80% 50% at 20% 20%, rgba(99, 102, 241, 0.15) 0%, transparent 50%),
    radial-gradient(ellipse 60% 40% at 80% 80%, rgba(168, 85, 247, 0.1) 0%, transparent 50%),
    radial-gradient(ellipse 50% 60% at 50% 50%, rgba(16, 185, 129, 0.05) 0%, transparent 60%);
  pointer-events: none;
  z-index: 0;
}

/* Enhanced Auth Heading with Gradient Animation */
.auth-heading {
  background: linear-gradient(
    135deg, 
    var(--color-text-primary) 0%, 
    var(--color-accent-primary) 50%, 
    var(--color-accent-cyan) 100%
  );
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
  background-size: 200% 200%;
  animation: gradient-shift 8s ease infinite;
}

@keyframes gradient-shift {
  0%, 100% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
}

/* Enhanced Feature Items Hover */
.feature-item {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.feature-item:hover {
  transform: translateX(8px);
  border-color: var(--color-accent-primary);
  box-shadow: 0 4px 20px rgba(66, 85, 255, 0.15);
}

.feature-item BaseIcon {
  transition: transform 0.3s ease;
}

.feature-item:hover BaseIcon {
  transform: scale(1.1) rotate(5deg);
}

/* Enhanced Error Banner */
.auth-error-banner {
  animation: slideDown 0.3s ease-out;
}

/* Enhanced Submit Button */
.auth-form .btn-primary {
  position: relative;
  overflow: hidden;
}

.auth-form .btn-primary::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transform: translateX(-100%);
  transition: transform 0.5s ease;
}

.auth-form .btn-primary:hover::before {
  transform: translateX(100%);
}

/* Enhanced Google Button */
.btn-google {
  position: relative;
  overflow: hidden;
}

.btn-google::after {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
  transform: translateX(-100%);
  transition: transform 0.5s ease;
}

.btn-google:hover::after {
  transform: translateX(100%);
}

/* Demo Info Enhancement */
.demo-info {
  animation: fadeIn 0.5s ease-out 0.5s both;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
