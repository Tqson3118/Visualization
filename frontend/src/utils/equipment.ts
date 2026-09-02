import type { InventoryItemDto } from '@/api/gamification';

/** Nhóm trang bị hiển thị trong Kho đồ — ưu tiên itemKey prefix (khớp data/shop_items.json),
 *  type int (0=consumable/1=avatar/2=frame) chỉ là fallback cho key lạ. */
export type EquipGroup = 'avatar' | 'frame';

export type FrameVariant = 'neon' | 'gold' | 'cyber' | 'fire' | 'ice' | 'default';
export type AvatarVariant = 'cyber' | 'gold' | 'neon' | 'wizard' | 'bot' | '';

export function equipGroup(item: InventoryItemDto): EquipGroup | null {
  if (item.itemKey.startsWith('avatar')) return 'avatar';
  if (item.itemKey.startsWith('frame')) return 'frame';
  if (item.type === 1) return 'avatar';
  if (item.type === 2) return 'frame';
  return null;
}

/** Vật phẩm đang trang bị trong nhóm (cùng nhóm → tối đa 1 item isEquipped) */
export function equippedItem(items: InventoryItemDto[], group: EquipGroup): InventoryItemDto | null {
  return items.find((i) => i.isEquipped && equipGroup(i) === group) ?? null;
}

const FRAME_VARIANTS: Record<string, FrameVariant> = {
  'frame-neon': 'neon',
  'frame-gold': 'gold',
  'frame-cyber': 'cyber',
  'frame-fire': 'fire',
  'frame-ice': 'ice',
};

export function frameVariant(itemKey: string): FrameVariant {
  return FRAME_VARIANTS[itemKey] ?? 'default';
}

const AVATAR_VARIANTS: Record<string, AvatarVariant> = {
  'avatar-cyber-hacker': 'cyber',
  'avatar-gold-knight': 'gold',
  'avatar-neon-ninja': 'neon',
  'avatar-wizard': 'wizard',
  'avatar-ai-bot': 'bot',
};

export function avatarVariant(itemKey: string): AvatarVariant {
  return AVATAR_VARIANTS[itemKey] ?? '';
}

const AVATAR_IMAGE_URLS: Record<string, string> = {
  'avatar-cyber-hacker': '/assets/avatars/cyber-hacker.svg',
  'avatar-gold-knight': '/assets/avatars/gold-knight.svg',
  'avatar-neon-ninja': '/assets/avatars/neon-ninja.svg',
  'avatar-wizard': '/assets/avatars/wizard.svg',
  'avatar-ai-bot': '/assets/avatars/ai-bot.svg',
  'avatar-dragon': '/assets/avatars/dragon.svg',
};

export function avatarImageUrl(itemKey: string): string {
  if (typeof window !== 'undefined') {
    try {
      const custom = JSON.parse(localStorage.getItem('custom_shop_assets') || '{}');
      if (custom[itemKey]) return custom[itemKey];
    } catch {}
  }
  if (AVATAR_IMAGE_URLS[itemKey]) return AVATAR_IMAGE_URLS[itemKey];
  if (itemKey.includes('dragon')) return '/assets/avatars/dragon.svg';
  return '';
}

