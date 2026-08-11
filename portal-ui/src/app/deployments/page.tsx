"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { 
  ArrowLeft, Cpu, Activity, PlayCircle, AlertTriangle, 
  RefreshCw, Globe, CheckCircle2, Server, HelpCircle 
} from "lucide-react";

interface Deployment {
  id: string;
  projectId: string;
  projectName: string;
  buildId: string;
  version: number;
  environment: string;
  domain: string;
  containerId: string;
  imageTag: string;
  status: string;
  createdAt: string;
  completedAt: string | null;
  healthStatus: string;
  healthScore: number;
}

interface Stats {
  runningApps: number;
  failedDeployments: number;
  successRate: number;
  avgDurationSeconds: number;
}

export default function DeploymentsDashboard() {
  const [loading, setLoading] = useState<boolean>(true);
  const [deployments, setDeployments] = useState<Deployment[]>([]);
  const [stats, setStats] = useState<Stats>({
    runningApps: 0,
    failedDeployments: 0,
    successRate: 100.0,
    avgDurationSeconds: 12.5
  });

  const fetchDeploymentsData = async () => {
    setLoading(true);
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8080";

      const listRes = await fetch(`${apiBase}/api/deployments`);
      if (listRes.ok) {
        const data = await listRes.json();
        setDeployments(data);
      }

      const statsRes = await fetch(`${apiBase}/api/deployments/stats`);
      if (statsRes.ok) {
        const data = await statsRes.json();
        setStats({
          runningApps: data.runningApps,
          failedDeployments: data.failedDeployments,
          successRate: data.successRate,
          avgDurationSeconds: data.avgDurationSeconds
        });
      }
    } catch (err) {
      console.warn("API offline, falling back to mock deployment metrics.", err);
      // Premium Mock Fallback
      setDeployments([
        {
          id: "dep-1",
          projectId: "proj-1",
          projectName: "Acme Logistics Portal",
          buildId: "build-14",
          version: 1,
          environment: "Production",
          domain: "acmelogistics.local",
          containerId: "df-api-acmelogistics",
          imageTag: "devforge/generated-api:latest",
          status: "Running",
          createdAt: new Date(Date.now() - 3600000).toISOString(),
          completedAt: new Date(Date.now() - 3588000).toISOString(),
          healthStatus: "Healthy",
          healthScore: 100
        },
        {
          id: "dep-2",
          projectId: "proj-2",
          projectName: "SaaS CRM Integration",
          buildId: "build-3",
          version: 2,
          environment: "Production",
          domain: "saascrm.local",
          containerId: "df-api-saascrm",
          imageTag: "devforge/generated-api:v2",
          status: "Running",
          createdAt: new Date(Date.now() - 7200000).toISOString(),
          completedAt: new Date(Date.now() - 7187000).toISOString(),
          healthStatus: "Healthy",
          healthScore: 100
        },
        {
          id: "dep-3",
          projectId: "proj-3",
          projectName: "Employee HR Platform",
          buildId: "build-7",
          version: 1,
          environment: "Production",
          domain: "employeehr.local",
          containerId: "df-api-employeehr",
          imageTag: "devforge/generated-api:failed",
          status: "Failed",
          createdAt: new Date(Date.now() - 86400000).toISOString(),
          completedAt: new Date(Date.now() - 86395000).toISOString(),
          healthStatus: "Offline",
          healthScore: 0
        }
      ]);

      setStats({
        runningApps: 2,
        failedDeployments: 1,
        successRate: 66.7,
        avgDurationSeconds: 12.5
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchDeploymentsData();
  }, []);

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Running":
        return <span className="px-2.5 py-1 text-xs font-semibold text-emerald-400 bg-emerald-950/40 border border-emerald-800/40 rounded-full">Running</span>;
      case "Failed":
        return <span className="px-2.5 py-1 text-xs font-semibold text-rose-400 bg-rose-950/40 border border-rose-800/40 rounded-full">Failed</span>;
      case "Deploying":
      case "Provisioning":
        return <span className="px-2.5 py-1 text-xs font-semibold text-amber-400 bg-amber-950/40 border border-amber-800/40 rounded-full animate-pulse">Deploying...</span>;
      default:
        return <span className="px-2.5 py-1 text-xs font-semibold text-zinc-400 bg-zinc-900 border border-zinc-700/50 rounded-full">{status}</span>;
    }
  };

  const getHealthBadge = (healthStatus: string, score: number) => {
    switch (healthStatus) {
      case "Healthy":
        return <span className="text-emerald-400 font-semibold">{score}% (Healthy)</span>;
      case "Warning":
        return <span className="text-amber-400 font-semibold">{score}% (Warning)</span>;
      case "Degraded":
        return <span className="text-pink-400 font-semibold">{score}% (Degraded)</span>;
      default:
        return <span className="text-rose-400 font-semibold">{score}% (Offline)</span>;
    }
  };

  return (
    <div className="min-h-screen bg-[#050508] text-zinc-100 font-sans p-6 md:p-12 selection:bg-indigo-500 selection:text-white">
      {/* Back link */}
      <div className="mb-6">
        <Link href="/" className="inline-flex items-center gap-2 text-zinc-400 hover:text-indigo-400 transition-colors text-sm font-semibold">
          <ArrowLeft className="w-4 h-4" /> Back to Dashboard
        </Link>
      </div>

      {/* Header */}
      <header className="flex flex-col md:flex-row justify-between items-start md:items-center gap-4 border-b border-zinc-800/80 pb-8 mb-12">
        <div>
          <h1 className="text-3xl font-extrabold tracking-tight bg-gradient-to-r from-indigo-400 via-purple-400 to-pink-400 bg-clip-text text-transparent">
            Deployment Orchestration Console
          </h1>
          <p className="text-zinc-400 text-sm mt-1">Autonomous Container Provisioning & Health Routing</p>
        </div>
        <div>
          <button 
            onClick={fetchDeploymentsData} 
            className="flex items-center gap-2 px-4 py-2 bg-zinc-900 border border-zinc-800 hover:border-zinc-700 rounded-lg text-sm font-semibold text-zinc-200 transition-all"
            disabled={loading}
          >
            <RefreshCw className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} /> Sync Console
          </button>
        </div>
      </header>

      {/* Stats Section */}
      <section className="grid grid-cols-1 md:grid-cols-4 gap-6 mb-12">
        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Running Apps</span>
            <span className="text-3xl font-bold mt-1 block text-indigo-400">{stats.runningApps}</span>
          </div>
          <Server className="w-8 h-8 text-indigo-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Failed Deployments</span>
            <span className="text-3xl font-bold mt-1 block text-rose-400">{stats.failedDeployments}</span>
          </div>
          <AlertTriangle className="w-8 h-8 text-rose-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Success Rate</span>
            <span className="text-3xl font-bold mt-1 block text-emerald-400">{stats.successRate}%</span>
          </div>
          <CheckCircle2 className="w-8 h-8 text-emerald-400" />
        </div>

        <div className="bg-[#0b0c14] border border-zinc-800/60 p-6 rounded-xl flex items-center justify-between shadow-xl">
          <div>
            <span className="text-xs text-zinc-400 block uppercase tracking-wider font-semibold">Avg Duration</span>
            <span className="text-3xl font-bold mt-1 block text-purple-400">{stats.avgDurationSeconds}s</span>
          </div>
          <Activity className="w-8 h-8 text-purple-400" />
        </div>
      </section>

      {/* Table Section */}
      <main className="bg-[#0b0c14]/40 border border-zinc-800/80 rounded-xl p-6 shadow-xl">
        <h2 className="text-xl font-bold flex items-center gap-2 mb-6">
          <Globe className="w-5 h-5 text-indigo-400" /> Live Microservices Routing Table
        </h2>

        {deployments.length === 0 ? (
          <div className="text-center py-12 text-zinc-500">
            No running applications found. Trigger a generation build first.
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-zinc-800 text-xs uppercase tracking-wider text-zinc-400 font-semibold">
                  <th className="py-4 px-4">App Name</th>
                  <th className="py-4 px-4">Version</th>
                  <th className="py-4 px-4">Allocated Domain</th>
                  <th className="py-4 px-4">Docker Image Tag</th>
                  <th className="py-4 px-4">Runtime Status</th>
                  <th className="py-4 px-4">Health Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-900 text-sm">
                {deployments.map((d) => (
                  <tr key={d.id} className="hover:bg-zinc-900/30 transition-colors">
                    <td className="py-4 px-4 font-bold text-zinc-200">{d.projectName}</td>
                    <td className="py-4 px-4 font-mono text-zinc-400">v{d.version}</td>
                    <td className="py-4 px-4">
                      {d.status === "Running" ? (
                        <a 
                          href={`http://${d.domain}`} 
                          target="_blank" 
                          rel="noreferrer" 
                          className="text-indigo-400 hover:underline font-semibold flex items-center gap-1"
                        >
                          {d.domain}
                        </a>
                      ) : (
                        <span className="text-zinc-500">{d.domain}</span>
                      )}
                    </td>
                    <td className="py-4 px-4 font-mono text-xs text-zinc-500">{d.imageTag}</td>
                    <td className="py-4 px-4">{getStatusBadge(d.status)}</td>
                    <td className="py-4 px-4 text-xs font-mono">{getHealthBadge(d.healthStatus, d.healthScore)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>
    </div>
  );
}
