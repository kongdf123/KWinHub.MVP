import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import type { WorkflowDefinition, WorkflowStep } from './types';

interface NodeLayout {
  id: string;
  x: number;
  y: number;
  width: number;
  height: number;
}

interface DiagramProps {
  workflow: WorkflowDefinition;
  selectedStepId: string | null;
  activePath: string[];
  onSelect: (stepId: string) => void;
  locale: 'en' | 'zh-CN';
}

const NODE_WIDTH = 180;
const NODE_HEIGHT = 56;
const H_GAP = 60;
const V_GAP = 40;

function computeLayout(workflow: WorkflowDefinition): { nodes: NodeLayout[]; width: number; height: number } {
  const steps = workflow.steps;
  const incoming = new Map<string, string[]>();
  const outgoing = new Map<string, string[]>();
  for (const step of steps) {
    incoming.set(step.id, []);
    outgoing.set(step.id, []);
  }
  for (const edge of workflow.edges) {
    incoming.get(edge.to)?.push(edge.from);
    outgoing.get(edge.from)?.push(edge.to);
  }

  const layer = new Map<string, number>();
  const visited = new Set<string>();

  function assignLayer(id: string, currentLayer: number) {
    if (visited.has(id)) return;
    visited.add(id);
    const existing = layer.get(id) ?? 0;
    layer.set(id, Math.max(existing, currentLayer));
    for (const next of outgoing.get(id) ?? []) {
      assignLayer(next, currentLayer + 1);
    }
  }

  const roots = steps.filter((s) => (incoming.get(s.id) ?? []).length === 0);
  for (const root of roots) {
    assignLayer(root.id, 0);
  }
  for (const step of steps) {
    if (!visited.has(step.id)) {
      assignLayer(step.id, 0);
    }
  }

  const maxLayer = Math.max(0, ...Array.from(layer.values()));
  const columns: string[][] = Array.from({ length: maxLayer + 1 }, () => []);
  for (const step of steps) {
    columns[layer.get(step.id) ?? 0].push(step.id);
  }

  const nodes: NodeLayout[] = [];
  for (let col = 0; col < columns.length; col++) {
    const colSteps = columns[col];
    const colHeight = colSteps.length * NODE_HEIGHT + (colSteps.length - 1) * V_GAP;
    const startY = -colHeight / 2;
    for (let row = 0; row < colSteps.length; row++) {
      const stepId = colSteps[row];
      nodes.push({
        id: stepId,
        x: col * (NODE_WIDTH + H_GAP),
        y: startY + row * (NODE_HEIGHT + V_GAP),
        width: NODE_WIDTH,
        height: NODE_HEIGHT,
      });
    }
  }

  const xValues = nodes.map((n) => n.x);
  const yValues = nodes.map((n) => n.y);
  const minX = Math.min(0, ...xValues) - 40;
  const maxX = Math.max(NODE_WIDTH, ...xValues.map((x) => x + NODE_WIDTH)) + 40;
  const minY = Math.min(0, ...yValues) - 40;
  const maxY = Math.max(NODE_HEIGHT, ...yValues.map((y) => y + NODE_HEIGHT)) + 40;

  for (const node of nodes) {
    node.x -= minX;
    node.y -= minY;
  }

  return { nodes, width: maxX - minX, height: maxY - minY };
}

function edgePath(from: NodeLayout, to: NodeLayout): string {
  const x1 = from.x + from.width;
  const y1 = from.y + from.height / 2;
  const x2 = to.x;
  const y2 = to.y + to.height / 2;
  const dx = Math.max(40, (x2 - x1) / 2);
  return `M ${x1} ${y1} C ${x1 + dx} ${y1}, ${x2 - dx} ${y2}, ${x2} ${y2}`;
}

