// src/composables/__tests__/useSoundEffects.spec.ts
import { describe, expect, it, beforeEach, afterEach, vi } from 'vitest';
import { useSoundEffects } from '../useSoundEffects';

describe('useSoundEffects composable', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it('manages mute toggle and persists to localStorage', () => {
    const { isMuted, toggleMute, setMuted } = useSoundEffects();

    expect(isMuted.value).toBe(false);
    const newMuted = toggleMute();
    expect(newMuted).toBe(true);
    expect(isMuted.value).toBe(true);
    expect(localStorage.getItem('dsa_sfx_muted')).toBe('true');

    setMuted(false);
    expect(isMuted.value).toBe(false);
    expect(localStorage.getItem('dsa_sfx_muted')).toBe('false');
  });

  it('clamps volume within [0, 1] range', () => {
    const { volume, setVolume } = useSoundEffects();

    setVolume(0.8);
    expect(volume.value).toBe(0.8);
    expect(localStorage.getItem('dsa_sfx_volume')).toBe('0.8');

    setVolume(1.5);
    expect(volume.value).toBe(1.0);

    setVolume(-0.5);
    expect(volume.value).toBe(0.0);
  });

  // Test 3 — thay test pass-fake cũ bằng mock AudioContext THẬT (B5):
  // - playCompare(0) phải tạo oscillator và gọi start()/stop().
  // - Khi setMuted(true), playCompare bi/mute sớm → KHÔNG tạo oscillator mới (đếm qua spy).
  it('playCompare calls oscillator start/stop; muted skips new oscillator', async () => {
    // audioCtx là singleton module-level → resetModules + dynamic import để có instance sạch
    vi.resetModules();
    type FakeOsc = {
      type: string;
      connect: ReturnType<typeof vi.fn>;
      start: ReturnType<typeof vi.fn>;
      stop: ReturnType<typeof vi.fn>;
      frequency: {
        setValueAtTime: ReturnType<typeof vi.fn>;
        exponentialRampToValueAtTime: ReturnType<typeof vi.fn>;
        linearRampToValueAtTime: ReturnType<typeof vi.fn>;
      };
    };
    const oscInstances: FakeOsc[] = [];

    const FakeAudioContext = class {
      state = 'running';
      currentTime = 0;
      destination = {};
      createOscillator(): FakeOsc {
        const osc: FakeOsc = {
          type: '',
          connect: vi.fn(),
          start: vi.fn(),
          stop: vi.fn(),
          frequency: {
            setValueAtTime: vi.fn(),
            exponentialRampToValueAtTime: vi.fn(),
            linearRampToValueAtTime: vi.fn(),
          },
        };
        oscInstances.push(osc);
        return osc;
      }
      createGain() {
        return {
          gain: { setValueAtTime: vi.fn(), exponentialRampToValueAtTime: vi.fn(), linearRampToValueAtTime: vi.fn() },
          connect: vi.fn(),
        };
      }
      createBiquadFilter() {
        return { type: '', frequency: { setValueAtTime: vi.fn() }, connect: vi.fn() };
      }
      resume() {}
    };
    vi.stubGlobal('AudioContext', FakeAudioContext);

    // resetModules → dynamic import trả module mới; useSoundEffects() là hàm trả các action
    const sfx = await import('../useSoundEffects');
    const { playCompare, setMuted } = sfx.useSoundEffects();

    // Unmuted → oscillator được tạo và start/stop được gọi
    playCompare(0);
    expect(oscInstances.length).toBeGreaterThan(0);
    const first = oscInstances[0];
    expect(first.start).toHaveBeenCalled();
    expect(first.stop).toHaveBeenCalled();

    // Muted → không tạo oscillator mới
    const before = oscInstances.length;
    setMuted(true);
    playCompare(0);
    expect(oscInstances.length).toBe(before);
  });
});