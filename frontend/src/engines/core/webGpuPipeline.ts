// engines/core/webGpuPipeline.ts — WebGPU pipeline (đồ thị lực Coulomb/Hooke — task riêng)
//
// ⚠️ OPTIONAL — NGOÀI LUỒNG EDV CHÍNH, CẦN RÀ WebGPU TRƯỚC KHI DÙNG (task renderer graph).
// NGUỒN BÊ: source/VisualizationDSA3/frontend/src/core/WebGpuPipeline.ts (V3 — bê NGUYÊN VẸN,
// chỉ thay `as any` bằng unknown-cast + thêm header này).

export const GRAPH_FORCE_COMPUTE_WGSL = `
struct Node {
  x:  f32,
  y:  f32,
  vx: f32,
  vy: f32,
}

struct Params {
  node_count:      u32,
  repulsion_force: f32,
  damping:         f32,
  delta_time:      f32,
}

@group(0) @binding(0) var<storage, read_write> nodes: array<Node>;
@group(0) @binding(1) var<uniform>              params: Params;

@compute @workgroup_size(64)
fn main(@builtin(global_invocation_id) gid: vec3u) {
  let idx = gid.x;
  if (idx >= params.node_count) { return; }

  var force = vec2f(0.0, 0.0);
  let pos   = vec2f(nodes[idx].x, nodes[idx].y);

  // Coulomb repulsion — O(N) per node, fully parallel across workgroups
  for (var j = 0u; j < params.node_count; j = j + 1u) {
    if (j == idx) { continue; }
    let other = vec2f(nodes[j].x, nodes[j].y);
    let diff  = pos - other;
    let dist  = max(length(diff), 0.01);
    force += normalize(diff) * (params.repulsion_force / (dist * dist));
  }

  // Integrate with damping
  nodes[idx].vx = (nodes[idx].vx + force.x * params.delta_time) * params.damping;
  nodes[idx].vy = (nodes[idx].vy + force.y * params.delta_time) * params.damping;
  nodes[idx].x  = nodes[idx].x + nodes[idx].vx * params.delta_time;
  nodes[idx].y  = nodes[idx].y + nodes[idx].vy * params.delta_time;
}
`;

export interface WebGpuCapabilities {
  supported: boolean;
  adapterName: string;
  device: GPUDevice | null;
  adapter: GPUAdapter | null;
  error: string | null;
}

/** Kiểm tra trình duyệt có WebGPU + lấy device (bất khả dụng → trả về thông tin lỗi, không ném). */
export async function probeWebGpu(): Promise<WebGpuCapabilities> {
  const unsupported: WebGpuCapabilities = {
    supported: false,
    adapterName: '',
    device: null,
    adapter: null,
    error: null,
  };

  if (typeof navigator === 'undefined' || !navigator.gpu) {
    return { ...unsupported, error: 'WebGPU không được trình duyệt hỗ trợ.' };
  }

  try {
    const adapter = await navigator.gpu.requestAdapter();
    if (!adapter) {
      return { ...unsupported, error: 'Không tìm thấy WebGPU adapter.' };
    }

    const device = await adapter.requestDevice();

    let adapterName = 'GPU';
    const info = adapter.info as { device?: string; vendor?: string } | undefined;
    if (info) {
      adapterName = info.device || info.vendor || 'GPU';
    } else {
      // Fallback cho adapter cũ chưa có `info` (chuẩn cũ requestAdapterInfo).
      const legacy = adapter as unknown as {
        requestAdapterInfo?: () => Promise<{ device?: string; vendor?: string }>;
      };
      if (typeof legacy.requestAdapterInfo === 'function') {
        try {
          const adapterInfo = await legacy.requestAdapterInfo();
          adapterName = adapterInfo.device || adapterInfo.vendor || 'GPU';
        } catch {
          // Giữ tên mặc định 'GPU'.
        }
      }
    }

    return {
      supported: true,
      adapterName,
      device,
      adapter,
      error: null,
    };
  } catch (err) {
    const msg = err instanceof Error ? err.message : 'Lỗi khởi tạo WebGPU.';
    return { ...unsupported, error: msg };
  }
}

export interface WebGpuCanvasContext {
  context: GPUCanvasContext;
  format: GPUTextureFormat;
}

/** Cấu hình canvas để vẽ bằng WebGPU (throw nếu trình duyệt không hỗ trợ context). */
export function initCanvasContext(
  canvas: HTMLCanvasElement,
  device: GPUDevice,
): WebGpuCanvasContext {
  const context = canvas.getContext('webgpu') as unknown as GPUCanvasContext | null;
  if (!context) {
    throw new Error('Không thể khởi tạo WebGPU canvas context.');
  }

  const format = navigator.gpu.getPreferredCanvasFormat();
  context.configure({ device, format, alphaMode: 'premultiplied' });

  return { context, format };
}

/** Tạo compute pipeline từ WGSL (layout auto — bindgroup khớp theo code shader). */
export function createComputePipeline(
  device: GPUDevice,
  wgslCode: string,
  entryPoint: string = 'main',
): GPUComputePipeline {
  const shaderModule = device.createShaderModule({ code: wgslCode });
  return device.createComputePipeline({
    layout: 'auto',
    compute: { module: shaderModule, entryPoint },
  });
}
