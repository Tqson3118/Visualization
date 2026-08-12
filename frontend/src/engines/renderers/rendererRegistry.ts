// engines/renderers/rendererRegistry.ts — Đăng ký renderer theo kind (SDD §4.4)
//
// CanvasArea (components/simulator/CanvasArea.vue) gọi getRendererForKind(kind)
// để chọn renderer thật; kind chưa có renderer → component giữ fallback vẽ inline cũ.

import { ArrayRenderer } from './arrayRenderer';
import { GraphRenderer } from './graphRenderer';
import { HashTableRenderer } from './hashTableRenderer';
import type { Renderer } from './interface';
import { ListRenderer } from './listRenderer';
import { StackQueueRenderer } from './stackQueueRenderer';
import { TreeRenderer } from './treeRenderer';

const registry = new Map<string, Renderer>();

/** Đăng ký 1 renderer cho tất cả kind mà nó hỗ trợ. */
export function registerRenderer(renderer: Renderer): void {
  for (const kind of renderer.supportedKinds) {
    registry.set(kind, renderer);
  }
}

/** Lấy renderer cho kind; trả về null nếu chưa đăng ký (component dùng fallback cũ). */
export function getRendererForKind(kind: string): Renderer | null {
  return registry.get(kind) ?? null;
}

/** Danh sách mọi renderer đã đăng ký (gọi 1 lần lúc khởi tạo module). */
export const ALL_RENDERERS: Renderer[] = (() => {
  const renderers = [
    new ArrayRenderer(),
    new StackQueueRenderer(),
    new ListRenderer(),
    new TreeRenderer(),
    new HashTableRenderer(),
    new GraphRenderer(),
  ];
  for (const renderer of renderers) registerRenderer(renderer);
  return renderers;
})();
