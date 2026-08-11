"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { ArrowLeft, Terminal, CheckCircle2, Play, ExternalLink, RefreshCw, Cpu, ShieldAlert } from "lucide-react";

interface LogMessage {
  time: string;
  source: "Control" | "ContextBuilder" | "GeminiPro" | "Compiler" | "Playwright" | "Deployment" | "Self-Healing";
  message: string;
  type: "info" | "success" | "warning" | "error";
}

export default function BuildMonitor() {
  const [currentStep, setCurrentStep] = useState(0);
  const [logs, setLogs] = useState<LogMessage[]>([]);
  const [isBuilding, setIsBuilding] = useState(true);

  const steps = [
    "Context Construction",
    "Gemini Pro Code Scaffolding",
    "Project Compiler Checks",
    "xUnit Integration Testing",
    "Playwright UI Validations",
    "SonarQube Static Analysis",
    "Nginx Route Map & Live Host"
  ];

  const rawLogTemplates: LogMessage[] = [
    { time: "11:40:02", source: "Control", message: "Initializing manufacturing pipeline for job df_job_9981...", type: "info" },
    { time: "11:40:05", source: "ContextBuilder", message: "Extracting schemas models. Context maps compiled (4,210 tokens inputs).", type: "info" },
    { time: "11:40:08", source: "GeminiPro", message: "Manufacturing C# Clean Architecture solution controllers & migrations (48,200 tokens out)...", type: "success" },
    { time: "11:40:15", source: "Compiler", message: "Invoking sandboxed build compiler runtime 'dotnet build'...", type: "info" },
    { time: "11:40:18", source: "Compiler", message: "ERROR: Entity 'Order.cs' line 12: Namespace missing DB relational reference.", type: "error" },
    { time: "11:40:19", source: "Self-Healing", message: "Diagnosing stack trace. Feeding errors back to Gemini Repair Agent...", type: "warning" },
    { time: "11:40:22", source: "GeminiPro", message: "Resolving syntax bindings in entity mapping configurations...", type: "success" },
    { time: "11:40:24", source: "Compiler", message: "Re-compiling backend solution structure... Build Succeeded (0 Errors, 0 Warnings).", type: "success" },
    { time: "11:40:26", source: "Compiler", message: "Building frontend Next.js App Router static optimization layers...", type: "info" },
    { time: "11:40:30", source: "Playwright", message: "Launching Playwright browser container. Executing 12 end-to-end CRUD tests...", type: "info" },
    { time: "11:40:34", source: "Playwright", message: "SUCCESS: 12/12 visual assertions passed (Customers list, Add Orders, Forms validation).", type: "success" },
    { time: "11:40:35", source: "Control", message: "Invoking SonarQube quality checks... Security rating A, 0 Vulnerabilities.", type: "success" },
    { time: "11:40:36", source: "Deployment", message: "Assembling ARM64 container image. Syncing local proxy rules...", type: "info" },
    { time: "11:40:38", source: "Deployment", message: "Container is active! URL mapped: http://acmeportal.local.devforge", type: "success" }
  ];

  useEffect(() => {
    let logIndex = 0;
    const interval = setInterval(() => {
      if (logIndex < rawLogTemplates.length) {
        const nextLog = rawLogTemplates[logIndex];
        setLogs((prev) => [...prev, nextLog]);

        // Map logs index to UI steps progression
        if (logIndex === 1) setCurrentStep(1);
        if (logIndex === 3) setCurrentStep(2);
        if (logIndex === 7) setCurrentStep(3);
        if (logIndex === 9) setCurrentStep(4);
        if (logIndex === 11) setCurrentStep(5);
        if (logIndex === 13) {
          setCurrentStep(6);
          setIsBuilding(false);
        }

        logIndex++;
      } else {
        clearInterval(interval);
      }
    }, 1800); // Stream logs smoothly every 1.8s

    return () => clearInterval(interval);
  }, []);

  const getLogTypeStyle = (type: LogMessage["type"]) => {
    switch (type) {
      case "success": return "text-emerald-400";
      case "warning": return "text-amber-400";
      case "error": return "text-rose-500 font-bold";
      default: return "text-zinc-400";
    }
  };

  return (
    <div className="min-h-screen bg-[#050508] text-zinc-100 font-sans p-6 md:p-12 flex flex-col lg:flex-row gap-8">
      {/* Left Panel: Build Status Stepper */}
      <div className="w-full lg:w-[360px] space-y-6 shrink-0">
        <Link href="/" className="flex items-center gap-2 text-zinc-400 hover:text-zinc-100 transition-colors text-sm mb-6">
          <ArrowLeft className="w-4 h-4" /> Back to Panel
        </Link>

        <div className="bg-[#0b0c14] border border-zinc-800/80 p-6 rounded-2xl shadow-xl">
          <h2 className="text-lg font-bold mb-6 flex items-center gap-2">
            <Cpu className="w-5 h-5 text-indigo-400 animate-spin" /> Manufacturing Steps
          </h2>
          
          <div className="space-y-6 relative before:absolute before:left-3.5 before:top-2 before:bottom-2 before:w-[1px] before:bg-zinc-800">
            {steps.map((step, idx) => {
              const isActive = idx === currentStep;
              const isPassed = idx < currentStep;
              
              return (
                <div key={idx} className="flex items-start gap-4 relative">
                  <div className={`w-7 h-7 rounded-full flex items-center justify-center font-bold text-xs border transition-all ${
                    isPassed 
                      ? "bg-emerald-500/20 border-emerald-500 text-emerald-400"
                      : isActive
                        ? "bg-indigo-500/20 border-indigo-500 text-indigo-300 animate-pulse"
                        : "bg-[#050508] border-zinc-800 text-zinc-500"
                  }`}>
                    {isPassed ? "✓" : idx + 1}
                  </div>
                  <div>
                    <h3 className={`text-sm font-semibold transition-colors ${
                      isActive ? "text-indigo-400" : isPassed ? "text-zinc-300" : "text-zinc-500"
                    }`}>{step}</h3>
                    <p className="text-[10px] text-zinc-500 mt-0.5">
                      {isPassed ? "Complete" : isActive ? "Processing..." : "Pending"}
                    </p>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Right Panel: Streaming Console Log & Live URL */}
      <div className="flex-1 flex flex-col gap-6">
        {/* Success Card / Launch */}
        {!isBuilding && (
          <div className="bg-emerald-950/20 border-2 border-emerald-900/60 p-6 rounded-2xl shadow-2xl animate-fade-in flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
            <div>
              <h2 className="text-xl font-extrabold text-emerald-400 flex items-center gap-2">
                <CheckCircle2 className="w-6 h-6" /> System Manufactured Successfully!
              </h2>
              <p className="text-zinc-300 text-sm mt-1">All pipelines validated. DB migration active, SSO authentication active.</p>
              
              <div className="mt-3 text-xs bg-zinc-950/50 p-3 rounded-lg border border-emerald-900/30 max-w-md font-mono text-zinc-400">
                <span className="block text-emerald-500 font-semibold mb-1">Superuser Credentials:</span>
                Admin User: <span className="text-zinc-200">admin@devforge.local</span> <br />
                Password: <span className="text-zinc-200">DF_Admin_9981</span>
              </div>
            </div>

            <a 
              href="http://acmeportal.local.devforge" 
              target="_blank" 
              rel="noopener noreferrer" 
              className="flex items-center gap-2 px-6 py-3.5 bg-gradient-to-r from-emerald-600 to-teal-600 hover:from-emerald-500 hover:to-teal-500 transition-all font-bold text-sm rounded-lg shadow-xl text-white whitespace-nowrap"
            >
              Launch Platform <ExternalLink className="w-4 h-4" />
            </a>
          </div>
        )}

        {/* Live Build Output Terminal Console */}
        <div className="flex-1 bg-[#050508] border border-zinc-800/80 rounded-2xl shadow-2xl flex flex-col overflow-hidden min-h-[480px]">
          <div className="bg-[#0b0c14]/60 px-6 py-3 border-b border-zinc-900 flex justify-between items-center shrink-0">
            <span className="text-xs font-bold text-zinc-300 flex items-center gap-2">
              <Terminal className="w-4 h-4 text-purple-400" /> Build Log Output console
            </span>
            {isBuilding && (
              <span className="text-[10px] text-zinc-500 flex items-center gap-1.5 animate-pulse font-semibold">
                <RefreshCw className="w-3.5 h-3.5 animate-spin" /> RUNNING COMPILATION...
              </span>
            )}
          </div>

          <div className="flex-1 p-6 font-mono text-xs overflow-y-auto space-y-3 bg-[#050508]/40">
            {logs.map((log, idx) => (
              <div key={idx} className="flex gap-4 border-b border-zinc-900/40 pb-2">
                <span className="text-zinc-600 select-none shrink-0">{log.time}</span>
                <span className="text-indigo-400 font-bold shrink-0">[{log.source}]</span>
                <span className={getLogTypeStyle(log.type)}>{log.message}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
