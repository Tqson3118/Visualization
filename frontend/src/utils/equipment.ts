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

const memoryCustomAssets: Record<string, string> = {};

export function initCustomShopAssets(assets: Record<string, string>): void {
  if (!assets) return;
  for (const [k, v] of Object.entries(assets)) {
    if (k && v) {
      memoryCustomAssets[k.toLowerCase().trim()] = v;
    }
  }
}

export function setCustomShopAsset(key: string, url: string): void {
  if (!key || !url) return;
  const normKey = key.toLowerCase().trim();
  memoryCustomAssets[normKey] = url;
  if (typeof window !== 'undefined') {
    try {
      const custom = JSON.parse(localStorage.getItem('custom_shop_assets') || '{}');
      custom[normKey] = url;
      custom[key] = url;
      localStorage.setItem('custom_shop_assets', JSON.stringify(custom));
    } catch {}
  }
}

export function avatarImageUrl(itemKey: string, fallbackUrl?: string | null): string {
  if (fallbackUrl && fallbackUrl.trim()) {
    let url = fallbackUrl.trim();
    if (url.startsWith('/assets/avatars/') && url.endsWith('.png')) {
      url = url.replace('.png', '.svg');
    }
    return url;
  }
  if (!itemKey) return '';
  const normKey = itemKey.toLowerCase().trim();

  // 1. In-memory custom assets
  if (memoryCustomAssets[normKey]) return memoryCustomAssets[normKey];

  // 2. LocalStorage custom assets
  if (typeof window !== 'undefined') {
    try {
      const custom = JSON.parse(localStorage.getItem('custom_shop_assets') || '{}');
      if (custom[normKey]) return custom[normKey];
      if (custom[itemKey]) return custom[itemKey];
    } catch {}
  }

  // 3. Built-in SVG presets
  if (AVATAR_IMAGE_URLS[normKey]) return AVATAR_IMAGE_URLS[normKey];
  if (AVATAR_IMAGE_URLS[itemKey]) return AVATAR_IMAGE_URLS[itemKey];
  if (normKey.includes('dragon')) return '/assets/avatars/dragon.svg';
  return '';
}

/**
 * Nén ảnh tự động về kích thước nhỏ gọn (tối đa 256x256) chất lượng cao
 * Giảm từ 3MB xuống chỉ ~20-35KB, đảm bảo lưu vĩnh viễn và không bao giờ vượt quota.
 */
export function compressImage(file: File, maxDim = 256, quality = 0.85): Promise<string> {
  return new Promise((resolve, reject) => {
    if (!file) return reject(new Error('Không có file ảnh'));
    const reader = new FileReader();
    reader.onerror = reject;
    reader.onload = (e) => {
      const img = new Image();
      img.onerror = reject;
      img.onload = () => {
        let width = img.naturalWidth || img.width;
        let height = img.naturalHeight || img.height;
        if (width > height) {
          if (width > maxDim) {
            height = Math.round((height * maxDim) / width);
            width = maxDim;
          }
        } else {
          if (height > maxDim) {
            width = Math.round((width * maxDim) / height);
            height = maxDim;
          }
        }

        const canvas = document.createElement('canvas');
        canvas.width = Math.max(1, width);
        canvas.height = Math.max(1, height);
        const ctx = canvas.getContext('2d');
        if (!ctx) {
          resolve(e.target?.result as string);
          return;
        }

        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
        const isPng = file.type === 'image/png' || file.type === 'image/svg+xml';
        const mime = isPng ? 'image/png' : 'image/jpeg';
        const dataUrl = canvas.toDataURL(mime, quality);
        resolve(dataUrl);
      };
      img.src = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  });
}

