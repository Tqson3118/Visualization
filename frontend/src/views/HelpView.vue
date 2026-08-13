<script setup lang="ts">
// HelpView — Màn 12: FAQ accordion + liên hệ (FR-7.2)
// H-E2: hero Aurora soft + FAQ card (lucide chevron) + form shadcn (Input/Button).
// GIỮ NGUYÊN logic FAQ/contact + aria-expanded/role.
import { ref } from 'vue';
import { RouterLink } from 'vue-router';
import { CheckCircle2, ChevronDown, LifeBuoy, Mail } from 'lucide-vue-next';

import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import { messages } from '@/i18n/vi';

const openIndex = ref<number | null>(0);

const FAQS = [
  {
    q: 'Đăng ký tài khoản giảng viên như thế nào?',
    a: 'Tích checkbox "Tôi là giảng viên" ở form đăng ký. Tài khoản sẽ ở trạng thái chờ duyệt — Admin duyệt xong bạn nhận email thông báo.',
  },
  {
    q: 'Quên mật khẩu phải làm sao?',
    a: 'Vào trang "Quên mật khẩu?", nhập email đăng ký — chúng tôi gửi link đặt lại (hiệu lực 30 phút, dùng 1 lần).',
  },
  {
    q: 'Mô phỏng báo "Hết tim" — tim hồi như thế nào?',
    a: 'Bản Free: 5 tim, hồi 1 tim sau 30 phút. Mỗi lần mở node/mô phỏng trừ 1 tim (trừ 3 demo công khai và nội dung đã pass). Premium: 30 tim, hồi 10 phút.',
  },
  {
    q: 'Tại sao không thấy nút "Xuất PDF CheatSheet"?',
    a: 'Xuất PDF CheatSheet là quyền lợi Premium (FR-10.7). Nâng cấp tại /premium.',
  },
  {
    q: 'Code bị lỗi "Vượt quá giới hạn sandbox" nghĩa là gì?',
    a: 'Sandbox giới hạn 10 giây / 64MB / 200 dòng code / 50.000 bước trace. Kiểm tra vòng lặp vô hạn hoặc dữ liệu quá lớn.',
  },
  {
    q: 'Làm sao báo lỗi hệ thống?',
    a: 'Bạn có thể gửi phản hồi qua form liên hệ bên dưới — chúng tôi ghi nhận và phản hồi sớm nhất có thể.',
  },
];

const contact = ref({ name: '', email: '', message: '' });
const contactError = ref('');
const contactSent = ref(false);

function submitContact(): void {
  contactError.value = '';
  if (contact.value.name.trim().length < 2 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(contact.value.email)) {
    contactError.value = 'Vui lòng điền tên và email hợp lệ.';
    return;
  }
  if (contact.value.message.trim().length < 10) {
    contactError.value = 'Nội dung phải từ 10 ký tự.';
    return;
  }
  contactSent.value = true;
}

function toggle(idx: number): void {
  openIndex.value = openIndex.value === idx ? null : idx;
}
</script>

<template>
  <main class="help container">
    <!-- Hero — Aurora soft (palette 1 nhạt) -->
    <header class="help__chrome">
      <nav class="help__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'home' }">{{ messages.help.breadcrumbHome }}</RouterLink>
        <span aria-hidden="true">/</span>
        <span>{{ messages.help.title }}</span>
      </nav>
      <div class="help__hero">
        <span class="help__icon" aria-hidden="true">
          <LifeBuoy :size="22" />
        </span>
        <div>
          <h1 class="help__title">{{ messages.help.title }}</h1>
          <p class="help__sub">{{ messages.help.sub }}</p>
        </div>
      </div>
    </header>

    <div class="help__grid">
      <section class="help__faq" :aria-label="messages.help.faqAria">
        <div
          v-for="(faq, idx) in FAQS"
          :key="idx"
          class="help__item card"
        >
          <button
            type="button"
            class="help__question"
            :aria-expanded="openIndex === idx"
            @click="toggle(idx)"
          >
            <span class="help__question-text">{{ faq.q }}</span>
            <ChevronDown
              :size="18"
              class="help__chevron"
              :class="{ 'help__chevron--open': openIndex === idx }"
              aria-hidden="true"
            />
          </button>
          <Transition name="faq">
            <p v-if="openIndex === idx" class="help__answer">{{ faq.a }}</p>
          </Transition>
        </div>
      </section>

      <section class="help__contact card">
        <header class="help__contact-head">
          <span class="help__contact-icon" aria-hidden="true">
            <Mail :size="18" />
          </span>
          <div>
            <h2 class="help__contact-title">{{ messages.help.contactTitle }}</h2>
            <p class="help__contact-sub">{{ messages.help.contactSub }}</p>
          </div>
        </header>

        <div v-if="contactSent" class="help__sent" role="status">
          <CheckCircle2 :size="26" class="help__sent-icon" aria-hidden="true" />
          <div>
            <p class="help__sent-title">{{ messages.help.sentTitle }}</p>
            <p class="help__sent-desc">{{ messages.help.sentDesc }}</p>
          </div>
        </div>

        <form v-else class="help__form" novalidate @submit.prevent="submitContact">
          <Input
            v-model="contact.name"
            :label="messages.help.nameLabel"
            :placeholder="messages.help.namePlaceholder"
            icon="user"
            autocomplete="name"
            required
          />
          <Input
            v-model="contact.email"
            type="email"
            :label="messages.help.emailLabel"
            :placeholder="messages.help.emailPlaceholder"
            icon="mail"
            autocomplete="email"
            required
          />
          <div class="help__field">
            <label for="help-message" class="mb-1.5 block text-sm font-semibold">
              {{ messages.help.messageLabel }}
              <span class="text-destructive" aria-hidden="true"> *</span>
            </label>
            <textarea
              id="help-message"
              v-model="contact.message"
              class="input help__textarea"
              rows="5"
              maxlength="1000"
              :placeholder="messages.help.messagePlaceholder"
              aria-label="Nội dung"
            />
          </div>
          <p v-if="contactError" class="help__error" role="alert">{{ contactError }}</p>
          <Button type="submit" class="help__submit">
            {{ messages.help.submit }}
          </Button>
        </form>
      </section>
    </div>
  </main>
