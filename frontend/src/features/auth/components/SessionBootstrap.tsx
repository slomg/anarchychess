"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import Cookies from "js-cookie";

import { createGuestUser } from "@/lib/apiClient";
import rawClient from "@/lib/apiClient/rawClient";
import AuthRefresh from "./AuthRefresh";
import constants from "@/lib/constants";

/**
 * Create a guest user and retry
 */
const SessionBootstrap = () => {
    const router = useRouter();
    const shouldBeLoggedIn =
        Cookies.get(constants.COOKIES.IS_LOGGED_IN) !== undefined;

    useEffect(() => {
        if (shouldBeLoggedIn) return;

        async function bootstrapGuestSession() {
            const { error } = await createGuestUser({ client: rawClient });
            if (error) {
                console.error("SessionBootstrap createGuestUser", error);
                router.replace(constants.PATHS.SIGNIN);
                return;
            }

            router.refresh();
        }
        bootstrapGuestSession();
    }, [router, shouldBeLoggedIn]);

    return shouldBeLoggedIn && <AuthRefresh />;
};
export default SessionBootstrap;
