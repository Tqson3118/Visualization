<script setup lang="ts">
// CardPremium — Card level-1/2 có micro-feedback (UI-PREMIUM 0B):
// hover translateY(-2px) + border-strong + shadow-sm, group hover micro-shift
// cho icon/title, variant interactive (clickable) vs static (display only).
// KHÔNG đổi API Card cũ — extends qua props + slot passthrough.
// Giữ nguyên luật DESIGN.md §6: card CẤM shadow tĩnh — shadow chỉ xuất hiện ở hover.
import { computed, type Component } from 'vue';

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from './card';

const props = withDefaults(
  defineProps<{
    variant?: 'static' | 'interactive';
    /** 1 trong 3 trạng thái thuật toán → glow icon/block (ngôn ngữ dữ liệu §2.1) */
    glow?: 'none' | 'data-core' | 'resolved' | 'conflict';
    icon?: Component | null;
    title?: string;
    description?: string;
    /** micro-shift nội dung khi group hover (icon/title) */
    shift?: boolean;
  }>(),
  {
    variant: 'static',
    glow: 'none',
    icon: null,
    title: '',
    description: '',
    shift: false,
  },
);

const glowClass = computed(() => {
  switch (props.glow) {
    case 'resolved':
      return 'ui-cardpremium--glow-resolved';
    case 'conflict':
      return 'ui-cardpremium--glow-conflict';
    case 'data-core':
      return 'ui-cardpremium--glow-data';
    default:
      return '';
  }
});
</script>

<template>
  <Card
    class="ui-cardpremium"
    :class="[
      variant === 'interactive' ? 'ui-cardpremium--interactive' : '',
      glowClass,
      shift ? 'ui-cardpremium--shift' : '',
    ]"
  >
    <CardHeader v-if="title || icon">
      <div v-if="icon" class="ui-cardpremium__icon" aria-hidden="true">
        <component :is="icon" :size="18" />
      </div>
      <CardTitle v-if="title" class="ui-cardpremium__title">{{ title }}</CardTitle>
      <CardDescription v-if="description" class="ui-cardpremium__desc">
        {{ description }}
      </CardDescription>
    </CardHeader>
    <CardContent class="ui-cardpremium__content">
      <slot />
    </CardContent>
  </Card>
</template>

<style scoped>
.ui-cardpremium {
  display: flex;
  flex-direction: column;
  transition:
    border-color 150ms var(--ease-out-expo),
    box-shadow 150ms var(--ease-out-expo),
    transform 150ms var(--ease-out-expo),
    background-color 150ms var(--ease-out-expo);
}

.ui-cardpremium--interactive:hover {
  transform: translateY(-2px);
  border-color: var(--color-border-strong);
  box-shadow: var(--shadow-sm);
}

.ui-cardpremium--interactive:focus-within {
  box-shadow: var(--glow-primary);
}

.ui-cardpremium__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-sm);
  transition:
    transform 150ms var(--ease-out-expo),
    box-shadow 150ms var(--ease-out-expo),
    background-color 150ms var(--ease-out-expo);
}

.ui-cardpremium:hover .ui-cardpremium__icon {
  transform: translateY(-1px);
}

/* Glow theo trạng thái thuật toán — nguồn palette 6 màu (KHÔNG màu mới) */
.ui-cardpremium--glow-data .ui-cardpremium__icon,
.ui-cardpremium--glow-data-core .ui-cardpremium__icon {
  box-shadow: var(--glow-data-core);
  background: color-mix(in srgb, var(--color-data-core) 12%, var(--color-muted));
  color: var(--color-data-core);
}

.ui-cardpremium--glow-resolved .ui-cardpremium__icon {
  box-shadow: var(--glow-resolved);
  background: color-mix(in srgb, var(--color-resolved) 12%, var(--color-muted));
  color: var(--color-resolved);
}

.ui-cardpremium--glow-conflict .ui-cardpremium__icon {
  box-shadow: var(--glow-conflict);
  background: color-mix(in srgb, var(--color-conflict) 12%, var(--color-muted));
  color: var(--color-conflict);
}

/* Micro-shift nội dung khi group hover */
.ui-cardpremium--shift .ui-cardpremium__title,
.ui-cardpremium--shift .ui-cardpremium__icon {
  transition: transform 150ms var(--ease-out-expo);
}

.ui-cardpremium--shift:hover .ui-cardpremium__title {
  transform: translateX(2px);
}

.ui-cardpremium--shift:hover .ui-cardpremium__icon {
  transform: translateX(2px);
}

.ui-cardpremium__title {
  font-size: var(--text-md);
  font-weight: 600;
  line-height: 1.3;
  letter-spacing: -0.015em;
}

.ui-cardpremium__desc {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.ui-cardpremium__content {
  margin-top: auto;
}
</style>
