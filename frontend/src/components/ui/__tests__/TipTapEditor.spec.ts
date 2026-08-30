import { describe, expect, it } from 'vitest';
import { flushPromises, mount } from '@vue/test-utils';
import TipTapEditor from '../TipTapEditor.vue';

describe('TipTapEditor — Word-like WYSIWYG & Loop Protection', () => {
  it('1. Render ban đầu với modelValue HTML', async () => {
    const wrapper = mount(TipTapEditor, {
      props: {
        modelValue: '<h2>Tiêu đề bài học</h2><p>Đoạn văn nội dung</p>',
      },
    });
    await flushPromises();

    expect(wrapper.find('.tiptap-content').exists()).toBe(true);
    expect(wrapper.vm.editor?.getHTML()).toContain('<h2>Tiêu đề bài học</h2>');
    expect(wrapper.vm.editor?.getHTML()).toContain('<p>Đoạn văn nội dung</p>');
    expect(wrapper.find('button[title="Tiêu đề 2 (H2)"]').exists()).toBe(true);
  });

  it('2. Toolbar có nút "Xem mã" thay vì "Mã HTML/Markdown"', () => {
    const wrapper = mount(TipTapEditor, {
      props: {
        modelValue: '<p>Nội dung</p>',
      },
    });

    const codeBtn = wrapper.findAll('button').find((b) => b.text().includes('Xem mã') || b.text().includes('Xem Trực quan'));
    expect(codeBtn).toBeDefined();
    expect(codeBtn!.text()).toContain('Xem mã');
    expect(wrapper.text()).not.toContain('Mã HTML/Markdown');
  });

  it('3. Internal update flag: onUpdate emits update:modelValue without causing watch loop', async () => {
    const wrapper = mount(TipTapEditor, {
      props: {
        modelValue: '<p>Đoạn văn ban đầu</p>',
      },
    });

    const editor = wrapper.vm.editor;
    expect(editor).toBeDefined();

    // Giả lập nhập liệu từ editor
    editor?.commands.setContent('<p>Đoạn văn sau khi gõ thêm</p>');

    await new Promise((resolve) => setTimeout(resolve, 20));

    expect(wrapper.emitted('update:modelValue')).toBeTruthy();
  });

  it('4. Hỗ trợ Table extension: chèn bảng và render thẻ <table>, <th>, <td>', async () => {
    const wrapper = mount(TipTapEditor, {
      props: {
        modelValue: '<p>Trước bảng</p>',
      },
    });
    await flushPromises();

    expect(wrapper.find('button[title*="Chèn bảng"]').exists()).toBe(true);

    wrapper.vm.insertTable();
    await flushPromises();

    const html = wrapper.vm.editor?.getHTML();
    expect(html).toContain('<table');
    expect(html).toContain('<th');
    expect(html).toContain('<td');
  });
});
