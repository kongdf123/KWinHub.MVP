import { useRef, useState } from 'react';
import {
  Upload, RotateCcw, RefreshCw, ChevronLeft, ChevronRight, Copy, Download,
  Plus, X, AlertTriangle, CheckCircle2, PlayCircle, FileText, Film,
  Cpu, Clock, Server, KeyRound, Link2, Check,
} from 'lucide-react';
import { useApp } from '@/lib/app-context';
import { TopBar } from '@/components/layout/TopBar';
import { DEMO_ATHLETES, DEMO_DEVICES, DEMO_PROTOCOLS } from '@/lib/demo-data';
import { useUploadSimulator } from './use-upload-simulator';
import type { FailureMode, WizardState } from './types';

const STEP_LABELS = [
  { en: 'Client & Session', cn: '客户端与会话' },
  { en: 'Devices & Trials', cn: '设备与试次' },
  { en: 'Upload Files', cn: '上传文件' },
  { en: 'Review & Submit', cn: '审核并提交' },
  { en: 'Result', cn: '提交结果' },
];

const FAILURE_OPTIONS: { value: FailureMode; en: string; cn: string }[] = [
  { value: 'none', en: 'Success path', cn: '成功路径' },
  { value: 'upload', en: 'Upload failure', cn: '上传失败' },
  { value: 'validation', en: 'Validation failure', cn: '校验失败' },
  { value: 'processing', en: 'Processing failure', cn: '处理失败' },
];

