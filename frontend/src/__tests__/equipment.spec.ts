import { describe, expect, it } from 'vitest';
import type { InventoryItemDto } from '@/api/gamification';
import {
  avatarImageUrl,
  avatarVariant,
  equipGroup,
  equippedItem,
  frameVariant,
} from '@/utils/equipment';

function makeItem(overrides: Partial<InventoryItemDto>): InventoryItemDto {
  return {
    id: 1,
    itemId: 1,
    itemKey: 'test-key',
    name: 'Test Item',
    quantity: 1,
    type: 0,
    isEquipped: false,
    expiresAt: null,
    ...overrides,
  };
}

describe('Equipment Utilities — Module L (U-EQP-001 ~ U-EQP-011)', () => {
  it('U-EQP-001: equipGroup nhận diện avatar theo itemKey prefix', () => {
    const item = makeItem({ itemKey: 'avatar-cyber-hacker', type: 0 });
    expect(equipGroup(item)).toBe('avatar');
  });

  it('U-EQP-002: equipGroup nhận diện frame theo itemKey prefix', () => {
    const item = makeItem({ itemKey: 'frame-neon', type: 0 });
    expect(equipGroup(item)).toBe('frame');
  });

  it('U-EQP-003: equipGroup fallback theo type khi itemKey không khớp prefix', () => {
    const avatarTypeItem = makeItem({ itemKey: 'custom-item-1', type: 1 });
    expect(equipGroup(avatarTypeItem)).toBe('avatar');

    const frameTypeItem = makeItem({ itemKey: 'custom-item-2', type: 2 });
    expect(equipGroup(frameTypeItem)).toBe('frame');
  });

  it('U-EQP-004: equipGroup trả về null cho key và type không hợp lệ/consumable', () => {
    const item = makeItem({ itemKey: 'consumable-heart-pack', type: 0 });
    expect(equipGroup(item)).toBeNull();
  });

  it('U-EQP-005: equippedItem tìm đúng item isEquipped trong nhóm', () => {
    const items: InventoryItemDto[] = [
      makeItem({ id: 101, itemKey: 'avatar-cyber-hacker', isEquipped: false }),
      makeItem({ id: 102, itemKey: 'avatar-wizard', isEquipped: true }),
      makeItem({ id: 201, itemKey: 'frame-neon', isEquipped: true }),
    ];
    const equippedAvatar = equippedItem(items, 'avatar');
    expect(equippedAvatar).not.toBeNull();
    expect(equippedAvatar?.id).toBe(102);
    expect(equippedAvatar?.itemKey).toBe('avatar-wizard');
  });

  it('U-EQP-006: equippedItem trả về null khi không có item nào được trang bị trong nhóm', () => {
    const items: InventoryItemDto[] = [
      makeItem({ id: 101, itemKey: 'avatar-cyber-hacker', isEquipped: false }),
      makeItem({ id: 201, itemKey: 'frame-neon', isEquipped: true }),
    ];
    expect(equippedItem(items, 'avatar')).toBeNull();
  });

  it('U-EQP-007: frameVariant trả đúng variant cho tất cả key hợp lệ', () => {
    expect(frameVariant('frame-neon')).toBe('neon');
    expect(frameVariant('frame-gold')).toBe('gold');
    expect(frameVariant('frame-cyber')).toBe('cyber');
    expect(frameVariant('frame-fire')).toBe('fire');
    expect(frameVariant('frame-ice')).toBe('ice');
  });

  it('U-EQP-008: frameVariant fallback về default khi key không tồn tại', () => {
    expect(frameVariant('frame-unknown')).toBe('default');
    expect(frameVariant('')).toBe('default');
  });

  it('U-EQP-009: avatarVariant trả đúng variant cho tất cả key hợp lệ', () => {
    expect(avatarVariant('avatar-cyber-hacker')).toBe('cyber');
    expect(avatarVariant('avatar-gold-knight')).toBe('gold');
    expect(avatarVariant('avatar-neon-ninja')).toBe('neon');
    expect(avatarVariant('avatar-wizard')).toBe('wizard');
    expect(avatarVariant('avatar-ai-bot')).toBe('bot');
    expect(avatarVariant('avatar-unknown')).toBe('');
  });

  it('U-EQP-010: avatarImageUrl trả đúng URL cho tất cả avatar key hợp lệ', () => {
    expect(avatarImageUrl('avatar-cyber-hacker')).toBe('/assets/avatars/cyber-hacker.svg');
    expect(avatarImageUrl('avatar-gold-knight')).toBe('/assets/avatars/gold-knight.svg');
    expect(avatarImageUrl('avatar-neon-ninja')).toBe('/assets/avatars/neon-ninja.svg');
    expect(avatarImageUrl('avatar-wizard')).toBe('/assets/avatars/wizard.svg');
    expect(avatarImageUrl('avatar-ai-bot')).toBe('/assets/avatars/ai-bot.svg');
  });

  it('U-EQP-011: avatarImageUrl trả về chuỗi rỗng khi key không tồn tại', () => {
    expect(avatarImageUrl('avatar-unknown')).toBe('');
    expect(avatarImageUrl('')).toBe('');
  });
});
