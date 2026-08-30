import { beforeEach, describe, expect, it, vi } from 'vitest';
import { mount } from '@vue/test-utils';
import { nextTick } from 'vue';
import OutlineTree from '../OutlineTree.vue';
import type { PathItemDto } from '@/api/pathItems';

const STORAGE_KEY = 'metqua.studio.outline.open.v1';

function makeItem(partial: Partial<PathItemDto> & { id: number; itemType: PathItemDto['itemType'] }): PathItemDto {
  return {
    pathId: 10,
    parentId: null,
    title: 'Mục ' + partial.id,
    sortOrder: 1,
    children: [],
    ...partial,
  } as PathItemDto;
}

const folder = makeItem({
  id: 1,
  itemType: 'folder',
  title: 'Chương 1: Mảng & Danh sách liên kết',
  description: 'Nền tảng cấu trúc dữ liệu',
  sortOrder: 1,
  children: [
    makeItem({ id: 2, itemType: 'theory', parentId: 1, title: 'Bài 1.1: Giới thiệu Mảng động', sortOrder: 1 }),
    makeItem({ id: 3, itemType: 'quiz', parentId: 1, title: 'Quiz 1.1: Trắc nghiệm Mảng', sortOrder: 2 }),
    makeItem({ id: 4, itemType: 'lab', parentId: 1, title: 'Lab 1.1: Đảo ngược mảng', sortOrder: 3 }),
  ],
});

const mockItems: PathItemDto[] = [folder];

