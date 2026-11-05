import { RotateCcw, History, Link2 } from 'lucide-react';
import { useApp } from '@/lib/app-context';
import { TopBar } from '@/components/layout/TopBar';
import { useUploadSimulator } from './use-upload-simulator';
import { UploadConfigPanel } from './UploadConfigPanel';
import { PipelineVisualization } from './PipelineVisualization';
import { UploadLogPanel } from './UploadLogPanel';
import type { UploadConfig } from './types';

export function UploadSimulatorPage() {
  const { lang } = useApp();
  const cn = lang === 'cn';
  const { job, isRunning, start, reset, history } = useUploadSimulator();

  const handleStart = (config: UploadConfig) => {
    start(config);
  };

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
                ? '模拟客户端到 KWinHub API 的异步采集、上传、校验与处理管线'
                : 'Simulate the async capture → upload → validate → process pipeline to the KWinHub API'}
            </p>
          </div>
        </div>
      </div>

      <div className="px-6 py-4">
        <div className="mb-4 rounded-lg border border-amber-300 bg-amber-50 p-4">
          <div className="flex items-start gap-3">
            <div className="mt-0.5 flex h-5 w-5 shrink-0 items-center justify-center rounded-full bg-amber-400 text-white">
              <span className="text-xs font-bold">!</span>
            </div>
            <div>
              <h3 className="text-sm font-semibold text-amber-900">
                {cn ? '仅用于原型演示' : 'Prototype Demonstration Only'}
              </h3>
              <p className="mt-1 text-sm leading-relaxed text-amber-800">
                {cn
                  ? '本工具用于演示客户端数据采集与服务端异步处理的完整链路，包含幂等键、状态轮询与故障注入。所有数据均为本地模拟，不产生真实 API 调用。'
                  : 'This tool demonstrates the full client capture to server async-processing pipeline, including idempotency keys, status polling, and failure injection. All data is locally simulated and makes no real API calls.'}
              </p>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-5 lg:grid-cols-12">
          <section className="lg:col-span-4">
            <div className="rounded-lg border border-border-default bg-surface-panel p-5">
              <h3 className="mb-4 text-sm font-semibold text-content-primary">
                {cn ? '上传配置' : 'Upload configuration'}
              </h3>
              <UploadConfigPanel lang={lang} disabled={isRunning} onSubmit={handleStart} />
            </div>

            {job && (
              <div className="mt-4 flex items-center gap-2">
                <button
                  onClick={reset}
                  className="inline-flex items-center gap-1.5 rounded-md border border-border-default bg-surface-panel px-3 py-1.5 text-sm text-content-secondary hover:bg-surface-hover"
                >
                  <RotateCcw className="h-3.5 w-3.5" />
                  {cn ? '重置' : 'Reset'}
                </button>
                <div className="flex items-center gap-1.5 text-xs text-content-secondary">
                  <Link2 className="h-3.5 w-3.5" />
                  <span className="truncate">{job.statusUrl}</span>
                </div>
              </div>
            )}
          </section>

          <section className="lg:col-span-4">
            <div className="rounded-lg border border-border-default bg-surface-panel p-5">
              <PipelineVisualization lang={lang} job={job} />
            </div>
          </section>

          <section className="lg:col-span-4">
            <div className="flex h-full min-h-[320px] flex-col rounded-lg border border-border-default bg-surface-panel p-5">
              <UploadLogPanel lang={lang} job={job} />
            </div>
          </section>
        </div>

        {history.length > 0 && (
          <div className="mt-5 rounded-lg border border-border-default bg-surface-panel p-5">
            <div className="mb-3 flex items-center gap-2">
              <History className="h-4 w-4 text-content-secondary" />
              <h3 className="text-sm font-semibold text-content-primary">
                {cn ? '最近模拟记录' : 'Recent simulations'}
              </h3>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-border-default text-left text-xs text-content-secondary">
                    <th className="py-2 pr-4 font-medium">{cn ? '时间' : 'Time'}</th>
                    <th className="py-2 pr-4 font-medium">{cn ? '运动员' : 'Athlete'}</th>
                    <th className="py-2 pr-4 font-medium">{cn ? '方案' : 'Protocol'}</th>
                    <th className="py-2 pr-4 font-medium">{cn ? '故障模式' : 'Failure'}</th>
                    <th className="py-2 pr-4 font-medium">{cn ? '最终状态' : 'Final status'}</th>
                  </tr>
                </thead>
                <tbody>
                  {history.map((h) => (
                    <tr key={h.id} className="border-b border-border-default/60 last:border-0">
                      <td className="py-2 pr-4 text-content-secondary">
                        {new Date(h.startedAt).toLocaleString()}
                      </td>
                      <td className="py-2 pr-4 text-content-primary">{h.config.athleteId}</td>
                      <td className="py-2 pr-4 text-content-primary">{h.config.protocolId}</td>
                      <td className="py-2 pr-4 text-content-secondary">{h.config.failureMode}</td>
                      <td className="py-2 pr-4">
                        <span
                          className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                            h.currentStatus.endsWith('_FAILED')
                              ? 'bg-status-danger/10 text-status-danger'
                              : 'bg-status-success/10 text-status-success'
                          }`}
                        >
                          {h.currentStatus}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}