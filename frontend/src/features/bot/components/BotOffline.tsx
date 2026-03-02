"use client";

import { useEffect, useEffectEvent, useState } from "react";
import { useRouter } from "next/navigation";

import { checkBotHealth } from "@/lib/apiClient";
import Button from "@/components/ui/Button";
import constants from "@/lib/constants";

const BotOffline = () => {
    const router = useRouter();
    const [isRefreshing, setIsRefreshing] = useState(false);

    async function tryAgain() {
        setIsRefreshing(true);

        const delay = new Promise((resolve) => setTimeout(resolve, 3000));
        await Promise.all([checkHealthAndRedirect(), delay]);

        setIsRefreshing(false);
    }

    async function checkHealthAndRedirect() {
        const { error, data: isHealthy } = await checkBotHealth();
        if (error || isHealthy === undefined) {
            console.error("BotOffline", error);
            return;
        }
        if (isHealthy) {
            router.replace(constants.PATHS.BOT);
        }
    }

    const checkHealthAndRedirectEvent = useEffectEvent(checkHealthAndRedirect);
    useEffect(() => void checkHealthAndRedirectEvent(), []);

    return (
        <div
            className="mx-auto flex h-screen w-screen max-w-lg flex-1 flex-col
                items-center justify-center gap-5 px-1 text-center"
        >
            <h1 className="text-3xl">Anarchy Bot Is offline</h1>
            <p className="text-text/80 text-balance">
                Anarchy Bot runs on a separate VM to handle its processing
                needs. A VM with 4 vCPU could cost upwards of $140 per month, so
                to reduce cost, it&apos;s hosted on an Azure Spot VM, which uses
                leftover server capacity. Spot VMs can be reclaimed at any time
                when Azure needs the capacity, so the bot may be temporarily
                offline, roughly 5% downtime.
            </p>
            <Button
                className="w-full"
                onClick={tryAgain}
                disabled={isRefreshing}
            >
                Try Again
            </Button>
        </div>
    );
};
export default BotOffline;
