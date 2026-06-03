<script lang="ts">

	import type { TableInfo } from '$lib/api/types';

	let {table, onDoubleClick, onMouseDown, coords}: {
		table: TableInfo,
		onDoubleClick: (e: MouseEvent) => void,
		onMouseDown: (e: MouseEvent, name: string) => void,
		coords: {x: number, y: number}
	} = $props();
</script>

<div
	class="table-card"
	style="left: {coords?.x ?? 0}px; top: {coords?.y ?? 0}px"
	onmousedown={(e) => onMouseDown(e, table.name)}
	ondblclick={onDoubleClick}
	role="presentation"
>
	<div class="table-header">{table.name}</div>
	<div class="table-body">
		{#each table.columns as col}
			<div class="col">
				<span class="name">
					{#if col.isPrimaryKey}
						<span class="badge pk">PK</span>
					{/if}
					{#if col.isForeignKey}
						<span class="badge fk">FK</span>
					{/if}
					{col.name}
				</span>
				<span class="type">{col.dataType?.toLowerCase?.() ?? ''}</span>
			</div>
		{/each}
	</div>
</div>


<style>
	.table-card{
			position: absolute;
			background: var(--color-background-primary);
			border: 1px solid var(--color-border-tertiary);
			border-radius: 10px;
			width: 250px;
			box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
			z-index: 5;
			transition: border-color 0.15s, box-shadow 0.15s;
			cursor: default;
	}

	.table-card:hover{
			border-color: var(--color-border-info);
			box-shadow: 0 12px 36px rgba(0, 0, 0, 0.4);
	}

	.table-card:active{
			cursor: grabbing;
			z-index: 50;
	}

	.table-header{
			padding: 0.7rem 1rem;
			background: rgba(255, 255, 255, 0.03);
			border-bottom: 1px solid var(--color-border-tertiary);
			font-weight: bold;
			color: var(--color-text-info);
			border-radius: 10px 10px 0 0;
	}

	.col{
			display: flex;
			justify-content: space-between;
			align-items: center;
			padding: 0.4rem 1rem;
			font-size: 0.8rem;
			gap: 0.4rem;
	}

	.name{
			color: var(--color-text-primary);
			display: flex;
			align-items: center;
			gap: 0.3rem;
			flex: 1;
	}

	.type{
			color: var(--color-text-secondary);
			font-size: 0.7rem;
			font-style: italic;
			white-space: nowrap;
	}

	.badge{
			font-size: 0.6rem;
			font-weight: 700;
			padding: 0.05rem 0.3rem;
			border-radius: 3px;
			font-family: monospace;
	}

	.pk{
      background: rgba(250,204,21,0.15);
			color: #fbbf24;
			border: 1px solid rgba(250,204,21,0.3);
	}

	.fk{
			background: rgba(52,211,153,0.12);
      color: #34d399;
      border: 1px solid rgba(52,211,153,0.3);
	}

</style>