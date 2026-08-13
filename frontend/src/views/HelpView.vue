<script setup lang="ts">
// HelpView â€” MÃ n 12: FAQ accordion + liÃªn há»‡ (FR-7.2).
// View-quality (nhÃ³m A): bá» hero aurora-soft + gradient icon/title + shadow â†’ surface band
// level-2; FAQ item bá» .card legacy (shadow + all 250ms ease) â†’ token card; trigger qua
// buttonVariants ghost + aria-controls; chevron/FAQ transition easing chuáº©n; submit size lg.
// GIá»® NGUYÃŠN logic FAQ/contact + aria-expanded.
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
    q: 'ÄÄƒng kÃ½ tÃ i khoáº£n giáº£ng viÃªn nhÆ° tháº¿ nÃ o?',
    a: 'TÃ­ch checkbox "TÃ´i lÃ  giáº£ng viÃªn" á»Ÿ form Ä‘Äƒng kÃ½. TÃ i khoáº£n sáº½ á»Ÿ tráº¡ng thÃ¡i chá» duyá»‡t â€” Admin duyá»‡t xong báº¡n nháº­n email thÃ´ng bÃ¡o.',
  },
  {
    q: 'QuÃªn máº­t kháº©u pháº£i lÃ m sao?',
    a: 'VÃ o trang "QuÃªn máº­t kháº©u?", nháº­p email Ä‘Äƒng kÃ½ â€” chÃºng tÃ´i gá»­i link Ä‘áº·t láº¡i (hiá»‡u lá»±c 30 phÃºt, dÃ¹ng 1 láº§n).',
  },
  {
    q: 'MÃ´ phá»ng bÃ¡o "Háº¿t tim" â€” tim há»“i nhÆ° tháº¿ nÃ o?',
    a: 'Báº£n Free: 5 tim, há»“i 1 tim sau 30 phÃºt. Má»—i láº§n má»Ÿ node/mÃ´ phá»ng trá»« 1 tim (trá»« 3 demo cÃ´ng khai vÃ  ná»™i dung Ä‘Ã£ pass). Premium: 30 tim, há»“i 10 phÃºt.',
  },
  {
    q: 'Táº¡i sao khÃ´ng tháº¥y nÃºt "Xuáº¥t PDF CheatSheet"?',
    a: 'Xuáº¥t PDF CheatSheet lÃ  quyá»n lá»£i Premium (FR-10.7). NÃ¢ng cáº¥p táº¡i /premium.',
  },
  {
    q: 'Code bá»‹ lá»—i "VÆ°á»£t quÃ¡ giá»›i háº¡n sandbox" nghÄ©a lÃ  gÃ¬?',
    a: 'Sandbox giá»›i háº¡n 10 giÃ¢y / 64MB / 200 dÃ²ng code / 50.000 bÆ°á»›c trace. Kiá»ƒm tra vÃ²ng láº·p vÃ´ háº¡n hoáº·c dá»¯ liá»‡u quÃ¡ lá»›n.',
  },
  {
    q: 'LÃ m sao bÃ¡o lá»—i há»‡ thá»‘ng?',
    a: 'Báº¡n cÃ³ thá»ƒ gá»­i pháº£n há»“i qua form liÃªn há»‡ bÃªn dÆ°á»›i â€” chÃºng tÃ´i ghi nháº­n vÃ  pháº£n há»“i sá»›m nháº¥t cÃ³ thá»ƒ.',
  },
];

const contact = ref({ name: '', email: '', message: '' });
const contactError = ref('');
const contactSent = ref(false);

function submitContact(): void {
  contactError.value = '';
  if (contact.value.name.trim().length < 2 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(contact.value.email)) {
    contactError.value = 'Vui lÃ²ng Ä‘iá»n tÃªn vÃ  email há»£p lá»‡.';
    return;
  }
  if (contact.value.message.trim().length < 10) {
    contactError.value = 'Ná»™i dung pháº£i tá»« 10 kÃ½ tá»±.';
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
        <div
          v-for="(faq, idx) in FAQS"
          :key="idx"
          class="help__item"
        >
          <Button
            type="button"
            variant="ghost"
            class="help__question h-auto w-full justify-between whitespace-normal gap-3 rounded-md px-3 py-2 text-left"
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

/* FAQ expand â€” enter/exit easing chuáº©n DESIGN.md Â§7 (transform + opacity) */
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
