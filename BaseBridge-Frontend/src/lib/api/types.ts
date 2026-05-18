export interface DbConfig{
	dbType: string;
	host: string;
	dbName: string;
	user: string;
	password: string;
}

export interface AiConfig{
	provider: string;
	apiKey: string;
	modelName: string;
}

export interface EndpointParameter{
	name: string;
	type: string;
}

export interface EndpointData{
	name: string;
	path: string;
	query: string;
	description: string;
	parameters?: EndpointParameter[];
}

export interface DeleteRequest{
	name: string;
}

export interface GenerateEndpointRequest{
	name: string;
	endpointDescription: string;
	parameters?: EndpointParameter[];
}

export interface SaveEndpointRequest{
	name: string;
	path: string;
	endpointDescription: string;
	validatedQuery: string;
	parameters?: EndpointParameter[];
}

export interface GenerateResponse{
	generatedQuery: string;
	message: string;
	parameters?: EndpointParameter[];
}

export interface RelationshipInfo{
	column: string;
	referencedTable: string;
	referencedColumn: string;
	type: string
}

export interface ColumnInfo{
	name: string;
	dataType: string;
	isPrimaryKey: boolean;
	isForeignKey: boolean;
}

export interface TableInfo{
	name: string;
	columns: ColumnInfo[];
	relationships: RelationshipInfo[];
	sampleData: Record<string, any>[];
}

export interface DatabaseSchemaResponse{
	tables: TableInfo[];
}