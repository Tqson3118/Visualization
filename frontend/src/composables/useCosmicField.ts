/**
 * Vũ trụ tương tác (cosmic field): các hạt điểm trôi nhẹ, những điểm gần nhau
 * nối bằng đường kẻ mờ (chòm sao); RÊ CHUỘT → các hạt quanh con trỏ nối thành
 * chòm theo chuột kèm lực hút nhẹ. Canvas fixed, pointer-events: none.
 * Tôn trọng prefers-reduced-motion: chỉ vẽ 1 khung tĩnh (không chuyển động).
 */
export function startCosmicField(canvas: HTMLCanvasElement): () => void {
  const ctx = canvas.getContext('2d')!;

  const reduced = typeof window !== 'undefined'
    && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  interface Particle {
    x: number;
    y: number;
    vx: number;
    vy: number;
    r: number;
    white: boolean;
  }

  let particles: Particle[] = [];
  const mouse = { x: -9999, y: -9999, active: false };

  // Sao băng: xuất hiện ngẫu nhiên mỗi ~5s, 1–2 cơn một lần, lao chéo xuống có đuôi mờ dần
  interface Meteor {
    x: number;
    y: number;
    vx: number;
    vy: number;
    len: number;
  }

  let meteors: Meteor[] = [];
  let nextMeteorAt = performance.now() + 4000;

  const LINK_DIST = 120;     // khoảng cách tối đa nối 2 hạt
  const CURSOR_DIST = 170;   // bán kính nối hạt ↔ chuột
  const MIN_COUNT = 22;      // ít chấm hơn — tránh quá rối
  const MAX_COUNT = 48;
  const METEOR_INTERVAL_MIN = 4000; // ms
  const METEOR_INTERVAL_MAX = 6500; // ms

  function spawnMeteors(now: number): void {
    if (reduced) {
      nextMeteorAt = now + METEOR_INTERVAL_MAX;
      return;
    }
    const count = Math.random() < 0.5 ? 1 : 2; // 1–2 cơn
    for (let i = 0; i < count; i++) {
      const vx = (Math.random() - 0.5) * 8;          // hơi chéo
      const vy = 7 + Math.random() * 6;              // lao xuống
      meteors.push({
        x: Math.random() * (window.innerWidth * 0.85) + window.innerWidth * 0.05,
        y: Math.random() * window.innerHeight * 0.4,
        vx,
        vy,
        len: 90 + Math.random() * 70,                // chiều dài đuôi
      });
    }
    nextMeteorAt = now + METEOR_INTERVAL_MIN + Math.random() * (METEOR_INTERVAL_MAX - METEOR_INTERVAL_MIN);
  }

  function targetCount(): number {
    return Math.max(MIN_COUNT, Math.min(MAX_COUNT, Math.floor((window.innerWidth * window.innerHeight) / 16000)));
  }

  function resize(): void {
    const dpr = Math.min(window.devicePixelRatio || 1, 2);
    canvas.width = Math.floor(window.innerWidth * dpr);
    canvas.height = Math.floor(window.innerHeight * dpr);
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
    const want = targetCount();
    while (particles.length < want) {
      particles.push({
        x: Math.random() * window.innerWidth,
        y: Math.random() * window.innerHeight,
        vx: (Math.random() - 0.5) * 0.5,
        vy: (Math.random() - 0.5) * 0.5,
        r: Math.random() * 1.6 + 0.8,
        white: Math.random() < 0.25,
      });
    }
    if (particles.length > want) particles = particles.slice(0, want);
  }

  function onMove(e: MouseEvent): void {
    mouse.x = e.clientX;
    mouse.y = e.clientY;
    mouse.active = true;
    if (reduced) drawFrame();
  }

  function onLeave(): void {
    mouse.active = false;
    mouse.x = -9999;
    mouse.y = -9999;
    if (reduced) drawFrame();
  }

  function drawFrame(): void {
    ctx.clearRect(0, 0, window.innerWidth, window.innerHeight);

    // Nối các cặp hạt gần nhau
    for (let i = 0; i < particles.length; i++) {
      const a = particles[i];
      for (let j = i + 1; j < particles.length; j++) {
        const b = particles[j];
        const dx = a.x - b.x;
        const dy = a.y - b.y;
        const d2 = dx * dx + dy * dy;
        if (d2 < LINK_DIST * LINK_DIST) {
          const alpha = (1 - Math.sqrt(d2) / LINK_DIST) * 0.22;
          ctx.strokeStyle = `rgba(168, 85, 247, ${alpha})`;
          ctx.lineWidth = 1;
          ctx.beginPath();
          ctx.moveTo(a.x, a.y);
          ctx.lineTo(b.x, b.y);
          ctx.stroke();
        }
      }
    }

    // Nối hạt ↔ chuột
    if (mouse.active) {
      for (const p of particles) {
        const dx = p.x - mouse.x;
        const dy = p.y - mouse.y;
        const d2 = dx * dx + dy * dy;
        if (d2 < CURSOR_DIST * CURSOR_DIST) {
          const alpha = (1 - Math.sqrt(d2) / CURSOR_DIST) * 0.5;
          ctx.strokeStyle = `rgba(192, 132, 252, ${alpha})`;
          ctx.lineWidth = 1;
          ctx.beginPath();
          ctx.moveTo(p.x, p.y);
          ctx.lineTo(mouse.x, mouse.y);
          ctx.stroke();
        }
      }
    }

    // Vẽ sao băng (đuôi gradient mờ dần + đầu sáng)
    for (const m of meteors) {
      const speed = Math.hypot(m.vx, m.vy) || 1;
      const tx = m.x - (m.vx / speed) * m.len;
      const ty = m.y - (m.vy / speed) * m.len;
      const grad = ctx.createLinearGradient(m.x, m.y, tx, ty);
      grad.addColorStop(0, 'rgba(255, 255, 255, 0.95)');
      grad.addColorStop(0.35, 'rgba(192, 132, 252, 0.55)');
      grad.addColorStop(1, 'rgba(192, 132, 252, 0)');
      ctx.strokeStyle = grad;
      ctx.lineWidth = 1.6;
      ctx.lineCap = 'round';
      ctx.beginPath();
      ctx.moveTo(tx, ty);
      ctx.lineTo(m.x, m.y);
      ctx.stroke();
      // đầu sao băng: lõi trắng + quầng tím nhẹ
      ctx.fillStyle = 'rgba(255, 255, 255, 0.28)';
      ctx.beginPath();
      ctx.arc(m.x, m.y, 3.2, 0, Math.PI * 2);
      ctx.fill();
      ctx.fillStyle = 'rgba(255, 255, 255, 0.95)';
      ctx.beginPath();
      ctx.arc(m.x, m.y, 1.4, 0, Math.PI * 2);
      ctx.fill();
    }

    // Vẽ hạt
    for (const p of particles) {
      ctx.fillStyle = p.white ? 'rgba(255, 255, 255, 0.75)' : 'rgba(192, 132, 252, 0.8)';
      ctx.beginPath();
      ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
      ctx.fill();
    }
  }

  function tick(): void {
    const now = performance.now();
    if (now >= nextMeteorAt) spawnMeteors(now);

    // Sao băng bay + loại bỏ khi ra khỏi màn hình
    for (let i = meteors.length - 1; i >= 0; i--) {
      const m = meteors[i];
      m.x += m.vx;
      m.y += m.vy;
      if (m.y > window.innerHeight + 60 || m.x < -120 || m.x > window.innerWidth + 120) {
        meteors.splice(i, 1);
      }
    }

    // Lực hút nhẹ về phía chuột
    if (mouse.active) {
      for (const p of particles) {
        const dx = mouse.x - p.x;
        const dy = mouse.y - p.y;
        const d2 = dx * dx + dy * dy;
        if (d2 < CURSOR_DIST * CURSOR_DIST && d2 > 0.01) {
          const f = 0.02 / Math.sqrt(d2);
          p.vx += dx * f * 0.4;
          p.vy += dy * f * 0.4;
        }
        // Giới hạn tốc độ
        const sp = Math.hypot(p.vx, p.vy);
        const max = 1.4;
        if (sp > max) {
          p.vx = (p.vx / sp) * max;
          p.vy = (p.vy / sp) * max;
        }
      }
    }

    for (const p of particles) {
      p.x += p.vx;
      p.y += p.vy;
      if (p.x < -20) p.x = window.innerWidth + 20;
      if (p.x > window.innerWidth + 20) p.x = -20;
      if (p.y < -20) p.y = window.innerHeight + 20;
      if (p.y > window.innerHeight + 20) p.y = -20;
    }

    drawFrame();
    raf = requestAnimationFrame(tick);
  }

  let raf = 0;

  window.addEventListener('resize', resize);
  window.addEventListener('mousemove', onMove, { passive: true });
  window.addEventListener('mouseleave', onLeave);

  resize();
  if (reduced) {
    drawFrame();
  } else {
    raf = requestAnimationFrame(tick);
  }

  return () => {
    cancelAnimationFrame(raf);
    window.removeEventListener('resize', resize);
    window.removeEventListener('mousemove', onMove);
    window.removeEventListener('mouseleave', onLeave);
  };
}
