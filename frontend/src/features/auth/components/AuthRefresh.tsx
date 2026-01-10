"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";

import rawClient from "@/lib/apiClient/rawClient";
import { refresh } from "@/lib/apiClient";
import constants from "@/lib/constants";

/**
 * Refresh access token and retry
 */
const AuthRefresh = () => {
    const router = useRouter();

    useEffect(() => {
        async function handleRefresh() {
            const { error } = await refresh({ client: rawClient });
            if (error) {
                console.log(error);
                router.replace(constants.PATHS.LOGOUT);
                return;
            }

            router.refresh();
        }
        handleRefresh();
    }, [router]);

    return null;
};
export default AuthRefresh;
