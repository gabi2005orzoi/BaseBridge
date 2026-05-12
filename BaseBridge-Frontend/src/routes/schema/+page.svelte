<script lang="ts">
	import { onMount } from 'svelte';
	import { adminApi } from '$lib/api/endpoint';

	// ─── State ────────────────────────────────────────────────────────────────
	let schema = $state<{ tables: any[] } | null>(null);
	let loading = $state(true);
	let scale = $state(1);

	// Pan
	let panX = $state(0);
	let panY = $state(0);
	let isPanning = $state(false);
	let panStart = { x: 0, y: 0 };

	// Track mousedown time to distinguish single vs double click
	let lastMouseDownTime = 0;

	// Drag tables
	let positions = $state<Record<string, { x: number; y: number }>>({});
	let draggingTable = $state<string | null>(null);
	let dragOffset = { x: 0, y: 0 };

	// Modal
	let showModal = $state(false);
	let selectedTable = $state<any>(null);

	// ─── Load ────────────────────────────────────────────────────────────────
	// onMount must be synchronous to return a cleanup fn; async logic is in loadSchema()
	onMount(() => {
		window.addEventListener('wheel', handleWheel, { passive: false });
		loadSchema();
		return () => window.removeEventListener('wheel', handleWheel);
	});

	async function loadSchema() {
		try {
			schema = await adminApi.getDatabaseSchema();
			if (schema?.tables) {
				schema.tables.forEach((t, i) => {
					const cols = Math.ceil(Math.sqrt(schema!.tables.length + 1));
					positions[t.name] = {
						x: 80 + (i % cols) * 320,
						y: 80 + Math.floor(i / cols) * 380
					};
				});
			}
		} catch (e) {
			console.error(e);
		} finally {
			loading = false;
		}
	}

	// ─── Zoom ─────────────────────────────────────────────────────────────────
	function clampScale(v: number) {
		return Math.min(Math.max(v, 0.15), 4);
	}

	function handleWheel(e: WheelEvent) {
		e.preventDefault();
		if (e.ctrlKey) {
			const zoomFactor = 0.005;
			scale = clampScale(scale - e.deltaY * zoomFactor * scale);
		} else {
			// Pan with scroll when not zooming: multiply for better speed
			panX -= e.deltaX * 1.2;
			panY -= e.deltaY * 1.2;
		}
	}

	function handleKeyDown(e: KeyboardEvent) {
		if (!e.ctrlKey) return;
		if (e.key === '+' || e.key === '=') { e.preventDefault(); scale = clampScale(scale + 0.1); }
		else if (e.key === '-') { e.preventDefault(); scale = clampScale(scale - 0.1); }
		else if (e.key === '0') { e.preventDefault(); scale = 1; panX = 0; panY = 0; }
	}

	// ─── Pan ─────────────────────────────────────────────────────────────────
	function canvasMouseDown(e: MouseEvent) {
		// Only left mouse button
		if (e.button !== 0) return;
		// Suppress pan on double-click: if two mousedowns happen within 300ms, skip
		const now = Date.now();
		const isDoubleClick = (now - lastMouseDownTime) < 300;
		lastMouseDownTime = now;
		if (isDoubleClick) return;

		// Don't pan if clicking on a card or UI element
		const target = e.target as HTMLElement;
		if (target.closest('.table-card')) return;
		if (target.closest('.zoom-controls')) return;

		isPanning = true;
		panStart = { x: e.clientX - panX, y: e.clientY - panY };
	}

	// ─── Table Drag ──────────────────────────────────────────────────────────
	function tableMouseDown(e: MouseEvent, name: string) {
		e.stopPropagation();
		draggingTable = name;
		dragOffset = {
			x: e.clientX / scale - panX / scale - positions[name].x,
			y: e.clientY / scale - panY / scale - positions[name].y
		};
	}

	// ─── Global Mouse Move / Up ───────────────────────────────────────────────
	function handleMouseMove(e: MouseEvent) {
		if (draggingTable) {
			positions[draggingTable] = {
				x: e.clientX / scale - panX / scale - dragOffset.x,
				y: e.clientY / scale - panY / scale - dragOffset.y
			};
		} else if (isPanning) {
			panX = e.clientX - panStart.x;
			panY = e.clientY - panStart.y;
		}
	}

	function handleMouseUp() {
		draggingTable = null;
		isPanning = false;
	}

	// ─── Relationships ───────────────────────────────────────────────────────
	const CARD_W = 250;
	const CARD_H_BASE = 44;
	const COL_H = 32;

	function cardHeight(table: any): number {
		return CARD_H_BASE + (table.columns?.length ?? 0) * COL_H - 23;
	}

	function getRelPath(fromTable: any, toName: string, relType: string) {
		const fp = positions[fromTable.name];
		const tp = positions[toName];
		if (!fp || !tp) return null;

		const fH = cardHeight(fromTable);
		const toTable = schema?.tables.find(t => t.name === toName);
		const tH = toTable ? cardHeight(toTable) : 200;

		const fCx = fp.x + CARD_W / 2;
		const fCy = fp.y + fH / 2;
		const tCx = tp.x + CARD_W / 2;
		const tCy = tp.y + tH / 2;

		function port(px: number, py: number, ph: number, cx: number, cy: number) {
			const dx = cx - (px + CARD_W / 2);
			const dy = cy - (py + ph / 2);
			if (Math.abs(dx) > Math.abs(dy)) {
				return dx > 0
					? { x: px + CARD_W, y: py + ph / 2 }
					: { x: px, y: py + ph / 2 };
			} else {
				return dy > 0
					? { x: px + CARD_W / 2, y: py + ph }
					: { x: px + CARD_W / 2, y: py };
			}
		}

		const src = port(fp.x, fp.y, fH, tCx, tCy);
		const dst = port(tp.x, tp.y, tH, fCx, fCy);
		const cp = Math.min(Math.abs(dst.x - src.x), Math.abs(dst.y - src.y), 120) + 40;

		function ctrlDir(px: number, py: number, ph: number, cx: number, cy: number) {
			const ddx = cx - (px + CARD_W / 2);
			const ddy = cy - (py + ph / 2);
			if (Math.abs(ddx) > Math.abs(ddy)) {
				return ddx > 0 ? { x: cp, y: 0 } : { x: -cp, y: 0 };
			} else {
				return ddy > 0 ? { x: 0, y: cp } : { x: 0, y: -cp };
			}
		}

		const c1 = ctrlDir(fp.x, fp.y, fH, tCx, tCy);
		const c2 = ctrlDir(tp.x, tp.y, tH, fCx, fCy);

		const d = `M ${src.x} ${src.y} C ${src.x + c1.x} ${src.y + c1.y}, ${dst.x + c2.x} ${dst.y + c2.y}, ${dst.x} ${dst.y}`;

		function bezier(t: number, p0: number, p1: number, p2: number, p3: number) {
			const mt = 1 - t;
			return mt * mt * mt * p0 + 3 * mt * mt * t * p1 + 3 * mt * t * t * p2 + t * t * t * p3;
		}
		const labelX = bezier(0.5, src.x, src.x + c1.x, dst.x + c2.x, dst.x);
		const labelY = bezier(0.5, src.y, src.y + c1.y, dst.y + c2.y, dst.y);

		return { d, labelX, labelY, type: relType || 'one-to-many' };
	}
