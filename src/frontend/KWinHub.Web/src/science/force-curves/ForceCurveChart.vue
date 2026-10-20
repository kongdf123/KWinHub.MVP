<script setup lang="ts">
import { computed } from 'vue'
import VChart from 'vue-echarts'
import './echarts-setup'
import type { ForceCurveData } from './types'

const props = defineProps<{
  data: ForceCurveData
}>()

const option = computed(() => ({
  tooltip: {
    trigger: 'axis'
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '3%',
    containLabel: true
  },
  xAxis: {
    type: 'value',
    name: 'Time (s)',
    min: 0,
    max: props.data.durationSeconds
  },
  yAxis: {
    type: 'value',
    name: 'Force (N)'
  },
  series: props.data.series.map((s) => ({
    name: s.name,
    type: 'line',
    showSymbol: false,
    data: s.data.map((p) => [p.t, p.v]),
    lineStyle: { width: 1.5 }
  }))
}))
</script>

<template>
  <div class="force-curve">
    <VChart :option="option" :autoresize="true" style="height: 400px" />
  </div>
</template>