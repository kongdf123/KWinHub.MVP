import { useMemo, useState } from 'react';
import { Upload, Users, Activity, Cpu, AlertTriangle, CheckCircle2 } from 'lucide-react';
import { DEMO_ATHLETES, DEMO_DEVICES, DEMO_PROTOCOLS } from '@/lib/demo-data';
import type { SimulatedFailureMode, UploadConfig } from './types';
import type { Lang } from '@/lib/types';

interface UploadConfigPanelProps {
  lang: Lang;
  disabled: boolean;
  onSubmit: (config: UploadConfig) => void;
}

const FAILURE_OPTIONS: { value: SimulatedFailureMode; label: string; labelCn: string; desc: string; descCn: string }[] = [
  { value: 'none', label: 'Success path', labelCn: '成功路径', desc: 'Full pipeline to Published', descCn: '完整管线至已发布' },
  { value: 'upload', label: 'Upload failure', labelCn: '上传失败', desc: 'Network timeout during transfer', descCn: '传输时网络超时' },
  { value: 'validation', label: 'Validation failure', labelCn: '校验失败', desc: 'Protocol validity rule rejected', descCn: '协议有效性规则驳回' },
  { value: 'processing', label: 'Processing failure', labelCn: '处理失败', desc: 'Metric computation error', descCn: '指标计算错误' },
];

