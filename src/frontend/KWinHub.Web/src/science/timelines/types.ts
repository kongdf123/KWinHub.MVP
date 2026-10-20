export interface TimelineSegment {
  id: string
  label: string
  startTime: number
  endTime: number
  color?: string
}

export interface TimelineData {
  segments: TimelineSegment[]
  durationSeconds: number
}