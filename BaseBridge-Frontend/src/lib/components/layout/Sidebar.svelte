<script lang="ts">
	import { page } from '$app/stores';

	let isHomeActive = $derived(String($page.url.pathname) === '/');
	let isConfigActive = $derived(String($page.url.pathname).includes('/config'));
</script>

<aside class="sidebar">
	<div class="sidebar__brand">
		<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
			<ellipse cx="12" cy="5" rx="9" ry="3"/>
			<path d="M21 12c0 1.66-4 3-9 3s-9-1.34-9-3"/>
			<path d="M3 5v14c0 1.66 4 3 9 3s9-1.34 9-3V5"/>
		</svg>
		<span class="brand-text">BaseBridge</span>
	</div>

	<nav class="sidebar__nav">
		<button class="nav-link" class:active={isHomeActive} onclick={() => window.location.href = '/'}>
			<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
				<rect x="3" y="3" width="7" height="7"/>
				<rect x="14" y="3" width="7" height="7"/>
				<rect x="14" y="14" width="7" height="7"/>
				<rect x="3" y="14" width="7" height="7"/>
			</svg>
			Dashboard
		</button>

		<button class="nav-link" class:active={isConfigActive} onclick={() => window.location.href = '/config'}>
			<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
				<circle cx="12" cy="12" r="3"/>
				<path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"/>
			</svg>
			Configuration
		</button>
	</nav>
</aside>

<style>
    .sidebar { width: 260px; background: var(--color-background-primary); border-right: 1px solid var(--color-border-tertiary); display: flex; flex-direction: column; height: 100vh; position: sticky; top: 0; }
    .sidebar__brand {
        height: 70px;
        padding: 0 1.5rem;
        display: flex;
        align-items: center;
        gap: 0.75rem;
        border-bottom: 1px solid var(--color-border-tertiary);
        color: var(--color-text-info);
    }
    .brand-text { font-weight: 700; font-size: 1.25rem; color: var(--color-text-primary); letter-spacing: -0.5px; }
    .sidebar__nav { padding: 1.5rem 1rem; display: flex; flex-direction: column; gap: 0.5rem; }

    .nav-link {
        display: flex;
        align-items: center;
        gap: 0.75rem;
        padding: 0.75rem 1rem;
        text-decoration: none;
        color: var(--color-text-secondary);
        border-radius: var(--border-radius-md);
        font-weight: 500;
        transition: all 0.2s ease;
        background: transparent;
        border: none;
        width: 100%;
        font-family: inherit;
        font-size: 1rem;
        cursor: pointer;
        text-align: left;
    }
    .nav-link:hover { background: var(--color-background-secondary); color: var(--color-text-primary); }
    .nav-link.active { background: var(--color-background-info); color: var(--color-text-info); }
</style>