</script>

<svelte:window
	onkeydown={handleKeyDown}
	onmousemove={handleMouseMove}
	onmouseup={handleMouseUp}
/>

<!-- Outer wrapper: full viewport minus the 60px app shell above -->
<div class="schema-root">

	<!-- ── Header ─────────────────────────────────────────────────────────── -->
	<div class="header-overlay">
		<div class="header-left">
			<h1>Database Visualizer</h1>
			<p>Double-click table for sample data</p>
		</div>
		<div class="legend">
			<span class="leg-item leg-1n">1:N</span>
			<span class="leg-item leg-11">1:1</span>
			<span class="leg-item leg-nm">N:M</span>
		</div>
	</div>

	<!-- ── Canvas (sits strictly below header) ────────────────────────────── -->
	<div
		class="schema-canvas"
		onmousedown={canvasMouseDown}
		role="presentation"
		style:cursor={isPanning ? 'grabbing' : draggingTable ? 'grabbing' : 'grab'}
	>
		{#if loading}
			<div class="loader">Generating…</div>
		{:else}
			<!-- zoom + pan layer -->
			<div
				class="zoom-layer"
				style="transform: translate({panX}px, {panY}px) scale({scale}); transform-origin: 0 0;"
			>
				<!-- Relationship lines -->
				<svg class="lines-layer">
					<defs>
						<marker id="crow" markerWidth="16" markerHeight="16" refX="11" refY="8" orient="auto">
							<path d="M 1 8 L 13 2 M 1 8 L 13 14 M 1 8 L 13 8" stroke="var(--color-text-info)" fill="none" stroke-width="1.8"/>
						</marker>
						<marker id="one-bar" markerWidth="10" markerHeight="16" refX="1" refY="8" orient="auto">
							<line x1="2" y1="1" x2="2" y2="15" stroke="var(--color-text-info)" stroke-width="2.5"/>
						</marker>
						<marker id="crow-pink" markerWidth="16" markerHeight="16" refX="11" refY="8" orient="auto">
							<path d="M 1 8 L 13 2 M 1 8 L 13 14 M 1 8 L 13 8" stroke="#f472b6" fill="none" stroke-width="1.8"/>
						</marker>
						<marker id="one-bar-pink" markerWidth="10" markerHeight="16" refX="1" refY="8" orient="auto">
							<line x1="2" y1="1" x2="2" y2="15" stroke="#f472b6" stroke-width="2.5"/>
						</marker>
						<marker id="one-bar-violet" markerWidth="10" markerHeight="16" refX="1" refY="8" orient="auto">
							<line x1="2" y1="1" x2="2" y2="15" stroke="#a78bfa" stroke-width="2.5"/>
						</marker>
						<marker id="one-bar-violet-end" markerWidth="10" markerHeight="16" refX="9" refY="8" orient="auto">
							<line x1="8" y1="1" x2="8" y2="15" stroke="#a78bfa" stroke-width="2.5"/>
						</marker>
					</defs>

					{#if schema}
						{#each schema.tables as table}
							{#each table.relationships as rel}
								{@const rp = getRelPath(table, rel.referencedTable, rel.type)}
								{#if rp}
									{@const isOO = rp.type === 'one-to-one'}
									{@const isMM = rp.type === 'many-to-many'}
									{@const color = isOO ? '#a78bfa' : isMM ? '#f472b6' : 'var(--color-text-info)'}
									<path
										d={rp.d}
										stroke={color}
										stroke-width="2"
										fill="none"
										opacity="0.45"
										marker-start={isOO ? "url(#one-bar-violet)" : isMM ? "url(#crow-pink)" : "url(#one-bar)"}
										marker-end={isOO ? "url(#one-bar-violet-end)" : isMM ? "url(#crow-pink)" : "url(#crow)"}
									/>
									<!-- Relation type label -->
									<rect
										x={rp.labelX - 16}
										y={rp.labelY - 9}
										width="32"
										height="17"
										rx="4"
										fill="#0d1117"
										stroke={color}
										stroke-width="1"
										opacity="0.9"
									/>
									<text
										x={rp.labelX}
										y={rp.labelY + 4}
										text-anchor="middle"
										font-size="9"
										font-family="monospace"
										fill={color}
										font-weight="600"
									>{isOO ? '1:1' : isMM ? 'N:M' : '1:N'}</text>
								{/if}
							{/each}
						{/each}
					{/if}
				</svg>

				<!-- Table cards -->
				{#if schema}
					{#each schema.tables as table}
						<div
							class="table-card"
							style="left: {positions[table.name]?.x ?? 0}px; top: {positions[table.name]?.y ?? 0}px;"
							onmousedown={(e) => tableMouseDown(e, table.name)}
							ondblclick={(e) => { e.stopPropagation(); selectedTable = table; showModal = true; }}
							role="presentation"
						>
							<div class="table-header">{table.name}</div>
							<div class="table-body">
								{#each table.columns as col}
									<div class="col">
										<span class="name">
											{#if col.isPrimaryKey}<span class="badge pk">PK</span>{/if}
											{#if col.isForeignKey}<span class="badge fk">FK</span>{/if}
											{col.name}
										</span>
										<span class="type">{col.dataType?.toLowerCase?.() ?? ''}</span>
									</div>
								{/each}
							</div>
						</div>
					{/each}
				{/if}
			</div>
		{/if}

		<!-- Zoom controls -->
		<div class="zoom-controls">
			<button onclick={() => scale = clampScale(scale + 0.1)}>+</button>
			<div class="zoom-level">{Math.round(scale * 100)}%</div>
			<button onclick={() => scale = clampScale(scale - 0.1)}>−</button>
			<button class="reset-btn" onclick={() => { scale = 1; panX = 0; panY = 0; }}>Reset</button>
		</div>
	</div>
</div>

<!-- ── Modal ──────────────────────────────────────────────────────────────── -->
{#if showModal && selectedTable}
	<div class="modal-backdrop" onclick={() => showModal = false} role="presentation">
		<div class="modal-content" onclick={e => e.stopPropagation()} role="presentation">
			<div class="modal-top">
				<h2>{selectedTable.name} — Sample Data</h2>
				<button class="close-btn" onclick={() => showModal = false}>&times;</button>
			</div>
			<div class="scroll-area">
				{#if selectedTable.sampleData?.length}
					<table>
						<thead>
						<tr>{#each Object.keys(selectedTable.sampleData[0] || {}) as k}<th>{k}</th>{/each}</tr>
						</thead>
						<tbody>
						{#each selectedTable.sampleData as row}
							<tr>{#each Object.values(row) as v}<td>{v}</td>{/each}</tr>
						{/each}
						</tbody>
					</table>
				{:else}
					<p class="no-data">No sample data available.</p>
				{/if}
			</div>
		</div>
	</div>
{/if}

<style>
    /* ── Root wrapper ────────────────────────────────────────────────────── */
    .schema-root {
        display: flex;
        flex-direction: column;
        width: 100%;
        height: calc(100vh - 70px - 4rem);
        overflow: hidden;
        position: relative;
        /* Disable browser back/forward navigation gestures on touchpad */
        overscroll-behavior-x: none;
    }

    /* ── Header — fixed height, never overlapped ─────────────────────────── */
    .header-overlay {
        flex-shrink: 0;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0.75rem 1.5rem;
        background: var(--color-background-secondary);
        border-bottom: 1px solid var(--color-border-tertiary);
        z-index: 5; /* Keep below global header (z-index 10) */
    }
    .header-left h1 {
        margin: 0;
        font-size: 1.4rem;
        color: var(--color-text-primary);
    }
    .header-left p {
        margin: 0.15rem 0 0;
        font-size: 0.78rem;
        color: var(--color-text-secondary);
    }
    .header-left strong { color: var(--color-text-info); }

    /* Legend chips */
    .legend { display: flex; gap: 0.5rem; align-items: center; }
    .leg-item {
        font-size: 0.7rem;
        font-weight: 700;
        padding: 0.2rem 0.55rem;
        border-radius: 4px;
        font-family: monospace;
        letter-spacing: 0.04em;
    }
    .leg-1n { background: rgba(var(--color-text-info-rgb, 96,165,250), 0.12); color: var(--color-text-info); border: 1px solid rgba(96,165,250,0.3); }
    .leg-11 { background: rgba(167,139,250,0.12); color: #a78bfa; border: 1px solid rgba(167,139,250,0.3); }
    .leg-nm { background: rgba(244,114,182,0.12); color: #f472b6; border: 1px solid rgba(244,114,182,0.3); }

    /* ── Canvas — takes remaining space, clips content ───────────────────── */
    .schema-canvas {
        flex: 1;
        position: relative;
        overflow: hidden;
        background-color: var(--color-background-secondary);
    }

    /* ── Zoom / Pan layer ────────────────────────────────────────────────── */
    .zoom-layer {
        position: absolute;
        top: 0; left: 0;
        width: 100%; height: 100%;
        will-change: transform;
        /* Moving background here ensures it pans and zooms with the tables */
        background-image: radial-gradient(rgba(255,255,255,0.05) 1px, transparent 1px);
        background-size: 40px 40px;
    }
    .lines-layer {
        position: absolute;
        top: 0; left: 0;
        width: 10000px; height: 10000px;
        pointer-events: none;
        overflow: visible;
    }

    /* ── Table Card ──────────────────────────────────────────────────────── */
    .table-card {
        position: absolute;
        width: 250px;
        background: var(--color-background-primary);
        border: 1px solid var(--color-border-tertiary);
        border-radius: 10px;
        box-shadow: 0 10px 30px rgba(0,0,0,0.3);
        cursor: grab;
        z-index: 5;
        transition: border-color 0.15s, box-shadow 0.15s;
    }
    .table-card:hover {
        border-color: var(--color-border-info);
        box-shadow: 0 12px 36px rgba(0,0,0,0.4);
    }
    .table-card:active { cursor: grabbing; z-index: 50; }

    .table-header {
        padding: 0.7rem 1rem;
        background: rgba(255,255,255,0.03);
        border-bottom: 1px solid var(--color-border-tertiary);
        font-weight: bold;
        color: var(--color-text-info);
        border-radius: 10px 10px 0 0;
    }

    .col {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 0.4rem 1rem;
        font-size: 0.8rem;
        gap: 0.4rem;
    }
    .name { color: var(--color-text-primary); display: flex; align-items: center; gap: 0.3rem; flex: 1; }
    .type { color: var(--color-text-secondary); font-size: 0.7rem; font-style: italic; white-space: nowrap; }

    .badge {
        font-size: 0.6rem;
        font-weight: 700;
        padding: 0.05rem 0.3rem;
        border-radius: 3px;
        font-style: normal;
        font-family: monospace;
    }
    .pk { background: rgba(250,204,21,0.15); color: #fbbf24; border: 1px solid rgba(250,204,21,0.3); }
    .fk { background: rgba(52,211,153,0.12); color: #34d399; border: 1px solid rgba(52,211,153,0.3); }

    /* ── Zoom Controls ───────────────────────────────────────────────────── */
    .zoom-controls {
        position: absolute;
        bottom: 2rem;
        right: 2rem;
        z-index: 200;
        background: var(--color-background-primary);
        padding: 0.4rem;
        border-radius: 8px;
        border: 1px solid var(--color-border-tertiary);
        display: flex;
        align-items: center;
        gap: 0.5rem;
        box-shadow: 0 4px 15px rgba(0,0,0,0.3);
    }
    .zoom-controls button {
        background: var(--color-background-secondary);
        border: none;
        color: var(--color-text-primary);
        width: 30px;
        height: 30px;
        border-radius: 4px;
        cursor: pointer;
        font-weight: bold;
        font-size: 1rem;
    }
    .zoom-controls button:hover { background: var(--color-background-info); color: white; }
    .zoom-level {
        min-width: 50px;
        text-align: center;
        font-size: 0.85rem;
        font-weight: bold;
        color: var(--color-text-info);
    }
    .reset-btn { width: auto !important; padding: 0 0.75rem !important; }

    /* ── Loader ──────────────────────────────────────────────────────────── */
    .loader {
        position: absolute;
        top: 50%; left: 50%;
        transform: translate(-50%, -50%);
        color: var(--color-text-info);
        font-size: 1.2rem;
    }

    /* ── Modal ───────────────────────────────────────────────────────────── */
    .modal-backdrop {
        position: fixed;
        inset: 0;
        background: rgba(0,0,0,0.85);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1000;
        backdrop-filter: blur(8px);
    }
    .modal-content {
        background: var(--color-background-primary);
        width: 95%;
        max-width: 1200px;
        max-height: 80vh;
        border-radius: 16px;
        padding: 2rem;
        border: 1px solid var(--color-border-info);
        display: flex;
        flex-direction: column;
    }
    .modal-top {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 0.5rem;
    }
    .modal-top h2 { margin: 0; color: var(--color-text-info); font-size: 1.1rem; }
    .close-btn {
        background: none;
        border: none;
        color: var(--color-text-primary);
        font-size: 2rem;
        cursor: pointer;
        line-height: 1;
    }
    .scroll-area {
        overflow: auto;
        border: 1px solid var(--color-border-tertiary);
        border-radius: 8px;
        margin-top: 1rem;
        flex: 1;
    }
    table { width: 100%; border-collapse: collapse; }
    th, td { padding: 0.8rem; text-align: left; border: 1px solid var(--color-border-tertiary); font-size: 0.8rem; }
    th { color: var(--color-text-info); position: sticky; top: 0; background: var(--color-background-primary); }
    td { color: var(--color-text-primary); }
    .no-data { text-align: center; padding: 2rem; color: var(--color-text-secondary); }
</style>
