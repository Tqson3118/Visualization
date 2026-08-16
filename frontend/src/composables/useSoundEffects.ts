// src/composables/useSoundEffects.ts
// Micro-SFX audio feedback system (Game-style audio for algorithm visualizations)
// Dual-layer architecture:
// 1. Primary: Web Audio API procedural synthesizer (100% standalone, 0 external file dependency)
// 2. Secondary: Howler.js integration for master volume and fallback audio playback
// Persists user preferences (muted, volume) in localStorage.

import { getCurrentInstance, onMounted, ref } from 'vue';
import { Howler } from 'howler';

const STORAGE_KEY_MUTED = 'dsa_sfx_muted';
const STORAGE_KEY_VOLUME = 'dsa_sfx_volume';

// Global shared state so mute/volume is synchronized across all components
const isMuted = ref<boolean>(false);
const volume = ref<number>(0.6);
let audioCtx: AudioContext | null = null;
let audioCtxInitialized = false;

function loadStoredPreferences(): void {
  if (typeof window === 'undefined') return;
  try {
    const savedMuted = localStorage.getItem(STORAGE_KEY_MUTED);
    if (savedMuted !== null) {
      isMuted.value = savedMuted === 'true';
    }
    const savedVol = localStorage.getItem(STORAGE_KEY_VOLUME);
    if (savedVol !== null) {
      const v = parseFloat(savedVol);
      if (!isNaN(v) && v >= 0 && v <= 1) {
        volume.value = v;
      }
    }
    Howler.mute(isMuted.value);
    Howler.volume(volume.value);
  } catch {}
}

loadStoredPreferences();

/** Initialize AudioContext on first user interaction */
function getAudioContext(): AudioContext | null {
  if (typeof window === 'undefined') return null;
  if (!audioCtx) {
    const AudioContextClass = window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext;
    if (AudioContextClass) {
      try {
        audioCtx = new AudioContextClass();
      } catch (e) {
        console.warn('[useSoundEffects] Failed to create AudioContext:', e);
      }
    }
  }
  if (audioCtx && audioCtx.state === 'suspended' && !audioCtxInitialized) {
    const resume = () => {
      if (audioCtx && audioCtx.state === 'suspended') {
        audioCtx.resume().catch(() => {});
      }
      window.removeEventListener('click', resume);
      window.removeEventListener('keydown', resume);
      window.removeEventListener('touchstart', resume);
    };
    // B5 — bọc khối lắng nghe resume trong try/catch: chỉ set audioCtxInitialized = true
    // SAU khi cả 3 addEventListener đăng ký thành công (nếu fail, để false và thử lại lần sau).
    try {
      window.addEventListener('click', resume, { once: true });
      window.addEventListener('keydown', resume, { once: true });
      window.addEventListener('touchstart', resume, { once: true });
      audioCtxInitialized = true;
    } catch (e) {
      console.warn('[useSoundEffects] Failed to add resume listeners:', e);
    }
  }
  return audioCtx;
}