describe('OutlineTree.vue', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('renders root folder and children items with appropriate badges', () => {
    const wrapper = mount(OutlineTree, {
      props: {
        items: mockItems,
        selectedItemId: null,
      },
    });

    expect(wrapper.text()).toContain('Chương 1: Mảng & Danh sách liên kết');
    expect(wrapper.text()).toContain('Bài 1.1: Giới thiệu Mảng động');
    expect(wrapper.text()).toContain('Quiz 1.1: Trắc nghiệm Mảng');
    expect(wrapper.text()).toContain('Lab 1.1: Đảo ngược mảng');
    expect(wrapper.text()).toContain('Chương');
    expect(wrapper.text()).toContain('Lý thuyết');
    expect(wrapper.text()).toContain('Trắc nghiệm');
    expect(wrapper.text()).toContain('Codelab');
  });

  it('emits select when an item row is clicked', async () => {
    const wrapper = mount(OutlineTree, {
      props: {
        items: mockItems,
        selectedItemId: null,
      },
    });

    const firstRow = wrapper.findAll('.tree-node-wrapper')[0];
    await firstRow.find('[role="treeitem"]').trigger('click');
    expect(wrapper.emitted('select')).toBeTruthy();
    expect(wrapper.emitted('select')![0][0]).toEqual(mockItems[0]);
  });

  it('emits add with parentId null when toolbar add popover options are clicked', async () => {
    const wrapper = mount(OutlineTree, {
      props: { items: mockItems },
    });

    // Mở popover "Thêm mục"
    await wrapper.find('[data-testid="add-item-trigger"]').trigger('click');

    await wrapper.find('[data-testid="add-item-folder"]').trigger('click');
    expect(wrapper.emitted('add')).toBeTruthy();
    expect(wrapper.emitted('add')![0]).toEqual(['folder', null]);

    await wrapper.find('[data-testid="add-item-trigger"]').trigger('click');
    await wrapper.find('[data-testid="add-item-theory"]').trigger('click');
    expect(wrapper.emitted('add')![1]).toEqual(['theory', null]);

    await wrapper.find('[data-testid="add-item-trigger"]').trigger('click');
    await wrapper.find('[data-testid="add-item-quiz"]').trigger('click');
    expect(wrapper.emitted('add')![2]).toEqual(['quiz', null]);

    await wrapper.find('[data-testid="add-item-trigger"]').trigger('click');
    await wrapper.find('[data-testid="add-item-lab"]').trigger('click');
    expect(wrapper.emitted('add')![3]).toEqual(['lab', null]);
  });

  it('emits add with folder parentId when using the node "+" popover', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });

    // Nút "+" trên folder có aria-label "Thêm mục vào chương này"
    const plusBtn = wrapper.findAll('[aria-label="Thêm mục vào chương này"]')[0];
    expect(plusBtn).toBeDefined();
    await plusBtn.trigger('click');
    await wrapper.find('[data-testid="node-add-quiz-1"]').trigger('click');
    expect(wrapper.emitted('add')).toBeTruthy();
    expect(wrapper.emitted('add')![0]).toEqual(['quiz', 1]);
  });

  it('rename flow: inline edit emits rename with trimmed title', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });

    await wrapper.find('[data-testid="node-menu-2"]').trigger('click');
    await wrapper.find('[data-testid="node-menu-rename"]').trigger('click');

    const input = wrapper.find('[data-testid="rename-input-2"]');
    expect(input.exists()).toBe(true);
    expect((input.element as HTMLInputElement).value).toBe('Bài 1.1: Giới thiệu Mảng động');

    await input.setValue('  Bài 1.1 (bản mới)  ');
    await input.trigger('keydown.enter');

    expect(wrapper.emitted('rename')).toBeTruthy();
    const [emittedItem, emittedTitle] = wrapper.emitted('rename')![0];
    expect((emittedItem as PathItemDto).id).toBe(2);
    expect(emittedTitle).toBe('Bài 1.1 (bản mới)');
  });

  it('rename flow: Escape cancels without emitting', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });

    await wrapper.find('[data-testid="node-menu-2"]').trigger('click');
    await wrapper.find('[data-testid="node-menu-rename"]').trigger('click');
    const input = wrapper.find('[data-testid="rename-input-2"]');
    await input.setValue('Không lưu');
    await input.trigger('keydown.esc');

    expect(wrapper.emitted('rename')).toBeFalsy();
    expect(wrapper.text()).toContain('Bài 1.1: Giới thiệu Mảng động');
  });

  it('move flow: node menu "Di chuyển đến…" emits moveItem with folder payload', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });

    await wrapper.find('[data-testid="node-menu-2"]').trigger('click');
    await wrapper.find('[data-testid="node-menu-move"]').trigger('click');

    // Đích "Cấp gốc" + đích folder "Chương 1"
    expect(wrapper.find('[data-testid="move-target-root"]').exists()).toBe(true);
    await wrapper.find('[data-testid="move-target-1"]').trigger('click');

    expect(wrapper.emitted('moveItem')).toBeTruthy();
    const [item, target] = wrapper.emitted('moveItem')![0];
    expect((item as PathItemDto).id).toBe(2);
    expect(target).toEqual({ parentId: 1, sortOrder: 3 });
  });

  it('move flow: offers root as destination for top-level items', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });

    await wrapper.find('[data-testid="node-menu-1"]').trigger('click');
    await wrapper.find('[data-testid="node-menu-move"]').trigger('click');

    expect(wrapper.find('[data-testid="move-target-root"]').exists()).toBe(true);
    // Không đề xuất chính nó làm đích
    expect(wrapper.find('[data-testid="move-target-1"]').exists()).toBe(false);
  });

  it('drag-drop reorder: dropping a child onto the folder emits moveItem API payload', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });

    const dataTransfer = (): DataTransfer =>
      ({
        setData: vi.fn(),
        effectAllowed: null,
        dropEffect: null,
      }) as unknown as DataTransfer;

    // Kéo Quiz (id 3) thả vào folder "Chương 1" (vùng giữa → 'into')
    await wrapper.find('[data-testid="drag-handle-3"]').trigger('dragstart', { dataTransfer: dataTransfer() });
    await wrapper.find('[data-testid="outline-node-1"]').trigger('dragover', { dataTransfer: dataTransfer(), clientY: 0 });
    await wrapper.find('[data-testid="outline-node-1"]').trigger('drop', { dataTransfer: dataTransfer() });

    expect(wrapper.emitted('moveItem')).toBeTruthy();
    const [item, target] = wrapper.emitted('moveItem')![0];
    expect((item as PathItemDto).id).toBe(3);
    expect(target).toEqual({ parentId: 1, sortOrder: 3 });

    // Kết thúc kéo: state được dọn sạch → lần drop kế tiếp không phát sự kiện ma
    await wrapper.find('[data-testid="outline-node-2"]').trigger('dragover', { dataTransfer: dataTransfer(), clientY: 0 });
    await wrapper.find('[data-testid="outline-node-2"]').trigger('drop', { dataTransfer: dataTransfer() });
    expect(wrapper.emitted('moveItem')!.length).toBe(1);
  });

  it('drag-drop: prevents dropping an item into its own subtree', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });
    const dataTransfer = (): DataTransfer =>
      ({ setData: vi.fn(), effectAllowed: null, dropEffect: null }) as unknown as DataTransfer;

    // Kéo folder 1 rồi thả vào con của nó (node 2) → bị chặn
    await wrapper.find('[data-testid="drag-handle-1"]').trigger('dragstart', { dataTransfer: dataTransfer() });
    await wrapper.find('[data-testid="outline-node-2"]').trigger('dragover', { dataTransfer: dataTransfer(), clientY: 0 });
    await wrapper.find('[data-testid="outline-node-2"]').trigger('drop', { dataTransfer: dataTransfer() });

    expect(wrapper.emitted('moveItem')).toBeFalsy();
  });

  it('delete flow: node menu emits delete', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });

    await wrapper.find('[data-testid="node-menu-3"]').trigger('click');
    await wrapper.find('[data-testid="node-menu-delete"]').trigger('click');

    expect(wrapper.emitted('delete')).toBeTruthy();
    expect((wrapper.emitted('delete')![0][0] as PathItemDto).id).toBe(3);
  });

  it('open-state persistence: toggling a folder writes localStorage and survives remount', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });
    await nextTick();

    // Mặc định: mở
    expect(wrapper.text()).toContain('Bài 1.1: Giới thiệu Mảng động');

    await wrapper.find('[data-testid="folder-toggle-1"]').trigger('click');
    await nextTick();

    // Đã đóng: ẩn con
    expect(wrapper.text()).not.toContain('Bài 1.1: Giới thiệu Mảng động');
    expect(JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '{}')).toEqual({ 1: false });

    wrapper.unmount();

    // Mount lại: trạng thái đóng được khôi phục từ localStorage
    const wrapper2 = mount(OutlineTree, { props: { items: mockItems } });
    await nextTick();
    expect(wrapper2.text()).not.toContain('Bài 1.1: Giới thiệu Mảng động');
    expect(wrapper2.find('[data-testid="folder-toggle-1"]').attributes('aria-expanded')).toBe('false');

    // Mở lại → lưu trạng thái mở
    await wrapper2.find('[data-testid="folder-toggle-1"]').trigger('click');
    await nextTick();
    expect(JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '{}')).toEqual({ 1: true });
    expect(wrapper2.text()).toContain('Bài 1.1: Giới thiệu Mảng động');

    wrapper2.unmount();
  });

  it('readonly mode hides drag handles and action buttons', () => {
    const wrapper = mount(OutlineTree, {
      props: { items: mockItems, readonly: true },
    });

    expect(wrapper.find('[data-testid="drag-handle-1"]').exists()).toBe(false);
    expect(wrapper.find('[data-testid="node-menu-1"]').exists()).toBe(false);
    expect(wrapper.find('[data-testid="add-item-trigger"]').exists()).toBe(false);
  });

  it('closes add-item menu when clicking outside', async () => {
    const wrapper = mount(OutlineTree, { props: { items: mockItems } });

    await wrapper.find('[data-testid="add-item-trigger"]').trigger('click');
    expect(wrapper.find('[data-testid="add-item-folder"]').exists()).toBe(true);

    // Simulate clicking outside on document
    document.dispatchEvent(new MouseEvent('click', { bubbles: true }));
    await nextTick();

    expect(wrapper.find('[data-testid="add-item-folder"]').exists()).toBe(false);
    wrapper.unmount();
  });
});