export function WorkflowDiagram({ workflow, selectedStepId, activePath, onSelect, locale }: DiagramProps) {
  const [scale, setScale] = useState(1);
  const [pan, setPan] = useState({ x: 0, y: 0 });
  const [dragging, setDragging] = useState(false);
  const dragStart = useRef({ x: 0, y: 0, panX: 0, panY: 0 });

  const { nodes, width, height } = useMemo(() => computeLayout(workflow), [workflow]);
  const nodeMap = useMemo(() => new Map(nodes.map((n) => [n.id, n])), [nodes]);

  const stepText = (step: WorkflowStep) => (locale === 'zh-CN' ? step.title.zhCN : step.title.en);

  const handleWheel = useCallback((e: React.WheelEvent) => {
    e.preventDefault();
    const delta = e.deltaY > 0 ? 0.9 : 1.1;
    setScale((s) => Math.min(2.5, Math.max(0.3, s * delta)));
  }, []);

  const handleMouseDown = useCallback(
    (e: React.MouseEvent) => {
      if ((e.target as HTMLElement).closest('[data-node]')) return;
      setDragging(true);
      dragStart.current = { x: e.clientX, y: e.clientY, panX: pan.x, panY: pan.y };
    },
    [pan],
  );

  useEffect(() => {
    if (!dragging) return;
    const handleMove = (e: MouseEvent) => {
      setPan({
        x: dragStart.current.panX + (e.clientX - dragStart.current.x),
        y: dragStart.current.panY + (e.clientY - dragStart.current.y),
      });
    };
    const handleUp = () => setDragging(false);
    window.addEventListener('mousemove', handleMove);
    window.addEventListener('mouseup', handleUp);
    return () => {
      window.removeEventListener('mousemove', handleMove);
      window.removeEventListener('mouseup', handleUp);
    };
  }, [dragging]);

  const resetView = () => {
    setScale(1);
    setPan({ x: 0, y: 0 });
  };

  const isOnActivePath = (stepId: string) => activePath.includes(stepId);
  const isSelected = (stepId: string) => selectedStepId === stepId;

  return (
    <div className="relative h-[480px] w-full overflow-hidden rounded-lg border border-default bg-surface-panel">
      <div className="absolute right-3 top-3 z-10 flex gap-1">
        <button
          type="button"
          onClick={() => setScale((s) => Math.min(2.5, s * 1.2))}
          className="rounded border border-default bg-surface-panel px-2 py-1 text-sm text-content-secondary hover:bg-surface-hover"
          aria-label="Zoom in"
        >
          +
        </button>
        <button
          type="button"
          onClick={() => setScale((s) => Math.max(0.3, s * 0.8))}
          className="rounded border border-default bg-surface-panel px-2 py-1 text-sm text-content-secondary hover:bg-surface-hover"
          aria-label="Zoom out"
        >
          −
        </button>
        <button
          type="button"
          onClick={resetView}
          className="rounded border border-default bg-surface-panel px-2 py-1 text-xs text-content-secondary hover:bg-surface-hover"
        >
          {locale === 'zh-CN' ? '重置' : 'Reset'}
        </button>
      </div>

      <div
        className="h-full w-full cursor-grab active:cursor-grabbing"
        onWheel={handleWheel}
        onMouseDown={handleMouseDown}
      >
        <svg width="100%" height="100%" style={{ overflow: 'visible' }} role="img" aria-label={`${workflow.name[locale]} workflow diagram`}>
          <g transform={`translate(${pan.x + 40} ${pan.y + 40}) scale(${scale})`}>
            <defs>
              <marker id="arrow-default" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto" markerUnits="strokeWidth">
                <path d="M0,0 L0,6 L9,3 z" fill="#94a3b8" />
              </marker>
              <marker id="arrow-active" markerWidth="10" markerHeight="10" refX="9" refY="3" orient="auto" markerUnits="strokeWidth">
                <path d="M0,0 L0,6 L9,3 z" fill="#0d9488" />
              </marker>
            </defs>

            {workflow.edges.map((edge) => {
              const from = nodeMap.get(edge.from);
              const to = nodeMap.get(edge.to);
              if (!from || !to) return null;
              const active = isOnActivePath(edge.from) && isOnActivePath(edge.to);
              return (
                <g key={edge.id}>
                  <path
                    d={edgePath(from, to)}
                    fill="none"
                    stroke={active ? '#0d9488' : '#94a3b8'}
                    strokeWidth={active ? 2.5 : 1.5}
                    markerEnd={active ? 'url(#arrow-active)' : 'url(#arrow-default)'}
                  />
                  {edge.label ? (
                    <text
                      x={(from.x + from.width + to.x) / 2}
                      y={(from.y + from.height / 2 + to.y + to.height / 2) / 2 - 6}
                      textAnchor="middle"
                      className="fill-ink-500 text-[10px]"
                    >
                      {edge.label[locale]}
                    </text>
                  ) : null}
                </g>
              );
            })}

            {nodes.map((node) => {
              const step = workflow.steps.find((s) => s.id === node.id);
              if (!step) return null;
              const selected = isSelected(step.id);
              return (
                <g
                  key={node.id}
                  data-node
                  transform={`translate(${node.x} ${node.y})`}
                  onClick={() => onSelect(step.id)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                      e.preventDefault();
                      onSelect(step.id);
                    }
                  }}
                  tabIndex={0}
                  role="button"
                  aria-label={stepText(step)}
                  className="cursor-pointer focus:outline-none focus-visible:ring-2 focus-visible:ring-accent"
                >
                  <rect
                    width={node.width}
                    height={node.height}
                    rx={8}
                    className={`stroke-2 ${selected ? 'fill-accent stroke-accent' : isOnActivePath(step.id) ? 'fill-teal-50 stroke-teal-400' : 'fill-surface-panel stroke-ink-300'}`}
                  />
                  <foreignObject x={8} y={8} width={node.width - 16} height={node.height - 16}>
                    <div
                      className={`flex h-full items-center justify-center text-center text-xs font-medium leading-tight ${selected ? 'text-white' : isOnActivePath(step.id) ? 'text-teal-800' : 'text-ink-700'}`}
                      style={{ display: 'flex', alignItems: 'center', justifyContent: 'center' }}
                    >
                      {stepText(step)}
                    </div>
                  </foreignObject>
                </g>
              );
            })}
          </g>
        </svg>
      </div>
    </div>
  );
}