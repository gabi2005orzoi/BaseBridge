<script lang="ts">
	import type { DatabaseSchemaResponse, TableInfo } from '$lib/api/types';
	import { onMount } from 'svelte';
	import { adminApi } from '$lib/api/endpoint';
	import SampleDataModal from '$lib/components/db_schema/SampleDataModal.svelte';
	import TableCard from '$lib/components/db_schema/TableCard.svelte';

	// ─── State ────────────────────────────────────────────────────────────────
	let schema = $state<DatabaseSchemaResponse | null>(null);
	let loading = $state(true);
	let scale = $state(1);

	// Pan
	let panX = $state(0);
	let panY = $state(0);
	let isPanning = $state(false);
	let panStart = {x: 0, y: 0};

	// Track mousedown time to distinguish single vs double click
	let lastMouseTime = 0;

	// Drag tables
	let positions = $state<Record<string, {x: number, y: number}>>({});
	let draggingTable = $state<string | null> (null);
	let dragOffset = {x: 0, y: 0};

	// Modal
	let showModal = $state(false);
	let selectedTable = $state<TableInfo | null>(null);

	// ─── Load ────────────────────────────────────────────────────────────────
	onMount(() => {
		window.addEventListener('wheel', handleWheel, {passive: false});
		loadSchema();
		return () => window.removeEventListener('wheel', handleWheel);
	});

	async function loadSchema(){
		try{
			schema = await adminApi.getDatabaseSchema();
			if(schema?.tables){
				schema.tables.forEach((t, i) => {
					const cols = Math.ceil(Math.sqrt(schema!.tables.length + 1));
					positions[t.name] = {
						x: 80 + (i % cols) * 320,
						y: 80 + Math.floor(i / cols) * 380
					}
				});
			}
		} catch (e){
			console.error(e);
		} finally {
			loading = false;
		}
	}

	function clampScale(v: number){
		return Math.min(Math.max(v, 0.15), 4);
	}

	function handleWheel(e: WheelEvent){
		e.preventDefault();
		if(showModal)
			return
		if(e.ctrlKey){
			const zoomFactor = 0.005;
			scale = clampScale(scale - e.deltaY * zoomFactor * scale);
		} else {
			panX -= e.deltaX * 1.2;
			panY -= e.deltaY * 1.2;
		}
	}

	function handleKeyDown(e: KeyboardEvent){
		if(!e.ctrlKey) return;
		if(e.key === '+' || e.key === '='){
			e.preventDefault();
			scale = clampScale(scale + 0.1);
		}
		else if(e.key === '-'){
			e.preventDefault();
			scale = clampScale(scale - 0.1);
		}
		else if(e.key === '0'){
			e.preventDefault();
			scale = 1;
			panX = 0;
			panY = 0;
		}
	}

	// ─── Pan ─────────────────────────────────────────────────────────────────
	function canvasMouseDown(e: MouseEvent){
		if(e.button !== 0)
			return;
		const now = Date.now();
		const isDoubleClick = (now-lastMouseTime)<300;
		lastMouseTime = now;
		if(isDoubleClick)
			return;

		const target = e.target as HTMLElement;
		if(target.closest('.table-card'))
			return;
		if(target.closest('.zoom-controls'))
			return;

		isPanning = true;
		panStart = {
			x: e.clientX - panX,
			y: e.clientY - panY
		};
	}

	// ─── Table Drag ──────────────────────────────────────────────────────────
	function tableMouseDown(e: MouseEvent, name: string){
		e.stopPropagation();
		draggingTable = name;
		dragOffset = {
			x: e.clientX/scale - panX/scale - positions[name].x,
			y: e.clientY/scale - panY/scale - positions[name].y
		}
	}
	// ─── Global Mouse Move / Up ───────────────────────────────────────────────
	function handleMouseMove(e: MouseEvent){
		if(draggingTable){
			positions[draggingTable] = {
				x: e.clientX/scale - panX/scale - dragOffset.x,
				y: e.clientY/scale - panY/scale - dragOffset.y
			}
		} else if(isPanning){
			panX = e.clientX - panStart.x;
			panY = e.clientY - panStart.y;
		}
	}

	function handleMouseUp(){
		draggingTable = null;
		isPanning = false;
	}

	// ─── Relationships ───────────────────────────────────────────────────────
	const CARD_W = 250;
	const CARD_H_BASE = 44;
	const COL_H = 32;

	function cardHeight(table: TableInfo):  number{
		return CARD_H_BASE + (table.columns?.length ?? 0) * COL_H - 23;
	}

	function getRelPath(fromTable: TableInfo, toName: string, relType: string){
		const fp = positions[fromTable.name];
		const tp = positions[toName];

		if(!fp || !tp)
			return null;

		const fH = cardHeight(fromTable);
		const toTable = schema?.tables.find(t => t.name == toName);
		const tH = toTable ? cardHeight(toTable) : 200;

		const fCx = fp.x + CARD_W/2;
		const fCy = fp.y + fH/2;
		const tCx = tp.x + CARD_W/2;
		const tCy = tp.y + tH/2;

		function port(px: number, py: number, ph: number, cx: number, cy: number){
			const dx = cx - (px + CARD_W/2);
			const dy = cy - (py + ph/2);
			if(Math.abs(dx) > Math.abs(dy)){
				return dx > 0
					? {x: px + CARD_W, y: py + ph/2}
					: {x: px, y: py + ph/2};
			} else {
				return dy > 0
					? {x: px + CARD_W/2, y: py + ph}
					: {x: px + CARD_W/2, y: py};
			}
		}

		const src = port(fp.x, fp.y, fH, tCx, tCy);
		const dst = port(tp.x, tp.y, tH, fCx, fCy);
		const cp = Math.min(Math.abs(dst.x - src.x), Math.abs(dst.y - src.y), 120) + 40;

		function ctrlDir(px: number, py: number, ph: number, cx: number, cy: number){
			const ddx = cx - (px + CARD_W/2);
			const ddy = cy - (py + ph/2);
			if(Math.abs(ddx) > Math.abs(ddy)){
				return ddx > 0
					? {x: cp, y: 0}
					: {x: -cp, y: 0};
			} else {
				return ddy > 0
					? {x: 0, y:  cp}
					: {x: 0, y: -cp};
			}
		}

		const c1 = ctrlDir(fp.x, fp.y, fH, tCx, tCy);
		const c2 = ctrlDir(tp.x, tp.y, tH, fCx, fCy);

		const d = `M ${src.x} ${src.y} C ${src.x + c1.x} ${src.y + c1.y}, ${dst.x + c2.x} ${dst.y + c2.y}, ${dst.x} ${dst.y}`;
		function bezier(t: number, p0: number, p1: number, p2: number, p3: number){
			const mt = 1 - t;
			return mt * mt * mt * p0 + 3 * mt * mt * t * p1 + 3 * mt * t * t * p2 + t * t * t * p3;
		}

		const labelX = bezier(0.5, src.x, src.x + c1.x, dst.x + c2.x, dst.x);
		const labelY = bezier(0.5, src.y, src.y + c1.y, dst.y + c2.y, dst.y);

		return {d, labelX, labelY, type: relType || 'one-to-many'};
	}
