"use client";

import { useRouter } from "next/navigation";
import { useEffect } from "react";
import Cookies from "js-cookie";

import { createGuestUser } from "@/lib/apiClient";
import RefreshRedirect from "./RefreshRedirect";
import constants from "@/lib/constants";

/**
 * Create a guest user and retry
 */
const GuestRedirect = () => {
    const router = useRouter();
    const shouldBeLoggedIn =
        Cookies.get(constants.COOKIES.IS_LOGGED_IN) !== undefined;

    useEffect(() => {
        if (shouldBeLoggedIn) return;

        async function handleCreateGuest() {
            const { error } = await createGuestUser();
            if (error) {
                console.error(error);
                router.replace(constants.PATHS.SIGNIN);
                return;
            }

            router.refresh();
        }
        handleCreateGuest();
    }, [router, shouldBeLoggedIn]);

    return shouldBeLoggedIn && <RefreshRedirect />;
};
export default GuestRedirect;
