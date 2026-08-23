import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

import AdminHeroStrip from './AdminHeroStrip.vue';

// AdminHeroStrip — mono strip block-token: size phải REACTIVE theo props.count
// (bug 14/08: size tính 1 lần ở setup → count thay đổi sau khi data load thì
// số block không cập nhật — cố định ở giá trị khởi tạo).
describe('AdminHeroStrip', () => {
  it('render count filled blocks + index mono + caption', () => {
    const wrapper = mount(AdminHeroStrip, {
      props: { count: 3, label: '03 CHỈ SỐ' },
    });
    const blocks = wrapper.findAll('.admin-strip__block');
    expect(blocks).toHaveLength(3);
    expect(blocks.filter((b) => !b.classes().includes('admin-strip__block--empty'))).toHaveLength(3);
    expect(wrapper.findAll('.admin-strip__index span')).toHaveLength(3);
    expect(wrapper.find('.admin-strip__caption').text()).toBe('03 CHỈ SỐ');
  });

  it('count=0 → 1 block empty (giữ panel khi chưa có dữ liệu)', () => {
    const wrapper = mount(AdminHeroStrip, {
      props: { count: 0, label: '00 CHỜ DUYỆT' },
    });
    const blocks = wrapper.findAll('.admin-strip__block');
    expect(blocks).toHaveLength(1);
    expect(blocks[0].classes()).toContain('admin-strip__block--empty');
  });

  it('cập nhật số block khi count thay đổi sau khi mount (reactive)', async () => {
    const wrapper = mount(AdminHeroStrip, {
      props: { count: 0, label: '00' },
    });
    expect(wrapper.findAll('.admin-strip__block')).toHaveLength(1);

    await wrapper.setProps({ count: 2, label: '02' });
    const blocks = wrapper.findAll('.admin-strip__block');
    expect(blocks).toHaveLength(2);
    expect(blocks.filter((b) => !b.classes().includes('admin-strip__block--empty'))).toHaveLength(2);
    expect(wrapper.findAll('.admin-strip__index span')).toHaveLength(2);
  });

  it('activeIndices thắng count (filled theo danh sách cụ thể)', () => {
    const wrapper = mount(AdminHeroStrip, {
      props: { count: 5, activeIndices: [0, 2] },
    });
    const blocks = wrapper.findAll('.admin-strip__block');
    expect(blocks).toHaveLength(5);
    expect(blocks.filter((b) => !b.classes().includes('admin-strip__block--empty'))).toHaveLength(2);
  });
});
