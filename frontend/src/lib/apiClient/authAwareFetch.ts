import ensureAuth from "@/features/auth/lib/ensureAuth";
import { navigate } from "@/actions/navigate";
import constants from "../constants";

export default async function authAwareFetch(
    input: URL | RequestInfo,
    init?: RequestInit,
): Promise<Response> {
    const response = await fetch(input, init);

    // if the server is making this request we don't want to auto refresh
    const isServerRequest = typeof window === "undefined";
    if (response.status !== 401 || isServerRequest) return response;

    const canRetryAuthed = await ensureAuth();
    if (!canRetryAuthed) {
        await navigate(constants.PATHS.LOGOUT);
        return response;
    }

    const newResponse = await fetch(input, init);
    if (newResponse.status === 401) {
        await navigate(constants.PATHS.LOGOUT);
        return newResponse;
    }

    return newResponse;
}
