import { ErrorCode } from "@/lib/apiClient";

export interface SignalRError {
    code: ErrorCode;
    description: string;
}
