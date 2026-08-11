"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { ArrowLeft, Save, Play, Plus, Trash2, Key, Database, Settings2 } from "lucide-react";

interface Attribute {
  name: string;
  type: string;
  isNullable: boolean;
  isPrimaryKey: boolean;
  isForeignKey: boolean;
  references?: string;
}

interface Entity {
  name: string;
  tableName: string;
  attributes: Attribute[];
}

export default function SchemaBuilder() {
  const router = useRouter();
  const params = useParams();
  const projectId = params?.id as string;

  const [entities, setEntities] = useState<Entity[]>([
    {
      name: "Customer",
      tableName: "customers",
      attributes: [
        { name: "Id", type: "uuid", isNullable: false, isPrimaryKey: true, isForeignKey: false },
        { name: "Name", type: "varchar", isNullable: false, isPrimaryKey: false, isForeignKey: false },
        { name: "Email", type: "varchar", isNullable: false, isPrimaryKey: false, isForeignKey: false }
      ]
    },
    {
      name: "Order",
      tableName: "orders",
      attributes: [
        { name: "Id", type: "uuid", isNullable: false, isPrimaryKey: true, isForeignKey: false },
        { name: "CustomerId", type: "uuid", isNullable: false, isPrimaryKey: false, isForeignKey: true, references: "Customer" },
        { name: "Amount", type: "decimal", isNullable: false, isPrimaryKey: false, isForeignKey: false }
      ]
    }
  ]);

  const [selectedEntityIndex, setSelectedEntityIndex] = useState<number>(0);
  const [newAttrName, setNewAttrName] = useState("");
  const [newAttrType, setNewAttrType] = useState("varchar");
  const [newAttrNullable, setNewAttrNullable] = useState(true);

  const addEntity = () => {
    const newEntity: Entity = {
      name: "NewTable",
      tableName: "new_tables",
      attributes: [
        { name: "Id", type: "uuid", isNullable: false, isPrimaryKey: true, isForeignKey: false }
      ]
    };
    setEntities([...entities, newEntity]);
    setSelectedEntityIndex(entities.length);
  };

  const addAttribute = () => {
    if (!newAttrName.trim()) return;
    const updated = [...entities];
    updated[selectedEntityIndex].attributes.push({
      name: newAttrName,
      type: newAttrType,
      isNullable: newAttrNullable,
      isPrimaryKey: false,
      isForeignKey: false
    });
    setEntities(updated);
    setNewAttrName("");
  };

  const removeAttribute = (attrIndex: number) => {
    const updated = [...entities];
    updated[selectedEntityIndex].attributes.splice(attrIndex, 1);
    setEntities(updated);
  };

  const handleGenerate = () => {
    router.push(`/projects/${projectId}/build`);
  };

  return (
    <div className="min-h-screen bg-[#050508] text-zinc-100 font-sans flex flex-col h-screen">
      {/* Top Header Controls */}
      <header className="flex justify-between items-center px-6 py-4 border-b border-zinc-800/80 bg-[#0b0c14]/40 shrink-0">
        <div className="flex items-center gap-4">
          <Link href="/" className="p-2 hover:bg-zinc-900 border border-zinc-800 rounded-lg transition-colors">
            <ArrowLeft className="w-4 h-4 text-zinc-400" />
          </Link>
          <div>
            <h1 className="text-lg font-bold">Visual Schema Editor</h1>
            <p className="text-zinc-500 text-xs">Project Schema (v1.2)</p>
          </div>
        </div>
        
        <div className="flex gap-3">
          <button className="flex items-center gap-2 px-4 py-2 border border-zinc-800 hover:border-zinc-700 bg-zinc-900/50 font-semibold rounded-lg text-sm transition-colors">
            <Save className="w-4 h-4" /> Save Draft
          </button>
          <button onClick={handleGenerate} className="flex items-center gap-2 px-4 py-2 bg-gradient-to-r from-indigo-600 to-purple-600 hover:from-indigo-500 hover:to-purple-500 font-semibold rounded-lg text-sm shadow-lg shadow-indigo-950/40 transition-all">
            <Play className="w-4 h-4 fill-current" /> Manufacture System
          </button>
        </div>
      </header>

      {/* Grid Canvas Section */}
      <div className="flex-1 flex overflow-hidden">
        {/* Left Side: Tables List */}
        <aside className="w-64 border-r border-zinc-900/80 bg-[#050508]/80 p-6 flex flex-col justify-between overflow-y-auto shrink-0">
          <div className="space-y-4">
            <h2 className="text-xs uppercase tracking-wider font-semibold text-zinc-500 flex items-center justify-between">
              Database Tables
              <button onClick={addEntity} className="p-1 hover:bg-zinc-900 border border-zinc-800 rounded text-zinc-300">
                <Plus className="w-3.5 h-3.5" />
              </button>
            </h2>
            <div className="space-y-1.5">
              {entities.map((e, idx) => (
                <button
                  key={idx}
                  onClick={() => setSelectedEntityIndex(idx)}
                  className={`w-full text-start px-3 py-2.5 rounded-lg text-sm font-semibold flex items-center justify-between transition-colors ${
                    selectedEntityIndex === idx
                      ? "bg-indigo-950/30 text-indigo-400 border border-indigo-950"
                      : "hover:bg-zinc-900 text-zinc-400"
                  }`}
                >
                  <span className="flex items-center gap-2">
                    <Database className="w-4 h-4" /> {e.name}
                  </span>
                  <span className="text-[10px] text-zinc-600">{e.attributes.length} fields</span>
                </button>
              ))}
            </div>
          </div>
        </aside>

        {/* Center Canvas: Selected Entity Table Visualizer */}
        <main className="flex-1 bg-[#050508] p-8 flex items-center justify-center overflow-auto">
          {entities[selectedEntityIndex] && (
            <div className="w-[380px] bg-[#0b0c14] border-2 border-indigo-950 rounded-2xl overflow-hidden shadow-2xl">
              <div className="bg-indigo-950/20 px-5 py-4 border-b border-indigo-950 flex justify-between items-center">
                <div>
                  <h3 className="font-extrabold text-zinc-200">{entities[selectedEntityIndex].name}</h3>
                  <p className="text-[10px] font-mono text-indigo-400">table: {entities[selectedEntityIndex].tableName}</p>
                </div>
              </div>

              <div className="p-4 space-y-2">
                {entities[selectedEntityIndex].attributes.map((attr, idx) => (
                  <div key={idx} className="flex justify-between items-center p-2 hover:bg-zinc-900/60 rounded-md text-xs font-mono text-zinc-300 group">
                    <div className="flex items-center gap-2">
                      {attr.isPrimaryKey && <Key className="w-3.5 h-3.5 text-amber-500" />}
                      <span className={attr.isPrimaryKey ? "font-bold text-zinc-100" : ""}>{attr.name}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className="text-[10px] text-zinc-500 uppercase">{attr.type}</span>
                      {attr.isNullable && <span className="text-[9px] text-zinc-600 font-semibold">[NULL]</span>}
                      {attr.references && <span className="text-[9px] text-indigo-400 border border-indigo-800 px-1 rounded">FK: {attr.references}</span>}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </main>

        {/* Right Side: Columns & Constraints Config Panel */}
        {entities[selectedEntityIndex] && (
          <aside className="w-80 border-l border-zinc-900/80 bg-[#0b0c14]/30 p-6 flex flex-col justify-between overflow-y-auto shrink-0">
            <div className="space-y-6">
              <h2 className="text-sm font-bold flex items-center gap-2 text-zinc-300 border-b border-zinc-900 pb-3">
                <Settings2 className="w-4 h-4 text-indigo-500" /> Table Editor: {entities[selectedEntityIndex].name}
              </h2>

              {/* Attributes Config List */}
              <div className="space-y-4">
                <h3 className="text-xs text-zinc-500 font-semibold uppercase tracking-wider">Field Editor</h3>
                <div className="space-y-2 max-h-[300px] overflow-y-auto pr-1">
                  {entities[selectedEntityIndex].attributes.map((attr, idx) => (
                    <div key={idx} className="flex items-center justify-between p-2.5 bg-zinc-900/40 border border-zinc-800 rounded-lg text-xs">
                      <div>
                        <span className="font-semibold text-zinc-200">{attr.name}</span>
                        <span className="text-[10px] text-zinc-500 block">{attr.type}</span>
                      </div>
                      {!attr.isPrimaryKey && (
                        <button onClick={() => removeAttribute(idx)} className="p-1 hover:bg-rose-950/30 text-rose-500 rounded transition-all">
                          <Trash2 className="w-3.5 h-3.5" />
                        </button>
                      )}
                    </div>
                  ))}
                </div>
              </div>

              {/* Add New Attribute */}
              <div className="space-y-4 border-t border-zinc-900 pt-6">
                <h3 className="text-xs text-zinc-500 font-semibold uppercase tracking-wider">Add Field</h3>
                
                <div className="space-y-3">
                  <input
                    type="text"
                    value={newAttrName}
                    onChange={(e) => setNewAttrName(e.target.value)}
                    placeholder="Field name (e.g. Price)"
                    className="w-full bg-[#050508] border border-zinc-800 focus:border-indigo-500 rounded px-3 py-2 text-xs text-zinc-100 outline-none placeholder-zinc-600"
                  />

                  <div className="flex gap-2">
                    <select
                      value={newAttrType}
                      onChange={(e) => setNewAttrType(e.target.value)}
                      className="flex-1 bg-[#050508] border border-zinc-800 text-xs text-zinc-300 rounded px-2 py-2 outline-none"
                    >
                      <option value="varchar">Varchar</option>
                      <option value="integer">Integer</option>
                      <option value="decimal">Decimal</option>
                      <option value="boolean">Boolean</option>
                      <option value="datetime">Datetime</option>
                      <option value="uuid">UUID</option>
                    </select>

                    <label className="flex items-center gap-2 px-3 bg-zinc-900 border border-zinc-800 rounded text-xs text-zinc-400 cursor-pointer select-none">
                      <input
                        type="checkbox"
                        checked={newAttrNullable}
                        onChange={(e) => setNewAttrNullable(e.target.checked)}
                        className="accent-indigo-500"
                      />
                      Null
                    </label>
                  </div>

                  <button
                    onClick={addAttribute}
                    className="w-full py-2 bg-zinc-900 hover:bg-zinc-800 font-semibold border border-zinc-800 text-xs rounded transition-all"
                  >
                    Add Column
                  </button>
                </div>
              </div>
            </div>
          </aside>
        )}
      </div>
    </div>
  );
}
