"use client";

import { useRouter } from "next/navigation";
import { useEffect, useRef } from "react";

import rawClient from "@/lib/apiClient/rawClient";
import { refresh } from "@/lib/apiClient";
import constants from "@/lib/constants";

/**
 * Refresh access token and retry
 */
const AuthRefresh = () => {
    const router = useRouter();
    const hasRefreshedRef = useRef(false);

    useEffect(() => {
        if (hasRefreshedRef.current) return;
        hasRefreshedRef.current = true;

        async function handleRefresh() {
            const { error } = await refresh({ client: rawClient });
            if (error) {
                console.error("AuthRefresh", error);
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