</template>

<style scoped>
.help {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 900px;
}

/* ── Hero — Aurora soft ── */
.help__chrome {
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

.help__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.help__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.help__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.help__icon {
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

.help__title {
  font-size: clamp(var(--text-2xl), 4vw, var(--text-3xl));
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.help__sub {
  font-size: var(--text-sm);
  /* H-E2: foreground 92% — text-muted chỉ 4.26:1 trên aurora-soft light (sát fail AA) */
  color: color-mix(in srgb, var(--color-foreground) 92%, transparent);
  max-width: 64ch;
  margin-top: 2px;
}

.help__grid {
  display: grid;
  grid-template-columns: 3fr 2fr;
  gap: var(--space-lg);
  align-items: start;
}

.help__faq { display: flex; flex-direction: column; gap: var(--space-sm); }

.help__item { padding: var(--space-sm) var(--space-md); }

.help__item:hover { border-color: color-mix(in srgb, var(--color-primary) 35%, var(--color-border)); }

.help__question {
  width: 100%;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
  background: none;
  border: none;
  font-weight: 700;
  font-size: var(--text-sm);
  text-align: left;
  cursor: pointer;
  padding: var(--space-xs) 0;
  color: var(--color-foreground);
}

.help__question-text { line-height: 1.45; }

.help__chevron {
  flex-shrink: 0;
  transition: transform 200ms ease;
  color: var(--color-text-muted);
}

.help__chevron--open { transform: rotate(180deg); }

.help__answer {
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  padding-bottom: var(--space-sm);
  line-height: 1.7;
}

.help__contact {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  position: sticky;
  top: 80px;
}

.help__contact-head {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding-bottom: var(--space-sm);
  border-bottom: 1px solid var(--color-border);
}

.help__contact-icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background-image: var(--gradient-aurora);
  color: #fff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.help__contact-title { font-size: var(--text-md); }

.help__contact-sub { font-size: var(--text-xs); color: var(--color-text-muted); margin-top: 2px; }

.help__form { display: flex; flex-direction: column; gap: var(--space-md); }

.help__field { display: flex; flex-direction: column; gap: 6px; }

.help__textarea { resize: vertical; min-height: 120px; }

.help__error { color: var(--color-destructive); font-size: var(--text-sm); }

.help__submit { align-self: flex-start; }

.help__sent {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px solid color-mix(in srgb, var(--color-success) 40%, var(--color-border));
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-success) 8%, var(--color-surface));
}

.help__sent-icon { color: var(--color-success); flex-shrink: 0; }

.help__sent-title { color: var(--color-success); font-weight: 700; font-size: var(--text-sm); }

.help__sent-desc { font-size: var(--text-xs); color: var(--color-text-muted); margin-top: 2px; }

.faq-enter-active, .faq-leave-active { transition: opacity 200ms ease, transform 200ms ease; }
.faq-enter-from, .faq-leave-to { opacity: 0; transform: translateY(-4px); }

@media (max-width: 800px) {
  .help__grid { grid-template-columns: 1fr; }
  .help__contact { position: static; }
  .help__chrome { padding: var(--space-md); }
}
</style>
