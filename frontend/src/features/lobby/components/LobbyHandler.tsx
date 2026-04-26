"use client";

import { useEffect } from "react";

import gameStartRedirect from "@/features/liveGame/lib/gameStartRedirect";
import { useLobbyEmitter, useLobbyEvent } from "../hooks/useLobbyHub";
import { usePathname, useRouter } from "next/navigation";
import useLobbyStore from "../stores/lobbyStore";

const LobbyHandler = () => {
    const router = useRouter();
    const pathname = usePathname();

    const sendLobbyEvents = useLobbyEmitter();

    useLobbyEvent("MatchFoundAsync", async (token) => {
        await gameStartRedirect(token, router);
    });

    useLobbyEvent("ReceiveOngoingGamesAsync", (games) => {
        const { addOngoingGames } = useLobbyStore.getState();
        addOngoingGames(games);
    });

    useLobbyEvent("OngoingGameEndedAsync", (gameToken) => {
        const { removeOngoingGame } = useLobbyStore.getState();
        removeOngoingGame(gameToken);
    });

    useEffect(() => {
        const {
            seeks,
            requestedOpenSeek,
            lastSeekingPath,
            setLastSeekingPath,
            setRequestedOpenSeek,
            clearSeeks,
        } = useLobbyStore.getState();

        if (lastSeekingPath === null) {
            setLastSeekingPath(pathname);
            return;
        }

        if (lastSeekingPath === pathname) {
            return;
        }

        setLastSeekingPath(pathname);
        clearSeeks();

        if (requestedOpenSeek || seeks.size !== 0) {
            sendLobbyEvents("CleanupConnectionAsync");
            setRequestedOpenSeek(false);
        }
    }, [pathname, sendLobbyEvents]);

    return null;
};
export default LobbyHandler;