export function UploadConfigPanel({ lang, disabled, onSubmit }: UploadConfigPanelProps) {
  const cn = lang === 'cn';
  const [athleteId, setAthleteId] = useState(DEMO_ATHLETES[0].id);
  const [protocolId, setProtocolId] = useState(DEMO_PROTOCOLS[0].id);
  const [deviceId, setDeviceId] = useState(DEMO_DEVICES[0].id);
  const [trials, setTrials] = useState(3);
  const [failureMode, setFailureMode] = useState<SimulatedFailureMode>('none');
  const [operatorName, setOperatorName] = useState('Demo Operator');

  const selectedProtocol = useMemo(
    () => DEMO_PROTOCOLS.find((p) => p.id === protocolId),
    [protocolId],
  );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit({ athleteId, protocolId, deviceId, trials, failureMode, operatorName });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <Field label={cn ? '运动员' : 'Athlete'} icon={<Users className="h-4 w-4" />}>
          <select
            value={athleteId}
            onChange={(e) => setAthleteId(e.target.value)}
            disabled={disabled}
            className="w-full rounded-md border border-border-default bg-surface-panel px-3 py-2 text-sm text-content-primary focus:border-accent-primary focus:outline-none focus:ring-2 focus:ring-accent-primary/20"
          >
            {DEMO_ATHLETES.map((a) => (
              <option key={a.id} value={a.id}>
                {a.lastName} {a.firstName} — {a.position}
              </option>
            ))}
          </select>
        </Field>

        <Field label={cn ? '测试方案' : 'Protocol'} icon={<Activity className="h-4 w-4" />}>
          <select
            value={protocolId}
            onChange={(e) => setProtocolId(e.target.value)}
            disabled={disabled}
            className="w-full rounded-md border border-border-default bg-surface-panel px-3 py-2 text-sm text-content-primary focus:border-accent-primary focus:outline-none focus:ring-2 focus:ring-accent-primary/20"
          >
            {DEMO_PROTOCOLS.map((p) => (
              <option key={p.id} value={p.id}>
                {p.name} (v{p.version})
              </option>
            ))}
          </select>
        </Field>

        <Field label={cn ? '采集设备' : 'Device'} icon={<Cpu className="h-4 w-4" />}>
          <select
            value={deviceId}
            onChange={(e) => setDeviceId(e.target.value)}
            disabled={disabled}
            className="w-full rounded-md border border-border-default bg-surface-panel px-3 py-2 text-sm text-content-primary focus:border-accent-primary focus:outline-none focus:ring-2 focus:ring-accent-primary/20"
          >
            {DEMO_DEVICES.map((d) => (
              <option key={d.id} value={d.id}>
                {d.type} · {d.serialNumber}
              </option>
            ))}
          </select>
        </Field>

        <Field label={cn ? '试次数量' : 'Trials'}>
          <input
            type="number"
            min={1}
            max={10}
            value={trials}
            onChange={(e) => setTrials(Math.max(1, Math.min(10, Number(e.target.value) || 1)))}
            disabled={disabled}
            className="w-full rounded-md border border-border-default bg-surface-panel px-3 py-2 text-sm text-content-primary focus:border-accent-primary focus:outline-none focus:ring-2 focus:ring-accent-primary/20"
          />
        </Field>

        <Field label={cn ? '操作员' : 'Operator'} className="md:col-span-2">
          <input
            type="text"
            value={operatorName}
            onChange={(e) => setOperatorName(e.target.value)}
            disabled={disabled}
            className="w-full rounded-md border border-border-default bg-surface-panel px-3 py-2 text-sm text-content-primary focus:border-accent-primary focus:outline-none focus:ring-2 focus:ring-accent-primary/20"
          />
        </Field>
      </div>

      {selectedProtocol && (
        <div className="rounded-md border border-border-default bg-surface-hover/50 px-4 py-3 text-xs text-content-secondary">
          <span className="font-medium text-content-primary">{selectedProtocol.name}</span>
          {cn ? ` · 输入类型: ${selectedProtocol.input.dataTypes.join(', ')} · 采集模式: ${selectedProtocol.input.captureMode}` : ` · Input: ${selectedProtocol.input.dataTypes.join(', ')} · Capture: ${selectedProtocol.input.captureMode}`}
        </div>
      )}

      <div>
        <label className="mb-2 block text-sm font-medium text-content-primary">
          {cn ? '故障注入模式' : 'Failure injection mode'}
        </label>
        <div className="grid grid-cols-2 gap-2 md:grid-cols-4">
          {FAILURE_OPTIONS.map((opt) => {
            const active = failureMode === opt.value;
            return (
              <button
                key={opt.value}
                type="button"
                onClick={() => setFailureMode(opt.value)}
                disabled={disabled}
                className={`rounded-md border px-3 py-2 text-left transition ${
                  active
                    ? 'border-accent-primary bg-accent-soft'
                    : 'border-border-default bg-surface-panel hover:border-content-secondary/40'
                } disabled:cursor-not-allowed disabled:opacity-50`}
              >
                <div className="flex items-center gap-1.5">
                  {opt.value === 'none' ? (
                    <CheckCircle2 className="h-3.5 w-3.5 text-status-success" />
                  ) : (
                    <AlertTriangle className="h-3.5 w-3.5 text-status-warning" />
                  )}
                  <span className="text-xs font-medium text-content-primary">{cn ? opt.labelCn : opt.label}</span>
                </div>
                <p className="mt-1 text-[11px] text-content-secondary">{cn ? opt.descCn : opt.desc}</p>
              </button>
            );
          })}
        </div>
      </div>

      <button
        type="submit"
        disabled={disabled}
        className="inline-flex items-center gap-2 rounded-md bg-accent-primary px-4 py-2.5 text-sm font-medium text-white transition hover:bg-accent-hover disabled:cursor-not-allowed disabled:opacity-50"
      >
        <Upload className="h-4 w-4" />
        {disabled ? (cn ? '运行中…' : 'Running…') : cn ? '启动模拟上传' : 'Start simulated upload'}
      </button>
    </form>
  );
}

function Field({
  label,
  icon,
  children,
  className = '',
}: {
  label: string;
  icon?: React.ReactNode;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={className}>
      <label className="mb-1.5 flex items-center gap-1.5 text-sm font-medium text-content-primary">
        {icon}
        {label}
      </label>
      {children}
    </div>
  );
}