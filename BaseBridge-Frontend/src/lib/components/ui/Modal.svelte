<script lang="ts">
	import { ui } from '$lib/stores/ui.svelte';
	import type { Snippet } from 'svelte';

	interface Props {
		title: string;
		children: Snippet;
		footer?: Snippet;
		width?: 'sm' | 'md' | 'lg';
		onclose?: () => void;
	}

	const {
		title,
		children,
		footer,
		width = 'md',
		onclose,
	}: Props = $props();

	const widths = {
		sm: '380px',
		md: '520px',
		lg: '720px',
	};

	function handleClose() {
		onclose?.();
		ui.closeModal();
	}

	function handleBackdropClick(e: MouseEvent) {
		// Închide doar dacă userul a dat click pe backdrop, nu pe conținut
		if (e.target === e.currentTarget) handleClose();
	}

	function handleKeydown(e: KeyboardEvent) {
		if (e.key === 'Escape') handleClose();
	}
</script>

<svelte:window onkeydown={handleKeydown} />

<!-- Backdrop -->
<div
	class="backdrop"
	role="dialog"
	aria-modal="true"
	aria-labelledby="modal-title"
	onclick={handleBackdropClick}
>
	<!-- Fereastra modală -->
	<div class="modal" style="max-width: {widths[width]}">

		<!-- Header -->
		<div class="modal__header">
			<h2 id="modal-title" class="modal__title">{title}</h2>
			<button
				class="modal__close"
				onclick={handleClose}
				aria-label="Închide"
			>
				<svg width="16" height="16" viewBox="0 0 16 16" fill="none">
					<path d="M4 4l8 8M12 4l-8 8" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
				</svg>
			</button>
		</div>

		<!-- Conținut — injectat din componenta părinte -->
		<div class="modal__body">
			{@render children()}
		</div>

		<!-- Footer opțional — butoane de acțiune -->
		{#if footer}
			<div class="modal__footer">
				{@render footer()}
			</div>
		{/if}

	</div>
</div>

<style>
    .backdrop {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.4);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
        padding: 1rem;
        animation: fade-in 0.15s ease-out;
    }

    .modal {
        background: var(--color-background-primary);
        border: 0.5px solid var(--color-border-tertiary);
        border-radius: var(--border-radius-lg);
        width: 100%;
        animation: scale-in 0.15s ease-out;
        overflow: hidden;
    }

    .modal__header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 1.25rem 1.5rem 0;
    }

    .modal__title {
        font-size: 16px;
        font-weight: 500;
        margin: 0;
        color: var(--color-text-primary);
    }

    .modal__close {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 28px;
        height: 28px;
        border-radius: var(--border-radius-md);
        border: none;
        background: transparent;
        color: var(--color-text-secondary);
        cursor: pointer;
        transition: background 0.1s;
    }

    .modal__close:hover {
        background: var(--color-background-secondary);
        color: var(--color-text-primary);
    }

    .modal__body {
        padding: 1.25rem 1.5rem;
    }

    .modal__footer {
        padding: 0 1.5rem 1.25rem;
        display: flex;
        justify-content: flex-end;
        gap: 8px;
        border-top: 0.5px solid var(--color-border-tertiary);
        padding-top: 1rem;
        margin-top: -0.25rem;
    }

    @keyframes fade-in {
        from { opacity: 0; }
        to { opacity: 1; }
    }

    @keyframes scale-in {
        from { opacity: 0; transform: scale(0.97) translateY(-4px); }
        to { opacity: 1; transform: scale(1) translateY(0); }
    }
</style>