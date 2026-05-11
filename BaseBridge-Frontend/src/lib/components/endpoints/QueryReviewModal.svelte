<script lang="ts">
	import Modal from '$lib/components/ui/Modal.svelte';
	import Spinner from '$lib/components/ui/Spinner.svelte';
	import { endpointsStore } from '$lib/stores/endpoint.svelte';
	import { ui } from '$lib/stores/ui.svelte';
	import type { EndpointParameter } from '$lib/api/types';

	const PARAM_TYPES = ['string', 'int', 'decimal', 'bool', 'date'];
	let editableQuery = $state(endpointsStore.pendingQuery || '');
	let editableParameters = $state<EndpointParameter[]>((endpointsStore.pendingParameters ?? []).map(p => ({ ...p })));

	function addParam(){
		editableParameters = [...editableParameters, {name: '', type: 'string'}];
	}

	function removeParam(index: number) {
		editableParameters = editableParameters.filter((_, i) => i !== index);
	}

	function handleCancel() {
		endpointsStore.clearPending();
		ui.closeModal();
	}

	async function handleSave() {
		const req = endpointsStore.pendingRequest;
		if (!req || !editableQuery.trim()) return;

		const pathSlug = req.name.toLowerCase().trim().replace(/[^a-z0-9]+/g, '-');

		await endpointsStore.confirmAndSave({
			name: req.name,
			path: pathSlug,
			endpointDescription: req.endpointDescription,
			validatedQuery: editableQuery,
			parameters: editableParameters.filter(p => p.name.trim() !== '')
		});

		if (!endpointsStore.error) {
			ui.success('Endpoint saved and activated successfully!');
			ui.closeModal();
		} else {
			ui.error(endpointsStore.error);
		}
	}
</script>

<Modal title="SQL Query Review" width="lg" onclose={handleCancel}>
	<div class="review-content">
		<div class="alert-info">
			<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
				<path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/>
				<line x1="12" y1="9" x2="12" y2="13"/>
				<line x1="12" y1="17" x2="12.01" y2="17"/>
			</svg>
			<p>For security reasons, make sure the script below extracts the data correctly and uses only commands<strong>SELECT</strong>.</p>
		</div>

		<div class="form-group">
			<label for="sql-editor">AI-generated script:</label>
			<textarea
				id="sql-editor"
				class="code-editor"
				bind:value={editableQuery}
				rows="8"
			></textarea>
		</div>

		<div class="form-group parameter-info">
			<label>Parameters (editable — will be required in the URL as ?name=value)</label>
				{#each editableParameters as param, i (i)}
					<div class="param-row">
						<input type="text" bind:value={param.name} placeholder="name" />
							<select bind:value={param.type}>
								{#each PARAM_TYPES as t (t)}
									<option value={t}>{t}</option>
								{/each}
							</select>
						<button class="btn-remove" onclick={() => removeParam(i)}>✕</button>
					</div>
				{/each}
			<button class="btn-add-param" onclick={addParam}>+ Add Parameter</button>
			</div>
	</div>

	{#snippet footer()}
		<button class="btn-cancel" onclick={handleCancel}>Cancel</button>

		<button
			class="btn-success"
			onclick={handleSave}
			disabled={endpointsStore.loading || !editableQuery.trim()}
		>
			{#if endpointsStore.loading}
				<Spinner size="sm" />
			{:else}
				Approve and Save
			{/if}
		</button>
	{/snippet}
</Modal>

<style>
    .review-content { display: flex; flex-direction: column; gap: 1.25rem; }

    .alert-info { display: flex; align-items: flex-start; gap: 0.75rem; background: var(--color-background-warning); border: 1px solid var(--color-border-warning); padding: 1rem; border-radius: var(--border-radius-md); color: var(--color-text-warning); }
    .alert-info p { margin: 0; font-size: 0.9rem; line-height: 1.4; }
    .alert-info strong { font-weight: 600; }

    .form-group { display: flex; flex-direction: column; gap: 0.5rem; }
    .form-group label { font-size: 0.9rem; font-weight: 500; color: var(--color-text-primary); }

    .code-editor { font-family: var(--font-mono); font-size: 0.9rem; padding: 1rem; background: #1e1e1e; color: #d4d4d4; border: 1px solid var(--color-border-tertiary); border-radius: var(--border-radius-md); resize: vertical; line-height: 1.5; }
    .code-editor:focus { outline: 2px solid var(--color-text-info); outline-offset: -1px; }

    /* Parameters section */
    .parameter-info { margin-top: 0.25rem; }
    .parameter-info > label {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        margin-bottom: 0.25rem;
    }
    .parameter-info > label::before {
        content: '';
        display: inline-block;
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: var(--color-text-info);
        flex-shrink: 0;
    }
    .param-row {
        display: grid;
        grid-template-columns: 1fr 120px 32px;
        gap: 0.5rem;
        align-items: center;
        background: var(--color-background-secondary);
        border: 1px solid var(--color-border-tertiary);
        border-radius: var(--border-radius-md);
        padding: 0.5rem 0.6rem;
        transition: border-color 0.15s;
    }
    .param-row:focus-within { border-color: var(--color-text-info); }
    .param-row input {
        border: none;
        background: transparent;
        padding: 0.2rem 0.4rem;
        font-size: 0.875rem;
        font-family: var(--font-mono);
        color: var(--color-text-info);
        min-width: 0;
    }
    .param-row input:focus { outline: none; }
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

    .btn-success { background: var(--color-text-success); color: white; border: none; padding: 0.6rem 1.2rem; border-radius: var(--border-radius-md); cursor: pointer; font-weight: 500; display: inline-flex; align-items: center; gap: 0.5rem; transition: filter 0.2s; }
    .btn-success:hover:not(:disabled) { filter: brightness(1.1); }
    .btn-success:disabled { opacity: 0.7; cursor: not-allowed; }
</style>