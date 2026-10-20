import { useState, useEffect, useCallback } from "react";

const BLOCK_SIZE = 259;
const HEADER_SIZE = 4;
const CLASS_NAMES = ["Defender", "Archer", "Healer"];
const CLASS_BLOCKS = [0, 2, 3];
const CLASS_COLORS = {
  Defender: { bg: "rgba(59,130,246,0.08)", border: "#3b82f6", accent: "#2563eb", badge: "#dbeafe" },
  Archer: { bg: "rgba(234,88,12,0.08)", border: "#ea580c", accent: "#c2410c", badge: "#fed7aa" },
  Healer: { bg: "rgba(22,163,74,0.08)", border: "#16a34a", accent: "#15803d", badge: "#bbf7d0" },
};

function parseSave(buffer) {
  const view = new DataView(buffer);
  const numInts = buffer.byteLength / 4;
  const header = {
    version: view.getUint32(0, true),
    level: view.getUint32(4, true),
    slot: view.getUint32(8, true),
    reserved: view.getUint32(12, true),
  };

  const classes = CLASS_BLOCKS.map((blockNum, idx) => {
    const start = (HEADER_SIZE + blockNum * BLOCK_SIZE) * 4;
    const numNodes = view.getUint32(start, true);
    const nodes = [];
    for (let i = 0; i < numNodes; i++) {
      const nodeId = view.getUint32(start + (1 + i * 2) * 4, true);
      const points = view.getUint32(start + (2 + i * 2) * 4, true);
      nodes.push({ nodeId, points });
    }
    const totalSpent = nodes.reduce((s, n) => s + n.points, 0);
    return {
      name: CLASS_NAMES[idx],
      blockNum,
      numNodes,
      nodes,
      totalSpent,
    };
  });

  return { header, classes, buffer: new Uint8Array(buffer) };
}

function buildModifiedSave(originalBuffer, classes, newLevel) {
  const buf = new ArrayBuffer(originalBuffer.byteLength);
  const src = new Uint8Array(originalBuffer);
  const dst = new Uint8Array(buf);
  dst.set(src);

  const view = new DataView(buf);
  view.setUint32(4, newLevel, true);

  classes.forEach((cls) => {
    const start = (HEADER_SIZE + cls.blockNum * BLOCK_SIZE) * 4;
    view.setUint32(start, cls.nodes.length, true);
    // Clear the old node data area
    for (let i = 1; i < 1 + 128 * 2; i++) {
      view.setUint32(start + i * 4, 0, true);
    }
    cls.nodes.forEach((node, i) => {
      view.setUint32(start + (1 + i * 2) * 4, node.nodeId, true);
      view.setUint32(start + (2 + i * 2) * 4, node.points, true);
    });
  });

  return buf;
}

