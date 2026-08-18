// features/visual-shell/index.ts — shared visual shell + adapter contract (B1/B2)
export { default as SharedVisualizerShell } from './components/SharedVisualizerShell.vue';
export type { SharedVisualFrame, SharedVisualStatus } from './types/sharedVisualFrame';
export {
  legacyStepsToSortFrames,
  legacyStepsToSharedFrames,
  legacyStepsToSharedArrayFrames,
  parseElementValue,
  deriveVisualStatus,
  type LegacyStepAdapterOptions,
} from './adapters/legacyStepAdapter';
