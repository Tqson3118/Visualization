<script setup lang="ts">
// HelpView â€” MÃ n 12: FAQ accordion + liÃªn há»‡ (FR-7.2).
// View-quality (nhÃ³m A): bá» hero aurora-soft + gradient icon/title + shadow â†’ surface band
// level-2; FAQ item bá» .card legacy (shadow + all 250ms ease) â†’ token card; trigger qua
// buttonVariants ghost + aria-controls; chevron/FAQ transition easing chuáº©n; submit size lg.
// GIá»® NGUYÃŠN logic FAQ/contact + aria-expanded.
import { computed, ref } from 'vue';
import { RouterLink } from 'vue-router';
import { Motion } from 'motion-v';
import { CheckCircle2, ChevronDown, LifeBuoy, Mail, Search, User } from 'lucide-vue-next';

import { cn } from '@/lib/utils';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import { buttonVariants } from '@/components/ui/button';
import { messages } from '@/i18n/vi';

const openId = ref<string | null>('faq-0');
const searchQuery = ref('');

const FAQS = [
  {
    id: 'faq-0',
    q: 'Đăng ký tài khoản giảng viên như thế nào?',
    a: 'Tích checkbox "Tôi là giảng viên" ở form đăng ký. Tài khoản sẽ ở trạng thái chờ duyệt — Admin duyệt xong bạn nhận email thông báo.',
  },
  {
    id: 'faq-1',
    q: 'Quên mật khẩu phải làm sao?',
    a: 'Vào trang "Quên mật khẩu?", nhập email đăng ký — chúng tôi gửi link đặt lại (hiệu lực 30 phút, dùng 1 lần).',
  },
  {
    id: 'faq-2',
    q: 'Mô phỏng báo "Hết tim" — tim hồi như thế nào?',
    a: 'Bản Free: 5 tim, hồi 1 tim sau 30 phút. Mỗi lần mở node/mô phỏng trừ 1 tim (trừ 3 demo công khai và nội dung đã pass). Premium: 30 tim, hồi 10 phút.',
  },
  {
    id: 'faq-3',
    q: 'Tại sao không thấy nút "Xuất PDF CheatSheet"?',
    a: 'Xuất PDF CheatSheet là quyền lợi Premium (FR-10.7). Nâng cấp tại /premium.',
  },
  {
    id: 'faq-4',
    q: 'Code bị lỗi "Vượt quá giới hạn sandbox" nghĩa là gì?',
    a: 'Sandbox giới hạn 10 giây / 64MB / 200 dòng code / 50.000 bước trace. Kiểm tra vòng lặp vô hạn hoặc dữ liệu quá lớn.',
  },
  {
    id: 'faq-5',
    q: 'Làm sao báo lỗi hệ thống?',
    a: 'Bạn có thể gửi phản hồi qua form liên hệ bên dưới — chúng tôi ghi nhận và phản hồi sớm nhất có thể.',
  },
];

// ── UI-PREMIUM 1D: tìm kiếm FAQ (presentation only) — lọc theo câu hỏi/nội dung ──
const filteredFaqs = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  if (!q) return FAQS;
  return FAQS.filter((faq) => faq.q.toLowerCase().includes(q) || faq.a.toLowerCase().includes(q));
});

const hasFaqResult = computed(() => filteredFaqs.value.length > 0);

function toggle(id: string): void {
  openId.value = openId.value === id ? null : id;
}

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
</script>

