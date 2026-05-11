import {api} from './client';
import type {
	DbConfig,
	AiConfig,
	GenerateEndpointRequest,
	SaveEndpointRequest,
	EndpointData,
	GenerateResponse
} from '$lib/api/types';

export const adminApi = {
	saveDbConfig: (config: DbConfig)=>
		api.post<{message: string}>('/api/admin/db-config', config),
	saveAiConfig: (config: AiConfig) =>
		api.post<{message: string}>('/api/admin/ai-config', config),
	generateEndpoint: (req: GenerateEndpointRequest)=>
		api.post<GenerateResponse>('/api/admin/generate-endpoint', req),
	saveEndpoint: (req: SaveEndpointRequest)=>
		api.post<{message: string}>('/api/admin/save-endpoint', req),
	deleteEndpoint: (name: string) =>
		api.delete<{message: string}>('/api/admin/delete-endpoint', {name}),
	getAllEndpoint: () =>
		api.get<EndpointData[]>('/api/admin/all-endpoints'),
	getStatus: () => api.get<{ isDbConfigured: boolean; isAiConfigured: boolean }>('/api/admin/status')
};