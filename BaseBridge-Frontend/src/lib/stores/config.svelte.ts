import type { AiConfig, DbConfig } from '$lib/api/types';
import { adminApi } from '$lib/api/endpoint';

function createConfigStore() {
	let dbConfig = $state<DbConfig | null>(null);
	let aiConfig = $state<AiConfig | null>(null);
	let isDbConfigured = $state(typeof window !== 'undefined' ? localStorage.getItem('isDbConfigured') === 'true' : false);
	let loadingDb = $state(false);
	let loadingAi = $state(false);
	let error = $state<string | null>(null);

	const aiProvider = $derived(aiConfig?.provider ?? 'groq');
	const hasCustomAi = $derived(aiConfig !== null && !['groq', 'deepseek', 'gemini'].includes(aiConfig.provider));

	async function saveDb(config: DbConfig) {
		loadingDb = true;
		error = null;
		try {
			await adminApi.saveDbConfig(config);
			dbConfig = config;
			isDbConfigured = true;
			if (typeof window !== 'undefined') {
				localStorage.setItem('isDbConfigured', 'true');
			}
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to save DB configuration';
			throw e;
		} finally {
			loadingDb = false;
		}
	}

	async function saveAi(config: AiConfig) {
		loadingAi = true;
		error = null;
		try {
			await adminApi.saveAiConfig(config);
			aiConfig = config;
		} catch (e) {
			error = e instanceof Error ? e.message : 'Failed to save AI configuration';
			throw e;
		} finally {
			loadingAi = false;
		}
	}

	async function loadStatus(){
		try{
			const status = await adminApi.getStatus();
			isDbConfigured = status.isDbConfigured;
			if(typeof window !== 'undefined'){
				localStorage.setItem('isDbConfigured', String(status.isDbConfigured));
			}
		} catch {
			return;
		}
	}

	function clearError() {
		error = null;
	}

	return {
		get dbConfig() { return dbConfig; },
		get aiConfig() { return aiConfig; },
		get isDbConfigured() { return isDbConfigured; },
		get aiProvider() { return aiProvider; },
		get hasCustomAi() { return hasCustomAi; },
		get loadingDb() { return loadingDb; },
		get loadingAi() { return loadingAi; },
		get loading() { return loadingDb || loadingAi; },
		get error() { return error; },
		saveDb,
		saveAi,
		clearError,
		loadStatus
	};
}

export const configStore = createConfigStore();
