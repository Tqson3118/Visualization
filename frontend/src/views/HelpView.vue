<script setup lang="ts">
// HelpView — Màn 12: FAQ accordion + liên hệ (FR-7.2).
// View-quality (nhóm A): bỏ hero aurora-soft + gradient icon/title + shadow → surface band
// level-2; FAQ item bỏ .card legacy (shadow + all 250ms ease) → token card; trigger qua
// buttonVariants ghost + aria-controls; chevron/FAQ transition easing chuẩn; submit size lg.
// GIỮ NGUYÊN logic FAQ/contact + aria-expanded.
import { ref } from 'vue';
import { RouterLink } from 'vue-router';
import { Motion } from 'motion-v';
import { CheckCircle2, ChevronDown, LifeBuoy, Mail, User } from 'lucide-vue-next';

import { cn } from '@/lib/utils';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import { buttonVariants } from '@/components/ui/button';
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
    <!-- Hero — surface band level-2 (bỏ gradient aurora + shadow, §1/§6) -->
    <Motion
      class="help__chrome"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <nav class="help__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'home' }">{{ messages.help.breadcrumbHome }}</RouterLink>
        <span aria-hidden="true">/</span>
        <span aria-current="page">{{ messages.help.title }}</span>
      </nav>
      <div class="help__hero">
        <span class="help__icon" aria-hidden="true">
          <LifeBuoy :size="20" />
        </span>
        <div>
          <h1 class="help__title">{{ messages.help.title }}</h1>
          <p class="help__sub">{{ messages.help.sub }}</p>
        </div>
      </div>
    </Motion>

    <div class="help__grid">
      <section class="help__faq" :aria-label="messages.help.faqAria">
        <div
          v-for="(faq, idx) in FAQS"
          :key="idx"
          class="help__item"
        >
          <button
            type="button"
            :class="cn(
              buttonVariants({ variant: 'ghost' }),
              'help__question h-auto w-full justify-between whitespace-normal gap-3 rounded-md px-3 py-2 text-left',
            )"
            :aria-expanded="openIndex === idx"
            :aria-controls="`faq-answer-${idx}`"
            @click="toggle(idx)"
          >
            <span class="help__q-index" aria-hidden="true">{{ String(idx + 1).padStart(2, '0') }}</span>
            <span class="help__question-text">{{ faq.q }}</span>
            <ChevronDown
              :size="16"
              class="help__chevron"
              :class="{ 'help__chevron--open': openIndex === idx }"
              aria-hidden="true"
            />
          </button>
          <Transition name="faq">
            <p v-if="openIndex === idx" :id="`faq-answer-${idx}`" class="help__answer">{{ faq.a }}</p>
          </Transition>
        </div>
      </section>

      <section class="help__contact">
        <header class="help__contact-head">
          <span class="help__contact-icon" aria-hidden="true">
            <Mail :size="16" />
          </span>
          <div>
            <h2 class="help__contact-title">{{ messages.help.contactTitle }}</h2>
            <p class="help__contact-sub">{{ messages.help.contactSub }}</p>
          </div>
        </header>

        <div v-if="contactSent" class="help__sent" role="status">
          <CheckCircle2 :size="22" class="help__sent-icon" aria-hidden="true" />
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
            :icon="User"
            autocomplete="name"
            required
          />
          <Input
            v-model="contact.email"
            type="email"
            :label="messages.help.emailLabel"
            :placeholder="messages.help.emailPlaceholder"
            :icon="Mail"
            autocomplete="email"
            required
          />
          <div class="help__field">
            <label for="help-message" class="help__field-label">
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
          <Button type="submit" size="lg" class="help__submit">
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

/* ── Hero — surface band level-2 (§6): card-raised + border-subtle, KHÔNG shadow ── */
.help__chrome {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.help__breadcrumb {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

.help__breadcrumb a {
  color: var(--color-primary);
  font-weight: 600;
  padding-block: var(--space-xs);
}

.help__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.help__icon {
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

.help__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.help__sub {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  max-width: 64ch;
  margin-top: var(--space-xs);
}

.help__grid {
  display: grid;
  grid-template-columns: 3fr 2fr;
  gap: var(--space-lg);
  align-items: start;
}

.help__faq { display: flex; flex-direction: column; gap: var(--space-sm); }

.help__item {
  padding: var(--space-sm) var(--space-md);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
}

.help__item:hover { border-color: var(--color-border-strong); }

.help__q-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  color: var(--color-text-tertiary);
  flex-shrink: 0;
}

.help__question-text { line-height: 1.45; font-weight: 600; }

.help__chevron {
  flex-shrink: 0;
  color: var(--color-text-secondary);
  transition: transform 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.help__chevron--open { transform: rotate(180deg); }

.help__answer {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  padding: 0 var(--space-sm) var(--space-sm);
  line-height: 1.7;
  margin: 0;
}

.help__contact {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  position: sticky;
  top: 80px;
  padding: var(--space-lg);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
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
  background: var(--color-muted);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.help__contact-title {
  font-size: var(--text-xl);
  font-weight: 600;
  line-height: 1.25;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
  margin: 0;
}

.help__contact-sub { font-size: var(--text-xs); color: var(--color-text-tertiary); margin-top: var(--space-xs); }

.help__form { display: flex; flex-direction: column; gap: var(--space-md); }

.help__field { display: flex; flex-direction: column; gap: var(--space-sm); }

.help__field-label {
  display: block;
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--color-foreground);
}

.help__textarea { resize: vertical; min-height: 120px; }

.help__error { color: var(--color-destructive); font-size: var(--text-sm); margin: 0; }

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

.help__sent-title {
  color: var(--color-success);
  font-weight: 600;
  font-size: var(--text-sm);
  margin: 0;
}

.help__sent-desc { font-size: var(--text-xs); color: var(--color-text-secondary); margin-top: var(--space-xs); }

/* FAQ expand — enter/exit easing chuẩn DESIGN.md §7 (transform + opacity) */
.faq-enter-active {
  transition: opacity 200ms cubic-bezier(0.16, 1, 0.3, 1), transform 200ms cubic-bezier(0.16, 1, 0.3, 1);
}

.faq-leave-active {
  transition: opacity 150ms cubic-bezier(0.7, 0, 0.84, 0), transform 150ms cubic-bezier(0.7, 0, 0.84, 0);
}

.faq-enter-from, .faq-leave-to { opacity: 0; transform: translateY(-4px); }

@media (max-width: 800px) {
  .help__grid { grid-template-columns: 1fr; }
  .help__contact { position: static; }
  .help__chrome { padding: var(--space-md); }
}
</style>
