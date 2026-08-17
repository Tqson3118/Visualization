// router/aliases.spec.ts — Route canonical + compatibility aliases (Workstream A/A2).
// Kiểm chứng alias/redirect bằng cách đọc record thật của router rồi resolve target.
import { describe, expect, it } from 'vitest';
import type { RouteLocationRaw } from 'vue-router';

import router from './index';

/** Lấy redirect của route khớp from, gọi (nếu hàm) rồi resolve ra location cuối. */
function redirectTarget(from: RouteLocationRaw): ReturnType<typeof router.resolve> {
  const r = router.resolve(from);
  const rec = r.matched[r.matched.length - 1];
  const red = (rec?.redirect ?? undefined) as
    | string
    | ((to: Record<string, unknown>) => RouteLocationRaw)
    | undefined;
  if (typeof red === 'string') {
    const target = router.resolve(red);
    // giữ query/params nếu có
    return target;
  }
  if (typeof red === 'function') {
    const target = red({ path: r.path, params: r.params } as unknown as Record<string, unknown>);
    return router.resolve(target);
  }
  return r;
}

describe('route canonical /path + compatibility aliases', () => {
  it('/courses → /path (alias danh sách lộ trình)', () => {
    const r = redirectTarget({ path: '/courses' });
    expect(r.path).toBe('/path');
    expect(r.name).toBe('path');
  });

  it('/courses/:id → /path/:id (topicId == pathId)', () => {
    const r = redirectTarget({ path: '/courses/3' });
    expect(r.path).toBe('/path/3');
    expect(r.name).toBe('path-topic');
    expect(r.params.topicId).toBe('3');
  });

  it('/lessons/:lessonId là canonical; /learn/:lessonId redirect về đó', () => {
    const canonical = router.resolve('/lessons/9');
    expect(canonical.name).toBe('lesson');
    expect(canonical.params.lessonId).toBe('9');
    const legacy = redirectTarget({ path: '/learn/9' });
    expect(legacy.path).toBe('/lessons/9');
    expect(legacy.name).toBe('lesson');
  });

  it('/learn vẫn redirect /path', () => {
    const r = redirectTarget({ path: '/learn' });
    expect(r.path).toBe('/path');
  });

  it('legacy deep link /path/:topicId/node/:nodeId + final-test vẫn giữ', () => {
    const node = router.resolve('/path/1/node/2');
    expect(node.name).toBe('node-hub');
    expect(node.params.topicId).toBe('1');
    expect(node.params.nodeId).toBe('2');
    const fin = router.resolve('/path/1/final-test');
    expect(fin.name).toBe('final-test');
  });
});
