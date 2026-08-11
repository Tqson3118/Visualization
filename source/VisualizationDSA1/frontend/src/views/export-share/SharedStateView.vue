<template>
  <div class="shared-state-view h-full w-full flex items-center justify-center bg-bg-secondary font-sans p-6">
    <div class="max-w-lg w-full glass-panel rounded-3xl p-8 text-center">
      <div class="w-14 h-14 rounded-full bg-accent/10 border border-accent/25 flex items-center justify-center mx-auto mb-5">
        <BaseIcon name="export-share" class="w-7 h-7 text-accent" />
      </div>
      <h1 class="text-xl font-extrabold text-text-primary">Liên kết chia sẻ đã nhận</h1>
      <p class="text-sm text-text-muted mt-3 leading-relaxed">
        Cơ chế khôi phục trạng thái phòng lab từ liên kết đang được triển khai.
        Trạng thái mã hóa trong liên kết được hiển thị bên dưới.
      </p>
      <code
        class="mt-5 block max-h-40 overflow-y-auto rounded-xl bg-bg-surface border border-border-subtle px-4 py-3 text-left font-mono text-[11px] text-text-secondary break-all"
      >
        {{ statePayload || 'Không có dữ liệu trạng thái.' }}
      </code>
      <router-link
        to="/export-share"
        class="mt-6 inline-flex items-center gap-2 px-5 py-2.5 rounded-xl bg-accent text-white text-sm font-bold hover:bg-accent transition-all"
      >
        <BaseIcon name="arrow-right" class="w-4 h-4" />
        Mở Export / Share
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
import BaseIcon from '@/shared/components/BaseIcon.vue';

const route = useRoute();

const statePayload = computed(() => (route.query.state as string) || '');
</script>
