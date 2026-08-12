<script setup lang="ts">
// HelpView — Màn 12: FAQ accordion + liên hệ (FR-7.2)
import { ref } from 'vue';

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
    <header class="help__header">
      <h1 class="help__title">❓ Trợ giúp</h1>
      <p class="text-muted help__sub">Câu hỏi thường gặp — nếu chưa có câu trả lời, gửi liên hệ bên dưới.</p>
    </header>

    <div class="help__grid">
      <section class="help__faq">
        <div
          v-for="(faq, idx) in FAQS"
          :key="idx"
          class="help__item card"
        >
          <button type="button" class="help__question" :aria-expanded="openIndex === idx" @click="toggle(idx)">
            <span>{{ faq.q }}</span>
            <span class="help__chevron" :class="{ 'help__chevron--open': openIndex === idx }" aria-hidden="true">▾</span>
          </button>
          <Transition name="faq">
            <p v-if="openIndex === idx" class="help__answer">{{ faq.a }}</p>
          </Transition>
        </div>
      </section>

      <section class="help__contact card">
        <h2 class="help__contact-title">Liên hệ</h2>
        <div v-if="contactSent" class="help__sent" role="status">
          ✅ Đã gửi — chúng tôi sẽ phản hồi sớm!
        </div>
        <form v-else novalidate @submit.prevent="submitContact">
          <input v-model="contact.name" name="contact-name" class="input" placeholder="Tên của bạn" aria-label="Tên" />
          <input v-model="contact.email" name="contact-email" class="input" type="email" placeholder="Email" aria-label="Email" style="margin-top: 8px" />
          <textarea
            v-model="contact.message"
            name="contact-message"
            class="input"
            rows="5"
            placeholder="Nội dung cần hỗ trợ (tối đa 1000 ký tự)..."
            aria-label="Nội dung"
            maxlength="1000"
            style="margin-top: 8px; resize: vertical"
          />
          <p v-if="contactError" class="help__error" role="alert">{{ contactError }}</p>
          <button type="submit" class="btn btn-primary" style="margin-top: 12px">Gửi</button>
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

.help__title { font-size: var(--text-2xl); }
.help__sub { font-size: var(--text-sm); margin-top: 4px; }

.help__grid {
  display: grid;
  grid-template-columns: 3fr 2fr;
  gap: var(--space-lg);
  align-items: start;
}

.help__faq { display: flex; flex-direction: column; gap: var(--space-sm); }

.help__item { padding: var(--space-sm) var(--space-md); }

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

.help__chevron { transition: transform 200ms ease; color: var(--color-text-muted); }
.help__chevron--open { transform: rotate(180deg); }

.help__answer {
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  padding-bottom: var(--space-sm);
}

.help__contact { display: flex; flex-direction: column; gap: var(--space-sm); position: sticky; top: 80px; }

.help__contact-title { font-size: var(--text-md); }

.help__error { color: var(--color-destructive); font-size: var(--text-sm); margin-top: 8px; }

.help__sent { color: var(--color-success); font-weight: 700; }

.faq-enter-active, .faq-leave-active { transition: opacity 200ms ease; }
.faq-enter-from, .faq-leave-to { opacity: 0; }

@media (max-width: 800px) {
  .help__grid { grid-template-columns: 1fr; }
  .help__contact { position: static; }
}
</style>