</script>

<svelte:window
	onkeydown={handleKeyDown}
	onmousemove={handleMouseMove}
	onmouseup={handleMouseUp}
/>

<div class="schema-root">

	<!-- header -->
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

	<!-- canvas -->
	<div
		class="schema-canvas"
		onmousedown={canvasMouseDown}
		role="presentation"
		style:cursor={isPanning ? 'grabbing' : draggingTable ? 'grabbing' : 'grab'}
	>
		{#if loading}
			<div class="loader">Generating...</div>
		{:else}
			<!-- zoom  + pan layer -->
			<div class="zoom-layer" style="transform: translate({panX}px, {panY}px) scale({scale}); transform-origin: 0 0;">
				<!-- table cards -->
				{#if schema}
					{#each schema.tables as table}
						<TableCard
							table={table}
							onDoubleClick={(e) => {e.stopPropagation(); selectedTable = table; showModal = true}}
							onMouseDown={(e) => tableMouseDown(e, table.name)}
							coords={positions[table.name]}
						/>
					{/each}
				{/if}

				<!-- relationships -->
				<svg class="lines-layer">
					<defs>
						<marker id="crow" markerWidth="16" markerHeight="16" refX="1" refY="6.8" orient="auto">
							<path d="M 13 8 L 1 2 M 13 8 L 1 14 M 13 8 L 1 8" stroke="var(--color-text-info)" fill="none" stroke-width="1.8"/>
						</marker>
						<marker id="one-bar" markerWidth="10" markerHeight="16" refX="1" refY="9" orient="auto">
							<line x1="2" y1="1" x2="2" y2="15" stroke="var(--color-text-info)" stroke-width="2.5"/>
						</marker>
						<marker id="crow-pink" markerWidth="16" markerHeight="16" refX="13" refY="6.8" orient="auto">
							<path d="M 13 8 L 1 2 M 13 8 L 1 14 M 13 8 L 1 8" stroke="#f472b6" fill="none" stroke-width="1.8"/>
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
										marker-start={isOO ? "url(#one-bar-violet-end)" : isMM ? "url(#crow-pink)" : "url(#crow)"}
										marker-end={isOO ? "url(#one-bar-violet)" : isMM ? "url(#crow-pink)" : "url(#one-bar)"}
									/>
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
										y={rp.labelY}
										text-anchor="middle"
										font-size="9"
										font-family="monospace"
										fill={color}
										font-weight="600"
									>{isOO ? '1:1': isMM ? 'N:M' : '1:N'}</text>
								{/if}
							{/each}
						{/each}
					{/if}

				</svg>
			</div>
		{/if}

		{#if !showModal}
			<div class="zoom-controls">
				<button onclick={() => scale = clampScale(scale + 0.1)}>+</button>
				<div class="zoom-level">{Math.round(scale * 100)}%</div>
				<button onclick={() => scale = clampScale(scale - 0.1)}>-</button>
				<button class="reset-btn" onclick={() => {scale = 1; panX = 0; panY = 0;}}>Reset</button>
			</div>
		{/if}
	</div>
</div>

<!-- Sample data modal -->
{#if showModal && selectedTable}
	<SampleDataModal sampleData={selectedTable.sampleData} tableName={selectedTable.name} onClose={() => showModal = false}></SampleDataModal>
{/if}

<style>
	.schema-root{
			display: flex;
			flex-direction: column;
			width: 100%;
			height: calc(100vh - 70px - 4rem);
			overflow: hidden;
			position: relative;
			overscroll-behavior-x: none;
	}

	.header-overlay{
			flex-shrink: 0;
			display: flex;
			align-items: center;
			justify-content: space-between;
			padding: 0.75rem 1.5rem;
			background: var(--color-background-secondary);
			border-bottom: 1px solid var(--color-border-tertiary);
			z-index: 5;
	}

	.header-left h1{
			margin: 0;
			font-size: 1.4rem;
			color: var(--color-text-primary);
	}

	.header-left p{
			margin: 0.15rem 0 0;
			font-size: 0.8rem;
			color: var(--color-text-secondary);
	}

	.legend{
			display: flex;
			gap: 0.5rem;
			align-items: center;
	}

	.leg-item{
			font-size: 0.7rem;
			font-weight: 700;
			padding: 0.2rem 0.55rem;
			border-radius: 4px;
			font-family: monospace;
			letter-spacing: 0.04em;
	}

	.leg-1n{
			background: rgba(var(--color-text-info-rgb, 96, 165, 250), 0.12);
			color: var(--color-text-info);
			border: 1px solid rgba(96, 165, 250, 0.3);
	}

	.leg-11{
			background: rgba(167, 139, 250, 0.12);
			color: #a78bfa;
			border: 1px solid rgba(167, 139, 250, 0.3);
	}

	.leg-nm{
			background: rgba(244, 114, 182, 0.12);
			color: #f472b6;
			border: 1px solid rgba(244, 114, 182, 0.3);
	}

	.schema-canvas{
			flex: 1;
			position: relative;
			overflow: hidden;
			background-color: var(--color-background-secondary);
	}

	.zoom-layer{
			position: absolute;
			top: 0;
			left: 0;
			width: 100%;
			height: 100%;
			will-change: transform;
			background-image: radial-gradient(rgba(255, 255, 255, 0.05) 1px, transparent 1px);
			background-size: 40px 40px;
	}

	.lines-layer{
			position: absolute;
			top: 0;
			left: 0;
			width: 10000px;
			height: 10000px;
			pointer-events: none;
			overflow: visible;
	}

	/* zoom controls*/
	.zoom-controls{
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
			box-shadow: 0 4px 15px rgba(0, 0, 0, 0.3);
	}

	.zoom-controls button{
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

	.zoom-controls button:hover{
			background: var(--color-background-info);
			color: white;
	}

	.zoom-level{
			min-width: 50px;
			text-align: center;
			font-size: 0.85rem;
			font-weight: bold;
			color: var(--color-text-info);
	}

	.reset-btn{
			width: auto !important;
			padding: 0 0.75rem !important;
	}

	/* loader */
	.loader{
			position: absolute;
			top: 50%;
			left: 50%;
			transform: translate(-50%, -50%);
			color: var(--color-text-info);
			font-size: 1.2rem;
	}

</style>