<template>
  <main class="help container">
    <!-- Hero â€” surface band level-2 (bá» gradient aurora + shadow, Â§1/Â§6) -->
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
        <!-- Search FAQ (UI-PREMIUM 1D) — glow focus, filter presentation only -->
        <div class="help__search">
          <Search :size="16" class="help__search-icon" aria-hidden="true" />
          <input
            v-model="searchQuery"
            class="help__search-input"
            type="search"
            placeholder="Tìm câu hỏi..."
            aria-label="Tìm câu hỏi thường gặp"
          />
        </div>

        <div
          v-for="(faq, idx) in filteredFaqs"
          :key="faq.id"
          class="help__item"
        >
          <Button
            type="button"
            variant="ghost"
            class="help__question h-auto w-full justify-between whitespace-normal gap-3 rounded-md px-3 py-2 text-left"
            :aria-expanded="openId === faq.id"
            :aria-controls="`faq-answer-${faq.id}`"
            @click="toggle(faq.id)"
          >
            <span class="help__q-index" aria-hidden="true">{{ String(idx + 1).padStart(2, '0') }}</span>
            <span class="help__question-text">{{ faq.q }}</span>
            <ChevronDown
              :size="16"
              class="help__chevron"
              :class="{ 'help__chevron--open': openId === faq.id }"
              aria-hidden="true"
            />
          </button>
          <!-- Accordion height animation (grid-template-rows 0fr→1fr, easing chuẩn) -->
          <div
            :id="`faq-answer-${faq.id}`"
            class="help__answer-wrap"
            :class="{ 'help__answer-wrap--open': openId === faq.id }"
            :aria-hidden="openId !== faq.id"
          >
            <div class="help__answer-inner">
              <p class="help__answer">{{ faq.a }}</p>
            </div>
          </div>
        </div>

        <p v-if="!hasFaqResult" class="help__no-result" role="status">
          Không tìm thấy câu hỏi phù hợp với "{{ searchQuery }}".
        </p>
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
              aria-label="Ná»™i dung"
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

/* â”€â”€ Hero â€” surface band level-2 (Â§6): card-raised + border-subtle, KHÃ”NG shadow â”€â”€ */
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

/* ── Search FAQ (UI-PREMIUM 1D) — glow focus chuẩn token --glow-primary ── */
.help__search {
  position: relative;
  display: flex;
  align-items: center;
}

.help__search-icon {
  position: absolute;
  left: var(--space-md);
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-quaternary);
  pointer-events: none;
}

.help__search-input {
  width: 100%;
  height: 40px;
  padding: 0 var(--space-md) 0 var(--space-2xl);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-foreground);
  font-size: var(--text-sm);
  transition:
    border-color 150ms var(--ease-out-expo),
    box-shadow 150ms var(--ease-out-expo);
}

.help__search-input::placeholder { color: var(--color-text-quaternary); }

.help__search-input:focus-visible {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: var(--glow-primary);
}

.help__no-result {
  margin: var(--space-sm) 0 0;
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  line-height: 1.6;
}

.help__item {
  padding: var(--space-sm) var(--space-md);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  transition: border-color 150ms var(--ease-out-expo);
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
  transition: transform 250ms var(--ease-out-expo);
}

.help__chevron--open { transform: rotate(180deg); }

/* ── Accordion height animation (UI-PREMIUM 1D): grid-rows 0fr→1fr + fade-in ── */
.help__answer-wrap {
  display: grid;
  grid-template-rows: 0fr;
  transition: grid-template-rows 250ms var(--ease-out-expo);
}

.help__answer-wrap--open { grid-template-rows: 1fr; }

.help__answer-inner { overflow: hidden; min-height: 0; }

.help__answer {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  padding: 0 var(--space-sm) var(--space-sm);
  line-height: 1.7;
  margin: 0;
  opacity: 0;
  transform: translateY(-4px);
  transition:
    opacity 200ms var(--ease-out-expo) 60ms,
    transform 200ms var(--ease-out-expo) 60ms;
}

.help__answer-wrap--open .help__answer {
  opacity: 1;
  transform: translateY(0);
}

@media (prefers-reduced-motion: reduce) {
  .help__answer-wrap { transition: none; }
  .help__answer { opacity: 1; transform: none; transition: none; }
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

@media (max-width: 800px) {
  .help__grid { grid-template-columns: 1fr; }
  .help__contact { position: static; }
  .help__chrome { padding: var(--space-md); }
}
</style>