export function useSoundEffects() {
  if (getCurrentInstance()) {
    onMounted(() => {
      loadStoredPreferences();
    });
  }

  function toggleMute(): boolean {
    isMuted.value = !isMuted.value;
    try {
      localStorage.setItem(STORAGE_KEY_MUTED, String(isMuted.value));
    } catch {}
    Howler.mute(isMuted.value);
    return isMuted.value;
  }

  function setMuted(muted: boolean): void {
    isMuted.value = muted;
    try {
      localStorage.setItem(STORAGE_KEY_MUTED, String(muted));
    } catch {}
    Howler.mute(muted);
  }

  function setVolume(val: number): void {
    const clamped = Math.max(0, Math.min(1, val));
    volume.value = clamped;
    try {
      localStorage.setItem(STORAGE_KEY_VOLUME, String(clamped));
    } catch {}
    Howler.volume(clamped);
  }

  /**
   * Compare Click: High-tech crisp mechanical click with pitch mapping.
   * Higher values or index offsets create dynamic pitch scaling.
   */
  function playCompare(pitchOffset = 0): void {
    if (isMuted.value) return;
    const ctx = getAudioContext();
    if (!ctx) return;
    try {
      const now = ctx.currentTime;
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const filter = ctx.createBiquadFilter();

      // Base frequency 880Hz (A5) with micro offset
      const baseFreq = 880 + Math.min(600, Math.max(-300, pitchOffset * 40));
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(baseFreq, now);
      osc.frequency.exponentialRampToValueAtTime(baseFreq * 1.5, now + 0.015);
      osc.frequency.exponentialRampToValueAtTime(baseFreq * 0.5, now + 0.04);

      filter.type = 'highpass';
      filter.frequency.setValueAtTime(400, now);

      gain.gain.setValueAtTime(0.25 * volume.value, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.045);

      osc.connect(filter);
      filter.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.05);
    } catch {}
  }

  /**
   * Arc Swap Cluck: Parabolic frequency modulation simulating an arc movement.
   */
  function playSwap(): void {
    if (isMuted.value) return;
    const ctx = getAudioContext();
    if (!ctx) return;
    try {
      const now = ctx.currentTime;
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sine';
      // Swoop up and down in pitch (arc parabolic curve)
      osc.frequency.setValueAtTime(320, now);
      osc.frequency.linearRampToValueAtTime(640, now + 0.06);
      osc.frequency.exponentialRampToValueAtTime(240, now + 0.14);

      gain.gain.setValueAtTime(0.001, now);
      gain.gain.linearRampToValueAtTime(0.35 * volume.value, now + 0.04);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.15);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.16);
    } catch {}
  }

  /**
   * Laser Beam Pulse: Futuristic energetic beam for graph edge exploration.
   */
  function playLaser(): void {
    if (isMuted.value) return;
    const ctx = getAudioContext();
    if (!ctx) return;
    try {
      const now = ctx.currentTime;
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      const filter = ctx.createBiquadFilter();

      osc.type = 'sawtooth';
      osc.frequency.setValueAtTime(950, now);
      osc.frequency.exponentialRampToValueAtTime(180, now + 0.16);

      filter.type = 'lowpass';
      filter.frequency.setValueAtTime(2400, now);
      filter.frequency.exponentialRampToValueAtTime(400, now + 0.16);

      gain.gain.setValueAtTime(0.22 * volume.value, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.18);

      osc.connect(filter);
      filter.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.19);
    } catch {}
  }

  /**
   * Victory Chime: Triumphant arpeggio chord (C5 - E5 - G5 - B5 - C6) for algorithm completion.
   */
  function playVictory(): void {
    if (isMuted.value) return;
    const ctx = getAudioContext();
    if (!ctx) return;
    try {
      const now = ctx.currentTime;
      const notes = [523.25, 659.25, 783.99, 987.77, 1046.5]; // C5, E5, G5, B5, C6
      const noteDuration = 0.08;

      notes.forEach((freq, idx) => {
        const noteStart = now + idx * noteDuration;
        const osc = ctx.createOscillator();
        const gain = ctx.createGain();

        osc.type = 'sine';
        osc.frequency.setValueAtTime(freq, noteStart);

        gain.gain.setValueAtTime(0.001, noteStart);
        gain.gain.linearRampToValueAtTime(0.28 * volume.value, noteStart + 0.02);
        gain.gain.exponentialRampToValueAtTime(0.001, noteStart + 0.35);

        osc.connect(gain);
        gain.connect(ctx.destination);

        osc.start(noteStart);
        osc.stop(noteStart + 0.38);
      });
    } catch {}
  }

  /**
   * Push Bounce: Soft spring pop when element is inserted / pushed into stack/queue.
   */
  function playPush(): void {
    if (isMuted.value) return;
    const ctx = getAudioContext();
    if (!ctx) return;
    try {
      const now = ctx.currentTime;
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(260, now);
      osc.frequency.exponentialRampToValueAtTime(520, now + 0.03);
      osc.frequency.exponentialRampToValueAtTime(380, now + 0.08);

      gain.gain.setValueAtTime(0.3 * volume.value, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.09);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.1);
    } catch {}
  }

  /**
   * Pop Dissolve: Airy release / dissolve sound on pop / delete.
   */
  function playPop(): void {
    if (isMuted.value) return;
    const ctx = getAudioContext();
    if (!ctx) return;
    try {
      const now = ctx.currentTime;
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'triangle';
      osc.frequency.setValueAtTime(600, now);
      osc.frequency.exponentialRampToValueAtTime(120, now + 0.1);

      gain.gain.setValueAtTime(0.25 * volume.value, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.11);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.12);
    } catch {}
  }

  /**
   * Step Tick: Light subtle feedback on generic step change.
   */
  function playStep(): void {
    if (isMuted.value) return;
    const ctx = getAudioContext();
    if (!ctx) return;
    try {
      const now = ctx.currentTime;
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();

      osc.type = 'sine';
      osc.frequency.setValueAtTime(540, now);

      gain.gain.setValueAtTime(0.12 * volume.value, now);
      gain.gain.exponentialRampToValueAtTime(0.001, now + 0.025);

      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.start(now);
      osc.stop(now + 0.03);
    } catch {}
  }

  return {
    isMuted,
    volume,
    toggleMute,
    setMuted,
    setVolume,
    playCompare,
    playSwap,
    playLaser,
    playVictory,
    playPush,
    playPop,
    playStep,
  };
}
