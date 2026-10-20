export interface SignalPoint {
  t: number
  v: number
}

export interface SignalSeries {
  id: string
  name: string
  color?: string
  data: SignalPoint[]
}

export interface TimelineMarker {
  id: string
  label: string
  time: number
  color?: string
}

export interface ForceCurveData {
  series: SignalSeries[]
  markers: TimelineMarker[]
  durationSeconds: number
  sampleRateHz: number
}