import { StoreApi, useStore } from "zustand";
import { useRouter } from "next/navigation";
import { useRef } from "react";

import { ChessboardStore } from "@/features/chessboard/stores/chessboardStore";
import { decodeMovePath, decodeLegalMoves } from "../lib/moveDecoder";
import AudioPlayer, { AudioType } from "@/features/audio/audioPlayer";
import LegalMoves from "@/features/chessboard/lib/legalMoves";
import { LiveChessStore } from "../stores/liveChessStore";
import gameStartRedirect from "../lib/gameStartRedirect";
import { refetchGame } from "../lib/gameStateProcessor";
import handleMoveUpdate from "../lib/handleMoveUpdate";
import { Clocks, MoveSnapshot } from "@/lib/apiClient";
import { LogicalPoint } from "@/features/point/types";
import { useGameEvent } from "./useGameHub";

export default function useLiveChessEvents(
    liveChessStore: StoreApi<LiveChessStore>,
    chessboardStore: StoreApi<ChessboardStore>,
) {
    const gameToken = useStore(liveChessStore, (x) => x.gameToken);
    const router = useRouter();

    const queuedOvertimeRef = useRef<
        Map<
            number,
            { overtimeRemovals: LogicalPoint[]; legalMoves: LegalMoves }
        >
    >(new Map());

    async function processMove({
        move,
        plyNumber,
        clocks,
        legalMoves,
    }: {
        move: MoveSnapshot;
        plyNumber: number;
        clocks: Clocks;
        legalMoves?: LegalMoves;
    }): Promise<void> {
        const decodedMove = decodeMovePath(move.path);

        const queuedOvertime = queuedOvertimeRef.current.get(plyNumber);
        if (queuedOvertime) {
            legalMoves = queuedOvertime.legalMoves;
            decodedMove.overtimeRemovals.push(
                ...queuedOvertime.overtimeRemovals,
            );
        }

        const success = await handleMoveUpdate(
            liveChessStore,
            chessboardStore,
            {
                move: move,
                decodedMove,
                plyNumber,
                legalMoves,
                clocks,
            },
        );

        if (success) {
            queuedOvertimeRef.current.delete(plyNumber);
        } else {
            await refetchGame(liveChessStore, chessboardStore);
        }
    }

    useGameEvent(gameToken, "SyncRevisionAsync", async (currentRevision) => {
        const { sourceRevision } = liveChessStore.getState();
        if (sourceRevision !== currentRevision) {
            await refetchGame(liveChessStore, chessboardStore);
        }
    });

    useGameEvent(
        gameToken,
        "MoveMadeAsync",
        async (move, plyNumber, clocks, didMoveEndGame) => {
            const { viewer } = liveChessStore.getState();
            if (viewer.playerColor === move.nextSideToMove) return;

            // undefined = we don't know the legal moves
            // defined, but empty = we know the legal moves, there aren't any, no need to fetch them
            const legalMoves = didMoveEndGame
                ? LegalMoves.StableEmpty
                : undefined;

            await processMove({ move, plyNumber, clocks, legalMoves });
        },
    );

    useGameEvent(
        gameToken,
        "OpponentMoveMadeAsync",
        async (move, plyNumber, encodedLegalMoves, clocks) => {
            const { viewer } = liveChessStore.getState();
            if (viewer.playerColor === null) return;

            const decodedLegalMoves = decodeLegalMoves(encodedLegalMoves);
            await processMove({
                move,
                plyNumber,
                clocks,
                legalMoves: decodedLegalMoves,
            });
        },
    );

    useGameEvent(gameToken, "DrawStateChangeAsync", (drawState) =>
        liveChessStore.getState().drawStateChange(drawState),
    );

    useGameEvent(
        gameToken,
        "ReceiveOvertimeAsync",
        async (plyNumber, removedFrom, encodedLegalMoves) => {
            const { removePieceAt, addLegalMovesForPosition, positionHistory } =
                chessboardStore.getState();

            const legalMoves = decodeLegalMoves(encodedLegalMoves);

            const plyDiff = plyNumber - positionHistory.mainPlyCount;
            if (plyDiff > 1) {
                console.warn(
                    `Received overtime for ply ${plyNumber}, which is ahead of current main ply ${positionHistory.mainPlyCount} by more than 1`,
                );
                await refetchGame(liveChessStore, chessboardStore);
                return;
            } else if (plyDiff === 1) {
                const queue = queuedOvertimeRef.current.get(plyNumber) ?? {
                    overtimeRemovals: [],
                    legalMoves,
                };

                queue.legalMoves = legalMoves;
                queue.overtimeRemovals.push(removedFrom);
                queuedOvertimeRef.current.set(plyNumber, queue);
                return;
            }

            const position = positionHistory.getPositionWithPly(plyNumber);
            if (!position) {
                return;
            }

            addLegalMovesForPosition(legalMoves, position.positionId);
            position.commitOvertimeRemoval(removedFrom);
            removePieceAt(removedFrom);
        },
    );

    useGameEvent(gameToken, "ReceiveErrorAsync", async () => {
        await refetchGame(liveChessStore, chessboardStore);
    });

    useGameEvent(gameToken, "GameEndedAsync", async (result, finalClocks) => {
        const { setAllowHistoryChanges, positionHistory } =
            chessboardStore.getState();

        liveChessStore
            .getState()
            .endGame(positionHistory.mainPlyCount, result, finalClocks);
        setAllowHistoryChanges(true);
        AudioPlayer.playAudio(AudioType.GAME_END);
    });

    useGameEvent(
        gameToken,
        "RematchAcceptedAsync",
        async (createdGameToken) => {
            await gameStartRedirect(createdGameToken, router);
        },
    );
}