function SkillTreeCard({ cls, color, onChange }) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div
      style={{
        background: color.bg,
        border: `1.5px solid ${color.border}`,
        borderRadius: 14,
        padding: "18px 20px",
        marginBottom: 14,
      }}
    >
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          cursor: "pointer",
        }}
        onClick={() => setExpanded(!expanded)}
      >
        <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
          <span
            style={{
              fontSize: 20,
              fontWeight: 800,
              color: color.accent,
              fontFamily: "'Courier New', monospace",
              letterSpacing: "-0.5px",
            }}
          >
            {cls.name}
          </span>
          <span
            style={{
              background: color.badge,
              color: color.accent,
              fontSize: 11,
              fontWeight: 700,
              padding: "3px 9px",
              borderRadius: 20,
              fontFamily: "monospace",
            }}
          >
            {cls.nodes.length} nodes
          </span>
        </div>
        <div style={{ display: "flex", alignItems: "center", gap: 14 }}>
          <span
            style={{
              fontSize: 26,
              fontWeight: 900,
              color: color.accent,
              fontFamily: "monospace",
            }}
          >
            {cls.totalSpent}
          </span>
          <span style={{ fontSize: 11, color: "#888", marginRight: 6 }}>
            pts spent
          </span>
          <span
            style={{
              transform: expanded ? "rotate(180deg)" : "rotate(0)",
              transition: "transform 0.2s",
              fontSize: 16,
              color: "#999",
            }}
          >
            ▼
          </span>
        </div>
      </div>

      {expanded && (
        <div style={{ marginTop: 14 }}>
          <div
            style={{
              display: "flex",
              gap: 4,
              padding: "6px 0 10px",
              borderBottom: "1px solid rgba(0,0,0,0.06)",
              marginBottom: 8,
              fontSize: 10,
              fontWeight: 700,
              color: "#999",
              textTransform: "uppercase",
              letterSpacing: "0.5px",
              fontFamily: "monospace",
            }}
          >
            <span style={{ width: 70 }}>Node ID</span>
            <span style={{ width: 80 }}>Points</span>
            <span>Actions</span>
          </div>
          <div style={{ maxHeight: 340, overflowY: "auto" }}>
            {cls.nodes.map((node, i) => (
              <div
                key={i}
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: 4,
                  padding: "4px 0",
                  borderBottom: "1px solid rgba(0,0,0,0.03)",
                }}
              >
                <span
                  style={{
                    width: 70,
                    fontFamily: "monospace",
                    fontSize: 13,
                    fontWeight: 600,
                    color: "#444",
                  }}
                >
                  #{node.nodeId}
                </span>
                <input
                  type="number"
                  min={1}
                  max={10}
                  value={node.points}
                  onChange={(e) => {
                    const val = Math.max(1, Math.min(10, parseInt(e.target.value) || 1));
                    onChange(cls.name, i, val);
                  }}
                  style={{
                    width: 56,
                    padding: "4px 6px",
                    border: `1px solid ${color.border}44`,
                    borderRadius: 6,
                    fontFamily: "monospace",
                    fontSize: 13,
                    fontWeight: 700,
                    color: color.accent,
                    background: "white",
                    textAlign: "center",
                    outline: "none",
                  }}
                />
                <button
                  onClick={() => onChange(cls.name, i, Math.min(10, node.points + 1))}
                  style={{
                    width: 26,
                    height: 26,
                    border: `1px solid ${color.border}44`,
                    borderRadius: 6,
                    background: "white",
                    cursor: "pointer",
                    fontWeight: 700,
                    fontSize: 14,
                    color: color.accent,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    lineHeight: 1,
                  }}
                >
                  +
                </button>
                <button
                  onClick={() => onChange(cls.name, i, Math.max(1, node.points - 1))}
                  style={{
                    width: 26,
                    height: 26,
                    border: `1px solid ${color.border}44`,
                    borderRadius: 6,
                    background: "white",
                    cursor: "pointer",
                    fontWeight: 700,
                    fontSize: 14,
                    color: color.accent,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    lineHeight: 1,
                  }}
                >
                  −
                </button>
              </div>
            ))}
          </div>
          <div style={{ marginTop: 10, display: "flex", gap: 8 }}>
            <button
              onClick={() => {
                cls.nodes.forEach((_, i) => onChange(cls.name, i, 3));
              }}
              style={{
                padding: "5px 12px",
                borderRadius: 6,
                border: `1px solid ${color.border}66`,
                background: "white",
                cursor: "pointer",
                fontSize: 11,
                fontWeight: 600,
                color: color.accent,
              }}
            >
              Set all → 3
            </button>
            <button
              onClick={() => {
                cls.nodes.forEach((_, i) => onChange(cls.name, i, 5));
              }}
              style={{
                padding: "5px 12px",
                borderRadius: 6,
                border: `1px solid ${color.border}66`,
                background: "white",
                cursor: "pointer",
                fontSize: 11,
                fontWeight: 600,
                color: color.accent,
              }}
            >
              Set all → 5
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default function TangyTDSaveEditor() {
  const [saveData, setSaveData] = useState(null);
  const [classes, setClasses] = useState([]);
  const [level, setLevel] = useState(0);
  const [originalBuffer, setOriginalBuffer] = useState(null);
  const [status, setStatus] = useState(null);
  const [dragOver, setDragOver] = useState(false);

  const loadFile = useCallback((file) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const buf = e.target.result;
      const parsed = parseSave(buf);
      setSaveData(parsed);
      setClasses(
        parsed.classes.map((c) => ({
          ...c,
          nodes: c.nodes.map((n) => ({ ...n })),
        }))
      );
      setLevel(parsed.header.level);
      setOriginalBuffer(buf);
      setStatus(null);
    };
    reader.readAsArrayBuffer(file);
  }, []);

  const handleNodeChange = useCallback((className, nodeIdx, newPoints) => {
    setClasses((prev) =>
      prev.map((cls) => {
        if (cls.name !== className) return cls;
        const newNodes = cls.nodes.map((n, i) =>
          i === nodeIdx ? { ...n, points: newPoints } : n
        );
        return {
          ...cls,
          nodes: newNodes,
          totalSpent: newNodes.reduce((s, n) => s + n.points, 0),
        };
      })
    );
  }, []);

  const handleDownload = () => {
    const modBuf = buildModifiedSave(originalBuffer, classes, level);
    const blob = new Blob([modBuf], { type: "application/octet-stream" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "backup.cake";
    a.click();
    URL.revokeObjectURL(url);
    setStatus("Downloaded! Replace your original backup.cake (keep a backup!)");
  };

  const totalAllSpent = classes.reduce((s, c) => s + c.totalSpent, 0);

  if (!saveData) {
    return (
      <div
        style={{
          minHeight: "100vh",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          background: "linear-gradient(145deg, #0f172a 0%, #1e293b 100%)",
          fontFamily: "'Courier New', Courier, monospace",
        }}
      >
        <div
          style={{
            textAlign: "center",
            padding: 48,
            maxWidth: 480,
          }}
        >
          <div
            style={{
              fontSize: 42,
              fontWeight: 900,
              color: "#f59e0b",
              marginBottom: 4,
              letterSpacing: "-2px",
              textShadow: "0 0 40px rgba(245,158,11,0.3)",
            }}
          >
            TANGY TD
          </div>
          <div
            style={{
              fontSize: 14,
              color: "#94a3b8",
              fontWeight: 600,
              letterSpacing: "3px",
              textTransform: "uppercase",
              marginBottom: 36,
            }}
          >
            Save File Editor
          </div>

          <div
            onDragOver={(e) => {
              e.preventDefault();
              setDragOver(true);
            }}
            onDragLeave={() => setDragOver(false)}
            onDrop={(e) => {
              e.preventDefault();
              setDragOver(false);
              if (e.dataTransfer.files[0]) loadFile(e.dataTransfer.files[0]);
            }}
            style={{
              border: `2px dashed ${dragOver ? "#f59e0b" : "#475569"}`,
              borderRadius: 16,
              padding: "42px 24px",
              background: dragOver
                ? "rgba(245,158,11,0.05)"
                : "rgba(255,255,255,0.02)",
              transition: "all 0.2s",
              cursor: "pointer",
            }}
            onClick={() => {
              const input = document.createElement("input");
              input.type = "file";
              input.accept = ".cake";
              input.onchange = (e) => {
                if (e.target.files[0]) loadFile(e.target.files[0]);
              };
              input.click();
            }}
          >
            <div style={{ fontSize: 36, marginBottom: 12 }}>🎂</div>
            <div style={{ color: "#e2e8f0", fontSize: 15, fontWeight: 700 }}>
              Drop your backup.cake here
            </div>
            <div
              style={{
                color: "#64748b",
                fontSize: 12,
                marginTop: 6,
              }}
            >
              or click to browse
            </div>
          </div>

          <div
            style={{
              marginTop: 28,
              color: "#475569",
              fontSize: 11,
              lineHeight: 1.7,
            }}
          >
            Typically found in your Steam save directory
            <br />
            Always keep a backup of your original file!
          </div>
        </div>
      </div>
    );
  }

  return (
    <div
      style={{
        minHeight: "100vh",
        background: "#f8fafc",
        fontFamily: "'Courier New', Courier, monospace",
        padding: "20px 16px",
      }}
    >
      <div style={{ maxWidth: 560, margin: "0 auto" }}>
        {/* Header */}
        <div
          style={{
            background: "linear-gradient(135deg, #0f172a, #1e293b)",
            borderRadius: 16,
            padding: "20px 24px",
            marginBottom: 18,
            color: "white",
          }}
        >
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
            <div>
              <span style={{ fontSize: 22, fontWeight: 900, color: "#f59e0b", letterSpacing: "-1px" }}>
                TANGY TD
              </span>
              <span style={{ fontSize: 11, color: "#94a3b8", marginLeft: 10, letterSpacing: "2px", textTransform: "uppercase" }}>
                Save Editor
              </span>
            </div>
            <div style={{ textAlign: "right" }}>
              <div style={{ fontSize: 10, color: "#64748b", textTransform: "uppercase", letterSpacing: "1px" }}>
                Version
              </div>
              <div style={{ fontSize: 16, fontWeight: 800, color: "#e2e8f0" }}>
                {saveData.header.version}
              </div>
            </div>
          </div>

          <div
            style={{
              display: "flex",
              gap: 24,
              marginTop: 16,
              paddingTop: 14,
              borderTop: "1px solid rgba(255,255,255,0.08)",
            }}
          >
            <div>
              <div style={{ fontSize: 10, color: "#64748b", textTransform: "uppercase", letterSpacing: "1px", marginBottom: 4 }}>
                Level
              </div>
              <input
                type="number"
                min={1}
                max={999}
                value={level}
                onChange={(e) => setLevel(Math.max(1, parseInt(e.target.value) || 1))}
                style={{
                  width: 70,
                  padding: "6px 8px",
                  border: "1px solid #334155",
                  borderRadius: 8,
                  background: "#1e293b",
                  color: "#f59e0b",
                  fontFamily: "monospace",
                  fontSize: 18,
                  fontWeight: 800,
                  textAlign: "center",
                  outline: "none",
                }}
              />
            </div>
            <div>
              <div style={{ fontSize: 10, color: "#64748b", textTransform: "uppercase", letterSpacing: "1px", marginBottom: 4 }}>
                Total Skill Points
              </div>
              <div style={{ fontSize: 26, fontWeight: 900, color: "#f59e0b" }}>
                {totalAllSpent}
              </div>
            </div>
            <div>
              <div style={{ fontSize: 10, color: "#64748b", textTransform: "uppercase", letterSpacing: "1px", marginBottom: 4 }}>
                Save Slot
              </div>
              <div style={{ fontSize: 18, fontWeight: 800, color: "#e2e8f0" }}>
                {saveData.header.slot}
              </div>
            </div>
          </div>
        </div>

        {/* Skill Trees */}
        {classes.map((cls) => (
          <SkillTreeCard
            key={cls.name}
            cls={cls}
            color={CLASS_COLORS[cls.name]}
            onChange={handleNodeChange}
          />
        ))}

        {/* Actions */}
        <div
          style={{
            display: "flex",
            gap: 10,
            marginTop: 8,
            marginBottom: 16,
          }}
        >
          <button
            onClick={handleDownload}
            style={{
              flex: 1,
              padding: "14px 20px",
              borderRadius: 12,
              border: "none",
              background: "linear-gradient(135deg, #f59e0b, #d97706)",
              color: "#0f172a",
              fontFamily: "monospace",
              fontSize: 14,
              fontWeight: 900,
              cursor: "pointer",
              letterSpacing: "1px",
              textTransform: "uppercase",
              boxShadow: "0 4px 14px rgba(245,158,11,0.3)",
            }}
          >
            Download Modified Save
          </button>
          <button
            onClick={() => {
              setSaveData(null);
              setClasses([]);
              setOriginalBuffer(null);
              setStatus(null);
            }}
            style={{
              padding: "14px 18px",
              borderRadius: 12,
              border: "1px solid #e2e8f0",
              background: "white",
              color: "#64748b",
              fontFamily: "monospace",
              fontSize: 12,
              fontWeight: 700,
              cursor: "pointer",
            }}
          >
            Reset
          </button>
        </div>

        {status && (
          <div
            style={{
              background: "#ecfdf5",
              border: "1px solid #6ee7b7",
              borderRadius: 10,
              padding: "10px 14px",
              fontSize: 12,
              color: "#065f46",
              fontWeight: 600,
            }}
          >
            {status}
          </div>
        )}

        <div
          style={{
            marginTop: 20,
            padding: "14px 16px",
            background: "#fffbeb",
            border: "1px solid #fde68a",
            borderRadius: 10,
            fontSize: 11,
            color: "#92400e",
            lineHeight: 1.6,
          }}
        >
          <strong>How it works:</strong> Each class (Defender, Archer, Healer) has
          its own skill tree stored as node ID + points pairs. Expand a class to
          edit individual node point values, or use the bulk buttons. The level
          value in the header may affect available skill points in-game. Always
          back up your original file before replacing it.
        </div>
      </div>
    </div>
  );
}