export function UploadSimulatorPage() {
  const { lang } = useApp();
  const cn = lang === 'cn';
  const sim = useUploadSimulator();
  const { step, wizard, result, isRunning, history } = sim;

  return (
    <div>
      <TopBar />

      <div className="px-6 pt-6">
        <div className="flex items-start justify-between gap-4">
          <div>
            <h2 className="text-xl font-semibold text-content-primary">
              {cn ? '客户端上传模拟器' : 'Client Upload Simulator'}
            </h2>
            <p className="mt-1 text-sm text-content-secondary">
              {cn
                ? '以向导方式模拟客户端到 KWinHub API 的完整采集与上传链路'
                : 'Wizard-style simulation of the full client capture → upload pipeline'}
            </p>
          </div>
          <div className="flex items-center gap-2">
            <button
              onClick={sim.clearDemoData}
              className="inline-flex items-center gap-1.5 rounded-md border border-border-default bg-surface-panel px-3 py-1.5 text-xs text-content-secondary hover:bg-surface-hover"
            >
              <RotateCcw className="h-3.5 w-3.5" />
              {cn ? '清除演示数据' : 'Reset demo data'} ({sim.generatedCount})
            </button>
            <button
              onClick={sim.reset}
              className="inline-flex items-center gap-1.5 rounded-md border border-border-default bg-surface-panel px-3 py-1.5 text-xs text-content-secondary hover:bg-surface-hover"
            >
              <RefreshCw className="h-3.5 w-3.5" />
              {cn ? '新建会话' : 'New session'}
            </button>
          </div>
        </div>

        <div className="mt-4 mb-3 rounded-lg border border-amber-300 bg-amber-50 p-3">
          <p className="text-xs leading-relaxed text-amber-800">
            {cn
              ? '原型演示工具。所有数据为本地模拟，成功提交后会在「会话」与运动员时间线中出现。'
              : 'Prototype tool. All data is locally simulated; successful submissions appear in Sessions and the athlete timeline.'}
          </p>
        </div>

        <Stepper step={step} cn={cn} onJump={(s) => !isRunning && sim.goTo(s)} />

        <div className="mt-5">
          {step === 0 && <StepClient sim={sim} cn={cn} />}
          {step === 1 && <StepDevices sim={sim} cn={cn} />}
          {step === 2 && <StepFiles sim={sim} cn={cn} />}
          {step === 3 && <StepReview sim={sim} cn={cn} />}
          {step === 4 && <StepResult sim={sim} cn={cn} />}
        </div>

        {step < 4 && (
          <div className="mt-6 flex items-center justify-between">
            <button
              onClick={sim.back}
              disabled={step === 0}
              className="inline-flex items-center gap-1.5 rounded-md border border-border-default bg-surface-panel px-4 py-2 text-sm text-content-secondary hover:bg-surface-hover disabled:opacity-40"
            >
              <ChevronLeft className="h-4 w-4" />
              {cn ? '上一步' : 'Back'}
            </button>
            {step < 3 ? (
              <button
                onClick={sim.next}
                className="inline-flex items-center gap-1.5 rounded-md bg-accent-primary px-4 py-2 text-sm font-medium text-accent-foreground hover:bg-accent-hover"
              >
                {cn ? '下一步' : 'Next'}
                <ChevronRight className="h-4 w-4" />
              </button>
            ) : (
              <button
                onClick={sim.submit}
                disabled={isRunning}
                className="inline-flex items-center gap-1.5 rounded-md bg-status-success px-4 py-2 text-sm font-medium text-white hover:opacity-90 disabled:opacity-50"
              >
                <Upload className="h-4 w-4" />
                {cn ? '提交会话' : 'Submit session'}
              </button>
            )}
          </div>
        )}
      </div>

      {history.length > 0 && (
        <div className="px-6 py-6">
          <div className="rounded-lg border border-border-default bg-surface-panel p-5">
            <h3 className="mb-3 text-sm font-semibold text-content-primary">
              {cn ? '最近模拟记录' : 'Recent simulations'}
            </h3>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border-default text-left text-xs text-content-secondary">
                    <th className="py-2 pr-4 font-medium">{cn ? '时间' : 'Time'}</th>
                    <th className="py-2 pr-4 font-medium">{cn ? '运动员' : 'Athlete'}</th>
                    <th className="py-2 pr-4 font-medium">{cn ? '会话ID' : 'Session ID'}</th>
                    <th className="py-2 pr-4 font-medium">{cn ? '状态' : 'Status'}</th>
                  </tr>
                </thead>
                <tbody>
                  {history.map((h) => (
                    <tr key={h.sessionId} className="border-b border-border-default/60 last:border-0">
                      <td className="py-2 pr-4 text-content-secondary">{new Date(h.startedAt).toLocaleString()}</td>
                      <td className="py-2 pr-4 text-content-primary">{wizard.athleteId}</td>
                      <td className="py-2 pr-4 font-mono text-xs text-content-secondary">{h.sessionId}</td>
                      <td className="py-2 pr-4">
                        <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                          h.currentStatus === 'PUBLISHED' ? 'bg-status-success/10 text-status-success' : 'bg-status-danger/10 text-status-danger'
                        }`}>
                          {h.currentStatus}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function Stepper({ step, cn, onJump }: { step: number; cn: boolean; onJump: (s: number) => void }) {
  return (
    <div className="flex items-center">
      {STEP_LABELS.map((label, i) => (
        <div key={i} className="flex flex-1 items-center last:flex-none">
          <button
            onClick={() => onJump(i)}
            disabled={i > step}
            className="flex items-center gap-2 disabled:cursor-default"
          >
            <span className={`flex h-7 w-7 items-center justify-center rounded-full text-xs font-semibold ${
              i === step
                ? 'bg-accent-primary text-accent-foreground'
                : i < step
                ? 'bg-status-success text-white'
                : 'bg-border-default text-content-secondary'
            }`}>
              {i < step ? <Check className="h-3.5 w-3.5" /> : i + 1}
            </span>
            <span className={`text-xs font-medium ${i === step ? 'text-content-primary' : 'text-content-secondary'}`}>
              {cn ? label.cn : label.en}
            </span>
          </button>
          {i < STEP_LABELS.length - 1 && <div className={`mx-3 h-px flex-1 ${i < step ? 'bg-status-success' : 'bg-border-default'}`} />}
        </div>
      ))}
    </div>
  );
}

function Card({ children, title }: { children: React.ReactNode; title: string }) {
  return (
    <div className="rounded-lg border border-border-default bg-surface-panel p-5">
      <h3 className="mb-4 text-sm font-semibold text-content-primary">{title}</h3>
      {children}
    </div>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div>
      <label className="mb-1.5 block text-sm font-medium text-content-primary">{label}</label>
      {children}
    </div>
  );
}

const inputCls = 'w-full rounded-md border border-border-default bg-surface-panel px-3 py-2 text-sm text-content-primary focus:border-border-focus focus:outline-none focus:ring-2 focus:ring-accent-primary/20';

function StepClient({ sim, cn }: { sim: ReturnType<typeof useUploadSimulator>; cn: boolean }) {
  const { wizard, update, regenerateExternalSessionId } = sim;
  return (
    <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
      <Card title={cn ? '客户端信息' : 'Client identity'}>
        <div className="space-y-4">
          <Field label={cn ? '客户端ID' : 'Client ID'}>
            <input className={inputCls} value={wizard.clientId} onChange={(e) => update('clientId', e.target.value)} />
          </Field>
          <Field label={cn ? '应用名称' : 'Application'}>
            <input className={inputCls} value={wizard.clientApplication} onChange={(e) => update('clientApplication', e.target.value)} />
          </Field>
          <Field label={cn ? '版本' : 'Version'}>
            <input className={inputCls} value={wizard.clientVersion} onChange={(e) => update('clientVersion', e.target.value)} />
          </Field>
          <Field label={cn ? '环境' : 'Environment'}>
            <select className={inputCls} value={wizard.environment} onChange={(e) => update('environment', e.target.value as WizardState['environment'])}>
              <option value="production">production</option>
              <option value="staging">staging</option>
              <option value="development">development</option>
            </select>
          </Field>
        </div>
      </Card>

      <Card title={cn ? '会话身份' : 'Session identity'}>
        <div className="space-y-4">
          <Field label={cn ? '外部会话ID' : 'External session ID'}>
            <div className="flex gap-2">
              <input className={inputCls} value={wizard.externalSessionId} onChange={(e) => update('externalSessionId', e.target.value)} />
              <button onClick={regenerateExternalSessionId} className="shrink-0 rounded-md border border-border-default bg-surface-panel px-3 text-content-secondary hover:bg-surface-hover">
                <RefreshCw className="h-4 w-4" />
              </button>
            </div>
          </Field>
          <Field label={cn ? '会话名称' : 'Session name'}>
            <input className={inputCls} value={wizard.sessionName} onChange={(e) => update('sessionName', e.target.value)} placeholder={cn ? '可选' : 'Optional'} />
          </Field>
          <Field label={cn ? '采集时间' : 'Date / time'}>
            <input type="datetime-local" className={inputCls} value={wizard.date} onChange={(e) => update('date', e.target.value)} />
          </Field>
          <Field label={cn ? '操作员' : 'Operator'}>
            <input className={inputCls} value={wizard.operatorName} onChange={(e) => update('operatorName', e.target.value)} />
          </Field>
        </div>
      </Card>

      <Card title={cn ? '测试对象与方案' : 'Subject & protocol'}>
        <div className="space-y-4">
          <Field label={cn ? '运动员' : 'Athlete'}>
            <select className={inputCls} value={wizard.athleteId} onChange={(e) => update('athleteId', e.target.value)}>
              {DEMO_ATHLETES.map((a) => (
                <option key={a.id} value={a.id}>{a.lastName} {a.firstName} — {a.position}</option>
              ))}
            </select>
          </Field>
          <Field label={cn ? '测试方案' : 'Protocol'}>
            <select className={inputCls} value={wizard.protocolId} onChange={(e) => update('protocolId', e.target.value)}>
              {DEMO_PROTOCOLS.map((p) => (
                <option key={p.id} value={p.id}>{p.name} (v{p.version})</option>
              ))}
            </select>
          </Field>
          <Field label={cn ? '备注' : 'Notes'}>
            <textarea className={inputCls} rows={3} value={wizard.notes} onChange={(e) => update('notes', e.target.value)} placeholder={cn ? '可选备注' : 'Optional notes'} />
          </Field>
        </div>
      </Card>
    </div>
  );
}

function StepDevices({ sim, cn }: { sim: ReturnType<typeof useUploadSimulator>; cn: boolean }) {
  const { wizard, addDevice, removeDevice, addTrial, removeTrial, updateTrialStatus } = sim;
  return (
    <Card title={cn ? '设备与试次配置' : 'Devices & trials'}>
      <div className="space-y-4">
        {wizard.devices.map((group, di) => (
          <div key={di} className="rounded-md border border-border-default p-4">
            <div className="mb-3 flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Cpu className="h-4 w-4 text-content-secondary" />
                <select
                  className={inputCls + ' max-w-xs'}
                  value={group.device.id}
                  onChange={(e) => {
                    const dev = DEMO_DEVICES.find((d) => d.id === e.target.value)!;
                    sim.update('devices', wizard.devices.map((g, i) => i === di ? {
                      ...g,
                      device: { id: dev.id, externalDeviceId: dev.serialNumber, type: dev.type, serialNumber: dev.serialNumber },
                    } : g));
                  }}
                >
                  {DEMO_DEVICES.map((d) => (
                    <option key={d.id} value={d.id}>{d.type} · {d.serialNumber}</option>
                  ))}
                </select>
                <span className="font-mono text-xs text-content-secondary">{group.device.externalDeviceId}</span>
              </div>
              <button onClick={() => removeDevice(di)} className="text-content-secondary hover:text-status-danger">
                <X className="h-4 w-4" />
              </button>
            </div>
            <div className="ml-6 space-y-2">
              {group.trials.map((trial, ti) => (
                <div key={ti} className="flex items-center gap-3">
                  <span className="w-20 text-xs text-content-secondary">Trial {trial.trialNumber}</span>
                  <span className="font-mono text-xs text-content-tertiary">{trial.externalTrialId}</span>
                  <select
                    className={inputCls + ' max-w-[120px] py-1 text-xs'}
                    value={trial.status}
                    onChange={(e) => updateTrialStatus(di, ti, e.target.value as 'valid' | 'invalid')}
                  >
                    <option value="valid">{cn ? '有效' : 'valid'}</option>
                    <option value="invalid">{cn ? '无效' : 'invalid'}</option>
                  </select>
                  <button onClick={() => removeTrial(di, ti)} className="text-content-secondary hover:text-status-danger">
                    <X className="h-3.5 w-3.5" />
                  </button>
                </div>
              ))}
              <button onClick={() => addTrial(di)} className="inline-flex items-center gap-1 text-xs text-accent-text hover:underline">
                <Plus className="h-3 w-3" /> {cn ? '添加试次' : 'Add trial'}
              </button>
            </div>
          </div>
        ))}
        <button onClick={addDevice} className="inline-flex items-center gap-1.5 rounded-md border border-dashed border-border-default px-4 py-2 text-sm text-content-secondary hover:bg-surface-hover">
          <Plus className="h-4 w-4" /> {cn ? '添加设备' : 'Add device'}
        </button>
      </div>
    </Card>
  );
}

function StepFiles({ sim, cn }: { sim: ReturnType<typeof useUploadSimulator>; cn: boolean }) {
  const { wizard, setCsvFile, setVideoFile } = sim;
  const csvRef = useRef<HTMLInputElement>(null);
  const vidRef = useRef<HTMLInputElement>(null);

  return (
    <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
      <Card title={cn ? 'CSV 数据文件' : 'CSV data file'}>
        <input ref={csvRef} type="file" accept=".csv,text/csv" className="hidden" onChange={(e) => setCsvFile(e.target.files?.[0] ?? null)} />
        <button onClick={() => csvRef.current?.click()} className="flex w-full flex-col items-center justify-center rounded-md border border-dashed border-border-default py-10 text-content-secondary hover:bg-surface-hover">
          <FileText className="mb-2 h-8 w-8" />
          <span className="text-sm">{cn ? '点击选择 CSV 文件' : 'Click to select CSV file'}</span>
          <span className="mt-1 text-xs text-content-tertiary">{cn ? '将解析表头并预览前 10 行' : 'Headers parsed, first 10 rows previewed'}</span>
        </button>
        {wizard.csvFile && (
          <div className="mt-4">
            <div className="mb-2 flex items-center justify-between">
              <span className="text-sm font-medium text-content-primary">{wizard.csvFile.name}</span>
              <button onClick={() => setCsvFile(null)} className="text-content-secondary hover:text-status-danger"><X className="h-4 w-4" /></button>
            </div>
            <div className="text-xs text-content-secondary">{wizard.csvFile.sizeBytes.toLocaleString()} bytes · {wizard.csvPreview?.rowCount ?? 0} rows</div>
            {wizard.csvPreview && wizard.csvPreview.headers.length > 0 && (
              <div className="mt-3 max-h-60 overflow-auto rounded-md border border-border-default">
                <table className="w-full text-xs">
                  <thead className="bg-surface-hover">
                    <tr>{wizard.csvPreview.headers.map((h, i) => <th key={i} className="px-2 py-1.5 text-left font-medium text-content-secondary">{h}</th>)}</tr>
                  </thead>
                  <tbody>
                    {wizard.csvPreview.rows.map((row, ri) => (
                      <tr key={ri} className="border-t border-border-subtle">
                        {row.map((c, ci) => <td key={ci} className="px-2 py-1 text-content-primary">{c}</td>)}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}
      </Card>

      <Card title={cn ? '视频文件（可选）' : 'Video file (optional)'}>
        <input ref={vidRef} type="file" accept="video/*" className="hidden" onChange={(e) => setVideoFile(e.target.files?.[0] ?? null)} />
        <button onClick={() => vidRef.current?.click()} className="flex w-full flex-col items-center justify-center rounded-md border border-dashed border-border-default py-10 text-content-secondary hover:bg-surface-hover">
          <Film className="mb-2 h-8 w-8" />
          <span className="text-sm">{cn ? '点击选择视频文件' : 'Click to select video file'}</span>
        </button>
        {wizard.videoFile && (
          <div className="mt-4">
            <div className="mb-2 flex items-center justify-between">
              <span className="text-sm font-medium text-content-primary">{wizard.videoFile.name}</span>
              <button onClick={() => setVideoFile(null)} className="text-content-secondary hover:text-status-danger"><X className="h-4 w-4" /></button>
            </div>
            <div className="text-xs text-content-secondary">{wizard.videoFile.sizeBytes.toLocaleString()} bytes</div>
            {wizard.videoFile.previewUrl && (
              <video src={wizard.videoFile.previewUrl} controls className="mt-3 w-full rounded-md border border-border-default" />
            )}
          </div>
        )}
      </Card>
    </div>
  );
}

function StepReview({ sim, cn }: { sim: ReturnType<typeof useUploadSimulator>; cn: boolean }) {
  const { manifest, idempotencyKey, wizard, update } = sim;
  const [copied, setCopied] = useState(false);

  const headers = {
    'Content-Type': 'application/json',
    'Idempotency-Key': idempotencyKey,
    'X-Correlation-ID': `corr-${Math.random().toString(36).slice(2, 10)}`,
    'X-Client-Id': wizard.clientId,
    'X-Client-Version': wizard.clientVersion,
  };

  const jsonText = JSON.stringify({ headers, body: manifest }, null, 2);

  const copyJson = async () => {
    await navigator.clipboard.writeText(jsonText);
    setCopied(true);
    setTimeout(() => setCopied(false), 1500);
  };

  const downloadJson = () => {
    const blob = new Blob([jsonText], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${wizard.externalSessionId}.json`;
    a.click();
    URL.revokeObjectURL(url);
  };

  return (
    <div className="grid grid-cols-1 gap-5 lg:grid-cols-3">
      <div className="lg:col-span-2">
        <Card title={cn ? '请求载荷 (JSON)' : 'Request payload (JSON)'}>
          <div className="mb-3 flex items-center justify-end gap-2">
            <button onClick={copyJson} className="inline-flex items-center gap-1.5 rounded-md border border-border-default bg-surface-panel px-3 py-1.5 text-xs text-content-secondary hover:bg-surface-hover">
              {copied ? <Check className="h-3.5 w-3.5 text-status-success" /> : <Copy className="h-3.5 w-3.5" />}
              {copied ? (cn ? '已复制' : 'Copied') : (cn ? '复制' : 'Copy')}
            </button>
            <button onClick={downloadJson} className="inline-flex items-center gap-1.5 rounded-md border border-border-default bg-surface-panel px-3 py-1.5 text-xs text-content-secondary hover:bg-surface-hover">
              <Download className="h-3.5 w-3.5" />
              {cn ? '下载' : 'Download'}
            </button>
          </div>
          <pre className="max-h-96 overflow-auto rounded-md border border-border-default bg-surface-app p-4 font-mono text-xs text-content-primary">{jsonText}</pre>
        </Card>
      </div>

      <div className="space-y-5">
        <Card title={cn ? 'HTTP 头部' : 'HTTP headers'}>
          <div className="space-y-2 font-mono text-xs">
            {Object.entries(headers).map(([k, v]) => (
              <div key={k} className="flex flex-col">
                <span className="text-content-secondary">{k}:</span>
                <span className="break-all text-content-primary">{v}</span>
              </div>
            ))}
          </div>
        </Card>

        <Card title={cn ? '故障注入' : 'Failure injection'}>
          <div className="grid grid-cols-2 gap-2">
            {FAILURE_OPTIONS.map((opt) => {
              const active = wizard.failureMode === opt.value;
              return (
                <button
                  key={opt.value}
                  onClick={() => update('failureMode', opt.value)}
                  className={`rounded-md border px-3 py-2 text-left text-xs transition ${
                    active ? 'border-accent-primary bg-accent-soft' : 'border-border-default bg-surface-panel hover:border-content-secondary/40'
                  }`}
                >
                  <div className="flex items-center gap-1.5">
                    {opt.value === 'none'
                      ? <CheckCircle2 className="h-3.5 w-3.5 text-status-success" />
                      : <AlertTriangle className="h-3.5 w-3.5 text-status-warning" />}
                    <span className="font-medium text-content-primary">{cn ? opt.cn : opt.en}</span>
                  </div>
                </button>
              );
            })}
          </div>
        </Card>
      </div>
    </div>
  );
}

function StepResult({ sim, cn }: { sim: ReturnType<typeof useUploadSimulator>; cn: boolean }) {
  const { result, isRunning } = sim;
  if (!result) return null;

  const isSuccess = result.currentStatus === 'PUBLISHED';

  return (
    <div className="grid grid-cols-1 gap-5 lg:grid-cols-3">
      <div className="lg:col-span-2 space-y-5">
        <Card title={cn ? '生命周期' : 'Lifecycle'}>
          <div className="space-y-2">
            {result.stages.map((s, i) => (
              <div key={i} className="flex items-center gap-3">
                <span className={`flex h-6 w-6 items-center justify-center rounded-full text-xs ${
                  s.result === 'success' ? 'bg-status-success text-white' :
                  s.result === 'failed' ? 'bg-status-danger text-white' :
                  s.result === 'running' ? 'bg-accent-primary text-white animate-pulse' :
                  s.result === 'skipped' ? 'bg-border-default text-content-secondary' :
                  'bg-border-default text-content-tertiary'
                }`}>
                  {s.result === 'success' ? <Check className="h-3.5 w-3.5" /> :
                   s.result === 'failed' ? <X className="h-3.5 w-3.5" /> :
                   s.result === 'running' ? <Clock className="h-3 w-3" /> :
                   s.result === 'skipped' ? '–' : i + 1}
                </span>
                <div className="flex-1">
                  <div className="text-sm font-medium text-content-primary">{cn ? s.stage.labelCn : s.stage.label}</div>
                  {s.detail && <div className="text-xs text-status-danger">{s.detail}</div>}
                </div>
                <span className="font-mono text-xs text-content-secondary">{s.stage.status}</span>
              </div>
            ))}
          </div>
        </Card>

        <Card title={cn ? '日志' : 'Log'}>
          <div className="max-h-64 space-y-1 overflow-auto font-mono text-xs">
            {result.log.map((l) => (
              <div key={l.id} className="flex gap-2">
                <span className="text-content-tertiary shrink-0">{new Date(l.timestamp).toLocaleTimeString()}</span>
                <span className={`shrink-0 ${
                  l.level === 'error' ? 'text-status-danger' :
                  l.level === 'warning' ? 'text-status-warning' :
                  l.level === 'success' ? 'text-status-success' : 'text-content-secondary'
                }`}>[{l.level}]</span>
                <span className="text-content-primary">{cn ? l.messageCn : l.message}</span>
              </div>
            ))}
          </div>
        </Card>
      </div>

      <Card title={cn ? '会话详情' : 'Session details'}>
        <div className="space-y-3 text-sm">
          <div className={`inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs font-medium ${
            isSuccess ? 'bg-status-success/10 text-status-success' : 'bg-status-danger/10 text-status-danger'
          }`}>
            {isSuccess ? <CheckCircle2 className="h-3.5 w-3.5" /> : <AlertTriangle className="h-3.5 w-3.5" />}
            {result.currentStatus}
          </div>
          <DetailRow icon={<KeyRound className="h-3.5 w-3.5" />} label={cn ? '幂等键' : 'Idempotency key'} value={result.idempotencyKey} mono />
          <DetailRow icon={<Server className="h-3.5 w-3.5" />} label={cn ? '会话ID' : 'Session ID'} value={result.sessionId} mono />
          <DetailRow icon={<Link2 className="h-3.5 w-3.5" />} label={cn ? '状态URL' : 'Status URL'} value={result.statusUrl} mono />
          <DetailRow label={cn ? '关联ID' : 'Correlation ID'} value={result.correlationId} mono />
          <DetailRow label={cn ? '开始时间' : 'Started'} value={new Date(result.startedAt).toLocaleString()} />
          {result.completedAt && <DetailRow label={cn ? '完成时间' : 'Completed'} value={new Date(result.completedAt).toLocaleString()} />}
          {result.isDuplicate && (
            <div className="rounded-md bg-status-warning/10 p-2 text-xs text-status-warning">
              {cn ? '检测到重复幂等键 — 复用了已有会话' : 'Duplicate idempotency key — existing session reused'}
            </div>
          )}
          {result.errorMessage && (
            <div className="rounded-md bg-status-danger/10 p-2 text-xs text-status-danger">{result.errorMessage}</div>
          )}
          {isSuccess && (
            <div className="rounded-md bg-status-success/10 p-2 text-xs text-status-success">
              {cn ? '会话已发布，可在「会话」页面查看' : 'Session published — visible in Sessions page'}
            </div>
          )}
          {!isRunning && result.completedAt && (
            <button onClick={sim.reset} className="mt-2 inline-flex items-center gap-1.5 rounded-md border border-border-default bg-surface-panel px-4 py-2 text-sm text-content-secondary hover:bg-surface-hover">
              <PlayCircle className="h-4 w-4" />
              {cn ? '开始新的模拟' : 'Start new simulation'}
            </button>
          )}
        </div>
      </Card>
    </div>
  );
}

function DetailRow({ icon, label, value, mono }: { icon?: React.ReactNode; label: string; value: string; mono?: boolean }) {
  return (
    <div className="flex flex-col">
      <div className="flex items-center gap-1.5 text-xs text-content-secondary">
        {icon}
        {label}
      </div>
      <div className={`break-all ${mono ? 'font-mono text-xs' : 'text-sm'} text-content-primary`}>{value}</div>
    </div>
  );
}