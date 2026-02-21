import { LegalDocumentResponse } from "@/models/legal.models";
import { AxiosResponse } from "axios";
import { api, API_URL } from "./utils";

export function getLegal(documentType:string):Promise<AxiosResponse<LegalDocumentResponse>> {
    return api.get<LegalDocumentResponse>(`${API_URL}/legal?type=${documentType}`);
}