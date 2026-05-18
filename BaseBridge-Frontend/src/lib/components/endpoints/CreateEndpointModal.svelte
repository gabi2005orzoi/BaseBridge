<script lang="ts">
	import Modal from '$lib/components/ui/Modal.svelte';
	import Spinner from '$lib/components/ui/Spinner.svelte';
	import { endpointsStore } from '$lib/stores/endpoint.svelte';
	import { ui } from '$lib/stores/ui.svelte';
	import type { EndpointParameter } from '$lib/api/types';

	let name = $state('');
	let description = $state('');
	let parameters = $state<EndpointParameter[]>([]);
	let isPaginationMandatory = $state(true);

	const PARAM_TYPES = ['string', 'int', 'decimal', 'bool', 'date'];

	function addParameter(){
		parameters = [...parameters, {name: '', type: 'string'}];
	}

	function removeParameter(index: number){
		parameters = parameters.filter((_, i) => i !==index);
	}

	async function handleGenerate() {
		if (!name || !description) {
			ui.error('Please fill in both fields!');
			return;
		}

		const generated = await endpointsStore.generate({
			name,
			endpointDescription: description,
			parameters: parameters.filter(p => p.name.trim() !== ''),
			isPaginationMandatory
		});

		if (generated) {
			ui.openModal('review-query');
		} else {
			ui.error(endpointsStore.error || 'An error occurred while generating the AI query.');
		}
	}
</script>

<Modal title="Add a New Endpoint" onclose={() => ui.closeModal()}>
	<div class="form">
		<div class="form-group">
			<label for="name">Endpoint Name (no spaces)</label>
			<input
				type="text"
				id="name"
				bind:value={name}
				placeholder="ex: getActiveUsers"
			/>
		</div>

		<div class="form-group">
			<label for="description">What data does it return?</label>
			<textarea
				id="description"
				bind:value={description}
				rows="4"
				placeholder="e.g., Returns all users with the status 'active' and age over 18 from the Users table..."
			></textarea>
		</div>

		<div class="form-group checkbox-group">
			<label class="flex-label">
				<input type="checkbox" bind:checked={isPaginationMandatory}>
				<span>Require Pagination (Recommended for large datasets)</span>
			</label>
		</div>

		<div class="form-group">
			<label>Parameters (optional)</label>
			{#each parameters as param, i (i)}
				<div class="param-row">
					<input
						type="text"
						placeholder="Parameter name (ex: userId)"
						bind:value={param.name}
					/>
					<select bind:value={param.type}>
						{#each PARAM_TYPES as t (t)}
							<option value={t}>{t}</option>
						{/each}
					</select>
					<button class="btn-remove" onclick={() => removeParameter(i)}>✕</button>
				</div>
			{/each}
			<button class="btn-add-param" onclick={addParameter}>+ Add Parameter</button>
		</div>

	</div>

	{#snippet footer()}
		<button class="btn-cancel" onclick={() => ui.closeModal()}>
			Cancel
		</button>

		<button
			class="btn-primary"
			onclick={handleGenerate}
			disabled={endpointsStore.generating || !name || !description}
		>
			{#if endpointsStore.generating}
				<Spinner size="sm" /> Generate with AI...
			{:else}
				<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
					<path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/>
				</svg>
				Generate Query
			{/if}
		</button>
	{/snippet}
</Modal>

<style>
    .form { display: flex; flex-direction: column; gap: 1.25rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
    .form-group label { font-size: 0.9rem; font-weight: 500; color: var(--color-text-primary); }
    input, textarea { padding: 0.6rem; border: 1px solid var(--color-border-secondary); border-radius: var(--border-radius-md); background: var(--color-background-primary); color: var(--color-text-primary); font-family: var(--font-sans); resize: vertical; }
    input:focus, textarea:focus { outline: 2px solid var(--color-text-info); outline-offset: -1px; border-color: transparent; }

    /* Parameters section */
    .param-row {
        display: grid;
        grid-template-columns: 1fr 120px 32px;
        gap: 0.5rem;
        align-items: center;
        background: var(--color-background-secondary);
        border: 1px solid var(--color-border-tertiary);
        border-radius: var(--border-radius-md);
        padding: 0.5rem 0.6rem;
    }
    .param-row input {
        border: none;
        background: transparent;
        padding: 0.2rem 0.4rem;
        font-size: 0.875rem;
        font-family: var(--font-mono);
        color: var(--color-text-info);
    }
    .param-row input:focus {
        outline: none;
        background: rgba(255,255,255,0.04);
        border-radius: 4px;
    }
    .param-row input::placeholder { color: var(--color-text-secondary); font-family: var(--font-sans); }
    .param-row select {
        padding: 0.25rem 0.4rem;
        border: 1px solid var(--color-border-secondary);
        border-radius: var(--border-radius-md);
        background: var(--color-background-primary);
        color: var(--color-text-secondary);
        font-size: 0.8rem;
        cursor: pointer;
    }
    .btn-remove {
        background: transparent;
        border: none;
        color: var(--color-text-secondary);
        cursor: pointer;
        font-size: 0.85rem;
        padding: 0.2rem;
        border-radius: 4px;
        line-height: 1;
        transition: color 0.15s, background 0.15s;
    }
    .btn-remove:hover { color: #f87171; background: rgba(248,113,113,0.1); }
    .btn-add-param {
        align-self: flex-start;
        margin-top: 0.25rem;
        background: transparent;
        border: 1px dashed var(--color-border-secondary);
        color: var(--color-text-info);
        padding: 0.4rem 0.9rem;
        border-radius: var(--border-radius-md);
        cursor: pointer;
        font-size: 0.8rem;
        font-weight: 500;
        transition: background 0.15s, border-color 0.15s;
    }
    .btn-add-param:hover { background: rgba(99,179,237,0.08); border-color: var(--color-text-info); }

    .btn-cancel { background: transparent; color: var(--color-text-secondary); border: 1px solid var(--color-border-tertiary); padding: 0.6rem 1rem; border-radius: var(--border-radius-md); cursor: pointer; font-weight: 500; }
    .btn-cancel:hover { background: var(--color-background-secondary); color: var(--color-text-primary); }

    .btn-primary { background: var(--color-text-info); color: white; border: none; padding: 0.6rem 1.2rem; border-radius: var(--border-radius-md); cursor: pointer; font-weight: 500; display: inline-flex; align-items: center; gap: 0.5rem; transition: filter 0.2s; }
    .btn-primary:hover:not(:disabled) { filter: brightness(1.1); }
    .btn-primary:disabled { opacity: 0.7; cursor: not-allowed; }

		.checkbox-group{
				margin-top: 0.5rem;
		}

		.flex-label{
				display: flex;
				align-items: center;
				gap: 0.6rem;
				font-size: 0.9rem;
				cursor: pointer;
				color: var(--color-text-primary);
				font-weight: 500;
		}

		.flex-label input[type="checkbox"]{
				width: 1.1rem;
				height: 1.1rem;
				cursor: pointer;
				accent-color: var(--color-text-info);
		}
</style>