<script lang="ts">
	let {sampleData, tableName, onClose}: {
		sampleData: Record<string, any>[],
		tableName: string,
		onClose: () => void
	} = $props();
</script>

<div class="modal-backdrop" role="presentation" onclick={onClose}>
	<div class="modal-content" role="presentation" onclick={e => e.stopPropagation()}>
		<div class="modal-top">
			<h2>{tableName} - Sample Data</h2>
			<button class="close-btn" onclick={onClose}>&times;</button>
		</div>
		<div class="scroll-area">
			{#if sampleData?.length}
				<table>
					<thead>
						<tr>
							{#each Object.keys(sampleData[0] || {}) as k}
								<th>{k}</th>
							{/each}
						</tr>
					</thead>
					<tbody>
						{#each sampleData as row}
							<tr>
								{#each Object.values(row) as v}
									<td>{v}</td>
								{/each}
							</tr>
						{/each}
					</tbody>
				</table>
			{:else}
				<p class="no-data">No sample data available</p>
			{/if}
		</div>
	</div>
</div>


<style>
	.modal-backdrop{
			position: fixed;
			inset: 0;
			background: rgba(0,0,0,0.85);
			display: flex;
			justify-content: center;
			align-items: center;
			z-index: 100;
			backdrop-filter: blur(8px);
	}

	.modal-content{
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

	.modal-top{
			display: flex;
			justify-content: space-between;
			align-items: center;
			margin-bottom: 0.5rem;
	}

	.modal-top h2{
			margin: 0;
			color: var(--color-text-info);
			font-size: 1.1rem;
	}

	.close-btn{
			background: none;
			border: none;
			color: var(--color-text-primary);
			font-size: 2rem;
			cursor: pointer;
			line-height: 1;
	}

	.scroll-area{
			overflow: auto;
			border: 1px solid var(--color-border-tertiary);
			border-radius: 8px;
			margin-top: 1rem;
			flex: 1;
	}

	table{
			width: 100%;
			border-collapse: collapse;
	}

	th, td{
			padding: 0.8rem;
			text-align: left;
			border: 1px solid var(--color-border-tertiary);
			font-size: 0.8rem;
	}

	th{
			color: var(--color-text-info);
			position: sticky;
			top: 0;
			background: var(--color-background-primary);
	}

	td{
			color: var(--color-text-primary);
	}

	.no-data {
			text-align: center;
			padding: 2rem;
			color: var(--color-text-secondary);
	}

</style>