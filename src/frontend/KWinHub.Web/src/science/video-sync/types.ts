export interface VideoSyncState {
  currentTime: number
  isPlaying: boolean
  playbackRate: number
}

export interface VideoSyncConfig {
  videoUrl: string
  startTimeOffset: number
}